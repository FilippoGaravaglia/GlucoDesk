using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Desktop.ViewModels.Dashboard.Providers;

/// <summary>
/// Builds user-facing dashboard provider status messages.
/// </summary>
public static class DashboardProviderStatusPresenter
{
    /// <summary>
    /// Creates a provider status presentation for the supplied provider and freshness.
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
            CgmProviderKind.Mock => new DashboardProviderStatusPresentation(
                "Using Mock data",
                "Mock is the active live provider. Configure and select Dexcom or Nightscout in Settings to use real glucose data.",
                "Mock",
                false,
                true),

            CgmProviderKind.Nightscout => new DashboardProviderStatusPresentation(
                "Using Nightscout",
                BuildNightscoutMessage(freshness),
                "Nightscout",
                true,
                false),

            CgmProviderKind.DexcomSandbox => new DashboardProviderStatusPresentation(
                "Using Dexcom Sandbox",
                "Dexcom Sandbox is active. Data is simulated and intended for integration testing, not real glucose monitoring.",
                "Dexcom Sandbox",
                true,
                false),

            CgmProviderKind.DexcomOfficial => new DashboardProviderStatusPresentation(
                "Using Dexcom",
                BuildDexcomMessage(freshness),
                "Dexcom",
                true,
                false),

            _ => new DashboardProviderStatusPresentation(
                "Provider status unknown",
                "The active glucose data provider could not be identified.",
                "Unknown",
                false,
                false)
        };
    }

    #region Helpers

    /// <summary>
    /// Builds the Nightscout provider message for the supplied data freshness.
    /// </summary>
    /// <param name="freshness">The glucose data freshness.</param>
    /// <returns>The Nightscout provider message.</returns>
    private static string BuildNightscoutMessage(GlucoseDataFreshness freshness)
    {
        return freshness switch
        {
            GlucoseDataFreshness.NearRealTime =>
                "Nightscout is active and is expected to provide near real-time glucose entries from the configured Nightscout instance.",

            GlucoseDataFreshness.Live =>
                "Nightscout is active and is providing live glucose data from the configured Nightscout instance.",

            GlucoseDataFreshness.Historical =>
                "Nightscout is active and is currently showing historical glucose data.",

            GlucoseDataFreshness.Delayed =>
                "Nightscout is active, but the current reading is marked as delayed.",

            _ =>
                "Nightscout is active. Check the latest refresh status to verify whether data is currently available."
        };
    }

    /// <summary>
    /// Builds the Dexcom provider message for the supplied data freshness.
    /// </summary>
    /// <param name="freshness">The glucose data freshness.</param>
    /// <returns>The Dexcom provider message.</returns>
    private static string BuildDexcomMessage(GlucoseDataFreshness freshness)
    {
        return freshness switch
        {
            GlucoseDataFreshness.Delayed =>
                "Dexcom Official API is active. Data is official but may be delayed and should not be treated as real-time monitoring.",

            GlucoseDataFreshness.Historical =>
                "Dexcom Official API is active and is currently showing historical glucose data.",

            GlucoseDataFreshness.NearRealTime =>
                "Dexcom is active. The current reading is marked as near real-time, but GlucoDesk is not a medical monitoring device.",

            GlucoseDataFreshness.Live =>
                "Dexcom is active. GlucoDesk is showing provider data but must not be used for treatment decisions.",

            _ =>
                "Dexcom is active. Check the latest refresh status to verify whether data is currently available."
        };
    }

    #endregion
}