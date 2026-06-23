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
/// <param name="IsPrivacyModeEnabled">Whether the exact glucose value should be hidden in desktop presence.</param>
public sealed record DesktopPresenceDashboardState(
    string ProviderDisplayName,
    string LatestValueText,
    string TrendText,
    string FreshnessText,
    string LastUpdatedText,
    string StatusText,
    bool IsPrivacyModeEnabled = false);
