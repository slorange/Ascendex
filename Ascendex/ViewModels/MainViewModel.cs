using CommunityToolkit.Mvvm.ComponentModel;

namespace Ascendex.ViewModels;

public partial class MainViewModel : ViewModelBase
{
    [ObservableProperty]
    private string _greeting = "Welcome to Avalonia!";
}
