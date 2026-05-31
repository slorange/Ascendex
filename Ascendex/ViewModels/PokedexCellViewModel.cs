using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ascendex.ViewModels;

public partial class PokedexCellViewModel : ViewModelBase
{
    public static readonly IBrush UncaughtFill = Brushes.Black;

    public int CellIndex { get; }

    public string SpeciesName { get; }

    [ObservableProperty]
    private IBrush _fillBrush = UncaughtFill;

    [ObservableProperty]
    private string _tooltipText = string.Empty;

    public PokedexCellViewModel(int cellIndex, string speciesName)
    {
        CellIndex = cellIndex;
        SpeciesName = speciesName;
    }
}
