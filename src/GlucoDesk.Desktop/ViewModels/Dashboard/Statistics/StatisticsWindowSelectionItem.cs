namespace GlucoDesk.Desktop.ViewModels.Dashboard.Statistics;

/// <summary>
/// Represents a selectable local statistics time window.
/// </summary>
public sealed record StatisticsWindowSelectionItem
{
    /// <summary>
    /// Initializes a new instance of the <see cref="StatisticsWindowSelectionItem"/> class.
    /// </summary>
    /// <param name="label">The short window label displayed in the dashboard.</param>
    /// <param name="duration">The time window duration.</param>
    /// <param name="description">The user-facing window description.</param>
    public StatisticsWindowSelectionItem(
        string label,
        TimeSpan duration,
        string description)
    {
        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException("Statistics window label is required.", nameof(label));
        }

        if (duration <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(nameof(duration), duration, "Statistics window duration must be greater than zero.");
        }

        Label = label;
        Duration = duration;
        Description = description;
    }

    /// <summary>
    /// Gets the short window label displayed in the dashboard.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the time window duration.
    /// </summary>
    public TimeSpan Duration { get; }

    /// <summary>
    /// Gets the user-facing window description.
    /// </summary>
    public string Description { get; }
}