using GlucoDesk.Application.Cgm.BackgroundSync.Enums;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Cgm.BackgroundSync.State;

/// <summary>
/// Represents the observable runtime state of the CGM background sync.
/// </summary>
public sealed record BackgroundSyncStateSnapshot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundSyncStateSnapshot"/> class.
    /// </summary>
    /// <param name="isRunning">A value indicating whether the sync loop is running.</param>
    /// <param name="lastStatus">The last sync status.</param>
    /// <param name="lastProviderKind">The last provider kind.</param>
    /// <param name="lastReadingsCount">The last synced readings count.</param>
    /// <param name="lastAttemptedAt">The last attempted sync timestamp.</param>
    /// <param name="lastSucceededAt">The last successful sync timestamp.</param>
    /// <param name="lastStoppedAt">The last stopped timestamp.</param>
    /// <param name="statusMessage">The status message.</param>
    /// <param name="lastErrorMessage">The last error message.</param>
    public BackgroundSyncStateSnapshot(
        bool isRunning,
        BackgroundSyncStatus lastStatus,
        CgmProviderKind lastProviderKind,
        int lastReadingsCount,
        DateTimeOffset? lastAttemptedAt,
        DateTimeOffset? lastSucceededAt,
        DateTimeOffset? lastStoppedAt,
        string statusMessage,
        string? lastErrorMessage)
    {
        if (lastReadingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lastReadingsCount),
                lastReadingsCount,
                "Last readings count cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(statusMessage))
        {
            throw new ArgumentException(
                "Status message cannot be empty.",
                nameof(statusMessage));
        }

        IsRunning = isRunning;
        LastStatus = lastStatus;
        LastProviderKind = lastProviderKind;
        LastReadingsCount = lastReadingsCount;
        LastAttemptedAt = lastAttemptedAt;
        LastSucceededAt = lastSucceededAt;
        LastStoppedAt = lastStoppedAt;
        StatusMessage = statusMessage;
        LastErrorMessage = lastErrorMessage;
    }

    /// <summary>
    /// Gets a value indicating whether the sync loop is running.
    /// </summary>
    public bool IsRunning { get; }

    /// <summary>
    /// Gets the last sync status.
    /// </summary>
    public BackgroundSyncStatus LastStatus { get; }

    /// <summary>
    /// Gets the last provider kind.
    /// </summary>
    public CgmProviderKind LastProviderKind { get; }

    /// <summary>
    /// Gets the last synced readings count.
    /// </summary>
    public int LastReadingsCount { get; }

    /// <summary>
    /// Gets the last attempted sync timestamp.
    /// </summary>
    public DateTimeOffset? LastAttemptedAt { get; }

    /// <summary>
    /// Gets the last successful sync timestamp.
    /// </summary>
    public DateTimeOffset? LastSucceededAt { get; }

    /// <summary>
    /// Gets the last stopped timestamp.
    /// </summary>
    public DateTimeOffset? LastStoppedAt { get; }

    /// <summary>
    /// Gets the status message.
    /// </summary>
    public string StatusMessage { get; }

    /// <summary>
    /// Gets the last error message.
    /// </summary>
    public string? LastErrorMessage { get; }

    /// <summary>
    /// Gets the initial background sync state.
    /// </summary>
    public static BackgroundSyncStateSnapshot Initial => new(
        false,
        BackgroundSyncStatus.Unknown,
        CgmProviderKind.Unknown,
        0,
        null,
        null,
        null,
        "Background sync has not started yet.",
        null);
}