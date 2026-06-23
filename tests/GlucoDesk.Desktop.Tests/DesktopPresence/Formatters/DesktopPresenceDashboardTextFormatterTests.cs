using GlucoDesk.Desktop.DesktopPresence.Formatters;
using GlucoDesk.Desktop.DesktopPresence.Models;

namespace GlucoDesk.Desktop.Tests.DesktopPresence.Formatters;

public sealed class DesktopPresenceDashboardTextFormatterTests
{
    private readonly DesktopPresenceDashboardTextFormatter _formatter = new();

    [Fact]
    public void Format_ShouldShowCompactGlucoseValue_WhenDashboardHasCurrentReading()
    {
        // Arrange
        var state = new DesktopPresenceDashboardState(
            "Mock CGM Provider",
            "123 mg/dL",
            "→ Stable",
            "Near real-time",
            "12:30",
            "In range");

        // Act
        var result = _formatter.Format(state);

        // Assert
        Assert.Equal("123 mg/dL →", result.MenuHeader);
        Assert.Equal("GlucoDesk · 123 mg/dL → · Near real-time · updated 12:30 · In range", result.Tooltip);
    }

    [Fact]
    public void Format_ShouldSupportMmolValues_WhenDashboardUsesPreferredUnit()
    {
        // Arrange
        var state = new DesktopPresenceDashboardState(
            "Dexcom Share",
            "6.8 mmol/L",
            "↗ Rising",
            "Near real-time",
            "12:31",
            "In range");

        // Act
        var result = _formatter.Format(state);

        // Assert
        Assert.Equal("6.8 mmol/L ↗", result.MenuHeader);
        Assert.Equal("GlucoDesk · 6.8 mmol/L ↗ · Near real-time · updated 12:31 · In range", result.Tooltip);
    }

    [Fact]
    public void Format_ShouldHideGlucoseValue_WhenPrivacyModeIsEnabled()
    {
        // Arrange
        var state = new DesktopPresenceDashboardState(
            "Dexcom Share",
            "123 mg/dL",
            "→ Stable",
            "Near real-time",
            "12:30",
            "In range",
            IsPrivacyModeEnabled: true);

        // Act
        var result = _formatter.Format(state);

        // Assert
        Assert.Equal("Glucose hidden", result.MenuHeader);
        Assert.Equal("GlucoDesk · glucose hidden · Near real-time · updated 12:30 · In range", result.Tooltip);
    }

    [Fact]
    public void Format_ShouldShowWaitingState_WhenDashboardHasNoCurrentReading()
    {
        // Arrange
        var state = new DesktopPresenceDashboardState(
            "Not loaded",
            "—",
            "—",
            "—",
            "—",
            "Waiting for data");

        // Act
        var result = _formatter.Format(state);

        // Assert
        Assert.Equal("Waiting for data", result.MenuHeader);
        Assert.Equal("GlucoDesk · Waiting for data · Not loaded", result.Tooltip);
    }

    [Fact]
    public void Format_ShouldFallbackToWaitingText_WhenDashboardStateIsEmpty()
    {
        // Arrange
        var state = new DesktopPresenceDashboardState(
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty,
            string.Empty);

        // Act
        var result = _formatter.Format(state);

        // Assert
        Assert.Equal("Waiting for glucose data", result.MenuHeader);
        Assert.Equal("GlucoDesk · Waiting for glucose data", result.Tooltip);
    }
}
