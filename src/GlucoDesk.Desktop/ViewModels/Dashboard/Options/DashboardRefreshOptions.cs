namespace GlucoDesk.Desktop.ViewModels.Dashboard.Options;

/// <summary>
/// Represents refresh options used by the dashboard view model and view.
/// </summary>
public sealed record DashboardRefreshOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardRefreshOptions"/> class.
    /// </summary>
    /// <param name="autoRefreshInterval">The automatic refresh interval.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the refresh interval is invalid.</exception>
    public DashboardRefreshOptions(TimeSpan autoRefreshInterval)
    {
        if (autoRefreshInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(autoRefreshInterval),
                autoRefreshInterval,
                "Auto-refresh interval must be greater than zero.");
        }

        AutoRefreshInterval = autoRefreshInterval;
    }

    /// <summary>
    /// Gets the default dashboard refresh options.
    /// </summary>
    public static DashboardRefreshOptions Default { get; } = new(TimeSpan.FromSeconds(30));

    /// <summary>
    /// Gets the automatic refresh interval.
    /// </summary>
    public TimeSpan AutoRefreshInterval { get; }
}