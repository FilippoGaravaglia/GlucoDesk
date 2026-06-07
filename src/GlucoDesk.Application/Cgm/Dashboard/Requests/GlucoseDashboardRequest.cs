namespace GlucoDesk.Application.Cgm.Dashboard.Requests;

/// <summary>
/// Represents a request used to build a glucose dashboard snapshot.
/// </summary>
public sealed record GlucoseDashboardRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseDashboardRequest"/> class.
    /// </summary>
    /// <param name="historyDuration">The recent readings duration requested by the dashboard.</param>
    /// <param name="staleThreshold">The maximum age allowed before the latest reading is considered stale.</param>
    /// <param name="maxReadings">The optional maximum number of recent readings.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when one of the supplied values is invalid.</exception>
    public GlucoseDashboardRequest(
        TimeSpan historyDuration,
        TimeSpan staleThreshold,
        int? maxReadings = null)
    {
        if (historyDuration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(historyDuration),
                historyDuration,
                "History duration must be greater than zero.");
        }

        if (staleThreshold <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(staleThreshold),
                staleThreshold,
                "Stale threshold must be greater than zero.");
        }

        if (maxReadings <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxReadings),
                maxReadings,
                "Maximum readings must be greater than zero.");
        }

        HistoryDuration = historyDuration;
        StaleThreshold = staleThreshold;
        MaxReadings = maxReadings;
    }

    /// <summary>
    /// Gets the default dashboard request.
    /// </summary>
    public static GlucoseDashboardRequest Default { get; } = new(
        TimeSpan.FromHours(3),
        TimeSpan.FromMinutes(15),
        maxReadings: 36);

    /// <summary>
    /// Gets the recent readings duration requested by the dashboard.
    /// </summary>
    public TimeSpan HistoryDuration { get; }

    /// <summary>
    /// Gets the maximum age allowed before the latest reading is considered stale.
    /// </summary>
    public TimeSpan StaleThreshold { get; }

    /// <summary>
    /// Gets the optional maximum number of recent readings.
    /// </summary>
    public int? MaxReadings { get; }
}