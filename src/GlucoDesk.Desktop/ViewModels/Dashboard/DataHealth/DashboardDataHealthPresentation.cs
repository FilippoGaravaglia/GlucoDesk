namespace GlucoDesk.Desktop.ViewModels.Dashboard.DataHealth;

/// <summary>
/// Represents the user-facing dashboard data health presentation.
/// </summary>
/// <param name="State">The data health state.</param>
/// <param name="Title">The status title.</param>
/// <param name="Message">The status message.</param>
/// <param name="BadgeText">The short badge text.</param>
/// <param name="IsDataStale">A value indicating whether the displayed data is stale.</param>
/// <param name="IsDataUnavailable">A value indicating whether provider data is unavailable.</param>
/// <param name="IsShowingRealProviderData">A value indicating whether real provider data is being shown.</param>
public sealed record DashboardDataHealthPresentation(
    DashboardDataHealthState State,
    string Title,
    string Message,
    string BadgeText,
    bool IsDataStale,
    bool IsDataUnavailable,
    bool IsShowingRealProviderData);