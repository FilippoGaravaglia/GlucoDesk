using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Desktop.BackgroundSync.Services.Abstractions;

/// <summary>
/// Defines desktop lifecycle operations for the in-app background sync loop.
/// </summary>
public interface IDesktopBackgroundSyncLifecycleService
{
    /// <summary>
    /// Starts the desktop background sync lifecycle.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The start operation result.</returns>
    Task<Result> StartAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Stops the desktop background sync lifecycle.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stop operation result.</returns>
    Task<Result> StopAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Gets a value indicating whether the background sync loop is running.
    /// </summary>
    bool IsRunning { get; }
}