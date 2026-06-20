using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.Requests;
using GlucoDesk.Application.Cgm.History.Continuity.Results;
using GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services;

/// <summary>
/// Default implementation of <see cref="ICgmBackfillPlanQueryService"/>.
/// </summary>
public sealed class CgmBackfillPlanQueryService : ICgmBackfillPlanQueryService
{
    private readonly IGlucoseHistoryContinuityQueryService _continuityQueryService;
    private readonly ICgmBackfillPlanService _backfillPlanService;

    /// <summary>
    /// Initializes a new instance of the <see cref="CgmBackfillPlanQueryService"/> class.
    /// </summary>
    /// <param name="continuityQueryService">The glucose history continuity query service.</param>
    /// <param name="backfillPlanService">The backfill plan service.</param>
    public CgmBackfillPlanQueryService(
        IGlucoseHistoryContinuityQueryService continuityQueryService,
        ICgmBackfillPlanService backfillPlanService)
    {
        ArgumentNullException.ThrowIfNull(continuityQueryService);
        ArgumentNullException.ThrowIfNull(backfillPlanService);

        _continuityQueryService = continuityQueryService;
        _backfillPlanService = backfillPlanService;
    }

    /// <inheritdoc />
    public async Task<Result<CgmBackfillPlan>> CreatePlanAsync(
        CgmBackfillPlanFromHistoryRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = ValidateRequest(request);

        if (validationResult.IsFailure)
        {
            return Result<CgmBackfillPlan>.Failure(validationResult.Error);
        }

        var continuityResult = await _continuityQueryService
            .AnalyzeLocalHistoryAsync(
                new GlucoseHistoryContinuityRequest(
                    request.StartsAt,
                    request.EndsAt),
                cancellationToken)
            .ConfigureAwait(false);

        if (continuityResult.IsFailure)
        {
            return Result<CgmBackfillPlan>.Failure(continuityResult.Error);
        }

        var detectedGaps = CreateDetectedGaps(continuityResult.Value);

        return await _backfillPlanService
            .CreatePlanAsync(
                new CgmBackfillPlanRequest(
                    request.StartsAt,
                    request.EndsAt,
                    detectedGaps),
                cancellationToken)
            .ConfigureAwait(false);
    }

    #region Helpers

    /// <summary>
    /// Validates the supplied backfill plan from history request.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <returns>The validation result.</returns>
    private static Result ValidateRequest(CgmBackfillPlanFromHistoryRequest request)
    {
        if (request.EndsAt <= request.StartsAt)
        {
            return Result.Failure(
                new Error(
                    "Backfill.InvalidHistoryWindow",
                    "The backfill history planning window must end after it starts."));
        }

        return Result.Success();
    }

    /// <summary>
    /// Creates detected backfill gaps from the local history continuity report.
    /// </summary>
    /// <param name="report">The local history continuity report.</param>
    /// <returns>The detected backfill gaps.</returns>
    private static IReadOnlyCollection<CgmBackfillDetectedGap> CreateDetectedGaps(
        GlucoseHistoryContinuityReport report)
    {
        return report.Gaps
            .OrderBy(gap => gap.StartsAt)
            .Select(gap => new CgmBackfillDetectedGap(
                gap.StartsAt,
                gap.EndsAt))
            .ToArray();
    }

    #endregion
}