using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Desktop.ViewModels.Dashboard.DataHealth;

/// <summary>
/// Builds user-facing dashboard data health messages.
/// </summary>
public static class DashboardDataHealthPresenter
{
    /// <summary>
    /// Creates a dashboard data health presentation.
    /// </summary>
    /// <param name="providerKind">The active provider kind.</param>
    /// <param name="freshness">The latest reading freshness.</param>
    /// <param name="readingCount">The number of available recent readings.</param>
    /// <param name="hasProviderError">A value indicating whether the provider refresh failed.</param>
    /// <param name="providerErrorMessage">The provider error message, when available.</param>
    /// <returns>The dashboard data health presentation.</returns>
    public static DashboardDataHealthPresentation Present(
        CgmProviderKind providerKind,
        GlucoseDataFreshness freshness,
        int readingCount,
        bool hasProviderError,
        string? providerErrorMessage)
    {
        if (providerKind is CgmProviderKind.Mock)
        {
            return new DashboardDataHealthPresentation(
                DashboardDataHealthState.MockData,
                "Demo data active",
                "The dashboard is currently showing Mock provider data. Configure and select Nightscout or Dexcom to use real provider data.",
                "Demo",
                false,
                false,
                false);
        }

        if (hasProviderError)
        {
            return new DashboardDataHealthPresentation(
                DashboardDataHealthState.ProviderError,
                "Provider refresh failed",
                string.IsNullOrWhiteSpace(providerErrorMessage)
                    ? "The active provider could not refresh glucose data."
                    : providerErrorMessage,
                "Error",
                false,
                true,
                false);
        }

        if (readingCount <= 0)
        {
            return new DashboardDataHealthPresentation(
                DashboardDataHealthState.NoReadings,
                "No recent readings",
                "The active provider is selected, but no recent glucose readings are currently available.",
                "No data",
                false,
                true,
                false);
        }

        if (IsStaleFreshness(freshness))
        {
            return new DashboardDataHealthPresentation(
                DashboardDataHealthState.StaleRealData,
                "Data freshness needs attention",
                BuildStaleMessage(freshness),
                "Check freshness",
                true,
                false,
                true);
        }

        return new DashboardDataHealthPresentation(
            DashboardDataHealthState.FreshRealData,
            "Real provider data active",
            BuildFreshMessage(freshness),
            "Fresh",
            false,
            false,
            true);
    }

    /// <summary>
    /// Creates a dashboard data health presentation for provider refresh failures when provider details are unavailable.
    /// </summary>
    /// <param name="providerErrorMessage">The provider error message.</param>
    /// <returns>The dashboard data health presentation.</returns>
    public static DashboardDataHealthPresentation PresentProviderError(
        string? providerErrorMessage)
    {
        return new DashboardDataHealthPresentation(
            DashboardDataHealthState.ProviderError,
            "Provider refresh failed",
            string.IsNullOrWhiteSpace(providerErrorMessage)
                ? "The active provider could not refresh glucose data."
                : providerErrorMessage,
            "Error",
            false,
            true,
            false);
    }

    #region Helpers

    /// <summary>
    /// Checks whether a freshness value should be treated as stale or uncertain for dashboard health purposes.
    /// </summary>
    /// <param name="freshness">The glucose data freshness.</param>
    /// <returns>True when the freshness requires attention; otherwise false.</returns>
    private static bool IsStaleFreshness(GlucoseDataFreshness freshness)
    {
        return freshness is GlucoseDataFreshness.Unknown
            or GlucoseDataFreshness.Delayed
            or GlucoseDataFreshness.Historical;
    }

    /// <summary>
    /// Builds a user-facing message for stale or uncertain provider data.
    /// </summary>
    /// <param name="freshness">The glucose data freshness.</param>
    /// <returns>The stale data message.</returns>
    private static string BuildStaleMessage(GlucoseDataFreshness freshness)
    {
        return freshness switch
        {
            GlucoseDataFreshness.Delayed =>
                "The active provider returned delayed glucose data. GlucoDesk can display it, but it should not be treated as real-time monitoring.",

            GlucoseDataFreshness.Historical =>
                "The active provider returned historical glucose data. Check the provider connection before relying on the displayed trend.",

            _ =>
                "The active provider returned glucose data with unknown freshness. Check the provider connection before relying on the displayed trend."
        };
    }

    /// <summary>
    /// Builds a user-facing message for fresh real provider data.
    /// </summary>
    /// <param name="freshness">The glucose data freshness.</param>
    /// <returns>The fresh data message.</returns>
    private static string BuildFreshMessage(GlucoseDataFreshness freshness)
    {
        return freshness switch
        {
            GlucoseDataFreshness.Live =>
                "The dashboard is showing live provider data. GlucoDesk remains a companion app and must not be used for treatment decisions.",

            GlucoseDataFreshness.NearRealTime =>
                "The dashboard is showing near real-time provider data. GlucoDesk remains a companion app and must not be used for treatment decisions.",

            _ =>
                "The dashboard is showing real provider data. GlucoDesk remains a companion app and must not be used for treatment decisions."
        };
    }

    #endregion
}