using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Dashboard.Providers;

namespace GlucoDesk.Desktop.Tests.ViewModels.Dashboard.Providers;

public sealed class DashboardProviderStatusPresenterTests
{
    [Fact]
    public void Present_ShouldReturnMockPresentation_WhenProviderIsMock()
    {
        var presentation = DashboardProviderStatusPresenter.Present(
            CgmProviderKind.Mock,
            GlucoseDataFreshness.Live);

        Assert.Equal("Using Mock data", presentation.Title);
        Assert.Equal("Mock", presentation.BadgeText);
        Assert.True(presentation.IsMockProvider);
        Assert.False(presentation.IsRealProvider);
        Assert.Contains("Mock is the active live provider", presentation.Message);
    }

    [Fact]
    public void Present_ShouldReturnNightscoutPresentation_WhenProviderIsNightscout()
    {
        var presentation = DashboardProviderStatusPresenter.Present(
            CgmProviderKind.Nightscout,
            GlucoseDataFreshness.NearRealTime);

        Assert.Equal("Using Nightscout", presentation.Title);
        Assert.Equal("Nightscout", presentation.BadgeText);
        Assert.True(presentation.IsRealProvider);
        Assert.False(presentation.IsMockProvider);
        Assert.Contains("near real-time", presentation.Message);
    }

    [Fact]
    public void Present_ShouldReturnDexcomSandboxPresentation_WhenProviderIsDexcomSandbox()
    {
        var presentation = DashboardProviderStatusPresenter.Present(
            CgmProviderKind.DexcomSandbox,
            GlucoseDataFreshness.Delayed);

        Assert.Equal("Using Dexcom Sandbox", presentation.Title);
        Assert.Equal("Dexcom Sandbox", presentation.BadgeText);
        Assert.True(presentation.IsRealProvider);
        Assert.False(presentation.IsMockProvider);
        Assert.Contains("simulated", presentation.Message);
    }

    [Fact]
    public void Present_ShouldReturnDexcomOfficialPresentation_WhenProviderIsDexcomOfficial()
    {
        var presentation = DashboardProviderStatusPresenter.Present(
            CgmProviderKind.DexcomOfficial,
            GlucoseDataFreshness.Delayed);

        Assert.Equal("Using Dexcom", presentation.Title);
        Assert.Equal("Dexcom", presentation.BadgeText);
        Assert.True(presentation.IsRealProvider);
        Assert.False(presentation.IsMockProvider);
        Assert.Contains("official", presentation.Message);
    }

    [Fact]
    public void Present_ShouldReturnUnknownPresentation_WhenProviderIsUnknown()
    {
        var presentation = DashboardProviderStatusPresenter.Present(
            CgmProviderKind.Unknown,
            GlucoseDataFreshness.Unknown);

        Assert.Equal("Provider status unknown", presentation.Title);
        Assert.Equal("Unknown", presentation.BadgeText);
        Assert.False(presentation.IsRealProvider);
        Assert.False(presentation.IsMockProvider);
    }
}