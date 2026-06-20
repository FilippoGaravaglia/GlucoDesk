using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services;

/// <summary>
/// Fetches historical CGM readings for planned backfill gaps using the active glucose data service.
/// </summary>
public sealed class CgmBackfillHistoricalReadingsFetcher : ICgmBackfillHistoricalReadingsFetcher
{
    private readonly IGlucoseDataService _glucoseDataService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CgmBackfillHistoricalReadingsFetcher"/> class.
    /// </summary>
    /// <param name="glucoseDataService">The glucose data service.</param>
    public CgmBackfillHistoricalReadingsFetcher(IGlucoseDataService glucoseDataService)
    {
        ArgumentNullException.ThrowIfNull(glucoseDataService);

        _glucoseDataService = glucoseDataService;
    }

    /// <inheritdoc />
    public async Task<Result<CgmBackfillFetchedGapResult>> FetchAsync(
        CgmBackfillPlanGap gap,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(gap);

        var validationResult = ValidateGap(gap);

        if (validationResult.IsFailure)
        {
            return Result<CgmBackfillFetchedGapResult>.Failure(validationResult.Error);
        }

        var readingsResult = await _glucoseDataService
            .GetHistoricalReadingsAsync(
                CreateReadingsRequest(gap),
                cancellationToken)
            .ConfigureAwait(false);

        if (readingsResult.IsFailure)
        {
            return Result<CgmBackfillFetchedGapResult>.Failure(readingsResult.Error);
        }

        return Result<CgmBackfillFetchedGapResult>.Success(
            new CgmBackfillFetchedGapResult(
                gap,
                readingsResult.Value.Readings.Count));
    }

    #region Helpers

    /// <summary>
    /// Validates the planned backfill gap before fetching historical readings.
    /// </summary>
    /// <param name="gap">The planned backfill gap.</param>
    /// <returns>The validation result.</returns>
    private static Result ValidateGap(CgmBackfillPlanGap gap)
    {
        if (gap.EndsAt <= gap.StartsAt)
        {
            return Result.Failure(
                new Error(
                    "Backfill.InvalidFetchGap",
                    "The historical readings fetch gap must end after it starts."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Creates a glucose readings request from the planned backfill gap.
    /// </summary>
    /// <param name="gap">The planned backfill gap.</param>
    /// <returns>The glucose readings request.</returns>
    private static GlucoseReadingsRequest CreateReadingsRequest(
        CgmBackfillPlanGap gap)
    {
        return new GlucoseReadingsRequest(
            gap.StartsAt,
            gap.EndsAt);
    }

    #endregion
}