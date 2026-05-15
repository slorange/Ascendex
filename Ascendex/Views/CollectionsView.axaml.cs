using Avalonia.Controls;

namespace Ascendex.Views;
public partial class CollectionsView : UserControl
{
    private const double HorizontalMarginTotal = 32;

    public CollectionsView()
    {
        InitializeComponent();
        SizeChanged += (_, _) => UpdatePokedexGridMeasure();
        AttachedToVisualTree += (_, _) => UpdatePokedexGridMeasure();
    }

    /// <summary>Keeps a 10×15 grid of square cells inside the control width (minus horizontal margin) and centered.</summary>
    private void UpdatePokedexGridMeasure()
    {
        if (!IsMeasureValid || Bounds.Width <= 0)
        {
            return;
        }

        var innerWidth = Bounds.Width - HorizontalMarginTotal;
        var cell = System.Math.Max(12.0, System.Math.Floor(innerWidth / 10.0));
        PokedexGridHost.Width = cell * 10;
        PokedexGridHost.Height = cell * 15;
    }
}