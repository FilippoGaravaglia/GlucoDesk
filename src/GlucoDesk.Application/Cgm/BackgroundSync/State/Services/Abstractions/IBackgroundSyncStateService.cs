using GlucoDesk.Application.Cgm.BackgroundSync.Results;

namespace GlucoDesk.Application.Cgm.BackgroundSync.State.Services.Abstractions;

/// <summary>
/// Defines operations for tracking the observable runtime state of CGM background sync.
/// </summary>
public interface IBackgroundSyncStateService
{
    /// <summary>
    /// Occurs when the background sync state changes.
    /// </summary>
    event EventHandler<BackgroundSyncStateSnapshot>? SnapshotChanged;

    /// <summary>
    /// Gets the current background sync state snapshot.
    /// </summary>
    BackgroundSyncStateSnapshot CurrentSnapshot { get; }

    /// <summary>
    /// Marks the background sync loop as started.
    /// </summary>
    /// <param name="timestamp">The timestamp.</param>
    void MarkStarted(DateTimeOffset timestamp);

    /// <summary>
    /// Marks the background sync loop as stopped.
    /// </summary>
    /// <param name="timestamp">The timestamp.</param>
    void MarkStopped(DateTimeOffset timestamp);

    /// <summary>
    /// Records a completed background sync iteration.
    /// </summary>
    /// <param name="iterationResult">The iteration result.</param>
    void RecordIteration(BackgroundSyncIterationResult iterationResult);

    /// <summary>
    /// Records an unexpected background sync failure.
    /// </summary>
    /// <param name="attemptedAt">The attempted sync timestamp.</param>
    /// <param name="message">The failure message.</param>
    void RecordFailure(DateTimeOffset attemptedAt, string message);
}