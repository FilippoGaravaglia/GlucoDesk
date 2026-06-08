using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Browsers;

/// <summary>
/// Defines system browser opening behavior for Dexcom OAuth authorization.
/// </summary>
public interface IDexcomSystemBrowser
{
    /// <summary>
    /// Opens the specified authorization URI in the system browser.
    /// </summary>
    /// <param name="authorizationUri">The Dexcom OAuth authorization URI.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The opened authorization URI.</returns>
    Task<Result<Uri>> OpenAsync(
        Uri authorizationUri,
        CancellationToken cancellationToken);
}