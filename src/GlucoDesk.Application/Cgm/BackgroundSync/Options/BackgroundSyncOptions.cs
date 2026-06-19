namespace GlucoDesk.Application.Cgm.BackgroundSync.Options;

/// <summary>
/// Provides options for the in-app CGM background sync loop.
/// </summary>
public sealed record BackgroundSyncOptions
{
    /// <summary>
    /// Gets the default sync interval.
    /// </summary>
    public static readonly TimeSpan DefaultSyncInterval = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets the default background sync options.
    /// </summary>
    public static BackgroundSyncOptions Default => new(
        DefaultSyncInterval,
        true,
        true);

    /// <summary>
    /// Initializes a new instance of the <see cref="BackgroundSyncOptions"/> class.
    /// </summary>
    /// <param name="syncInterval">The sync interval.</param>
    /// <param name="persistHistory">A value indicating whether readings should be persisted to local history.</param>
    /// <param name="publishWidgetState">A value indicating whether widget state should be published.</param>
    public BackgroundSyncOptions(
        TimeSpan syncInterval,
        bool persistHistory,
        bool publishWidgetState)
    {
        if (syncInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(syncInterval),
                syncInterval,
                "Sync interval must be greater than zero.");
        }

        SyncInterval = syncInterval;
        PersistHistory = persistHistory;
        PublishWidgetState = publishWidgetState;
    }

    /// <summary>
    /// Gets the sync interval.
    /// </summary>
    public TimeSpan SyncInterval { get; }

    /// <summary>
    /// Gets a value indicating whether readings should be persisted to local history.
    /// </summary>
    public bool PersistHistory { get; }

    /// <summary>
    /// Gets a value indicating whether widget state should be published.
    /// </summary>
    public bool PublishWidgetState { get; }
}