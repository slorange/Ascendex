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
    private static readonly TimeSpan TrainingTickInterval = TimeSpan.FromMilliseconds(16);
    private const double LevelGrowthFactor = 1.2;
    private const double ProgressPerTick = 1;

    private readonly Action<string> _recordTypeLevelUp;
    private readonly Action<PokemonTrainingBarViewModel> _recordLevelChanged;
    private readonly Action<PokemonTrainingBarViewModel> _toggleTrainingRequested;
    private readonly DispatcherTimer _trainingTimer;

    public PokemonTrainingBarViewModel(
        string name,
        string typeKey,
        string accentColor,
        string accentForegroundColor,
        Action<PokemonTrainingBarViewModel> toggleTrainingRequested,
        Action<PokemonTrainingBarViewModel> recordLevelChanged,
        Action<string> recordTypeLevelUp,
        double progressRequired = 60)
    {
        _recordTypeLevelUp = recordTypeLevelUp;
        _recordLevelChanged = recordLevelChanged;
        _toggleTrainingRequested = toggleTrainingRequested;
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

    public double ProgressRequired => BaseProgressRequired * Math.Pow(LevelGrowthFactor, Math.Max(0, Level - 1));

    public double ProgressFraction => ProgressRequired == 0 ? 0 : (double)Progress / ProgressRequired;

    public string TimeRemainingText => FormatTimeRemaining();

    public Thickness TrainingBorderThickness => IsTraining ? new Thickness(4) : new Thickness(1);

    public IBrush TrainingBorderBrush => IsTraining ? ActiveBorderBrush : IdleBorderBrush;

    [ObservableProperty]
    private int _level;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private bool _isTraining;

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

    private void OnTrainingTimerTick(object? sender, EventArgs e)
    {
        if (ProgressRequired <= 0)
        {
            return;
        }

        Progress += ProgressPerTick;

        if (Progress < ProgressRequired)
        {
            return;
        }

        Progress = 0;
        Level++;
        _recordTypeLevelUp(TypeKey);
    }

    private string FormatTimeRemaining()
    {
        if (ProgressRequired <= 0 || ProgressPerTick <= 0)
        {
            return "0s";
        }

        var remainingProgress = Math.Max(0, ProgressRequired - Progress);
        var remainingMilliseconds = remainingProgress / ProgressPerTick * TrainingTickInterval.TotalMilliseconds;
        var remaining = TimeSpan.FromMilliseconds(remainingMilliseconds);
        var totalSeconds = Math.Max(0, (int)Math.Floor(remaining.TotalSeconds));

        if (totalSeconds >= 60)
        {
            return $"{totalSeconds / 60}:{totalSeconds % 60:D2}";
        }

        return $"{totalSeconds}s";
    }
}
