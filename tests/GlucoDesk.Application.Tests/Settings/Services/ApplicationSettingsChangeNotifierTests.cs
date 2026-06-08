using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Application.Settings.Services;

namespace GlucoDesk.Application.Tests.Settings.Services;

public sealed class ApplicationSettingsChangeNotifierTests
{
    [Fact]
    public void NotifySettingsChanged_ShouldRaiseSettingsChanged()
    {
        var settings = new ApplicationSettings(dashboardRefreshInterval: TimeSpan.FromSeconds(45));
        var notifier = new ApplicationSettingsChangeNotifier();

        ApplicationSettings? notifiedSettings = null;

        notifier.SettingsChanged += (_, eventArgs) => notifiedSettings = eventArgs.Settings;

        notifier.NotifySettingsChanged(settings);

        Assert.Equal(settings, notifiedSettings);
    }

    [Fact]
    public void NotifySettingsChanged_ShouldRejectNullSettings()
    {
        var notifier = new ApplicationSettingsChangeNotifier();

        var exception = Assert.Throws<ArgumentNullException>(
            () => notifier.NotifySettingsChanged(null!));

        Assert.Equal("settings", exception.ParamName);
    }
}