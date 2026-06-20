using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Desktop.Cgm.History.Continuity.Enums;
using GlucoDesk.Desktop.Cgm.History.Continuity.Results;
using GlucoDesk.Desktop.Cgm.History.Continuity.Services.Abstractions;

namespace GlucoDesk.Desktop.Cgm.History.Continuity.Services;

/// <summary>
/// Thread-safe desktop status store for history continuity synchronization.
/// </summary>
public sealed class DesktopHistoryContinuitySyncStatusStore : IDesktopHistoryContinuitySyncStatusStore
{
    private readonly object _syncRoot = new();
    private readonly TimeProvider _timeProvider;

    private DesktopHistoryContinuitySyncStatusSnapshot _current =
        DesktopHistoryContinuitySyncStatusSnapshot.Idle;

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopHistoryContinuitySyncStatusStore"/> class.
    /// </summary>
    /// <param name="timeProvider">The time provider.</param>
    /// <exception cref="ArgumentNullException">Thrown when timeProvider is null.</exception>
    public DesktopHistoryContinuitySyncStatusStore(TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(timeProvider);

        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public event EventHandler<DesktopHistoryContinuitySyncStatusSnapshot>? StatusChanged;

    /// <inheritdoc />
    public DesktopHistoryContinuitySyncStatusSnapshot Current
    {
        get
        {
            lock (_syncRoot)
            {
                return _current;
            }
        }
    }

    /// <inheritdoc />
    public void MarkRunning(CgmHistoryContinuitySyncTrigger trigger)
    {
        var current = Current;
        var now = GetUtcNow();

        Update(new DesktopHistoryContinuitySyncStatusSnapshot(
            DesktopHistoryContinuitySyncRunState.Running,
            trigger,
            StartedAtUtc: now,
            CompletedAtUtc: null,
            current.LastSuccessfulSyncAtUtc,
            Message: $"History continuity synchronization started by {trigger}.",
            ErrorCode: null,
            ErrorDescription: null,
            TotalFetchedReadings: 0,
            AddedReadingsCount: 0,
            DuplicateReadingsCount: 0,
            StoredReadingsCount: 0,
            HasNewReadings: false));
    }

    /// <inheritdoc />
    public void MarkSucceeded(
        CgmHistoryContinuitySyncTrigger trigger,
        DesktopHistoryContinuitySyncRunResult runResult)
    {
        ArgumentNullException.ThrowIfNull(runResult);

        var current = Current;
        var now = GetUtcNow();
        var continuitySync = runResult.ContinuitySync;

        Update(new DesktopHistoryContinuitySyncStatusSnapshot(
            DesktopHistoryContinuitySyncRunState.Succeeded,
            trigger,
            ResolveStartedAt(current, trigger),
            CompletedAtUtc: now,
            LastSuccessfulSyncAtUtc: now,
            Message: NormalizeMessage(
                runResult.Message,
                "History continuity synchronization completed successfully."),
            ErrorCode: null,
            ErrorDescription: null,
            TotalFetchedReadings: continuitySync?.TotalFetchedReadings ?? 0,
            AddedReadingsCount: continuitySync?.AddedReadingsCount ?? 0,
            DuplicateReadingsCount: continuitySync?.DuplicateReadingsCount ?? 0,
            StoredReadingsCount: continuitySync?.StoredReadingsCount ?? 0,
            HasNewReadings: continuitySync?.HasNewReadings ?? false));
    }

    /// <inheritdoc />
    public void MarkSkipped(
        CgmHistoryContinuitySyncTrigger trigger,
        string message)
    {
        var current = Current;
        var now = GetUtcNow();

        Update(new DesktopHistoryContinuitySyncStatusSnapshot(
            DesktopHistoryContinuitySyncRunState.Skipped,
            trigger,
            StartedAtUtc: null,
            CompletedAtUtc: now,
            current.LastSuccessfulSyncAtUtc,
            Message: NormalizeMessage(
                message,
                "History continuity synchronization was skipped."),
            ErrorCode: null,
            ErrorDescription: null,
            TotalFetchedReadings: 0,
            AddedReadingsCount: 0,
            DuplicateReadingsCount: 0,
            StoredReadingsCount: 0,
            HasNewReadings: false));
    }

    /// <inheritdoc />
    public void MarkFailed(
        CgmHistoryContinuitySyncTrigger trigger,
        Error error)
    {
        var current = Current;
        var now = GetUtcNow();

        Update(new DesktopHistoryContinuitySyncStatusSnapshot(
            DesktopHistoryContinuitySyncRunState.Failed,
            trigger,
            ResolveStartedAt(current, trigger),
            CompletedAtUtc: now,
            current.LastSuccessfulSyncAtUtc,
            Message: NormalizeMessage(
                error.Message,
                "History continuity synchronization failed."),
            ErrorCode: error.Code,
            ErrorDescription: error.Message,
            TotalFetchedReadings: 0,
            AddedReadingsCount: 0,
            DuplicateReadingsCount: 0,
            StoredReadingsCount: 0,
            HasNewReadings: false));
    }

    /// <inheritdoc />
    public void MarkCanceled(CgmHistoryContinuitySyncTrigger trigger)
    {
        var current = Current;
        var now = GetUtcNow();

        Update(new DesktopHistoryContinuitySyncStatusSnapshot(
            DesktopHistoryContinuitySyncRunState.Canceled,
            trigger,
            ResolveStartedAt(current, trigger),
            CompletedAtUtc: now,
            current.LastSuccessfulSyncAtUtc,
            Message: "History continuity synchronization was canceled.",
            ErrorCode: null,
            ErrorDescription: null,
            TotalFetchedReadings: 0,
            AddedReadingsCount: 0,
            DuplicateReadingsCount: 0,
            StoredReadingsCount: 0,
            HasNewReadings: false));
    }

    #region Helpers

    /// <summary>
    /// Updates the current status snapshot and notifies subscribers.
    /// </summary>
    /// <param name="snapshot">The new status snapshot.</param>
    private void Update(DesktopHistoryContinuitySyncStatusSnapshot snapshot)
    {
        EventHandler<DesktopHistoryContinuitySyncStatusSnapshot>? handler;

        lock (_syncRoot)
        {
            _current = snapshot;
            handler = StatusChanged;
        }

        handler?.Invoke(this, snapshot);
    }

    /// <summary>
    /// Gets the current UTC timestamp.
    /// </summary>
    /// <returns>The current UTC timestamp.</returns>
    private DateTimeOffset GetUtcNow()
    {
        return _timeProvider.GetUtcNow();
    }

    /// <summary>
    /// Resolves the original start timestamp for the current trigger, when available.
    /// </summary>
    /// <param name="current">The current status snapshot.</param>
    /// <param name="trigger">The synchronization trigger.</param>
    /// <returns>The current start timestamp when the trigger matches; otherwise null.</returns>
    private static DateTimeOffset? ResolveStartedAt(
        DesktopHistoryContinuitySyncStatusSnapshot current,
        CgmHistoryContinuitySyncTrigger trigger)
    {
        return current.State == DesktopHistoryContinuitySyncRunState.Running &&
               current.Trigger == trigger
            ? current.StartedAtUtc
            : null;
    }

    /// <summary>
    /// Normalizes a status message using a fallback when the provided message is empty.
    /// </summary>
    /// <param name="message">The message to normalize.</param>
    /// <param name="fallback">The fallback message.</param>
    /// <returns>The normalized message.</returns>
    private static string NormalizeMessage(
        string? message,
        string fallback)
    {
        return string.IsNullOrWhiteSpace(message)
            ? fallback
            : message;
    }

    #endregion
}