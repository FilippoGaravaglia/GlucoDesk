using GlucoDesk.Application.Cgm.History.Analytics.Requests;
using GlucoDesk.Application.Cgm.History.Analytics.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.History.Analytics.Services.Abstractions;

/// <summary>
/// Defines application-level analytics operations over local glucose history.
/// </summary>
public interface IGlucoseHistoryAnalyticsService
{
    /// <summary>
    /// Calculates a glucose history summary for the requested period.
    /// </summary>
    /// <param name="request">The glucose history summary request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The glucose history summary result.</returns>
    Task<Result<GlucoseHistorySummaryResult>> GetSummaryAsync(
        GlucoseHistorySummaryRequest request,
        CancellationToken cancellationToken);
}