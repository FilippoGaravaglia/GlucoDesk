using GlucoDesk.Application.Cgm.WidgetState.Snapshots;

namespace GlucoDesk.Application.Cgm.WidgetState.Results;

/// <summary>
/// Represents the result of reading the local widget state.
/// </summary>
public sealed record GlucoseWidgetStateReadResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseWidgetStateReadResult"/> class.
    /// </summary>
    /// <param name="state">The widget state, when available.</param>
    public GlucoseWidgetStateReadResult(GlucoseWidgetState? state)
    {
        State = state;
    }

    /// <summary>
    /// Gets the widget state, when available.
    /// </summary>
    public GlucoseWidgetState? State { get; }

    /// <summary>
    /// Gets a value indicating whether a widget state is available.
    /// </summary>
    public bool HasState => State is not null;

    /// <summary>
    /// Creates an empty widget state read result.
    /// </summary>
    /// <returns>The empty read result.</returns>
    public static GlucoseWidgetStateReadResult Empty()
    {
        return new GlucoseWidgetStateReadResult((GlucoseWidgetState?)null);
    }
}