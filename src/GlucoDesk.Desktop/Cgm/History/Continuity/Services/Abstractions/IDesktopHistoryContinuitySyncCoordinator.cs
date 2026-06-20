using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.Cgm.History.Continuity.Results;

namespace GlucoDesk.Desktop.Cgm.History.Continuity.Services.Abstractions;

/// <summary>
/// Coordinates desktop history continuity synchronization runs.
/// </summary>
public interface IDesktopHistoryContinuitySyncCoordinator
{
    /// <summary>
    /// Runs a startup desktop history continuity synchronization.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The desktop history continuity synchronization run result.</returns>
    Task<Result<DesktopHistoryContinuitySyncRunResult>> RunStartupSyncAsync(
    CancellationToken cancellationToken);
    
    /// <summary>
    /// Runs a resume desktop history continuity synchronization.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The desktop history continuity synchronization run result.</returns>
    Task<Result<DesktopHistoryContinuitySyncRunResult>> RunResumeSyncAsync(
        CancellationToken cancellationToken);
    
    /// <summary>
    /// Runs a manual desktop history continuity synchronization.
    /// </summary>
    /// <param name="lookback">The manual synchronization lookback window.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The desktop history continuity synchronization run result.</returns>
    Task<Result<DesktopHistoryContinuitySyncRunResult>> RunManualSyncAsync(
        TimeSpan lookback,
        CancellationToken cancellationToken);
}
