using System.Reflection;
using GlucoDesk.Desktop.DesktopPresence.Services;
using GlucoDesk.Desktop.ViewModels.Dashboard;

namespace GlucoDesk.Desktop.Tests.DesktopPresence.Services;

public sealed class DesktopPresenceImmediateMenuBarIconUpdateTests
{
    [Theory]
    [InlineData(nameof(DashboardViewModel.CurrentGlucoseAlertKind))]
    [InlineData(nameof(DashboardViewModel.LatestValueText))]
    [InlineData(nameof(DashboardViewModel.TrendText))]
    [InlineData(nameof(DashboardViewModel.FreshnessText))]
    [InlineData(nameof(DashboardViewModel.LastUpdatedText))]
    [InlineData(nameof(DashboardViewModel.StatusText))]
    [InlineData(nameof(DashboardViewModel.IsBusy))]
    [InlineData(null)]
    [InlineData("")]
    public void ShouldRefreshFromDashboardProperty_ShouldReturnTrue_ForRelevantReadingState(
        string? propertyName)
    {
        Assert.True(
            ShouldRefreshFromDashboardProperty(propertyName));
    }

    [Theory]
    [InlineData("UnrelatedProperty")]
    [InlineData("SelectedTab")]
    [InlineData("WindowTitle")]
    public void ShouldRefreshFromDashboardProperty_ShouldReturnFalse_ForUnrelatedState(
        string propertyName)
    {
        Assert.False(
            ShouldRefreshFromDashboardProperty(propertyName));
    }

    [Theory]
    [InlineData("AboveTarget", false, "glucodesk-menubar-icon-high.png")]
    [InlineData("High", false, "glucodesk-menubar-icon-high.png")]
    [InlineData("BelowTarget", false, "glucodesk-menubar-icon-low.png")]
    [InlineData("Low", false, "glucodesk-menubar-icon-low.png")]
    [InlineData("InRange", false, "glucodesk-menubar-icon-in-range.png")]
    [InlineData(null, false, "glucodesk-menubar-icon-in-range.png")]
    [InlineData("AboveTarget", true, "glucodesk-menubar-icon-privacy.png")]
    [InlineData("BelowTarget", true, "glucodesk-menubar-icon-privacy.png")]
    [InlineData("InRange", true, "glucodesk-menubar-icon-privacy.png")]
    public void SelectMenuBarIconAssetUri_ShouldResolveImmediateEffectiveState(
        string? alertKindName,
        bool isPrivacyModeEnabled,
        string expectedAssetName)
    {
        var result = InvokePrivateStatic<string>(
            "SelectMenuBarIconAssetUri",
            alertKindName,
            isPrivacyModeEnabled);

        Assert.Contains(
            expectedAssetName,
            result,
            StringComparison.Ordinal);
    }

    private static bool ShouldRefreshFromDashboardProperty(
        string? propertyName)
    {
        return InvokePrivateStatic<bool>(
            "ShouldRefreshFromDashboardProperty",
            propertyName);
    }

    private static TResult InvokePrivateStatic<TResult>(
        string methodName,
        params object?[] arguments)
    {
        var method =
            typeof(AvaloniaDesktopPresenceLifecycleService).GetMethod(
                methodName,
                BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var result = method.Invoke(
            obj: null,
            parameters: arguments);

        return Assert.IsType<TResult>(result);
    }
}
