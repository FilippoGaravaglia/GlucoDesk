using System.Globalization;
using GlucoDesk.Desktop.DesktopPresence.Enums;
using GlucoDesk.Desktop.DesktopPresence.Models;
using GlucoDesk.Desktop.Localization;

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
        ArgumentNullException.ThrowIfNull(snapshot);

        return snapshot.DataState switch
        {
            DesktopPresenceDataState.ProviderNotConfigured =>
                new DesktopPresenceText(
                    Text("DesktopPresenceProviderNotConfiguredTooltip"),
                    Text("DesktopPresenceProviderNotConfigured")),

            DesktopPresenceDataState.NoData =>
                new DesktopPresenceText(
                    Text("DesktopPresenceNoDataTooltip"),
                    Text("DesktopPresenceNoData")),

            DesktopPresenceDataState.Fresh or DesktopPresenceDataState.Stale =>
                FormatReadingSnapshot(snapshot),

            _ =>
                new DesktopPresenceText(
                    Text("DesktopPresenceUnavailableTooltip"),
                    Text("DesktopPresenceUnavailable"))
        };
    }

    #region Helpers

    private static DesktopPresenceText FormatReadingSnapshot(
        DesktopPresenceSnapshot snapshot)
    {
        var freshnessText = snapshot.DataState is DesktopPresenceDataState.Fresh
            ? Text("DesktopPresenceFresh")
            : Text("DesktopPresenceStale");

        var ageText = FormatReadingAge(
            snapshot.ReadingTimestamp,
            snapshot.Now);

        if (snapshot.IsPrivacyModeEnabled)
        {
            return new DesktopPresenceText(
                $"{ProductName} · {Text("DesktopPresenceGlucoseHiddenLower")} · {freshnessText} · {ageText}",
                Text("DesktopPresencePrivacyEnabled"));
        }

        var valueText = FormatValue(
            snapshot.DisplayValue,
            snapshot.UnitSymbol);

        var trendText = FormatTrend(snapshot.TrendText);

        return new DesktopPresenceText(
            $"{ProductName} · {valueText}{trendText} · {freshnessText} · {ageText}",
            $"{valueText}{trendText}");
    }

    private static string FormatValue(
        decimal? displayValue,
        string unitSymbol)
    {
        if (displayValue is null)
        {
            return Text("DesktopPresenceGlucoseUnavailable");
        }

        var normalizedUnitSymbol = string.IsNullOrWhiteSpace(unitSymbol)
            ? "mg/dL"
            : unitSymbol.Trim();

        var valueText = displayValue.Value == decimal.Truncate(displayValue.Value)
            ? displayValue.Value.ToString("0", CultureInfo.InvariantCulture)
            : displayValue.Value.ToString("0.0", CultureInfo.InvariantCulture);

        return $"{valueText} {normalizedUnitSymbol}";
    }

    private static string FormatTrend(string? trendText)
    {
        return string.IsNullOrWhiteSpace(trendText)
            ? string.Empty
            : $" {trendText.Trim()}";
    }

    private static string FormatReadingAge(
        DateTimeOffset? readingTimestamp,
        DateTimeOffset now)
    {
        if (readingTimestamp is null)
        {
            return Text("DesktopPresenceUnknownAge");
        }

        var age = now - readingTimestamp.Value;

        if (age <= TimeSpan.Zero || age.TotalMinutes < 1)
        {
            return Text("DesktopPresenceJustNow");
        }

        if (age.TotalHours < 1)
        {
            return Format(
                "DesktopPresenceMinutesAgo",
                (int)age.TotalMinutes);
        }

        if (age.TotalDays < 1)
        {
            var hours = (int)age.TotalHours;
            var minutes = age.Minutes;

            return minutes == 0
                ? Format("DesktopPresenceHoursAgo", hours)
                : Format(
                    "DesktopPresenceHoursMinutesAgo",
                    hours,
                    minutes);
        }

        return Format(
            "DesktopPresenceDaysAgo",
            (int)age.TotalDays);
    }

    private static string Text(string key)
    {
        return LocalizationManager.GetString(key);
    }

    private static string Format(string key, params object[] arguments)
    {
        return string.Format(
            CultureInfo.CurrentCulture,
            Text(key),
            arguments);
    }

    #endregion
}
