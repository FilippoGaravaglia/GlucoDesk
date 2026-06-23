using System.Globalization;
using GlucoDesk.Desktop.DesktopPresence.Enums;
using GlucoDesk.Desktop.DesktopPresence.Models;

namespace GlucoDesk.Desktop.DesktopPresence.Formatters;

/// <summary>
/// Formats glucose state for the desktop presence indicator.
/// </summary>
public sealed class DesktopPresenceTextFormatter : IDesktopPresenceTextFormatter
{
    private const string ProductName = "GlucoDesk";

    /// <inheritdoc />
    public DesktopPresenceText Format(DesktopPresenceSnapshot snapshot)
    {
        return snapshot.DataState switch
        {
            DesktopPresenceDataState.ProviderNotConfigured => new DesktopPresenceText(
                $"{ProductName} · provider not configured",
                "Provider not configured"),

            DesktopPresenceDataState.NoData => new DesktopPresenceText(
                $"{ProductName} · no glucose data",
                "No glucose data available"),

            DesktopPresenceDataState.Fresh or DesktopPresenceDataState.Stale => FormatReadingSnapshot(snapshot),

            _ => new DesktopPresenceText(
                $"{ProductName} · unavailable",
                "Glucose status unavailable")
        };
    }

    #region Helpers

    /// <summary>
    /// Formats a snapshot that contains a glucose reading.
    /// </summary>
    /// <param name="snapshot">The desktop presence snapshot.</param>
    /// <returns>The formatted desktop presence text.</returns>
    private static DesktopPresenceText FormatReadingSnapshot(DesktopPresenceSnapshot snapshot)
    {
        var freshnessText = snapshot.DataState is DesktopPresenceDataState.Fresh
            ? "fresh"
            : "stale";

        var ageText = FormatReadingAge(
            snapshot.ReadingTimestamp,
            snapshot.Now);

        if (snapshot.IsPrivacyModeEnabled)
        {
            return new DesktopPresenceText(
                $"{ProductName} · glucose hidden · {freshnessText} · {ageText}",
                "Privacy mode enabled");
        }

        var valueText = FormatValue(
            snapshot.DisplayValue,
            snapshot.UnitSymbol);

        var trendText = FormatTrend(snapshot.TrendText);

        return new DesktopPresenceText(
            $"{ProductName} · {valueText}{trendText} · {freshnessText} · {ageText}",
            $"{valueText}{trendText}");
    }

    /// <summary>
    /// Formats the glucose value using the selected display unit.
    /// </summary>
    /// <param name="displayValue">The display glucose value.</param>
    /// <param name="unitSymbol">The selected unit symbol.</param>
    /// <returns>The formatted glucose value.</returns>
    private static string FormatValue(
        decimal? displayValue,
        string unitSymbol)
    {
        if (displayValue is null)
        {
            return "glucose unavailable";
        }

        var normalizedUnitSymbol = string.IsNullOrWhiteSpace(unitSymbol)
            ? "mg/dL"
            : unitSymbol.Trim();

        var valueText = displayValue.Value == decimal.Truncate(displayValue.Value)
            ? displayValue.Value.ToString("0", CultureInfo.InvariantCulture)
            : displayValue.Value.ToString("0.0", CultureInfo.InvariantCulture);

        return $"{valueText} {normalizedUnitSymbol}";
    }

    /// <summary>
    /// Formats the optional trend text.
    /// </summary>
    /// <param name="trendText">The optional trend text.</param>
    /// <returns>The formatted trend suffix.</returns>
    private static string FormatTrend(string? trendText)
    {
        return string.IsNullOrWhiteSpace(trendText)
            ? string.Empty
            : $" {trendText.Trim()}";
    }

    /// <summary>
    /// Formats the age of the glucose reading.
    /// </summary>
    /// <param name="readingTimestamp">The glucose reading timestamp.</param>
    /// <param name="now">The current timestamp.</param>
    /// <returns>The formatted reading age.</returns>
    private static string FormatReadingAge(
        DateTimeOffset? readingTimestamp,
        DateTimeOffset now)
    {
        if (readingTimestamp is null)
        {
            return "unknown age";
        }

        var age = now - readingTimestamp.Value;

        if (age <= TimeSpan.Zero)
        {
            return "just now";
        }

        if (age.TotalMinutes < 1)
        {
            return "just now";
        }

        if (age.TotalHours < 1)
        {
            return $"{(int)age.TotalMinutes} min ago";
        }

        if (age.TotalDays < 1)
        {
            var hours = (int)age.TotalHours;
            var minutes = age.Minutes;

            return minutes == 0
                ? $"{hours} h ago"
                : $"{hours} h {minutes} min ago";
        }

        return $"{(int)age.TotalDays} d ago";
    }

    #endregion
}
