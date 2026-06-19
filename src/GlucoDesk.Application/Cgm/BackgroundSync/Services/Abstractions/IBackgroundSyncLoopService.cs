using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.BackgroundSync.Services.Abstractions;

/// <summary>
/// Defines lifecycle operations for the in-app CGM background sync loop.
/// </summary>
public interface IBackgroundSyncLoopService
{
    /// <summary>
    /// Starts the background sync loop.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The start operation result.</returns>
    Task<Result> StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the background sync loop.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stop operation result.</returns>
    Task<Result> StopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a value indicating whether the background sync loop is currently running.
    /// </summary>
    bool IsRunning { get; }
}