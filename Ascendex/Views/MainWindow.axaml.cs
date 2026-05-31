using Avalonia.Controls;
using Ascendex.ViewModels;

namespace Ascendex.Views;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        Closing += OnClosing;
    }

    private void OnClosing(object? sender, WindowClosingEventArgs e)
    {
        if (DataContext is MainViewModel viewModel)
        {
            viewModel.Dispose();
        }
    }
}
