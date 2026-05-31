using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Media;
using Ascendex.ViewModels;

namespace Ascendex.Controls;

/// <summary>Collections grid cell: hover tooltip on desktop, tap flyout on touch platforms.</summary>
public class CollectionsInfoCell : Border
{
    public static readonly StyledProperty<string> DetailTextProperty =
        AvaloniaProperty.Register<CollectionsInfoCell, string>(nameof(DetailText));

    private static readonly bool UsesTapForDetails =
        OperatingSystem.IsAndroid() || OperatingSystem.IsIOS() || OperatingSystem.IsBrowser();

    static CollectionsInfoCell()
    {
        DetailTextProperty.Changed.AddClassHandler<CollectionsInfoCell>((cell, _) => cell.SyncDetailPresentation());
    }

    public CollectionsInfoCell()
    {
        if (UsesTapForDetails)
        {
            Tapped += OnTapped;
        }
    }

    public string DetailText
    {
        get => GetValue(DetailTextProperty);
        set => SetValue(DetailTextProperty, value);
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        SyncDetailPresentation();
    }

    private void SyncDetailPresentation()
    {
        if (UsesTapForDetails)
        {
            ToolTip.SetTip(this, null);
            return;
        }

        if (string.IsNullOrWhiteSpace(DetailText))
        {
            ToolTip.SetTip(this, null);
            return;
        }

        ToolTip.SetTip(this, CreateDetailContent());
    }

    private void OnTapped(object? sender, TappedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(DetailText))
        {
            return;
        }

        var flyout = FlyoutBase.GetAttachedFlyout(this) as Flyout;
        if (flyout is null)
        {
            flyout = new Flyout { ShowMode = FlyoutShowMode.Transient };
            FlyoutBase.SetAttachedFlyout(this, flyout);
        }

        flyout.Content = CreateDetailContent();
        FlyoutBase.ShowAttachedFlyout(this);
        e.Handled = true;
    }

    private Control CreateDetailContent() =>
        new Border
        {
            Background = Brush.Parse(MagicNumbersUI.CollectionsDetail.FlyoutBackground),
            Padding = new Thickness(MagicNumbersUI.CollectionsDetail.Padding),
            Child = new TextBlock
            {
                Text = DetailText,
                TextWrapping = TextWrapping.Wrap,
                MaxWidth = MagicNumbersUI.CollectionsDetail.MaxWidth,
                Foreground = Brushes.White,
                FontSize = MagicNumbersUI.CollectionsDetail.FontSize,
            },
        };
}
