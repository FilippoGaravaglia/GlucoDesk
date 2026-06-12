using GlucoDesk.Application.Common.Errors;

namespace GlucoDesk.Desktop.ViewModels.Dashboard.Errors;

/// <summary>
/// Maps application refresh errors to user-facing dashboard messages.
/// </summary>
public static class DashboardRefreshErrorPresenter
{
    /// <summary>
    /// Creates a user-facing presentation for a dashboard refresh error.
    /// </summary>
    /// <param name="error">The application error.</param>
    /// <returns>The dashboard refresh error presentation.</returns>
    public static DashboardRefreshErrorPresentation Present(Error error)
    {
        ArgumentNullException.ThrowIfNull(error);

        return error.Code switch
        {
            "Dexcom.TokenStoreEmpty" => new DashboardRefreshErrorPresentation(
                "Dexcom is not connected",
                "Dexcom is selected as provider, but no active Dexcom connection is available. Open Settings and connect Dexcom again.",
                error.Code),

            "Dexcom.TokenRefreshFailed" => new DashboardRefreshErrorPresentation(
                "Dexcom token refresh failed",
                "GlucoDesk could not refresh the Dexcom session. Reconnect Dexcom from Settings and try again.",
                error.Code),

            "Dexcom.TokenRequestFailed" => new DashboardRefreshErrorPresentation(
                "Dexcom token request failed",
                "GlucoDesk could not reach the Dexcom token service. Check your internet connection and try again.",
                error.Code),

            "Dexcom.TokenRequestTimeout" => new DashboardRefreshErrorPresentation(
                "Dexcom token request timed out",
                "The Dexcom token request took too long. Check your connection and try again.",
                error.Code),

            "Dexcom.ProviderClientSecretMissing" => new DashboardRefreshErrorPresentation(
                "Dexcom configuration incomplete",
                "Dexcom is selected, but the desktop runtime is missing the client secret. Check the Dexcom environment variables and restart GlucoDesk.",
                error.Code),

            "Dexcom.EgvUnauthorized" => new DashboardRefreshErrorPresentation(
                "Dexcom authorization required",
                "Dexcom rejected the current authorization. Reconnect Dexcom from Settings and try again.",
                error.Code),

            "Dexcom.EgvForbidden" => new DashboardRefreshErrorPresentation(
                "Dexcom access denied",
                "Dexcom denied access to glucose data for the current authorization. Reconnect Dexcom and verify the selected account.",
                error.Code),

            "Dexcom.EgvRateLimited" => new DashboardRefreshErrorPresentation(
                "Dexcom rate limit reached",
                "Dexcom is temporarily rate limiting requests. Wait a few minutes before refreshing again.",
                error.Code),

            "Dexcom.EgvServerUnavailable" => new DashboardRefreshErrorPresentation(
                "Dexcom temporarily unavailable",
                "Dexcom is currently unavailable or returned a server error. Try again later.",
                error.Code),

            "Dexcom.EgvNetworkError" => new DashboardRefreshErrorPresentation(
                "Dexcom network error",
                "GlucoDesk could not complete the Dexcom glucose request due to a network problem.",
                error.Code),

            "Dexcom.EgvRequestTimeout" => new DashboardRefreshErrorPresentation(
                "Dexcom request timed out",
                "The Dexcom glucose request took too long. Try refreshing again.",
                error.Code),

            "Dexcom.EgvInvalidResponse"
                or "Dexcom.EgvResponseNull"
                or "Dexcom.EgvRecordsMissing"
                or "Dexcom.EgvRecordNull"
                or "Dexcom.EgvMissingSystemTime"
                or "Dexcom.EgvInvalidSystemTime"
                or "Dexcom.EgvMissingValue"
                or "Dexcom.EgvInvalidValue"
                or "Dexcom.EgvUnsupportedUnit" => new DashboardRefreshErrorPresentation(
                    "Dexcom returned unreadable data",
                    "Dexcom returned glucose data that GlucoDesk could not process safely.",
                    error.Code),

            "Nightscout.EntriesUnauthorized" => new DashboardRefreshErrorPresentation(
                "Nightscout authorization required",
                "Nightscout rejected the current authorization. Check the configured Nightscout secret or access token.",
                error.Code),

            "Nightscout.EntriesForbidden" => new DashboardRefreshErrorPresentation(
                "Nightscout access denied",
                "Nightscout denied access to glucose entries. Verify the configured Nightscout permissions.",
                error.Code),

            "Nightscout.EntriesRateLimited" => new DashboardRefreshErrorPresentation(
                "Nightscout rate limit reached",
                "Nightscout is temporarily rate limiting requests. Wait a few minutes before refreshing again.",
                error.Code),

            "Nightscout.EntriesServerUnavailable" => new DashboardRefreshErrorPresentation(
                "Nightscout temporarily unavailable",
                "Nightscout is currently unavailable or returned a server error. Try again later.",
                error.Code),

            "Nightscout.EntriesNetworkError" => new DashboardRefreshErrorPresentation(
                "Nightscout network error",
                "GlucoDesk could not complete the Nightscout entries request due to a network problem.",
                error.Code),

            "Nightscout.EntriesRequestTimeout" => new DashboardRefreshErrorPresentation(
                "Nightscout request timed out",
                "The Nightscout entries request took too long. Try refreshing again.",
                error.Code),

            "Nightscout.EntriesInvalidResponse"
                or "Nightscout.EntriesNull"
                or "Nightscout.EntryNull"
                or "Nightscout.EntryMissingTimestamp"
                or "Nightscout.EntryInvalidDate"
                or "Nightscout.EntryMissingSgv"
                or "Nightscout.EntryInvalidSgv" => new DashboardRefreshErrorPresentation(
                    "Nightscout returned unreadable data",
                    "Nightscout returned glucose data that GlucoDesk could not process safely.",
                    error.Code),

            "Cgm.LiveProviderUnavailable"
                or "Cgm.LiveProviderNotFound"
                or "Cgm.HistoricalProviderUnavailable"
                or "Cgm.HistoricalProviderNotFound" => new DashboardRefreshErrorPresentation(
                    "Selected provider unavailable",
                    "The selected CGM provider is not available in the current desktop runtime. Open Settings and select an available provider.",
                    error.Code),

            _ when IsDexcomError(error.Code) => new DashboardRefreshErrorPresentation(
                "Dexcom refresh failed",
                "GlucoDesk could not refresh Dexcom glucose data. Check the Dexcom connection in Settings and try again.",
                error.Code),

            _ when IsNightscoutError(error.Code) => new DashboardRefreshErrorPresentation(
                "Nightscout refresh failed",
                "GlucoDesk could not refresh Nightscout glucose data. Check the Nightscout configuration and try again.",
                error.Code),

            _ => new DashboardRefreshErrorPresentation(
                "Unable to refresh glucose data",
                error.Message,
                error.Code)
        };
    }

    #region Helpers

    /// <summary>
    /// Checks whether the supplied error code belongs to the Dexcom integration.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <returns>True when the error is a Dexcom error; otherwise false.</returns>
    private static bool IsDexcomError(string errorCode)
    {
        return errorCode.StartsWith("Dexcom.", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Checks whether the supplied error code belongs to the Nightscout integration.
    /// </summary>
    /// <param name="errorCode">The error code.</param>
    /// <returns>True when the error is a Nightscout error; otherwise false.</returns>
    private static bool IsNightscoutError(string errorCode)
    {
        return errorCode.StartsWith("Nightscout.", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}