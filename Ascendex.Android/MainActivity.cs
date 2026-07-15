using Android.App;
using Android.Content.PM;
using Avalonia;
using Avalonia.Android;

namespace Ascendex.Android;

[Activity(
    Label = "Ascendex.Android",
    Theme = "@style/MyTheme.NoActionBar",
    Icon = "@drawable/icon",
    MainLauncher = true,
    ConfigurationChanges = ConfigChanges.Orientation | ConfigChanges.ScreenSize | ConfigChanges.UiMode)]
public class MainActivity : AvaloniaMainActivity<App>
{
    protected override AppBuilder CustomizeAppBuilder(AppBuilder builder)
    {
        return base.CustomizeAppBuilder(builder)
            .WithInterFont();
    }

    protected override void OnPause()
    {
        (global::Avalonia.Application.Current as global::Ascendex.App)?.Suspend();
        base.OnPause();
    }

    protected override void OnResume()
    {
        base.OnResume();
        (global::Avalonia.Application.Current as global::Ascendex.App)?.Resume();
    }

}
