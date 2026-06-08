using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Tests.Settings.Models;

public sealed class ApplicationSettingsTests
{
    [Fact]
    public void Default_ShouldUseExpectedValues()
    {
        var settings = ApplicationSettings.Default;

        Assert.Equal(CgmProviderKind.Mock, settings.ActiveLiveProvider);
        Assert.Equal(CgmProviderKind.Mock, settings.HistoricalProvider);
        Assert.Equal(GlucoseUnit.MgDl, settings.PreferredUnit);
        Assert.Equal(70, settings.TargetLowMgDl);
        Assert.Equal(180, settings.TargetHighMgDl);
        Assert.Equal(TimeSpan.FromSeconds(30), settings.DashboardRefreshInterval);
    }

    [Fact]
    public void Constructor_ShouldRejectUnknownActiveLiveProvider()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new ApplicationSettings(activeLiveProvider: CgmProviderKind.Unknown));

        Assert.Equal("activeLiveProvider", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectUnknownHistoricalProvider()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new ApplicationSettings(historicalProvider: CgmProviderKind.Unknown));

        Assert.Equal("historicalProvider", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidTargetLow()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new ApplicationSettings(targetLowMgDl: 0));

        Assert.Equal("targetLowMgDl", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidTargetHigh()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new ApplicationSettings(targetLowMgDl: 100, targetHighMgDl: 100));

        Assert.Equal("targetHighMgDl", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidDashboardRefreshInterval()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new ApplicationSettings(dashboardRefreshInterval: TimeSpan.Zero));

        Assert.Equal("dashboardRefreshInterval", exception.ParamName);
    }
}