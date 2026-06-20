using GlucoDesk.Application.Cgm.Backfill.Enums;
using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services;

/// <summary>
/// Default implementation of <see cref="ICgmBackfillExecutionService"/>.
/// </summary>
public sealed class CgmBackfillExecutionService : ICgmBackfillExecutionService
{
    private readonly ICgmBackfillRunService _runService;
    private readonly ICgmBackfillHistoricalReadingsFetcher _readingsFetcher;

    /// <summary>
    /// Initializes a new instance of the <see cref="CgmBackfillExecutionService"/> class.
    /// </summary>
    /// <param name="runService">The backfill run service.</param>
    /// <param name="readingsFetcher">The historical readings fetcher.</param>
    public CgmBackfillExecutionService(
        ICgmBackfillRunService runService,
        ICgmBackfillHistoricalReadingsFetcher readingsFetcher)
    {
        ArgumentNullException.ThrowIfNull(runService);
        ArgumentNullException.ThrowIfNull(readingsFetcher);

        _runService = runService;
        _readingsFetcher = readingsFetcher;
    }

    /// <inheritdoc />
    public async Task<Result<CgmBackfillExecutionResult>> ExecuteAsync(
        CgmBackfillRunRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = ValidateRequest(request);

        if (validationResult.IsFailure)
        {
            return Result<CgmBackfillExecutionResult>.Failure(validationResult.Error);
        }

        var runResult = await _runService
            .RunAsync(
                request,
                cancellationToken)
            .ConfigureAwait(false);

        if (runResult.IsFailure)
        {
            return Result<CgmBackfillExecutionResult>.Failure(runResult.Error);
        }

        var run = runResult.Value;

        if (!run.HasRecoverableGaps)
        {
            return Result<CgmBackfillExecutionResult>.Success(
                new CgmBackfillExecutionResult(
                    CgmBackfillExecutionStatus.SkippedNoRecoverableGaps,
                    run,
                    [],
                    "No recoverable local history gaps were found."));
        }

        var fetchedGaps = new List<CgmBackfillFetchedGapResult>();

        foreach (var gap in run.Plan.RecoverableGaps.OrderBy(gap => gap.StartsAt))
        {
            var fetchResult = await _readingsFetcher
                .FetchAsync(
                    gap,
                    cancellationToken)
                .ConfigureAwait(false);

            if (fetchResult.IsFailure)
            {
                return Result<CgmBackfillExecutionResult>.Failure(fetchResult.Error);
            }

            fetchedGaps.Add(fetchResult.Value);
        }

        return Result<CgmBackfillExecutionResult>.Success(
            new CgmBackfillExecutionResult(
                CgmBackfillExecutionStatus.Completed,
                run,
                fetchedGaps,
                "Historical CGM readings were fetched for the recoverable gaps."));
    }

    #region Helpers

    /// <summary>
    /// Validates the supplied backfill execution request.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <returns>The validation result.</returns>
    private static Result ValidateRequest(CgmBackfillRunRequest request)
    {
        if (request.EndsAt <= request.StartsAt)
        {
            return Result.Failure(
                new Error(
                    "Backfill.InvalidExecutionWindow",
                    "The backfill execution window must end after it starts."));
        }

        return Result.Success();
    }

    #endregion
}