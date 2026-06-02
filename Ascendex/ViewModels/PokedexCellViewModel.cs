using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ascendex.ViewModels;

public partial class PokedexCellViewModel : ViewModelBase
{
    public static readonly IBrush UncaughtFill = Brushes.Black;
    public static readonly IBrush UncaughtBorder = Brush.Parse(MagicNumbersUI.PokedexGrid.UncaughtBorder);
    public static readonly IBrush NormalBorder = Brush.Parse(MagicNumbersUI.PokedexGrid.NormalBorder);
    public static readonly IBrush ShinyBorder = Brush.Parse(MagicNumbersUI.PokedexGrid.ShinyBorder);

    public int CellIndex { get; }

    public string SpeciesName { get; }

    [ObservableProperty]
    private IBrush _fillBrush = UncaughtFill;

    [ObservableProperty]
    private IBrush _borderBrush = UncaughtBorder;

    [ObservableProperty]
    private string _tooltipText = string.Empty;

    public PokedexCellViewModel(int cellIndex, string speciesName)
    {
        CellIndex = cellIndex;
        SpeciesName = speciesName;
    }
}
