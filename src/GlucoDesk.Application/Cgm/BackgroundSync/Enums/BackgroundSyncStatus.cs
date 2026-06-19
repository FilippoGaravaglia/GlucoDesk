namespace GlucoDesk.Application.Cgm.BackgroundSync.Enums;

/// <summary>
/// Represents the outcome status of a CGM background sync iteration.
/// </summary>
public enum BackgroundSyncStatus
{
    /// <summary>
    /// The sync status is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The sync completed successfully.
    /// </summary>
    Succeeded = 1,

    /// <summary>
    /// The sync completed but no glucose readings were available.
    /// </summary>
    NoData = 2,

    /// <summary>
    /// The sync failed because the provider returned an error.
    /// </summary>
    ProviderFailed = 3,

    /// <summary>
    /// The sync was skipped because another sync is already running.
    /// </summary>
    SkippedAlreadyRunning = 4,

    /// <summary>
    /// The sync failed unexpectedly.
    /// </summary>
    Failed = 5
}