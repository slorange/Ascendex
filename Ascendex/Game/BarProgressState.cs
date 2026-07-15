using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ascendex.Game;

public interface IBarProgressState : INotifyPropertyChanged
{
    int Level { get; set; }
    double Progress { get; set; }
    bool IsTraining { get; set; }
    bool IsVisible { get; set; }
}

public partial class SpeciesProgress : ObservableObject, IBarProgressState
{
    public required string SpeciesRootName { get; init; }

    private int _level;

    private double _progress;

    public int Level
    {
        get => _level;
        set => SetProperty(ref _level, value);
    }

    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    [ObservableProperty]
    private bool _isTraining;

    [ObservableProperty]
    private bool _isCatching;

    [ObservableProperty]
    private bool _isVisible = true;

    [ObservableProperty]
    private bool _isShiny;

    internal bool SetSimulationProgress(double value)
    {
        if (_progress == value)
        {
            return false;
        }

        _progress = value;
        return true;
    }

    internal void IncrementSimulationLevel() => _level++;

    internal void PublishSimulationChanges(bool levelChanged, bool progressChanged)
    {
        if (levelChanged)
        {
            OnPropertyChanged(nameof(Level));
        }

        if (progressChanged)
        {
            OnPropertyChanged(nameof(Progress));
        }
    }
}

public partial class TrainerProgress : ObservableObject, IBarProgressState
{
    public required string TrainerId { get; init; }

    private int _level;

    private double _progress;

    public int Level
    {
        get => _level;
        set => SetProperty(ref _level, value);
    }

    public double Progress
    {
        get => _progress;
        set => SetProperty(ref _progress, value);
    }

    [ObservableProperty]
    private bool _isTraining;

    [ObservableProperty]
    private bool _isVisible = true;

    internal bool SetSimulationProgress(double value)
    {
        if (_progress == value)
        {
            return false;
        }

        _progress = value;
        return true;
    }

    internal void IncrementSimulationLevel() => _level++;

    internal void PublishSimulationChanges(bool levelChanged, bool progressChanged)
    {
        if (levelChanged)
        {
            OnPropertyChanged(nameof(Level));
        }

        if (progressChanged)
        {
            OnPropertyChanged(nameof(Progress));
        }
    }
}
