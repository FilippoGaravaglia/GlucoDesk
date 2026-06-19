using GlucoDesk.Application.Cgm.BackgroundSync.Enums;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Cgm.BackgroundSync.Results;

/// <summary>
/// Represents the result of a single CGM background sync iteration.
/// </summary>
public sealed record BackgroundSyncIterationResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundSyncIterationResult"/> class.
    /// </summary>
    /// <param name="status">The sync status.</param>
    /// <param name="providerKind">The provider kind.</param>
    /// <param name="readingsCount">The number of readings returned by the provider.</param>
    /// <param name="syncedAt">The sync timestamp.</param>
    /// <param name="message">The sync message.</param>
    public BackgroundSyncIterationResult(
        BackgroundSyncStatus status,
        CgmProviderKind providerKind,
        int readingsCount,
        DateTimeOffset syncedAt,
        string message)
    {
        if (readingsCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(readingsCount),
                readingsCount,
                "Readings count cannot be negative.");
        }

        if (string.IsNullOrWhiteSpace(message))
        {
            throw new ArgumentException(
                "Message cannot be empty.",
                nameof(message));
        }

        Status = status;
        ProviderKind = providerKind;
        ReadingsCount = readingsCount;
        SyncedAt = syncedAt;
        Message = message;
    }

    /// <summary>
    /// Gets the sync status.
    /// </summary>
    public BackgroundSyncStatus Status { get; }

    /// <summary>
    /// Gets the provider kind.
    /// </summary>
    public CgmProviderKind ProviderKind { get; }

    /// <summary>
    /// Gets the number of readings returned by the provider.
    /// </summary>
    public int ReadingsCount { get; }

    /// <summary>
    /// Gets the sync timestamp.
    /// </summary>
    public DateTimeOffset SyncedAt { get; }

    /// <summary>
    /// Gets the sync message.
    /// </summary>
    public string Message { get; }

    /// <summary>
    /// Creates a successful sync iteration result.
    /// </summary>
    /// <param name="providerKind">The provider kind.</param>
    /// <param name="readingsCount">The readings count.</param>
    /// <param name="syncedAt">The sync timestamp.</param>
    /// <returns>The sync iteration result.</returns>
    public static BackgroundSyncIterationResult Succeeded(
        CgmProviderKind providerKind,
        int readingsCount,
        DateTimeOffset syncedAt)
    {
        return new BackgroundSyncIterationResult(
            BackgroundSyncStatus.Succeeded,
            providerKind,
            readingsCount,
            syncedAt,
            "Background sync completed.");
    }

    /// <summary>
    /// Creates a no-data sync iteration result.
    /// </summary>
    /// <param name="providerKind">The provider kind.</param>
    /// <param name="syncedAt">The sync timestamp.</param>
    /// <returns>The sync iteration result.</returns>
    public static BackgroundSyncIterationResult NoData(
        CgmProviderKind providerKind,
        DateTimeOffset syncedAt)
    {
        return new BackgroundSyncIterationResult(
            BackgroundSyncStatus.NoData,
            providerKind,
            0,
            syncedAt,
            "Background sync completed with no glucose readings.");
    }

    /// <summary>
    /// Creates an already-running skipped sync iteration result.
    /// </summary>
    /// <param name="syncedAt">The sync timestamp.</param>
    /// <returns>The sync iteration result.</returns>
    public static BackgroundSyncIterationResult SkippedAlreadyRunning(DateTimeOffset syncedAt)
    {
        return new BackgroundSyncIterationResult(
            BackgroundSyncStatus.SkippedAlreadyRunning,
            CgmProviderKind.Unknown,
            0,
            syncedAt,
            "Background sync skipped because another sync is already running.");
    }

    /// <summary>
    /// Creates a provider-failed sync iteration result.
    /// </summary>
    /// <param name="syncedAt">The sync timestamp.</param>
    /// <param name="message">The failure message.</param>
    /// <returns>The sync iteration result.</returns>
    public static BackgroundSyncIterationResult ProviderFailed(
        DateTimeOffset syncedAt,
        string message)
    {
        return new BackgroundSyncIterationResult(
            BackgroundSyncStatus.ProviderFailed,
            CgmProviderKind.Unknown,
            0,
            syncedAt,
            string.IsNullOrWhiteSpace(message)
                ? "Background sync provider failed."
                : message);
    }

    /// <summary>
    /// Creates an unexpected failed sync iteration result.
    /// </summary>
    /// <param name="syncedAt">The sync timestamp.</param>
    /// <returns>The sync iteration result.</returns>
    public static BackgroundSyncIterationResult Failed(DateTimeOffset syncedAt)
    {
        return new BackgroundSyncIterationResult(
            BackgroundSyncStatus.Failed,
            CgmProviderKind.Unknown,
            0,
            syncedAt,
            "Background sync failed unexpectedly.");
    }
}