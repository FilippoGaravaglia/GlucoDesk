using GlucoDesk.Application.Cgm.Statistics.Requests;
using GlucoDesk.Application.Cgm.Statistics.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Statistics.Services.Abstractions;

/// <summary>
/// Defines application-level operations for glucose statistics.
/// </summary>
public interface IGlucoseStatisticsService
{
    /// <summary>
    /// Calculates glucose statistics from local history.
    /// </summary>
    /// <param name="request">The statistics request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The glucose statistics result.</returns>
    Task<Result<GlucoseStatisticsResult>> CalculateAsync(
        GlucoseStatisticsRequest request,
        CancellationToken cancellationToken);
}