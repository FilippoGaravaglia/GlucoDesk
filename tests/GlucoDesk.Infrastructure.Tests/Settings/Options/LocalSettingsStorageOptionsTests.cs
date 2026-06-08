using GlucoDesk.Infrastructure.Settings.Options;

namespace GlucoDesk.Infrastructure.Tests.Settings.Options;

public sealed class LocalSettingsStorageOptionsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidSettingsFilePath(string settingsFilePath)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new LocalSettingsStorageOptions(settingsFilePath));

        Assert.Equal("settingsFilePath", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldTrimSettingsFilePath()
    {
        var options = new LocalSettingsStorageOptions("  /tmp/glucodesk/settings.json  ");

        Assert.Equal("/tmp/glucodesk/settings.json", options.SettingsFilePath);
    }
}