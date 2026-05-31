using System;
using System.ComponentModel;
using Ascendex.Game;
using Avalonia;
using Avalonia.Media;
using Avalonia.Threading;
using CommunityToolkit.Mvvm.Input;

namespace Ascendex.ViewModels;

public partial class PokemonTrainingBarViewModel : ViewModelBase
{
    private static readonly IBrush IdleBorderBrush = Brush.Parse("#5F6470");
    private static readonly IBrush ActiveBorderBrush = Brushes.White;
    private static readonly TimeSpan TrainingTickInterval = TimeSpan.FromMilliseconds(GameBalance.Training.TickIntervalMilliseconds);

    private readonly IBarProgressState _progress;
    private readonly SpeciesProgress? _speciesProgress;
    private readonly Action<TypeLevelContribution[]> _recordTypeLevelContributions;
    private readonly Action<PokemonTrainingBarViewModel> _recordLevelChanged;
    private readonly Action<PokemonTrainingBarViewModel> _toggleTrainingRequested;
    private readonly Func<double>? _getTrainingProgressMultiplier;
    private readonly Func<double>? _getCatchProgressMultiplier;
    private readonly Func<bool>? _qualifiesForFirstCatchSpeedBonus;
    private readonly EvolutionStage[]? _evolutionChain;
    private readonly double _progressRequiredPerLevelExponent;
    private readonly double _catchDifficultyMultiplier;
    private readonly bool _allowsCatching;
    private readonly DispatcherTimer _trainingTimer;

    public PokemonTrainingBarViewModel(
        SpeciesProgress speciesProgress,
        string typeKey,
        string accentColor,
        string accentForegroundColor,
        Action<PokemonTrainingBarViewModel> toggleTrainingRequested,
        Action<PokemonTrainingBarViewModel> recordLevelChanged,
        Action<TypeLevelContribution[]> recordTypeLevelContributions,
        double progressRequired,
        double progressRequiredPerLevelExponent,
        Func<double>? getTrainingProgressMultiplier = null,
        EvolutionStage[]? evolutionChain = null,
        bool allowsCatching = true,
        Func<bool>? qualifiesForFirstCatchSpeedBonus = null,
        Func<double>? getCatchProgressMultiplier = null,
        double catchDifficultyMultiplier = 1.0)
        : this(
            speciesProgress,
            speciesProgress.SpeciesRootName,
            toggleTrainingRequested,
            recordLevelChanged,
            recordTypeLevelContributions,
            progressRequired,
            progressRequiredPerLevelExponent,
            getTrainingProgressMultiplier,
            evolutionChain,
            allowsCatching,
            qualifiesForFirstCatchSpeedBonus,
            getCatchProgressMultiplier,
            catchDifficultyMultiplier)
    {
        if (_evolutionChain != null)
        {
            ApplyEvolutionStageForCurrentLevel();
            OnPropertyChanged(nameof(ProgressRequired));
            OnPropertyChanged(nameof(ProgressFraction));
            OnPropertyChanged(nameof(VisualProgressFraction));
            OnPropertyChanged(nameof(TimeRemainingText));
            _recordLevelChanged(this);
        }
        else
        {
            Name = speciesProgress.SpeciesRootName;
            TypeKey = typeKey;
            AccentBrush = Brush.Parse(accentColor);
            AccentForegroundBrush = Brush.Parse(accentForegroundColor);
        }
    }

    public PokemonTrainingBarViewModel(
        TrainerProgress trainerProgress,
        string displayName,
        string typeKey,
        string accentColor,
        string accentForegroundColor,
        Action<PokemonTrainingBarViewModel> toggleTrainingRequested,
        Action<PokemonTrainingBarViewModel> recordLevelChanged,
        double progressRequired,
        double progressRequiredPerLevelExponent,
        Func<double>? getTrainingProgressMultiplier = null)
        : this(
            trainerProgress,
            displayName,
            toggleTrainingRequested,
            recordLevelChanged,
            static (TypeLevelContribution[] _) => { },
            progressRequired,
            progressRequiredPerLevelExponent,
            getTrainingProgressMultiplier,
            evolutionChain: null,
            allowsCatching: false,
            qualifiesForFirstCatchSpeedBonus: null,
            getCatchProgressMultiplier: null,
            catchDifficultyMultiplier: 1.0)
    {
        TrainerId = trainerProgress.TrainerId;
        Name = displayName;
        TypeKey = typeKey;
        AccentBrush = Brush.Parse(accentColor);
        AccentForegroundBrush = Brush.Parse(accentForegroundColor);
    }

    private PokemonTrainingBarViewModel(
        IBarProgressState progress,
        string speciesLineRoot,
        Action<PokemonTrainingBarViewModel> toggleTrainingRequested,
        Action<PokemonTrainingBarViewModel> recordLevelChanged,
        Action<TypeLevelContribution[]> recordTypeLevelContributions,
        double progressRequired,
        double progressRequiredPerLevelExponent,
        Func<double>? getTrainingProgressMultiplier,
        EvolutionStage[]? evolutionChain,
        bool allowsCatching,
        Func<bool>? qualifiesForFirstCatchSpeedBonus,
        Func<double>? getCatchProgressMultiplier,
        double catchDifficultyMultiplier)
    {
        _progress = progress;
        _speciesProgress = progress as SpeciesProgress;
        _recordTypeLevelContributions = recordTypeLevelContributions;
        _recordLevelChanged = recordLevelChanged;
        _toggleTrainingRequested = toggleTrainingRequested;
        _getTrainingProgressMultiplier = getTrainingProgressMultiplier;
        _getCatchProgressMultiplier = getCatchProgressMultiplier;
        _qualifiesForFirstCatchSpeedBonus = qualifiesForFirstCatchSpeedBonus;
        _allowsCatching = allowsCatching;
        _catchDifficultyMultiplier = catchDifficultyMultiplier > 0 ? catchDifficultyMultiplier : 1.0;
        _evolutionChain = evolutionChain is { Length: > 0 } ? evolutionChain : null;
        _progressRequiredPerLevelExponent = progressRequiredPerLevelExponent;
        BaseProgressRequired = progressRequired;
        SpeciesLineRoot = speciesLineRoot;

        _progress.PropertyChanged += OnProgressStateChanged;

        _trainingTimer = new DispatcherTimer
        {
            Interval = TrainingTickInterval,
        };
        _trainingTimer.Tick += OnTrainingTimerTick;
    }

    public string Name { get; private set; } = string.Empty;

    public string TypeKey { get; private set; } = string.Empty;

    public IBrush AccentBrush { get; private set; } = Brushes.Transparent;

    public IBrush AccentForegroundBrush { get; private set; } = Brushes.Transparent;

    /// <summary>Route species key for evolution lookup (initial species name); unchanged when the bar evolves.</summary>
    public string SpeciesLineRoot { get; }

    /// <summary>Set for battle trainer bars; used for progression unlock lookups.</summary>
    public string? TrainerId { get; }

    public double BaseProgressRequired { get; }

    public int Level
    {
        get => _progress.Level;
        private set => _progress.Level = value;
    }

    public double Progress
    {
        get => _progress.Progress;
        private set => _progress.Progress = value;
    }

    public bool IsTraining
    {
        get => _progress.IsTraining;
        private set => _progress.IsTraining = value;
    }

    public bool IsCatching
    {
        get => _speciesProgress?.IsCatching ?? false;
        private set
        {
            if (_speciesProgress != null)
            {
                _speciesProgress.IsCatching = value;
            }
        }
    }

    public bool IsVisible
    {
        get => _progress.IsVisible;
        set => _progress.IsVisible = value;
    }

    public double ProgressRequired => BaseProgressRequired * Math.Pow(_progressRequiredPerLevelExponent, Math.Max(0, Level));

    public double ProgressFraction => ProgressRequired == 0 ? 0 : Progress / ProgressRequired;

    /// <summary>Bar fill for UI: full while active if pace is ultra-fast; otherwise matches <see cref="ProgressFraction"/>.</summary>
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

    private void OnProgressStateChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IBarProgressState.Level):
                if (Level >= 1 && IsCatching)
                {
                    IsCatching = false;
                }

                ApplyEvolutionStageForCurrentLevel();
                OnPropertyChanged(nameof(Level));
                OnPropertyChanged(nameof(ProgressRequired));
                OnPropertyChanged(nameof(ProgressFraction));
                OnPropertyChanged(nameof(VisualProgressFraction));
                OnPropertyChanged(nameof(TimeRemainingText));
                NotifyLevelBadgeVisibilityChanged();
                _recordLevelChanged(this);
                break;
            case nameof(IBarProgressState.Progress):
                OnPropertyChanged(nameof(Progress));
                OnPropertyChanged(nameof(ProgressFraction));
                OnPropertyChanged(nameof(VisualProgressFraction));
                OnPropertyChanged(nameof(TimeRemainingText));
                break;
            case nameof(IBarProgressState.IsTraining):
            case nameof(SpeciesProgress.IsCatching):
                OnPropertyChanged(e.PropertyName!);
                OnActivityStateChanged();
                break;
            case nameof(IBarProgressState.IsVisible):
                OnPropertyChanged(nameof(IsVisible));
                break;
        }
    }

    private void NotifyLevelBadgeVisibilityChanged()
    {
        OnPropertyChanged(nameof(ShowLevelBadge));
        OnPropertyChanged(nameof(ShowCatchingPokeball));
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
        if (!IsVisible)
        {
            return;
        }

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

        _recordTypeLevelContributions(TypeLevelContributionRules.SplitTotal(primary, secondary, totalPoints));
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
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(TypeKey));
        OnPropertyChanged(nameof(AccentBrush));
        OnPropertyChanged(nameof(AccentForegroundBrush));
    }

    private void OnActivityStateChanged()
    {
        OnPropertyChanged(nameof(IsActivityActive));
        OnPropertyChanged(nameof(TrainingBorderThickness));
        OnPropertyChanged(nameof(TrainingBorderBrush));
        OnPropertyChanged(nameof(VisualProgressFraction));
        OnPropertyChanged(nameof(TimeRemainingText));

        if (IsTraining || IsCatching)
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

    public void SetTraining(bool isTraining) => IsTraining = isTraining;

    public void SetCatching(bool isCatching)
    {
        if (!_allowsCatching || Level > 0)
        {
            IsCatching = false;
            return;
        }

        IsCatching = isCatching;
    }

    public bool CanCatch => _allowsCatching && Level == 0;

    /// <summary>Sets bar level without catch; type points apply only while <see cref="IsVisible"/> (one tick per level gained).</summary>
    public void GrantAtLevel(int targetLevel)
    {
        if (Level >= targetLevel)
        {
            return;
        }

        IsCatching = false;
        var levelUps = targetLevel - Level;
        Level = targetLevel;

        if (!IsVisible || levelUps <= 0)
        {
            return;
        }

        for (var i = 0; i < levelUps; i++)
        {
            RecordTypeLevelContributionsForCurrentStage();
        }
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

        Progress += GetEffectiveProgressPerTick();

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
            if (newStageIndex > previousStageIndex && IsVisible)
            {
                var oldPerLevel = TypeLevelUpLookup.PointsForChainStage(chain.Length, previousStageIndex);
                var newPerLevel = TypeLevelUpLookup.PointsForChainStage(chain.Length, newStageIndex);
                var oldStage = chain[previousStageIndex];
                var newStage = chain[newStageIndex];
                var remove = TypeLevelContributionRules.Negate(
                    TypeLevelContributionRules.SplitTotal(oldStage.TypeKey, oldStage.SecondaryTypeKey, Level * oldPerLevel));
                var add = TypeLevelContributionRules.SplitTotal(newStage.TypeKey, newStage.SecondaryTypeKey, Level * newPerLevel);
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
        var getMultiplier = IsCatching ? _getCatchProgressMultiplier : _getTrainingProgressMultiplier;
        var raw = getMultiplier?.Invoke() ?? GameBalance.Training.NeutralSpeedMultiplier;
        if (double.IsNaN(raw) || double.IsInfinity(raw))
        {
            return GameBalance.Training.NeutralSpeedMultiplier;
        }

        return Math.Clamp(raw, GameBalance.Training.MinExternalSpeedMultiplier, GameBalance.Training.MaxExternalSpeedMultiplier);
    }

    private double GetActivitySpeedMultiplier()
    {
        if (!IsCatching)
        {
            return 1.0;
        }

        var multiplier = GameBalance.Routes.CatchSpeedMultiplier;
        if (_qualifiesForFirstCatchSpeedBonus?.Invoke() == true)
        {
            multiplier *= GameBalance.Routes.FirstCatchSpeedMultiplier;
        }

        return multiplier;
    }

    private double GetEffectiveProgressPerTick()
    {
        var perTick = GameBalance.Training.ProgressPerTick * GetActivitySpeedMultiplier() * GetClampedProgressMultiplier();
        if (IsCatching && _catchDifficultyMultiplier > 1.0)
        {
            perTick /= _catchDifficultyMultiplier;
        }

        return perTick;
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
