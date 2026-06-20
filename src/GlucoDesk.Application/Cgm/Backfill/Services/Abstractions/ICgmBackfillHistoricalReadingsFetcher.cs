using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;

/// <summary>
/// Fetches historical CGM readings for planned backfill gaps.
/// </summary>
public interface ICgmBackfillHistoricalReadingsFetcher
{
    /// <summary>
    /// Fetches historical readings for a planned backfill gap.
    /// </summary>
    /// <param name="gap">The planned backfill gap.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The fetched gap result.</returns>
    Task<Result<CgmBackfillFetchedGapResult>> FetchAsync(
        CgmBackfillPlanGap gap,
        CancellationToken cancellationToken);
}