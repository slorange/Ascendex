using System;
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

    private readonly GameSession _session;
    private readonly IBarProgressState _progress;
    private readonly SpeciesProgress? _speciesProgress;
    private readonly SpeciesBarConfig? _speciesConfig;
    private readonly TrainerBarConfig? _trainerConfig;
    private readonly Action<PokemonTrainingBarViewModel> _toggleTrainingRequested;
    private readonly EvolutionStage[]? _evolutionChain;

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
            NormalBrush = Brush.Parse(ResolveBarColorHex(speciesProgress.SpeciesRootName, normalColor, string.Empty));
        }

        NotifyProgressDerivedPropertiesChanged();
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
        NormalBrush = Brush.Parse(normalColor);

        _progress.PropertyChanged += OnProgressStateChanged;
        NotifyProgressDerivedPropertiesChanged();
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

    public double ProgressRequired => _speciesConfig != null
        ? TrainingSimulator.GetSpeciesProgressRequired(_speciesConfig, Level)
        : TrainingSimulator.GetTrainerProgressRequired(_trainerConfig!, Level);

    public double ProgressFraction => ProgressRequired == 0 ? 0 : Progress / ProgressRequired;

    public double VisualProgressFraction =>
        IsUltraFastTrainingPace() && IsActivityActive ? 1.0 : ProgressFraction;

    public string TimeRemainingText => FormatTimeRemaining();

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
                ApplyEvolutionStageForCurrentLevel();
                NotifyLevelBadgeVisibilityChanged();
                break;
            case nameof(IBarProgressState.Progress):
            case nameof(IBarProgressState.IsTraining):
            case nameof(SpeciesProgress.IsCatching):
            case nameof(SpeciesProgress.IsShiny):
            case nameof(IBarProgressState.IsVisible):
                if (e.PropertyName is nameof(IBarProgressState.IsTraining) or nameof(SpeciesProgress.IsCatching))
                {
                    OnPropertyChanged(nameof(IsActivityActive));
                    OnPropertyChanged(nameof(TrainingBorderThickness));
                    OnPropertyChanged(nameof(TrainingBorderBrush));
                }

                if (e.PropertyName is nameof(SpeciesProgress.IsCatching))
                {
                    NotifyLevelBadgeVisibilityChanged();
                }

                if (e.PropertyName is nameof(SpeciesProgress.IsShiny))
                {
                    OnPropertyChanged(nameof(ShowShinyIcon));
                    ApplyBarColorForCurrentStage();
                }

                break;
        }

        OnPropertyChanged(e.PropertyName!);
        NotifyProgressDerivedPropertiesChanged();
    }

    private void NotifyLevelBadgeVisibilityChanged()
    {
        OnPropertyChanged(nameof(ShowLevelBadge));
        OnPropertyChanged(nameof(ShowCatchingPokeball));
    }

    private void NotifyProgressDerivedPropertiesChanged()
    {
        OnPropertyChanged(nameof(ProgressRequired));
        OnPropertyChanged(nameof(ProgressFraction));
        OnPropertyChanged(nameof(VisualProgressFraction));
        OnPropertyChanged(nameof(TimeRemainingText));
    }

    private void ApplyEvolutionStageForCurrentLevel()
    {
        if (!TrainingSimulator.TryGetResolvedEvolutionStage(_evolutionChain, Level, out var stage))
        {
            return;
        }

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

        NormalBrush = Brush.Parse(colorHex);
        OnPropertyChanged(nameof(NormalBrush));
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
        OnPropertyChanged(nameof(TimeRemainingText));
        OnPropertyChanged(nameof(VisualProgressFraction));
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
        var effectivePerTick = GetEffectiveProgressPerTick();
        if (ProgressRequired <= 0 || effectivePerTick <= 0)
        {
            return false;
        }

        var fullBarMs = ProgressRequired / effectivePerTick * TrainingTickInterval.TotalMilliseconds;
        return fullBarMs < MagicNumbersUI.TimeRemaining.UltraFastFullBarMaxDurationSeconds * 1000.0;
    }

    private string FormatTimeRemaining()
    {
        var effectivePerTick = GetEffectiveProgressPerTick();
        if (ProgressRequired <= 0 || effectivePerTick <= 0)
        {
            return "0s";
        }

        if (IsUltraFastTrainingPace())
        {
            return "0s";
        }

        var remainingProgress = Math.Max(0, ProgressRequired - Progress);
        var remainingMilliseconds = remainingProgress / effectivePerTick * TrainingTickInterval.TotalMilliseconds;
        var remaining = TimeSpan.FromMilliseconds(remainingMilliseconds);
        var totalSeconds = (int)Math.Ceiling(Math.Max(0, remaining.TotalSeconds));

        if (totalSeconds >= MagicNumbersUI.TimeRemaining.SecondsBeforeMinuteTimeFormat)
        {
            return $"{totalSeconds / 60}:{totalSeconds % 60:D2}";
        }

        return $"{totalSeconds}s";
    }
}
