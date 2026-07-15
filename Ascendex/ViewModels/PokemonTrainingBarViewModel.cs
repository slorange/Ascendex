using System;
using System.Collections.Generic;
using System.ComponentModel;
using Ascendex.Game;
using Ascendex.Game.Content;
using Avalonia;
using Avalonia.Media;
using CommunityToolkit.Mvvm.Input;

namespace Ascendex.ViewModels;

public partial class PokemonTrainingBarViewModel : ViewModelBase
{
    private static readonly IBrush IdleBorderBrush = Brush.Parse("#5F6470");
    private static readonly IBrush ActiveBorderBrush = Brushes.White;
    private static readonly TimeSpan TrainingTickInterval = TimeSpan.FromMilliseconds(GameBalance.Training.TickIntervalMilliseconds);
    private static readonly Dictionary<string, IBrush> BrushCache = new(StringComparer.OrdinalIgnoreCase);

    private readonly GameSession _session;
    private readonly IBarProgressState _progress;
    private readonly SpeciesProgress? _speciesProgress;
    private readonly SpeciesBarConfig? _speciesConfig;
    private readonly TrainerBarConfig? _trainerConfig;
    private readonly Action<PokemonTrainingBarViewModel> _toggleTrainingRequested;
    private readonly EvolutionStage[]? _evolutionChain;
    private double _progressRequired;
    private double _effectiveProgressPerTick;
    private double _visualProgressFraction;
    private string _timeRemainingText = "0s";
    private int _activeEvolutionStageIndex = -1;

    public PokemonTrainingBarViewModel(
        GameSession session,
        SpeciesProgress speciesProgress,
        string typeKey,
        string normalColor,
        Action<PokemonTrainingBarViewModel> toggleTrainingRequested)
    {
        _session = session;
        _progress = speciesProgress;
        _speciesProgress = speciesProgress;
        _speciesConfig = session.GetSpeciesBarConfig(speciesProgress.SpeciesRootName);
        _evolutionChain = _speciesConfig.EvolutionChain;
        _toggleTrainingRequested = toggleTrainingRequested;
        SpeciesLineRoot = speciesProgress.SpeciesRootName;

        _progress.PropertyChanged += OnProgressStateChanged;

        if (_evolutionChain != null)
        {
            ApplyEvolutionStageForCurrentLevel();
        }
        else
        {
            Name = speciesProgress.SpeciesRootName;
            TypeKey = typeKey;
            NormalBrush = GetBrush(ResolveBarColorHex(speciesProgress.SpeciesRootName, normalColor, string.Empty));
        }

        RefreshProgressRequirement();
        RefreshCachedRateAndPresentation(refreshTimeText: true);
    }

    public PokemonTrainingBarViewModel(
        GameSession session,
        TrainerProgress trainerProgress,
        string displayName,
        string typeKey,
        string normalColor,
        Action<PokemonTrainingBarViewModel> toggleTrainingRequested)
    {
        _session = session;
        _progress = trainerProgress;
        _trainerConfig = session.GetTrainerBarConfig(trainerProgress.TrainerId);
        _toggleTrainingRequested = toggleTrainingRequested;
        TrainerId = trainerProgress.TrainerId;
        SpeciesLineRoot = displayName;
        Name = displayName;
        TypeKey = typeKey;
        NormalBrush = GetBrush(normalColor);

        _progress.PropertyChanged += OnProgressStateChanged;
        RefreshProgressRequirement();
        RefreshCachedRateAndPresentation(refreshTimeText: true);
    }

    public string Name { get; private set; } = string.Empty;

    public string TypeKey { get; private set; } = string.Empty;

    public IBrush NormalBrush { get; private set; } = Brushes.Transparent;

    public string SpeciesLineRoot { get; }

    public string? TrainerId { get; }

    public double BaseProgressRequired =>
        _speciesConfig?.BaseProgressRequired ?? _trainerConfig?.BaseProgressRequired ?? 0;

    public int Level => _progress.Level;

    public double Progress => _progress.Progress;

    public bool IsTraining => _progress.IsTraining;

    public bool IsCatching => _speciesProgress?.IsCatching ?? false;

    public bool IsVisible
    {
        get => _progress.IsVisible;
        set => _progress.IsVisible = value;
    }

    public double ProgressRequired => _progressRequired;

    public double ProgressFraction => ProgressRequired == 0 ? 0 : Progress / ProgressRequired;

    public double VisualProgressFraction => _visualProgressFraction;

    public string TimeRemainingText => _timeRemainingText;

    public bool IsActivityActive => IsTraining || IsCatching;

    public Thickness TrainingBorderThickness =>
        IsActivityActive
            ? new Thickness(MagicNumbersUI.TrainingBar.ActiveOutlineThickness)
            : new Thickness(MagicNumbersUI.TrainingBar.IdleOutlineThickness);

    public IBrush TrainingBorderBrush => IsActivityActive ? ActiveBorderBrush : IdleBorderBrush;

    public bool ShowLevelBadge => Level > 0;

    public bool ShowCatchingPokeball => Level == 0 && IsCatching;

    public bool ShowShinyIcon => _speciesProgress?.IsShiny == true;

    public bool CanCatch => _speciesConfig?.AllowsCatching == true && Level == 0;

    private void OnProgressStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IBarProgressState.Level):
                OnPropertyChanged(nameof(Level));
                RefreshProgressRequirement(notify: true);
                ApplyEvolutionStageForCurrentLevel();
                NotifyLevelBadgeVisibilityChanged();
                OnPropertyChanged(nameof(CanCatch));
                OnPropertyChanged(nameof(ProgressFraction));
                RefreshCachedRateAndPresentation(refreshTimeText: true);
                break;
            case nameof(IBarProgressState.Progress):
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressFraction));
                RefreshVisualProgress();
                break;
            case nameof(IBarProgressState.IsTraining):
                OnPropertyChanged(nameof(IsTraining));
                NotifyActivityChanged();
                RefreshCachedRateAndPresentation(refreshTimeText: true);
                break;
            case nameof(SpeciesProgress.IsCatching):
                OnPropertyChanged(nameof(IsCatching));
                NotifyActivityChanged();
                NotifyLevelBadgeVisibilityChanged();
                RefreshCachedRateAndPresentation(refreshTimeText: true);
                break;
            case nameof(SpeciesProgress.IsShiny):
                OnPropertyChanged(nameof(ShowShinyIcon));
                ApplyBarColorForCurrentStage();
                break;
            case nameof(IBarProgressState.IsVisible):
                OnPropertyChanged(nameof(IsVisible));
                break;
            default:
                if (!string.IsNullOrEmpty(e.PropertyName))
                {
                    OnPropertyChanged(e.PropertyName);
                }

                break;
        }
    }

    private void NotifyLevelBadgeVisibilityChanged()
    {
        OnPropertyChanged(nameof(ShowLevelBadge));
        OnPropertyChanged(nameof(ShowCatchingPokeball));
    }

    private void NotifyActivityChanged()
    {
        OnPropertyChanged(nameof(IsActivityActive));
        OnPropertyChanged(nameof(TrainingBorderThickness));
        OnPropertyChanged(nameof(TrainingBorderBrush));
    }

    private void ApplyEvolutionStageForCurrentLevel()
    {
        if (!TrainingSimulator.TryGetResolvedEvolutionStage(_evolutionChain, Level, out var stage))
        {
            return;
        }

        var stageIndex = TrainingSimulator.GetActiveStageIndexZeroBased(_evolutionChain!, Level);
        if (_activeEvolutionStageIndex == stageIndex)
        {
            return;
        }

        _activeEvolutionStageIndex = stageIndex;
        Name = stage.Name;
        TypeKey = stage.TypeKey;
        ApplyBarColorForCurrentStage(stage);
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(TypeKey));
    }

    private void ApplyBarColorForCurrentStage(EvolutionStage? stage = null)
    {
        if (stage == null && _evolutionChain != null)
        {
            if (!TrainingSimulator.TryGetResolvedEvolutionStage(_evolutionChain, Level, out var resolved))
            {
                return;
            }

            stage = resolved;
        }

        var colorHex = stage != null
            ? ResolveBarColorHex(stage.Value.Name, stage.Value.NormalColor, stage.Value.ShinyColor)
            : _speciesProgress != null && KantoSpeciesCatalog.TryGetColorsForDexName(
                _speciesProgress.SpeciesRootName,
                out var normal,
                out var shiny)
                ? ResolveBarColorHex(_speciesProgress.SpeciesRootName, normal, shiny)
                : null;

        if (colorHex == null)
        {
            return;
        }

        var brush = GetBrush(colorHex);
        if (!ReferenceEquals(NormalBrush, brush))
        {
            NormalBrush = brush;
            OnPropertyChanged(nameof(NormalBrush));
        }
    }

    private string ResolveBarColorHex(string speciesName, string normalColor, string shinyColor)
    {
        if (_speciesProgress?.IsShiny != true)
        {
            return normalColor;
        }

        if (!string.IsNullOrEmpty(shinyColor))
        {
            return shinyColor;
        }

        return KantoSpeciesCatalog.TryGetColorsForDexName(speciesName, out _, out var catalogShiny)
            ? catalogShiny
            : normalColor;
    }

    [RelayCommand]
    private void ToggleTraining() => _toggleTrainingRequested(this);

    public void NotifyTimeRemainingChanged()
    {
        RefreshCachedRateAndPresentation(refreshTimeText: true);
    }

    private double GetEffectiveProgressPerTick()
    {
        if (_speciesConfig != null && _speciesProgress != null)
        {
            return TrainingSimulator.GetSpeciesProgressPerTick(_session, _speciesProgress, _speciesConfig);
        }

        if (_trainerConfig != null && _progress is TrainerProgress trainerProgress)
        {
            return TrainingSimulator.GetTrainerProgressPerTick(_session, trainerProgress, _trainerConfig);
        }

        return 0;
    }

    private bool IsUltraFastTrainingPace()
    {
        if (_progressRequired <= 0 || _effectiveProgressPerTick <= 0)
        {
            return false;
        }

        var fullBarMs = _progressRequired / _effectiveProgressPerTick * TrainingTickInterval.TotalMilliseconds;
        return fullBarMs < MagicNumbersUI.TimeRemaining.UltraFastFullBarMaxDurationSeconds * 1000.0;
    }

    private string FormatTimeRemaining()
    {
        if (_progressRequired <= 0 || _effectiveProgressPerTick <= 0)
        {
            return "0s";
        }

        if (IsUltraFastTrainingPace())
        {
            return "0s";
        }

        var remainingProgress = Math.Max(0, _progressRequired - Progress);
        var remainingMilliseconds = remainingProgress / _effectiveProgressPerTick * TrainingTickInterval.TotalMilliseconds;
        var remaining = TimeSpan.FromMilliseconds(remainingMilliseconds);
        var totalSeconds = (int)Math.Ceiling(Math.Max(0, remaining.TotalSeconds));

        if (totalSeconds >= MagicNumbersUI.TimeRemaining.SecondsBeforeMinuteTimeFormat)
        {
            return $"{totalSeconds / 60}:{totalSeconds % 60:D2}";
        }

        return $"{totalSeconds}s";
    }

    private void RefreshProgressRequirement(bool notify = false)
    {
        _progressRequired = _speciesConfig != null
            ? _session.GetSpeciesProgressRequired(_speciesConfig, Level)
            : _session.GetTrainerProgressRequired(_trainerConfig!, Level);
        if (notify)
        {
            OnPropertyChanged(nameof(ProgressRequired));
        }
    }

    private void RefreshCachedRateAndPresentation(bool refreshTimeText)
    {
        _effectiveProgressPerTick = GetEffectiveProgressPerTick();
        RefreshVisualProgress();
        if (refreshTimeText)
        {
            var next = FormatTimeRemaining();
            if (_timeRemainingText != next)
            {
                _timeRemainingText = next;
                OnPropertyChanged(nameof(TimeRemainingText));
            }
        }
    }

    private void RefreshVisualProgress()
    {
        var next = IsUltraFastTrainingPace() && IsActivityActive ? 1.0 : ProgressFraction;
        if (Math.Abs(_visualProgressFraction - next) < double.Epsilon)
        {
            return;
        }

        _visualProgressFraction = next;
        OnPropertyChanged(nameof(VisualProgressFraction));
    }

    private static IBrush GetBrush(string colorHex)
    {
        if (!BrushCache.TryGetValue(colorHex, out var brush))
        {
            brush = Brush.Parse(colorHex);
            BrushCache[colorHex] = brush;
        }

        return brush;
    }
}
