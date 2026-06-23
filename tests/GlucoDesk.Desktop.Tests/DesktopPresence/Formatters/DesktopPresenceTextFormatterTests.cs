using GlucoDesk.Desktop.DesktopPresence.Enums;
using GlucoDesk.Desktop.DesktopPresence.Formatters;
using GlucoDesk.Desktop.DesktopPresence.Models;

namespace GlucoDesk.Desktop.Tests.DesktopPresence.Formatters;

public sealed class DesktopPresenceTextFormatterTests
{
    private readonly DesktopPresenceTextFormatter _formatter = new();

    [Fact]
    public void Format_ShouldShowProviderNotConfigured_WhenProviderIsNotConfigured()
    {
        // Arrange
        var snapshot = CreateSnapshot(
            DesktopPresenceDataState.ProviderNotConfigured,
            displayValue: null);

        // Act
        var result = _formatter.Format(snapshot);

        // Assert
        Assert.Equal("GlucoDesk · provider not configured", result.Tooltip);
        Assert.Equal("Provider not configured", result.MenuHeader);
    }

    [Fact]
    public void Format_ShouldShowNoData_WhenNoReadingIsAvailable()
    {
        // Arrange
        var snapshot = CreateSnapshot(
            DesktopPresenceDataState.NoData,
            displayValue: null);

        // Act
        var result = _formatter.Format(snapshot);

        // Assert
        Assert.Equal("GlucoDesk · no glucose data", result.Tooltip);
        Assert.Equal("No glucose data available", result.MenuHeader);
    }

    [Fact]
    public void Format_ShouldShowFreshReading_WhenReadingIsAvailable()
    {
        // Arrange
        var snapshot = CreateSnapshot(
            DesktopPresenceDataState.Fresh,
            displayValue: 108m,
            trendText: "→",
            readingTimestamp: TestNow.AddMinutes(-5));

        // Act
        var result = _formatter.Format(snapshot);

        // Assert
        Assert.Equal("GlucoDesk · 108 mg/dL → · fresh · 5 min ago", result.Tooltip);
        Assert.Equal("108 mg/dL →", result.MenuHeader);
    }

    [Fact]
    public void Format_ShouldShowDecimalValue_WhenSelectedUnitUsesDecimals()
    {
        // Arrange
        var snapshot = CreateSnapshot(
            DesktopPresenceDataState.Fresh,
            displayValue: 6.1m,
            unitSymbol: "mmol/L",
            trendText: "↗",
            readingTimestamp: TestNow.AddMinutes(-2));

        // Act
        var result = _formatter.Format(snapshot);

        // Assert
        Assert.Equal("GlucoDesk · 6.1 mmol/L ↗ · fresh · 2 min ago", result.Tooltip);
        Assert.Equal("6.1 mmol/L ↗", result.MenuHeader);
    }

    [Fact]
    public void Format_ShouldHideExactValue_WhenPrivacyModeIsEnabled()
    {
        // Arrange
        var snapshot = CreateSnapshot(
            DesktopPresenceDataState.Fresh,
            displayValue: 108m,
            trendText: "→",
            readingTimestamp: TestNow.AddMinutes(-5),
            isPrivacyModeEnabled: true);

        // Act
        var result = _formatter.Format(snapshot);

        // Assert
        Assert.Equal("GlucoDesk · glucose hidden · fresh · 5 min ago", result.Tooltip);
        Assert.Equal("Privacy mode enabled", result.MenuHeader);
    }

    [Fact]
    public void Format_ShouldShowStaleState_WhenReadingIsNoLongerFresh()
    {
        // Arrange
        var snapshot = CreateSnapshot(
            DesktopPresenceDataState.Stale,
            displayValue: 145m,
            trendText: "↘",
            readingTimestamp: TestNow.AddMinutes(-42));

        // Act
        var result = _formatter.Format(snapshot);

        // Assert
        Assert.Equal("GlucoDesk · 145 mg/dL ↘ · stale · 42 min ago", result.Tooltip);
        Assert.Equal("145 mg/dL ↘", result.MenuHeader);
    }

    #region Helpers

    private static DateTimeOffset TestNow => new(2026, 6, 22, 12, 0, 0, TimeSpan.Zero);

    /// <summary>
    /// Creates a desktop presence snapshot for formatter tests.
    /// </summary>
    /// <param name="dataState">The desktop presence data state.</param>
    /// <param name="displayValue">The optional display glucose value.</param>
    /// <param name="unitSymbol">The glucose unit symbol.</param>
    /// <param name="trendText">The optional trend text.</param>
    /// <param name="readingTimestamp">The optional reading timestamp.</param>
    /// <param name="isPrivacyModeEnabled">Whether privacy mode is enabled.</param>
    /// <returns>The created snapshot.</returns>
    private static DesktopPresenceSnapshot CreateSnapshot(
        DesktopPresenceDataState dataState,
        decimal? displayValue,
        string unitSymbol = "mg/dL",
        string? trendText = null,
        DateTimeOffset? readingTimestamp = null,
        bool isPrivacyModeEnabled = false)
    {
        return new DesktopPresenceSnapshot(
            dataState,
            displayValue,
            unitSymbol,
            trendText,
            readingTimestamp,
            TestNow,
            isPrivacyModeEnabled);
    }

    #endregion
}
