using GlucoDesk.Application.Cgm.History.Continuity.Requests;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;

/// <summary>
/// Defines operations for analyzing continuity of persisted local glucose history.
/// </summary>
public interface IGlucoseHistoryContinuityQueryService
{
    /// <summary>
    /// Analyzes the continuity of persisted local glucose history for the requested window.
    /// </summary>
    /// <param name="request">The continuity analysis request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The glucose history continuity report.</returns>
    Task<Result<GlucoseHistoryContinuityReport>> AnalyzeLocalHistoryAsync(
        GlucoseHistoryContinuityRequest request,
        CancellationToken cancellationToken);
}