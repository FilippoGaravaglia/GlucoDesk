using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Desktop.ViewModels.Dashboard.Providers;

/// <summary>
/// Builds dashboard provider status presentations.
/// </summary>
public static class DashboardProviderStatusPresenter
{
    /// <summary>
    /// Builds the dashboard provider status presentation.
    /// </summary>
    /// <param name="providerKind">The active provider kind.</param>
    /// <param name="freshness">The glucose data freshness.</param>
    /// <returns>The dashboard provider status presentation.</returns>
    public static DashboardProviderStatusPresentation Present(
        CgmProviderKind providerKind,
        GlucoseDataFreshness freshness)
    {
        return providerKind switch
        {
            CgmProviderKind.Mock =>
                new DashboardProviderStatusPresentation(
                    "Using Mock data",
                    "Mock is the active live provider. Configure and select Dexcom or Nightscout in Settings to use real glucose data.",
                    "Mock",
                    false,
                    true),

            CgmProviderKind.Nightscout =>
                new DashboardProviderStatusPresentation(
                    "Using Nightscout",
                    BuildRealProviderMessage("Nightscout", freshness),
                    "Nightscout",
                    true,
                    false),

            CgmProviderKind.DexcomShare =>
                new DashboardProviderStatusPresentation(
                    "Using Dexcom Share",
                    BuildRealProviderMessage("Dexcom Share", freshness),
                    "Dexcom Share",
                    true,
                    false),

            CgmProviderKind.DexcomSandbox =>
                new DashboardProviderStatusPresentation(
                    "Using Dexcom Sandbox",
                    "Dexcom Sandbox is active. This provider returns simulated data for integration testing and does not represent your real glucose data.",
                    "Dexcom Sandbox",
                    true,
                    false),

            CgmProviderKind.DexcomOfficial =>
                new DashboardProviderStatusPresentation(
                    "Using Dexcom",
                    BuildRealProviderMessage("official Dexcom", freshness),
                    "Dexcom",
                    true,
                    false),

            _ =>
                new DashboardProviderStatusPresentation(
                    "Provider status unknown",
                    "The active glucose data provider could not be identified.",
                    "Unknown",
                    false,
                    false)
        };
    }

    #region Helpers

    /// <summary>
    /// Builds a real provider status message.
    /// </summary>
    /// <param name="providerDisplayName">The provider display name.</param>
    /// <param name="freshness">The glucose data freshness.</param>
    /// <returns>The real provider status message.</returns>
    private static string BuildRealProviderMessage(
        string providerDisplayName,
        GlucoseDataFreshness freshness)
    {
        return freshness switch
        {
            GlucoseDataFreshness.Live =>
                $"{providerDisplayName} is the active glucose provider and is returning live data.",

            GlucoseDataFreshness.NearRealTime =>
                $"{providerDisplayName} is the active glucose provider and is returning near real-time data.",

            GlucoseDataFreshness.Delayed =>
                $"{providerDisplayName} is the active glucose provider and is returning delayed data.",

            GlucoseDataFreshness.Historical =>
                $"{providerDisplayName} is the active glucose provider and is returning historical data.",

            _ =>
                $"{providerDisplayName} is the active glucose provider."
        };
    }

    #endregion
}