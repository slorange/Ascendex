using System;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Ascendex.ViewModels;

public partial class PokemonTrainingBarViewModel : ViewModelBase
{
    private static readonly IBrush IdleBorderBrush = Brush.Parse("#5F6470");
    private static readonly IBrush ActiveBorderBrush = Brushes.White;
    private static readonly TimeSpan TrainingTickInterval = TimeSpan.FromMilliseconds(GameBalance.Training.TickIntervalMilliseconds);

    private readonly Action<TypeLevelContribution[]> _recordTypeLevelContributions;
    private readonly Action<PokemonTrainingBarViewModel> _recordLevelChanged;
    private readonly Action<PokemonTrainingBarViewModel> _toggleTrainingRequested;
    private readonly Func<double>? _getProgressMultiplier;
    private readonly EvolutionStage[]? _evolutionChain;
    private readonly double _progressRequiredPerLevelExponent;
    private readonly DispatcherTimer _trainingTimer;

    public PokemonTrainingBarViewModel(
        string name,
        string typeKey,
        string accentColor,
        string accentForegroundColor,
        Action<PokemonTrainingBarViewModel> toggleTrainingRequested,
        Action<PokemonTrainingBarViewModel> recordLevelChanged,
        Action<TypeLevelContribution[]> recordTypeLevelContributions,
        double progressRequired,
        double progressRequiredPerLevelExponent,
        Func<double>? getProgressMultiplier = null,
        EvolutionStage[]? evolutionChain = null)
    {
        _recordTypeLevelContributions = recordTypeLevelContributions;
        _recordLevelChanged = recordLevelChanged;
        _toggleTrainingRequested = toggleTrainingRequested;
        _getProgressMultiplier = getProgressMultiplier;
        _evolutionChain = evolutionChain is { Length: > 0 } ? evolutionChain : null;
        _progressRequiredPerLevelExponent = progressRequiredPerLevelExponent;
        BaseProgressRequired = progressRequired;
        Progress = 0;
        SpeciesLineRoot = name;

        if (_evolutionChain != null)
        {
            Level = 0;
            // Level defaults to 0 before assignment, so OnLevelChanged does not run; still need first stage name/colors.
            ApplyEvolutionStageForCurrentLevel();
            OnPropertyChanged(nameof(ProgressRequired));
            OnPropertyChanged(nameof(ProgressFraction));
            OnPropertyChanged(nameof(VisualProgressFraction));
            OnPropertyChanged(nameof(TimeRemainingText));
            _recordLevelChanged(this);
        }
        else
        {
            Name = name;
            TypeKey = typeKey;
            AccentBrush = Brush.Parse(accentColor);
            AccentForegroundBrush = Brush.Parse(accentForegroundColor);
            Level = 0;
        }

        _trainingTimer = new DispatcherTimer
        {
            Interval = TrainingTickInterval
        };
        _trainingTimer.Tick += OnTrainingTimerTick;
    }

    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _typeKey = string.Empty;

    [ObservableProperty]
    private IBrush _accentBrush = Brushes.Transparent;

    [ObservableProperty]
    private IBrush _accentForegroundBrush = Brushes.Transparent;

    /// <summary>Route species key for evolution lookup (initial species name); unchanged when the bar evolves.</summary>
    public string SpeciesLineRoot { get; }

    public double BaseProgressRequired { get; }

    public double ProgressRequired => BaseProgressRequired * Math.Pow(_progressRequiredPerLevelExponent, Math.Max(0, Level));

    public double ProgressFraction => ProgressRequired == 0 ? 0 : (double)Progress / ProgressRequired;

    /// <summary>Bar fill for UI: full while training if pace is ultra-fast; otherwise matches <see cref="ProgressFraction"/>.</summary>
    public double VisualProgressFraction =>
        IsUltraFastTrainingPace() && IsTraining ? 1.0 : ProgressFraction;

    public string TimeRemainingText => FormatTimeRemaining();

    public Thickness TrainingBorderThickness =>
        IsTraining
            ? new Thickness(MagicNumbersUI.TrainingBar.ActiveOutlineThickness)
            : new Thickness(MagicNumbersUI.TrainingBar.IdleOutlineThickness);

    public IBrush TrainingBorderBrush => IsTraining ? ActiveBorderBrush : IdleBorderBrush;

    [ObservableProperty]
    private int _level;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private bool _isTraining;

    /// <summary>When false, the row is hidden (used for battle unlock order; always true for route Pokémon).</summary>
    [ObservableProperty]
    private bool _isVisible = true;

    partial void OnProgressChanged(double value)
    {
        OnPropertyChanged(nameof(ProgressFraction));
        OnPropertyChanged(nameof(VisualProgressFraction));
        OnPropertyChanged(nameof(TimeRemainingText));
    }

    partial void OnLevelChanged(int value)
    {
        ApplyEvolutionStageForCurrentLevel();
        OnPropertyChanged(nameof(ProgressRequired));
        OnPropertyChanged(nameof(ProgressFraction));
        OnPropertyChanged(nameof(VisualProgressFraction));
        OnPropertyChanged(nameof(TimeRemainingText));
        _recordLevelChanged(this);
    }

    private int GetActiveStageIndexZeroBased()
    {
        if (_evolutionChain is not { Length: > 0 })
        {
            return 0;
        }

        var idx = 0;
        for (var i = 0; i < _evolutionChain.Length; i++)
        {
            if (Level >= _evolutionChain[i].MinLevel)
            {
                idx = i;
            }
            else
            {
                break;
            }
        }

        return idx;
    }

    private bool TryGetResolvedEvolutionStage(out EvolutionStage stage)
    {
        stage = default;
        if (_evolutionChain is not { Length: > 0 })
        {
            return false;
        }

        stage = _evolutionChain[0];
        foreach (var s in _evolutionChain)
        {
            if (Level >= s.MinLevel)
            {
                stage = s;
            }
            else
            {
                break;
            }
        }

        return true;
    }

    private void RecordTypeLevelContributionsForCurrentStage()
    {
        var chainLength = _evolutionChain?.Length ?? 1;
        var stageIndex = GetActiveStageIndexZeroBased();
        var totalPoints = TypeLevelUpLookup.PointsForChainStage(chainLength, stageIndex);

        string primary;
        string? secondary;
        if (TryGetResolvedEvolutionStage(out var resolved))
        {
            primary = resolved.TypeKey;
            secondary = resolved.SecondaryTypeKey;
        }
        else
        {
            primary = TypeKey;
            secondary = null;
        }

        _recordTypeLevelContributions(PokemonTypeContribution.SplitTotal(primary, secondary, totalPoints));
    }

    private void ApplyEvolutionStageForCurrentLevel()
    {
        if (!TryGetResolvedEvolutionStage(out var stage))
        {
            return;
        }

        Name = stage.Name;
        TypeKey = stage.TypeKey;
        AccentBrush = Brush.Parse(stage.AccentColor);
        AccentForegroundBrush = Brush.Parse(stage.ForegroundColor);
    }

    partial void OnIsTrainingChanged(bool value)
    {
        OnPropertyChanged(nameof(TrainingBorderThickness));
        OnPropertyChanged(nameof(TrainingBorderBrush));
        OnPropertyChanged(nameof(VisualProgressFraction));

        if (value)
        {
            _trainingTimer.Start();
            return;
        }

        _trainingTimer.Stop();
    }

    [RelayCommand]
    private void ToggleTraining()
    {
        _toggleTrainingRequested(this);
    }

    public void SetTraining(bool isTraining)
    {
        IsTraining = isTraining;
    }

    /// <summary>Call when an external factor changes effective training speed so <see cref="TimeRemainingText"/> and <see cref="VisualProgressFraction"/> refresh without waiting for the next tick.</summary>
    public void NotifyTimeRemainingChanged()
    {
        OnPropertyChanged(nameof(TimeRemainingText));
        OnPropertyChanged(nameof(VisualProgressFraction));
    }

    private void OnTrainingTimerTick(object? sender, EventArgs e)
    {
        if (ProgressRequired <= 0)
        {
            return;
        }

        var increase = GameBalance.Training.ProgressPerTick * GetClampedProgressMultiplier();
        Progress += increase;

		if (Progress < ProgressRequired)
        {
            return;
        }

        Progress = 0;

        if (_evolutionChain is { Length: > 0 } chain)
        {
            var previousStageIndex = GetActiveStageIndexZeroBased();
            Level++;
            var newStageIndex = GetActiveStageIndexZeroBased();
            if (newStageIndex > previousStageIndex)
            {
                var oldPerLevel = TypeLevelUpLookup.PointsForChainStage(chain.Length, previousStageIndex);
                var newPerLevel = TypeLevelUpLookup.PointsForChainStage(chain.Length, newStageIndex);
                var oldStage = chain[previousStageIndex];
                var newStage = chain[newStageIndex];
                var remove = PokemonTypeContribution.Negate(
                    PokemonTypeContribution.SplitTotal(oldStage.TypeKey, oldStage.SecondaryTypeKey, Level * oldPerLevel));
                var add = PokemonTypeContribution.SplitTotal(newStage.TypeKey, newStage.SecondaryTypeKey, Level * newPerLevel);
                _recordTypeLevelContributions([.. remove, .. add]);
            }
        }
        else
        {
            Level++;
        }

        RecordTypeLevelContributionsForCurrentStage();
    }

    private double GetClampedProgressMultiplier()
    {
        var raw = _getProgressMultiplier?.Invoke() ?? GameBalance.Training.NeutralSpeedMultiplier;
        if (double.IsNaN(raw) || double.IsInfinity(raw))
        {
            return GameBalance.Training.NeutralSpeedMultiplier;
        }

        return Math.Clamp(raw, GameBalance.Training.MinExternalSpeedMultiplier, GameBalance.Training.MaxExternalSpeedMultiplier);
    }

    private double GetEffectiveProgressPerTick() => GameBalance.Training.ProgressPerTick * GetClampedProgressMultiplier();

    /// <summary>True when a full bar at the current per-tick rate would complete in under <see cref="MagicNumbersUI.TimeRemaining.UltraFastFullBarMaxDurationSeconds"/> (presentation-only fast path).</summary>
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
