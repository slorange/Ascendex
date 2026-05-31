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

    [ObservableProperty]
    private int _level;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private bool _isTraining;

    [ObservableProperty]
    private bool _isCatching;

    [ObservableProperty]
    private bool _isVisible = true;
}

public partial class TrainerProgress : ObservableObject, IBarProgressState
{
    public required string TrainerId { get; init; }

    [ObservableProperty]
    private int _level;

    [ObservableProperty]
    private double _progress;

    [ObservableProperty]
    private bool _isTraining;

    [ObservableProperty]
    private bool _isVisible = true;
}
