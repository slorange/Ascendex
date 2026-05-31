using Ascendex.ViewModels;
using Avalonia.Controls;

namespace Ascendex.Views;

public partial class CollectionsView : UserControl
{
    public CollectionsView()
    {
        InitializeComponent();
        SizeChanged += (_, _) => UpdateGridMeasures();
        AttachedToVisualTree += (_, _) => UpdateGridMeasures();
    }

    private void UpdateGridMeasures()
    {
        if (!IsMeasureValid || Bounds.Width <= 0)
        {
            return;
        }

        var innerWidth = Bounds.Width - MagicNumbersUI.PokedexGrid.HorizontalMarginTotal;
        var cell = System.Math.Max(MagicNumbersUI.PokedexGrid.MinCellSize, System.Math.Floor(innerWidth / MagicNumbersUI.PokedexGrid.Columns));

        PokedexGridHost.Width = cell * MagicNumbersUI.PokedexGrid.Columns;
        PokedexGridHost.Height = cell * MagicNumbersUI.PokedexGrid.Rows;

        GymBadgeGridHost.Width = cell * MagicNumbersUI.BadgeGrid.GymColumns;
        GymBadgeGridHost.Height = cell;

        LeagueBadgeGridHost.Width = cell * MagicNumbersUI.BadgeGrid.LeagueColumns;
        LeagueBadgeGridHost.Height = cell;
    }
}
