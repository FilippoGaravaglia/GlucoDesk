using GlucoDesk.Desktop.DesktopPresence.Services;

namespace GlucoDesk.Desktop.Tests.DesktopPresence.Services;

public sealed class WindowsTrayIconThemeDetectorTests
{
    [Theory]
    [InlineData(0, true)]
    [InlineData(1, false)]
    [InlineData(2, false)]
    [InlineData(-1, false)]
    public void IsDarkSystemThemeRegistryValue_WhenValueIsInt_ShouldReturnExpectedResult(
        int value,
        bool expectedResult)
    {
        var result = WindowsTrayIconThemeDetector.IsDarkSystemThemeRegistryValue(value);

        Assert.Equal(expectedResult, result);
    }

    [Theory]
    [InlineData("0", true)]
    [InlineData("1", false)]
    [InlineData("invalid", false)]
    [InlineData("", false)]
    public void IsDarkSystemThemeRegistryValue_WhenValueIsString_ShouldReturnExpectedResult(
        string value,
        bool expectedResult)
    {
        var result = WindowsTrayIconThemeDetector.IsDarkSystemThemeRegistryValue(value);

        Assert.Equal(expectedResult, result);
    }

    [Fact]
    public void IsDarkSystemThemeRegistryValue_WhenValueIsMissing_ShouldReturnFalse()
    {
        var result = WindowsTrayIconThemeDetector.IsDarkSystemThemeRegistryValue(null);

        Assert.False(result);
    }
}
