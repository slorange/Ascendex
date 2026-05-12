using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Ascendex.ViewModels;

public partial class PokemonTrainingBarViewModel : ViewModelBase
{
    public PokemonTrainingBarViewModel(
        string name,
        string accentColor,
        string accentForegroundColor,
        int progressRequired = 10)
    {
        Name = name;
        AccentBrush = Brush.Parse(accentColor);
        AccentForegroundBrush = Brush.Parse(accentForegroundColor);
        ProgressRequired = progressRequired;
        Level = 1;
        Progress = 0;
    }

    public string Name { get; }

    public IBrush AccentBrush { get; }

    public IBrush AccentForegroundBrush { get; }

    public int ProgressRequired { get; }

    public double ProgressFraction => ProgressRequired == 0 ? 0 : (double)Progress / ProgressRequired;

    [ObservableProperty]
    private int _level;

    [ObservableProperty]
    private int _progress;

    partial void OnProgressChanged(int value)
    {
        OnPropertyChanged(nameof(ProgressFraction));
    }

    [RelayCommand]
    private void Advance()
    {
        Progress++;

        if (Progress < ProgressRequired)
        {
            return;
        }

        Progress = 0;
        Level++;
    }
}
