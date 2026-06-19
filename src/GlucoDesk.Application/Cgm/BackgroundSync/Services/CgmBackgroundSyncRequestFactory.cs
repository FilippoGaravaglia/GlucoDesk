using GlucoDesk.Application.Cgm.Dashboard.Requests;

namespace GlucoDesk.Application.Cgm.BackgroundSync.Services;

/// <summary>
/// Creates CGM background sync requests.
/// </summary>
public static class CgmBackgroundSyncRequestFactory
{
    /// <summary>
    /// Creates the default dashboard request used by background sync.
    /// </summary>
    /// <returns>The dashboard request.</returns>
    public static GlucoseDashboardRequest CreateDefaultDashboardRequest()
    {
        return new GlucoseDashboardRequest(
            TimeSpan.FromHours(24),
            TimeSpan.FromMinutes(15));
    }
}