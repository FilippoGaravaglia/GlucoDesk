using GlucoDesk.Application.Settings.Models;

namespace GlucoDesk.Application.Tests.Settings.Models;

public sealed class ApplicationSettingsGlucoseAlertTests
{
    [Fact]
    public void Default_ShouldEnableInAppAlertsAndPrivacyModeOnly()
    {
        var settings = ApplicationSettings.Default;

        Assert.True(settings.GlucoseAlertsEnabled);
        Assert.True(settings.LowGlucoseAlertsEnabled);
        Assert.True(settings.HighGlucoseAlertsEnabled);
        Assert.False(settings.NativeGlucoseNotificationsEnabled);
        Assert.True(settings.GlucoseAlertPrivacyModeEnabled);
        Assert.Equal(TimeSpan.FromMinutes(30), settings.GlucoseAlertRepeatInterval);
    }

    [Fact]
    public void Constructor_ShouldPreserveGlucoseAlertSettings()
    {
        var settings = new ApplicationSettings(
            glucoseAlertsEnabled: false,
            lowGlucoseAlertsEnabled: false,
            highGlucoseAlertsEnabled: true,
            nativeGlucoseNotificationsEnabled: true,
            glucoseAlertPrivacyModeEnabled: false,
            glucoseAlertRepeatInterval: TimeSpan.FromMinutes(45));

        Assert.False(settings.GlucoseAlertsEnabled);
        Assert.False(settings.LowGlucoseAlertsEnabled);
        Assert.True(settings.HighGlucoseAlertsEnabled);
        Assert.True(settings.NativeGlucoseNotificationsEnabled);
        Assert.False(settings.GlucoseAlertPrivacyModeEnabled);
        Assert.Equal(TimeSpan.FromMinutes(45), settings.GlucoseAlertRepeatInterval);
    }

    [Theory]
    [InlineData(4)]
    [InlineData(181)]
    public void Constructor_ShouldRejectUnsupportedGlucoseAlertRepeatInterval(int minutes)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationSettings(
            glucoseAlertRepeatInterval: TimeSpan.FromMinutes(minutes)));
    }
}
