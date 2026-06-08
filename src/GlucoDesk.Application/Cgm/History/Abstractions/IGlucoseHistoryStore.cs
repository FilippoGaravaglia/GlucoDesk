using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.History.Abstractions;

/// <summary>
/// Defines persistence operations for local glucose history.
/// </summary>
public interface IGlucoseHistoryStore
{
    /// <summary>
    /// Saves glucose readings into local history.
    /// </summary>
    /// <param name="readings">The glucose readings to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The save operation result.</returns>
    Task<Result> SaveReadingsAsync(
        IReadOnlyCollection<GlucoseReading> readings,
        CancellationToken cancellationToken);

    /// <summary>
    /// Gets glucose readings from local history.
    /// </summary>
    /// <param name="request">The history request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The glucose history result.</returns>
    Task<Result<GlucoseHistoryResult>> GetReadingsAsync(
        GlucoseHistoryRequest request,
        CancellationToken cancellationToken);
}