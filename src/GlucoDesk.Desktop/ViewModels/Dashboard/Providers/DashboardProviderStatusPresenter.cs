using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.Localization;
using System.Globalization;

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
                    T("DashboardMockProviderMessage"),
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
                    T("DashboardDexcomSandboxMessage"),
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
                    T("DashboardProviderStatusUnknown"),
                    T("DashboardProviderStatusUnknownMessage"),
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
                string.Format(CultureInfo.InvariantCulture, T("DashboardProviderConnectedLive"), providerDisplayName),

            GlucoseDataFreshness.NearRealTime =>
                string.Format(CultureInfo.InvariantCulture, T("DashboardProviderConnectedNearRealTime"), providerDisplayName),

            GlucoseDataFreshness.Delayed =>
                string.Format(CultureInfo.InvariantCulture, T("DashboardProviderConnectedDelayed"), providerDisplayName),

            GlucoseDataFreshness.Historical =>
                string.Format(CultureInfo.InvariantCulture, T("DashboardProviderConnectedHistorical"), providerDisplayName),

            _ =>
                string.Format(CultureInfo.InvariantCulture, T("DashboardProviderConnectedGeneric"), providerDisplayName)
        };
    }

    #endregion
    private static string T(string key)
    {
        return LocalizationManager.GetString(key);
    }

}