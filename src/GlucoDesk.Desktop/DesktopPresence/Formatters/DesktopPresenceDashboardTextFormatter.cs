using System.Globalization;
using GlucoDesk.Desktop.DesktopPresence.Models;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.DesktopPresence.Formatters;

/// <summary>
/// Formats dashboard presentation state for the desktop presence indicator.
/// </summary>
public sealed class DesktopPresenceDashboardTextFormatter :
    IDesktopPresenceDashboardTextFormatter
{
    private const string ProductName = "GlucoDesk";
    private const string EmptyValue = "—";

    /// <inheritdoc />
    public DesktopPresenceText Format(DesktopPresenceDashboardState state)
    {
        ArgumentNullException.ThrowIfNull(state);

        if (IsMissingValue(state.LatestValueText))
        {
            return FormatUnavailableState(state);
        }

        var menuHeader = state.IsPrivacyModeEnabled
            ? Text("DesktopPresenceGlucoseHidden")
            : FormatGlucoseMenuHeader(state);

        var tooltipValueText = state.IsPrivacyModeEnabled
            ? Text("DesktopPresenceGlucoseHiddenLower")
            : menuHeader;

        var tooltipParts = new[]
            {
                ProductName,
                tooltipValueText,
                NormalizeOptional(state.FreshnessText),
                FormatLastUpdated(state.LastUpdatedText),
                NormalizeOptional(state.StatusText)
            }
            .Where(part => !string.IsNullOrWhiteSpace(part));

        return new DesktopPresenceText(
            string.Join(" · ", tooltipParts),
            menuHeader);
    }

    #region Helpers

    private static string FormatGlucoseMenuHeader(
        DesktopPresenceDashboardState state)
    {
        var trendSymbol = ExtractTrendSymbol(state.TrendText);

        return string.IsNullOrWhiteSpace(trendSymbol)
            ? Normalize(state.LatestValueText)
            : $"{Normalize(state.LatestValueText)} {trendSymbol}";
    }

    private static DesktopPresenceText FormatUnavailableState(
        DesktopPresenceDashboardState state)
    {
        var statusText = NormalizeOptional(state.StatusText);

        if (string.IsNullOrWhiteSpace(statusText))
        {
            statusText = Text("DesktopPresenceWaitingForData");
        }

        var providerText = NormalizeOptional(state.ProviderDisplayName);

        var tooltipParts = new[]
            {
                ProductName,
                statusText,
                providerText
            }
            .Where(part => !string.IsNullOrWhiteSpace(part));

        return new DesktopPresenceText(
            string.Join(" · ", tooltipParts),
            statusText);
    }

    private static bool IsMissingValue(string? valueText)
    {
        return string.IsNullOrWhiteSpace(valueText)
               || string.Equals(
                   valueText.Trim(),
                   EmptyValue,
                   StringComparison.Ordinal);
    }

    private static string ExtractTrendSymbol(string? trendText)
    {
        var normalizedTrendText = NormalizeOptional(trendText);

        if (string.IsNullOrWhiteSpace(normalizedTrendText)
            || string.Equals(
                normalizedTrendText,
                EmptyValue,
                StringComparison.Ordinal))
        {
            return string.Empty;
        }

        var firstToken = normalizedTrendText
            .Split(
                ' ',
                StringSplitOptions.RemoveEmptyEntries
                | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(firstToken)
            ? string.Empty
            : firstToken;
    }

    private static string FormatLastUpdated(string? lastUpdatedText)
    {
        var normalizedLastUpdatedText = NormalizeOptional(lastUpdatedText);

        if (string.IsNullOrWhiteSpace(normalizedLastUpdatedText)
            || string.Equals(
                normalizedLastUpdatedText,
                EmptyValue,
                StringComparison.Ordinal))
        {
            return string.Empty;
        }

        return string.Format(
            CultureInfo.CurrentCulture,
            Text("DesktopPresenceUpdatedAt"),
            normalizedLastUpdatedText);
    }

    private static string Normalize(string text)
    {
        return text.Trim();
    }

    private static string NormalizeOptional(string? text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : text.Trim();
    }

    private static string Text(string key)
    {
        return LocalizationManager.GetString(key);
    }

    #endregion
}
