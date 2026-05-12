using System;
using Avalonia.Media.Imaging;
using Avalonia.Platform;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Ascendex.ViewModels;

public partial class TypeCounterViewModel : ViewModelBase
{
    public TypeCounterViewModel(string typeKey)
    {
        TypeKey = typeKey;
        using var iconStream = AssetLoader.Open(new Uri($"avares://Ascendex/Assets/icon_{typeKey}.png"));
        Icon = new Bitmap(iconStream);
    }

    public string TypeKey { get; }

    public Bitmap Icon { get; }

    [ObservableProperty]
    private int _count;
}
