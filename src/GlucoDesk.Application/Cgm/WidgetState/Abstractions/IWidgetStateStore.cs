using GlucoDesk.Application.Cgm.WidgetState.Results;
using GlucoDesk.Application.Cgm.WidgetState.Snapshots;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.WidgetState.Abstractions;

/// <summary>
/// Defines persistence operations for the local glucose widget state.
/// </summary>
public interface IWidgetStateStore
{
    /// <summary>
    /// Saves the widget state.
    /// </summary>
    /// <param name="state">The widget state to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The save operation result.</returns>
    Task<Result> SaveAsync(
        GlucoseWidgetState state,
        CancellationToken cancellationToken);

    /// <summary>
    /// Reads the current widget state.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The widget state read result.</returns>
    Task<Result<GlucoseWidgetStateReadResult>> ReadAsync(
        CancellationToken cancellationToken);

    /// <summary>
    /// Clears the current widget state.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The clear operation result.</returns>
    Task<Result> ClearAsync(CancellationToken cancellationToken);
}