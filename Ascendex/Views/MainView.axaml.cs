using System;
using System.Collections.Generic;
using System.ComponentModel;
using Avalonia.Controls;
using Ascendex.ViewModels;

namespace Ascendex.Views;

public partial class MainView : UserControl
{
    private readonly Dictionary<int, Control> _tabViews = new();
    private MainViewModel? _viewModel;

    public MainView()
    {
        InitializeComponent();
        DataContextChanged += OnDataContextChanged;
        AttachedToVisualTree += (_, _) => BindViewModel();
        DetachedFromVisualTree += (_, _) => UnbindViewModel();
    }

    private void OnDataContextChanged(object? sender, EventArgs e) => BindViewModel();

    private void BindViewModel()
    {
        var next = DataContext as MainViewModel;
        if (ReferenceEquals(_viewModel, next))
        {
            ShowSelectedTab();
            return;
        }

        UnbindViewModel();
        _viewModel = next;
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged += OnViewModelPropertyChanged;
        }

        ShowSelectedTab();
    }

    private void UnbindViewModel()
    {
        if (_viewModel is not null)
        {
            _viewModel.PropertyChanged -= OnViewModelPropertyChanged;
            _viewModel = null;
        }

        PageHost.Content = null;
        _tabViews.Clear();
    }

    private void OnViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(MainViewModel.SelectedMainTab))
        {
            ShowSelectedTab();
        }
    }

    private void ShowSelectedTab()
    {
        if (_viewModel is null)
        {
            PageHost.Content = null;
            return;
        }

        var tab = _viewModel.SelectedMainTab;
        if (!_tabViews.TryGetValue(tab, out var view))
        {
            view = tab switch
            {
                0 => new RoutesView(),
                1 => new BattlesView(),
                2 => new ShopView(),
                3 => new CollectionsView(),
                4 => new PrestigeView(),
                _ => new RoutesView(),
            };
            _tabViews[tab] = view;
        }

        PageHost.Content = view;
    }
}
