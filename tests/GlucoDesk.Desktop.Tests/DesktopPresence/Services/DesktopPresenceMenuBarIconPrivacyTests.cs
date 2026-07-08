using System.Reflection;
using GlucoDesk.Desktop.DesktopPresence.Services;

namespace GlucoDesk.Desktop.Tests.DesktopPresence.Services;

public sealed class DesktopPresenceMenuBarIconPrivacyTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("High")]
    [InlineData("AboveTarget")]
    [InlineData("Low")]
    [InlineData("BelowTarget")]
    [InlineData("InRange")]
    public void SelectMenuBarIconAssetUri_ShouldReturnPrivacyIcon_WhenPrivacyModeIsEnabled(
        string? alertKindName)
    {
        var assetUri = SelectMenuBarIconAssetUri(
            alertKindName,
            isPrivacyModeEnabled: true);

        Assert.Contains(
            "glucodesk-menubar-icon-privacy.png",
            assetUri,
            StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("High", "glucodesk-menubar-icon-high.png")]
    [InlineData("AboveTarget", "glucodesk-menubar-icon-high.png")]
    [InlineData("Low", "glucodesk-menubar-icon-low.png")]
    [InlineData("BelowTarget", "glucodesk-menubar-icon-low.png")]
    [InlineData(null, "glucodesk-menubar-icon-in-range.png")]
    [InlineData("InRange", "glucodesk-menubar-icon-in-range.png")]
    public void SelectMenuBarIconAssetUri_ShouldReturnGlycemicIcon_WhenPrivacyModeIsDisabled(
        string? alertKindName,
        string expectedAssetName)
    {
        var assetUri = SelectMenuBarIconAssetUri(
            alertKindName,
            isPrivacyModeEnabled: false);

        Assert.Contains(
            expectedAssetName,
            assetUri,
            StringComparison.Ordinal);
    }

    private static string SelectMenuBarIconAssetUri(
        string? alertKindName,
        bool isPrivacyModeEnabled)
    {
        var method = typeof(AvaloniaDesktopPresenceLifecycleService).GetMethod(
            "SelectMenuBarIconAssetUri",
            BindingFlags.NonPublic | BindingFlags.Static);

        Assert.NotNull(method);

        var result = method.Invoke(
            obj: null,
            parameters: new object?[]
            {
                alertKindName,
                isPrivacyModeEnabled
            });

        return Assert.IsType<string>(result);
    }
}
