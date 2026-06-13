namespace GlucoDesk.Desktop.ViewModels.Dashboard.Providers;

/// <summary>
/// Represents the user-facing provider status shown in the dashboard.
/// </summary>
/// <param name="Title">The status title.</param>
/// <param name="Message">The status message.</param>
/// <param name="BadgeText">The short provider badge text.</param>
/// <param name="IsRealProvider">A value indicating whether the active provider is a real provider.</param>
/// <param name="IsMockProvider">A value indicating whether the active provider is the mock provider.</param>
public sealed record DashboardProviderStatusPresentation(
    string Title,
    string Message,
    string BadgeText,
    bool IsRealProvider,
    bool IsMockProvider);