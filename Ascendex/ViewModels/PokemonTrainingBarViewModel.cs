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

    private readonly Action<string> _recordTypeLevelUp;
    private readonly Action<PokemonTrainingBarViewModel> _recordLevelChanged;
    private readonly Action<PokemonTrainingBarViewModel> _toggleTrainingRequested;
    private readonly Func<double>? _getProgressMultiplier;
    private readonly DispatcherTimer _trainingTimer;

    public PokemonTrainingBarViewModel(
        string name,
        string typeKey,
        string accentColor,
        string accentForegroundColor,
        Action<PokemonTrainingBarViewModel> toggleTrainingRequested,
        Action<PokemonTrainingBarViewModel> recordLevelChanged,
        Action<string> recordTypeLevelUp,
        double progressRequired,
        Func<double>? getProgressMultiplier = null)
    {
        _recordTypeLevelUp = recordTypeLevelUp;
        _recordLevelChanged = recordLevelChanged;
        _toggleTrainingRequested = toggleTrainingRequested;
        _getProgressMultiplier = getProgressMultiplier;
        Name = name;
        TypeKey = typeKey;
        AccentBrush = Brush.Parse(accentColor);
        AccentForegroundBrush = Brush.Parse(accentForegroundColor);
        BaseProgressRequired = progressRequired;
        Level = 1;
        Progress = 0;

        _trainingTimer = new DispatcherTimer
        {
            Interval = TrainingTickInterval
        };
        _trainingTimer.Tick += OnTrainingTimerTick;
    }

    public string Name { get; }

    public string TypeKey { get; }

    public IBrush AccentBrush { get; }

    public IBrush AccentForegroundBrush { get; }

    public double BaseProgressRequired { get; }

    public double ProgressRequired => BaseProgressRequired * Math.Pow(GameBalance.Training.ProgressRequiredPerLevelExponent, Math.Max(0, Level - 1));

    public double ProgressFraction => ProgressRequired == 0 ? 0 : (double)Progress / ProgressRequired;

    public string TimeRemainingText => FormatTimeRemaining();

    public Thickness TrainingBorderThickness =>
        IsTraining
            ? new Thickness(GameBalance.Training.ActiveTrainingBorderThickness)
            : new Thickness(GameBalance.Training.IdleTrainingBorderThickness);

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
        OnPropertyChanged(nameof(TimeRemainingText));
    }

    partial void OnLevelChanged(int value)
    {
        OnPropertyChanged(nameof(ProgressRequired));
        OnPropertyChanged(nameof(ProgressFraction));
        OnPropertyChanged(nameof(TimeRemainingText));
        _recordLevelChanged(this);
    }

    partial void OnIsTrainingChanged(bool value)
    {
        OnPropertyChanged(nameof(TrainingBorderThickness));
        OnPropertyChanged(nameof(TrainingBorderBrush));

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

    /// <summary>Call when an external factor changes effective training speed so <see cref="TimeRemainingText"/> refreshes without waiting for the next tick.</summary>
    public void NotifyTimeRemainingChanged() => OnPropertyChanged(nameof(TimeRemainingText));

    private void OnTrainingTimerTick(object? sender, EventArgs e)
    {
        if (ProgressRequired <= 0)
        {
            return;
        }

        Progress += GameBalance.Training.ProgressPerTick * GetClampedProgressMultiplier();

        if (Progress < ProgressRequired)
        {
            return;
        }

        Progress = 0;
        Level++;
        _recordTypeLevelUp(TypeKey);
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

    private string FormatTimeRemaining()
    {
        var effectivePerTick = GetEffectiveProgressPerTick();
        if (ProgressRequired <= 0 || effectivePerTick <= 0)
        {
            return "0s";
        }

        var remainingProgress = Math.Max(0, ProgressRequired - Progress);
        var remainingMilliseconds = remainingProgress / effectivePerTick * TrainingTickInterval.TotalMilliseconds;
        var remaining = TimeSpan.FromMilliseconds(remainingMilliseconds);
        var totalSeconds = Math.Max(0, (int)Math.Floor(remaining.TotalSeconds));

        if (totalSeconds >= GameBalance.Training.SecondsBeforeMinuteTimeFormat)
        {
            return $"{totalSeconds / 60}:{totalSeconds % 60:D2}";
        }

        return $"{totalSeconds}s";
    }
}
