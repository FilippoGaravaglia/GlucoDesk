using GlucoDesk.Desktop.DesktopPresence.Models;

namespace GlucoDesk.Desktop.DesktopPresence.Formatters;

/// <summary>
/// Formats dashboard presentation state for the desktop presence indicator.
/// </summary>
public sealed class DesktopPresenceDashboardTextFormatter : IDesktopPresenceDashboardTextFormatter
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
            ? "Glucose hidden"
            : FormatGlucoseMenuHeader(state);

        var tooltipValueText = state.IsPrivacyModeEnabled
            ? "glucose hidden"
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

    /// <summary>
    /// Formats the glucose menu header from dashboard value and trend text.
    /// </summary>
    /// <param name="state">The dashboard presentation state.</param>
    /// <returns>The formatted glucose menu header.</returns>
    private static string FormatGlucoseMenuHeader(DesktopPresenceDashboardState state)
    {
        var trendSymbol = ExtractTrendSymbol(state.TrendText);

        return string.IsNullOrWhiteSpace(trendSymbol)
            ? Normalize(state.LatestValueText)
            : $"{Normalize(state.LatestValueText)} {trendSymbol}";
    }

    /// <summary>
    /// Formats a dashboard state without a current glucose value.
    /// </summary>
    /// <param name="state">The dashboard presentation state.</param>
    /// <returns>The formatted desktop presence text.</returns>
    private static DesktopPresenceText FormatUnavailableState(DesktopPresenceDashboardState state)
    {
        var statusText = NormalizeOptional(state.StatusText);

        if (string.IsNullOrWhiteSpace(statusText))
        {
            statusText = "Waiting for glucose data";
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

    /// <summary>
    /// Determines whether a dashboard value text represents a missing glucose value.
    /// </summary>
    /// <param name="valueText">The dashboard value text.</param>
    /// <returns><c>true</c> when the value is missing; otherwise, <c>false</c>.</returns>
    private static bool IsMissingValue(string? valueText)
    {
        return string.IsNullOrWhiteSpace(valueText)
               || string.Equals(valueText.Trim(), EmptyValue, StringComparison.Ordinal);
    }

    /// <summary>
    /// Extracts the compact trend symbol from the dashboard trend text.
    /// </summary>
    /// <param name="trendText">The dashboard trend text.</param>
    /// <returns>The compact trend symbol, or an empty string.</returns>
    private static string ExtractTrendSymbol(string? trendText)
    {
        var normalizedTrendText = NormalizeOptional(trendText);

        if (string.IsNullOrWhiteSpace(normalizedTrendText))
        {
            return string.Empty;
        }

        if (string.Equals(normalizedTrendText, EmptyValue, StringComparison.Ordinal))
        {
            return string.Empty;
        }

        var firstToken = normalizedTrendText
            .Split(' ', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .FirstOrDefault();

        return string.IsNullOrWhiteSpace(firstToken)
            ? string.Empty
            : firstToken;
    }

    /// <summary>
    /// Formats the last updated text for the tooltip.
    /// </summary>
    /// <param name="lastUpdatedText">The dashboard last updated text.</param>
    /// <returns>The formatted last updated text.</returns>
    private static string FormatLastUpdated(string? lastUpdatedText)
    {
        var normalizedLastUpdatedText = NormalizeOptional(lastUpdatedText);

        if (string.IsNullOrWhiteSpace(normalizedLastUpdatedText))
        {
            return string.Empty;
        }

        if (string.Equals(normalizedLastUpdatedText, EmptyValue, StringComparison.Ordinal))
        {
            return string.Empty;
        }

        return $"updated {normalizedLastUpdatedText}";
    }

    /// <summary>
    /// Normalizes required user-facing text.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The normalized text.</returns>
    private static string Normalize(string text)
    {
        return text.Trim();
    }

    /// <summary>
    /// Normalizes optional user-facing text.
    /// </summary>
    /// <param name="text">The text to normalize.</param>
    /// <returns>The normalized text, or an empty string.</returns>
    private static string NormalizeOptional(string? text)
    {
        return string.IsNullOrWhiteSpace(text)
            ? string.Empty
            : text.Trim();
    }

    #endregion
}
