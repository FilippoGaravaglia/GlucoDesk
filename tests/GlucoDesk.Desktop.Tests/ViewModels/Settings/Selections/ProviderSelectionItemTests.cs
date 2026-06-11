using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Settings.Selections;

namespace GlucoDesk.Desktop.Tests.ViewModels.Settings.Selections;

public sealed class ProviderSelectionItemTests
{
    [Fact]
    public void Constructor_ShouldCreateAvailableProviderSelectionItem()
    {
        var item = new ProviderSelectionItem(
            CgmProviderKind.Mock,
            " Mock ",
            isAvailable: true);

        Assert.Equal(CgmProviderKind.Mock, item.Kind);
        Assert.Equal("Mock", item.DisplayName);
        Assert.True(item.IsAvailable);
        Assert.Equal("Provider is available.", item.AvailabilityMessage);
        Assert.Equal("Mock", item.DisplayLabel);
        Assert.Equal("Mock", item.ToString());
    }

    [Fact]
    public void Constructor_ShouldCreateUnavailableProviderSelectionItem()
    {
        var item = new ProviderSelectionItem(
            CgmProviderKind.DexcomOfficial,
            "Dexcom Official",
            isAvailable: false);

        Assert.Equal(CgmProviderKind.DexcomOfficial, item.Kind);
        Assert.Equal("Dexcom Official", item.DisplayName);
        Assert.False(item.IsAvailable);
        Assert.Equal("Provider is not configured in the current desktop runtime.", item.AvailabilityMessage);
        Assert.Equal("Dexcom Official (not configured)", item.DisplayLabel);
        Assert.Equal("Dexcom Official (not configured)", item.ToString());
    }

    [Fact]
    public void Constructor_ShouldRejectUnknownProviderKind()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new ProviderSelectionItem(CgmProviderKind.Unknown, "Unknown"));

        Assert.Equal("kind", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidDisplayName(string displayName)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new ProviderSelectionItem(CgmProviderKind.Mock, displayName));

        Assert.Equal("displayName", exception.ParamName);
    }
}