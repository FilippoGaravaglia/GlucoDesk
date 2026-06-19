using GlucoDesk.Application.Cgm.BackgroundSync.Enums;
using GlucoDesk.Application.Cgm.BackgroundSync.Results;
using GlucoDesk.Application.Cgm.BackgroundSync.State.Services.Abstractions;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Cgm.BackgroundSync.State.Services;

/// <summary>
/// Tracks the observable runtime state of CGM background sync.
/// </summary>
public sealed class BackgroundSyncStateService : IBackgroundSyncStateService
{
    private readonly object _syncRoot = new();

    private BackgroundSyncStateSnapshot _currentSnapshot = BackgroundSyncStateSnapshot.Initial;

    /// <inheritdoc />
    public event EventHandler<BackgroundSyncStateSnapshot>? SnapshotChanged;

    /// <inheritdoc />
    public BackgroundSyncStateSnapshot CurrentSnapshot
    {
        get
        {
            lock (_syncRoot)
            {
                return _currentSnapshot;
            }
        }
    }

    /// <inheritdoc />
    public void MarkStarted(DateTimeOffset timestamp)
    {
        UpdateSnapshot(current => new BackgroundSyncStateSnapshot(
            true,
            current.LastStatus,
            current.LastProviderKind,
            current.LastReadingsCount,
            current.LastAttemptedAt,
            current.LastSucceededAt,
            current.LastStoppedAt,
            "Background sync is running.",
            current.LastErrorMessage));
    }

    /// <inheritdoc />
    public void MarkStopped(DateTimeOffset timestamp)
    {
        UpdateSnapshot(current => new BackgroundSyncStateSnapshot(
            false,
            current.LastStatus,
            current.LastProviderKind,
            current.LastReadingsCount,
            current.LastAttemptedAt,
            current.LastSucceededAt,
            timestamp,
            "Background sync is stopped.",
            current.LastErrorMessage));
    }

    /// <inheritdoc />
    public void RecordIteration(BackgroundSyncIterationResult iterationResult)
    {
        ArgumentNullException.ThrowIfNull(iterationResult);

        var lastSucceededAt = iterationResult.Status == BackgroundSyncStatus.Succeeded
            ? iterationResult.SyncedAt
            : CurrentSnapshot.LastSucceededAt;

        var lastErrorMessage = iterationResult.Status is BackgroundSyncStatus.ProviderFailed or BackgroundSyncStatus.Failed
            ? iterationResult.Message
            : null;

        UpdateSnapshot(current => new BackgroundSyncStateSnapshot(
            current.IsRunning,
            iterationResult.Status,
            iterationResult.ProviderKind,
            iterationResult.ReadingsCount,
            iterationResult.SyncedAt,
            lastSucceededAt,
            current.LastStoppedAt,
            iterationResult.Message,
            lastErrorMessage));
    }

    /// <inheritdoc />
    public void RecordFailure(DateTimeOffset attemptedAt, string message)
    {
        var statusMessage = string.IsNullOrWhiteSpace(message)
            ? "Background sync failed unexpectedly."
            : message;

        UpdateSnapshot(current => new BackgroundSyncStateSnapshot(
            current.IsRunning,
            BackgroundSyncStatus.Failed,
            CgmProviderKind.Unknown,
            0,
            attemptedAt,
            current.LastSucceededAt,
            current.LastStoppedAt,
            statusMessage,
            statusMessage));
    }

    #region Helpers

    /// <summary>
    /// Updates the current snapshot and notifies listeners.
    /// </summary>
    /// <param name="updateFactory">The snapshot update factory.</param>
    private void UpdateSnapshot(Func<BackgroundSyncStateSnapshot, BackgroundSyncStateSnapshot> updateFactory)
    {
        BackgroundSyncStateSnapshot updatedSnapshot;

        lock (_syncRoot)
        {
            updatedSnapshot = updateFactory(_currentSnapshot);
            _currentSnapshot = updatedSnapshot;
        }

        SnapshotChanged?.Invoke(this, updatedSnapshot);
    }

    #endregion
}