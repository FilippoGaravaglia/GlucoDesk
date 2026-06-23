namespace GlucoDesk.Desktop.DesktopPresence.Models;

/// <summary>
/// Represents the dashboard presentation state used by the desktop presence indicator.
/// </summary>
/// <param name="ProviderDisplayName">The provider display name.</param>
/// <param name="LatestValueText">The latest glucose value text.</param>
/// <param name="TrendText">The glucose trend text.</param>
/// <param name="FreshnessText">The data freshness text.</param>
/// <param name="LastUpdatedText">The last updated text.</param>
/// <param name="StatusText">The dashboard status text.</param>
public sealed record DesktopPresenceDashboardState(
    string ProviderDisplayName,
    string LatestValueText,
    string TrendText,
    string FreshnessText,
    string LastUpdatedText,
    string StatusText);
