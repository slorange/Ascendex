using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using System.Linq;
using Avalonia.Markup.Xaml;
using Ascendex.ViewModels;
using Ascendex.Views;

namespace Ascendex;

public partial class App : Application
{
    public MainViewModel? MainViewModel { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            DisableAvaloniaDataAnnotationValidation();
            MainViewModel = ViewModels.MainViewModel.Create();
            desktop.MainWindow = new MainWindow
            {
                DataContext = MainViewModel,
            };
        }
        else if (ApplicationLifetime is ISingleViewApplicationLifetime singleViewPlatform)
        {
            MainViewModel = ViewModels.MainViewModel.Create();
            singleViewPlatform.MainView = new MainView
            {
                DataContext = MainViewModel,
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    public void Suspend() => MainViewModel?.Suspend();

    public void Resume() => MainViewModel?.Resume();

    private void DisableAvaloniaDataAnnotationValidation()
    {
        var dataValidationPluginsToRemove =
            BindingPlugins.DataValidators.OfType<DataAnnotationsValidationPlugin>().ToArray();

        foreach (var plugin in dataValidationPluginsToRemove)
        {
            BindingPlugins.DataValidators.Remove(plugin);
        }
    }
}
