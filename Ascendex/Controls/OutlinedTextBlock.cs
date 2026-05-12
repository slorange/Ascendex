using System;
using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Documents;
using Avalonia.Media;

namespace Ascendex.Controls;

public class OutlinedTextBlock : Control
{
    public static readonly StyledProperty<object?> TextProperty =
        AvaloniaProperty.Register<OutlinedTextBlock, object?>(nameof(Text));

    public static readonly StyledProperty<IBrush?> ForegroundProperty =
        TextElement.ForegroundProperty.AddOwner<OutlinedTextBlock>();

    public static readonly StyledProperty<IBrush?> OutlineBrushProperty =
        AvaloniaProperty.Register<OutlinedTextBlock, IBrush?>(nameof(OutlineBrush), Brushes.Black);

    public static readonly StyledProperty<double> OutlineThicknessProperty =
        AvaloniaProperty.Register<OutlinedTextBlock, double>(nameof(OutlineThickness), 1d);

    public static readonly StyledProperty<FontFamily> FontFamilyProperty =
        TextElement.FontFamilyProperty.AddOwner<OutlinedTextBlock>();

    public static readonly StyledProperty<double> FontSizeProperty =
        TextElement.FontSizeProperty.AddOwner<OutlinedTextBlock>();

    public static readonly StyledProperty<FontStyle> FontStyleProperty =
        TextElement.FontStyleProperty.AddOwner<OutlinedTextBlock>();

    public static readonly StyledProperty<FontWeight> FontWeightProperty =
        TextElement.FontWeightProperty.AddOwner<OutlinedTextBlock>();

    static OutlinedTextBlock()
    {
        AffectsMeasure<OutlinedTextBlock>(
            TextProperty,
            OutlineThicknessProperty,
            FontFamilyProperty,
            FontSizeProperty,
            FontStyleProperty,
            FontWeightProperty);

        AffectsRender<OutlinedTextBlock>(
            TextProperty,
            ForegroundProperty,
            OutlineBrushProperty,
            OutlineThicknessProperty,
            FontFamilyProperty,
            FontSizeProperty,
            FontStyleProperty,
            FontWeightProperty);
    }

    public object? Text
    {
        get => GetValue(TextProperty);
        set => SetValue(TextProperty, value);
    }

    public IBrush? Foreground
    {
        get => GetValue(ForegroundProperty);
        set => SetValue(ForegroundProperty, value);
    }

    public IBrush? OutlineBrush
    {
        get => GetValue(OutlineBrushProperty);
        set => SetValue(OutlineBrushProperty, value);
    }

    public double OutlineThickness
    {
        get => GetValue(OutlineThicknessProperty);
        set => SetValue(OutlineThicknessProperty, value);
    }

    public FontFamily FontFamily
    {
        get => GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public double FontSize
    {
        get => GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public FontStyle FontStyle
    {
        get => GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public FontWeight FontWeight
    {
        get => GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        var outlineThickness = Math.Max(0, OutlineThickness);
        var text = CreateFormattedText(Foreground ?? Brushes.White);
        var padding = outlineThickness * 2;

        return new Size(
            text.WidthIncludingTrailingWhitespace + padding,
            text.Height + padding);
    }

    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var textValue = GetTextValue();
        if (string.IsNullOrWhiteSpace(textValue))
        {
            return;
        }

        var outlineThickness = Math.Max(0, OutlineThickness);
        var origin = new Point(outlineThickness, outlineThickness);

        if (outlineThickness > 0 && OutlineBrush is not null)
        {
            var outlineText = CreateFormattedText(OutlineBrush);

            // Multi-pass drawing gives us a lightweight outline effect without a custom glyph pipeline.
            foreach (var offset in GetOutlineOffsets(outlineThickness))
            {
                context.DrawText(outlineText, origin + offset);
            }
        }

        var fillText = CreateFormattedText(Foreground ?? Brushes.White);
        context.DrawText(fillText, origin);
    }

    private FormattedText CreateFormattedText(IBrush foreground)
    {
        return new FormattedText(
            GetTextValue(),
            CultureInfo.CurrentCulture,
            FlowDirection,
            new Typeface(FontFamily, FontStyle, FontWeight),
            FontSize,
            foreground);
    }

    private string GetTextValue()
    {
        return Convert.ToString(Text, CultureInfo.CurrentCulture) ?? string.Empty;
    }

    private static Point[] GetOutlineOffsets(double outlineThickness)
    {
        return
        [
            new Point(-outlineThickness, 0),
            new Point(outlineThickness, 0),
            new Point(0, -outlineThickness),
            new Point(0, outlineThickness),
            new Point(-outlineThickness, -outlineThickness),
            new Point(-outlineThickness, outlineThickness),
            new Point(outlineThickness, -outlineThickness),
            new Point(outlineThickness, outlineThickness)
        ];
    }
}
