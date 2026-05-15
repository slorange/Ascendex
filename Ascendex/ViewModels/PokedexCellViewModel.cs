using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ascendex.ViewModels;

public partial class PokedexCellViewModel : ViewModelBase
{
    public static readonly IBrush UncaughtFill = Brushes.Black;

    [ObservableProperty]
    private IBrush _fillBrush = UncaughtFill;
}
