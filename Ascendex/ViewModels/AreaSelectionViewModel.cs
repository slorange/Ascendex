using System;
using System.Collections.Generic;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Ascendex.ViewModels;

public partial class AreaSelectionViewModel : ViewModelBase
{
    private static readonly IBrush SelectedBackgroundBrush = Brush.Parse("#6A6A6A");
    private static readonly IBrush UnselectedBackgroundBrush = Brush.Parse("#3A3F47");
    private static readonly IBrush SelectedBorderBrush = Brushes.White;
    private static readonly IBrush UnselectedBorderBrush = Brush.Parse("#5F6470");

    private readonly Action<AreaSelectionViewModel> _selectArea;

    public AreaSelectionViewModel(
        string shortLabel,
        string displayName,
        IReadOnlyList<PokemonTrainingBarViewModel> pokemonBars,
        Action<AreaSelectionViewModel> selectArea)
    {
        ShortLabel = shortLabel;
        DisplayName = displayName;
        PokemonBars = pokemonBars;
        _selectArea = selectArea;
    }

    public string ShortLabel { get; }

    public string DisplayName { get; }

    public IReadOnlyList<PokemonTrainingBarViewModel> PokemonBars { get; }

    public IBrush BackgroundBrush => IsSelected ? SelectedBackgroundBrush : UnselectedBackgroundBrush;

    public IBrush BorderBrush => IsSelected ? SelectedBorderBrush : UnselectedBorderBrush;

    [ObservableProperty]
    private bool _isSelected;

    partial void OnIsSelectedChanged(bool value)
    {
        OnPropertyChanged(nameof(BackgroundBrush));
        OnPropertyChanged(nameof(BorderBrush));
    }

    [RelayCommand]
    private void Select()
    {
        _selectArea(this);
    }
}
