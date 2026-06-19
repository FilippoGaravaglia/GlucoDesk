namespace GlucoDesk.Application.Cgm.History.Continuity.Requests;

/// <summary>
/// Represents a request for local glucose history continuity analysis.
/// </summary>
public sealed record GlucoseHistoryContinuityRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseHistoryContinuityRequest"/> class.
    /// </summary>
    /// <param name="windowStartsAt">The analyzed window start timestamp.</param>
    /// <param name="windowEndsAt">The analyzed window end timestamp.</param>
    public GlucoseHistoryContinuityRequest(
        DateTimeOffset windowStartsAt,
        DateTimeOffset windowEndsAt)
    {
        if (windowEndsAt <= windowStartsAt)
        {
            throw new ArgumentException(
                "Window end timestamp must be greater than window start timestamp.",
                nameof(windowEndsAt));
        }

        WindowStartsAt = windowStartsAt;
        WindowEndsAt = windowEndsAt;
    }

    /// <summary>
    /// Gets the analyzed window start timestamp.
    /// </summary>
    public DateTimeOffset WindowStartsAt { get; }

    /// <summary>
    /// Gets the analyzed window end timestamp.
    /// </summary>
    public DateTimeOffset WindowEndsAt { get; }
}