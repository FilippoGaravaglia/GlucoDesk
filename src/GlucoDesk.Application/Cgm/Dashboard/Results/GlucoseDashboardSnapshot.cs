using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.Dashboard.Results;

/// <summary>
/// Represents the data required by the glucose dashboard.
/// </summary>
public sealed record GlucoseDashboardSnapshot
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseDashboardSnapshot"/> class.
    /// </summary>
    /// <param name="metadata">The active provider metadata.</param>
    /// <param name="latestReading">The latest glucose reading, when available.</param>
    /// <param name="recentReadings">The recent glucose readings.</param>
    /// <param name="latestReadingRetrievedAt">The timestamp when the latest reading was retrieved.</param>
    /// <param name="recentReadingsRetrievedAt">The timestamp when recent readings were retrieved.</param>
    /// <param name="snapshotCreatedAt">The timestamp when the dashboard snapshot was created.</param>
    /// <param name="staleThreshold">The maximum age allowed before the latest reading is considered stale.</param>
    /// <exception cref="ArgumentNullException">Thrown when metadata or recent readings are null.</exception>
    /// <exception cref="ArgumentException">Thrown when one of the timestamps is not specified.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when staleThreshold is invalid.</exception>
    public GlucoseDashboardSnapshot(
        CgmProviderMetadata metadata,
        GlucoseReading? latestReading,
        IReadOnlyCollection<GlucoseReading> recentReadings,
        DateTimeOffset latestReadingRetrievedAt,
        DateTimeOffset recentReadingsRetrievedAt,
        DateTimeOffset snapshotCreatedAt,
        TimeSpan staleThreshold)
    {
        ArgumentNullException.ThrowIfNull(metadata);
        ArgumentNullException.ThrowIfNull(recentReadings);

        if (latestReadingRetrievedAt == default)
        {
            throw new ArgumentException(
                "Latest reading retrieval timestamp must be specified.",
                nameof(latestReadingRetrievedAt));
        }

        if (recentReadingsRetrievedAt == default)
        {
            throw new ArgumentException(
                "Recent readings retrieval timestamp must be specified.",
                nameof(recentReadingsRetrievedAt));
        }

        if (snapshotCreatedAt == default)
        {
            throw new ArgumentException(
                "Snapshot creation timestamp must be specified.",
                nameof(snapshotCreatedAt));
        }

        if (staleThreshold <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(staleThreshold),
                staleThreshold,
                "Stale threshold must be greater than zero.");
        }

        Metadata = metadata;
        LatestReading = latestReading;
        RecentReadings = recentReadings
            .OrderBy(reading => reading.Timestamp)
            .ToArray();

        LatestReadingRetrievedAt = latestReadingRetrievedAt;
        RecentReadingsRetrievedAt = recentReadingsRetrievedAt;
        SnapshotCreatedAt = snapshotCreatedAt;
        StaleThreshold = staleThreshold;
    }

    /// <summary>
    /// Gets the active provider metadata.
    /// </summary>
    public CgmProviderMetadata Metadata { get; }

    /// <summary>
    /// Gets the latest glucose reading, when available.
    /// </summary>
    public GlucoseReading? LatestReading { get; }

    /// <summary>
    /// Gets the recent glucose readings ordered by timestamp.
    /// </summary>
    public IReadOnlyCollection<GlucoseReading> RecentReadings { get; }

    /// <summary>
    /// Gets the timestamp when the latest reading was retrieved.
    /// </summary>
    public DateTimeOffset LatestReadingRetrievedAt { get; }

    /// <summary>
    /// Gets the timestamp when recent readings were retrieved.
    /// </summary>
    public DateTimeOffset RecentReadingsRetrievedAt { get; }

    /// <summary>
    /// Gets the timestamp when the dashboard snapshot was created.
    /// </summary>
    public DateTimeOffset SnapshotCreatedAt { get; }

    /// <summary>
    /// Gets the maximum age allowed before the latest reading is considered stale.
    /// </summary>
    public TimeSpan StaleThreshold { get; }

    /// <summary>
    /// Gets a value indicating whether the dashboard contains a latest reading.
    /// </summary>
    public bool HasLatestReading => LatestReading is not null;

    /// <summary>
    /// Gets a value indicating whether recent readings are available.
    /// </summary>
    public bool HasRecentReadings => RecentReadings.Count > 0;

    /// <summary>
    /// Gets a value indicating whether the latest reading is stale.
    /// </summary>
    public bool IsLatestReadingStale => LatestReading is null || LatestReading.IsStale(SnapshotCreatedAt, StaleThreshold);
}