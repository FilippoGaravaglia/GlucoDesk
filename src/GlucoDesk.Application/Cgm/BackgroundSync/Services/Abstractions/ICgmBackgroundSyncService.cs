using GlucoDesk.Application.Cgm.BackgroundSync.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.BackgroundSync.Services.Abstractions;

/// <summary>
/// Defines operations for the in-app CGM background sync foundation.
/// </summary>
public interface ICgmBackgroundSyncService
{
    /// <summary>
    /// Runs a single background sync iteration.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The background sync iteration result.</returns>
    Task<Result<BackgroundSyncIterationResult>> RunOnceAsync(
        CancellationToken cancellationToken);
}