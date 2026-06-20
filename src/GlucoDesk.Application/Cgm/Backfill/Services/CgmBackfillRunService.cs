using GlucoDesk.Application.Cgm.Backfill.Enums;
using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services;

/// <summary>
/// Default implementation of <see cref="ICgmBackfillRunService"/>.
/// </summary>
public sealed class CgmBackfillRunService : ICgmBackfillRunService
{
    private readonly ICgmBackfillPlanQueryService _planQueryService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CgmBackfillRunService"/> class.
    /// </summary>
    /// <param name="planQueryService">The backfill plan query service.</param>
    public CgmBackfillRunService(ICgmBackfillPlanQueryService planQueryService)
    {
        ArgumentNullException.ThrowIfNull(planQueryService);

        _planQueryService = planQueryService;
    }

    /// <inheritdoc />
    public async Task<Result<CgmBackfillRunResult>> RunAsync(
        CgmBackfillRunRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = ValidateRequest(request);

        if (validationResult.IsFailure)
        {
            return Result<CgmBackfillRunResult>.Failure(validationResult.Error);
        }

        var planResult = await _planQueryService
            .CreatePlanAsync(
                new CgmBackfillPlanFromHistoryRequest(
                    request.StartsAt,
                    request.EndsAt),
                cancellationToken)
            .ConfigureAwait(false);

        if (planResult.IsFailure)
        {
            return Result<CgmBackfillRunResult>.Failure(planResult.Error);
        }

        var plan = planResult.Value;

        if (!plan.CanBackfill)
        {
            return Result<CgmBackfillRunResult>.Success(
                new CgmBackfillRunResult(
                    CgmBackfillRunStatus.SkippedNoRecoverableGaps,
                    plan,
                    RecoverableGapsCount: 0,
                    Message: "No recoverable local history gaps were found."));
        }

        return Result<CgmBackfillRunResult>.Success(
            new CgmBackfillRunResult(
                CgmBackfillRunStatus.Planned,
                plan,
                RecoverableGapsCount: plan.RecoverableGaps.Count,
                Message: "A historical CGM backfill run can be attempted."));
    }

    #region Helpers

    /// <summary>
    /// Validates the supplied backfill run request.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <returns>The validation result.</returns>
    private static Result ValidateRequest(CgmBackfillRunRequest request)
    {
        if (request.EndsAt <= request.StartsAt)
        {
            return Result.Failure(
                new Error(
                    "Backfill.InvalidRunWindow",
                    "The backfill run window must end after it starts."));
        }

        return Result.Success();
    }

    #endregion
}