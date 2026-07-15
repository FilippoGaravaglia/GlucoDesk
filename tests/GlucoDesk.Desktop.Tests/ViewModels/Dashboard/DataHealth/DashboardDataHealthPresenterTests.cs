using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Dashboard.DataHealth;
using GlucoDesk.Desktop.Tests.Localization;

namespace GlucoDesk.Desktop.Tests.ViewModels.Dashboard.DataHealth;

public sealed class DashboardDataHealthPresenterTests : EnglishLocalizationTestBase
{
    [Fact]
    public void Present_ShouldReturnMockData_WhenProviderIsMock()
    {
        var presentation = DashboardDataHealthPresenter.Present(
            CgmProviderKind.Mock,
            GlucoseDataFreshness.NearRealTime,
            36,
            false,
            null);

        Assert.Equal(DashboardDataHealthState.MockData, presentation.State);
        Assert.Equal("Demo", presentation.BadgeText);
        Assert.False(presentation.IsShowingRealProviderData);
        Assert.False(presentation.IsDataUnavailable);
    }

    [Fact]
    public void Present_ShouldReturnProviderError_WhenProviderHasError()
    {
        var presentation = DashboardDataHealthPresenter.Present(
            CgmProviderKind.Nightscout,
            GlucoseDataFreshness.Unknown,
            0,
            true,
            "Nightscout network error.");

        Assert.Equal(DashboardDataHealthState.ProviderError, presentation.State);
        Assert.Equal("Error", presentation.BadgeText);
        Assert.True(presentation.IsDataUnavailable);
        Assert.False(presentation.IsShowingRealProviderData);
    }

    [Fact]
    public void Present_ShouldReturnNoReadings_WhenRealProviderHasNoReadings()
    {
        var presentation = DashboardDataHealthPresenter.Present(
            CgmProviderKind.Nightscout,
            GlucoseDataFreshness.NearRealTime,
            0,
            false,
            null);

        Assert.Equal(DashboardDataHealthState.NoReadings, presentation.State);
        Assert.Equal("No data", presentation.BadgeText);
        Assert.True(presentation.IsDataUnavailable);
        Assert.False(presentation.IsShowingRealProviderData);
    }

    [Fact]
    public void Present_ShouldReturnFreshRealData_WhenRealProviderHasNearRealTimeReadings()
    {
        var presentation = DashboardDataHealthPresenter.Present(
            CgmProviderKind.Nightscout,
            GlucoseDataFreshness.NearRealTime,
            12,
            false,
            null);

        Assert.Equal(DashboardDataHealthState.FreshRealData, presentation.State);
        Assert.Equal("Fresh", presentation.BadgeText);
        Assert.False(presentation.IsDataStale);
        Assert.False(presentation.IsDataUnavailable);
        Assert.True(presentation.IsShowingRealProviderData);
    }

    [Fact]
    public void Present_ShouldReturnStaleRealData_WhenRealProviderReturnsHistoricalReadings()
    {
        var presentation = DashboardDataHealthPresenter.Present(
            CgmProviderKind.DexcomOfficial,
            GlucoseDataFreshness.Historical,
            12,
            false,
            null);

        Assert.Equal(DashboardDataHealthState.StaleRealData, presentation.State);
        Assert.Equal("Check freshness", presentation.BadgeText);
        Assert.True(presentation.IsDataStale);
        Assert.True(presentation.IsShowingRealProviderData);
    }

    [Fact]
    public void PresentProviderError_ShouldReturnProviderErrorPresentation()
    {
        var presentation = DashboardDataHealthPresenter.PresentProviderError(
            "Provider unavailable.");

        Assert.Equal(DashboardDataHealthState.ProviderError, presentation.State);
        Assert.Equal("Provider refresh failed", presentation.Title);
        Assert.Equal("Error", presentation.BadgeText);
        Assert.True(presentation.IsDataUnavailable);
    }
}