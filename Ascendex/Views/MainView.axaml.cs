using System;
using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Ascendex.ViewModels;

namespace Ascendex.Views;

public partial class MainView : UserControl
{
    private MainViewModel? _viewModel;
    private bool _alignStripPending = true;
    private bool _stripProgrammaticScroll;
    private bool _stripUserPannedHorizontally;

    public MainView()
    {
        InitializeComponent();
        Loaded += OnLoaded;
        Unloaded += OnUnloaded;
        DataContextChanged += OnDataContextChanged;
    }

    private void OnDataContextChanged(object? sender, EventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel = null;
        }

        _stripUserPannedHorizontally = false;

        if (DataContext is MainViewModel vm)
        {
            _viewModel = vm;
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        RequestScrollAreaToSelection(alignToSelection: true);
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedAreaIndex))
        {
            RequestScrollAreaToSelection(alignToSelection: true);
        }
    }

    private void OnLoaded(object? sender, RoutedEventArgs e)
    {
        var scroll = this.FindControl<ScrollViewer>("AreaScrollViewer");
        if (scroll is not null)
        {
            scroll.SizeChanged -= OnAreaScrollViewerSizeChanged;
            scroll.SizeChanged += OnAreaScrollViewerSizeChanged;
            scroll.ScrollChanged -= OnAreaScrollViewerScrollChanged;
            scroll.ScrollChanged += OnAreaScrollViewerScrollChanged;
        }

        RequestScrollAreaToSelection(alignToSelection: true);
    }

    private void OnUnloaded(object? sender, RoutedEventArgs e)
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel = null;
        }

        var scroll = this.FindControl<ScrollViewer>("AreaScrollViewer");
        if (scroll is not null)
        {
            scroll.SizeChanged -= OnAreaScrollViewerSizeChanged;
            scroll.ScrollChanged -= OnAreaScrollViewerScrollChanged;
        }
    }

    private void OnAreaScrollViewerSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        RequestScrollAreaToSelection(alignToSelection: !_stripUserPannedHorizontally);
    }

    private void OnAreaScrollViewerScrollChanged(object? sender, ScrollChangedEventArgs e)
    {
        if (_stripProgrammaticScroll)
        {
            return;
        }

        if (Math.Abs(e.OffsetDelta.X) > 0.0001)
        {
            _stripUserPannedHorizontally = true;
        }
    }

    private void RequestScrollAreaToSelection(bool alignToSelection)
    {
        _alignStripPending = alignToSelection;
        Dispatcher.UIThread.Post(ScrollAreaStripDeferred, DispatcherPriority.Loaded);
    }

    private void ScrollAreaStripDeferred()
    {
        ScrollAreaStripToCenter(_alignStripPending);
    }

    private void ScrollAreaStripToCenter(bool alignToSelection)
    {
        var scroll = this.FindControl<ScrollViewer>("AreaScrollViewer");
        var items = this.FindControl<ItemsControl>("AreaItemsControl");
        var insets = this.FindControl<Border>("AreaStripInsets");
        var vm = _viewModel ?? DataContext as MainViewModel;

        if (scroll is null || items is null || vm is null)
        {
            return;
        }

        var index = vm.SelectedAreaIndex;
        if (index < 0 || index >= vm.AreaSelectors.Count)
        {
            return;
        }

        var viewportWidth = scroll.Viewport.Width;
        if (viewportWidth <= 0)
        {
            return;
        }

        const double cellWidth = 56;
        var inset = Math.Max(0, (viewportWidth - cellWidth) / 2);
        if (insets is not null)
        {
            var p = insets.Padding;
            if (Math.Abs(p.Left - inset) > 0.5 || Math.Abs(p.Right - inset) > 0.5)
            {
                insets.Padding = new Thickness(inset, 0, inset, 0);
                Dispatcher.UIThread.Post(ScrollAreaStripDeferred, DispatcherPriority.Loaded);
                return;
            }
        }

        if (!alignToSelection && _stripUserPannedHorizontally)
        {
            ClampStripHorizontalScroll(scroll);
            return;
        }

        var container = items.ContainerFromIndex(index) as Control;
        if (container is null)
        {
            Dispatcher.UIThread.Post(ScrollAreaStripDeferred, DispatcherPriority.Loaded);
            return;
        }

        var anchor = (Control?)insets ?? scroll;
        var topLeft = container.TranslatePoint(new Point(0, 0), anchor);
        if (topLeft is null)
        {
            return;
        }

        var centerX = topLeft.Value.X + container.Bounds.Width * 0.5;
        var targetOffsetX = centerX - viewportWidth * 0.5;
        var extentWidth = scroll.Extent.Width;
        var maxOffsetX = Math.Max(0, extentWidth - viewportWidth);
        var clamped = Math.Clamp(targetOffsetX, 0, maxOffsetX);

        _stripProgrammaticScroll = true;
        try
        {
            scroll.Offset = new Vector(clamped, scroll.Offset.Y);
        }
        finally
        {
            _stripProgrammaticScroll = false;
        }

        _stripUserPannedHorizontally = false;
    }

    private void ClampStripHorizontalScroll(ScrollViewer scroll)
    {
        var extentWidth = scroll.Extent.Width;
        var viewportWidth = scroll.Viewport.Width;
        var maxOffsetX = Math.Max(0, extentWidth - viewportWidth);
        var clamped = Math.Clamp(scroll.Offset.X, 0, maxOffsetX);
        if (Math.Abs(clamped - scroll.Offset.X) < 0.0001)
        {
            return;
        }

        _stripProgrammaticScroll = true;
        try
        {
            scroll.Offset = new Vector(clamped, scroll.Offset.Y);
        }
        finally
        {
            _stripProgrammaticScroll = false;
        }
    }
}
