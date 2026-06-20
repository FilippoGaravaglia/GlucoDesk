using GlucoDesk.Application.Cgm.Backfill.Requests;
using GlucoDesk.Application.Cgm.Backfill.Results;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Backfill.Services;

/// <summary>
/// Default implementation of <see cref="ICgmBackfillPlanService"/>.
/// </summary>
public sealed class CgmBackfillPlanService : ICgmBackfillPlanService
{
    private readonly ICgmBackfillCapabilityService _capabilityService;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="CgmBackfillPlanService"/> class.
    /// </summary>
    /// <param name="capabilityService">The backfill capability service.</param>
    /// <param name="timeProvider">The time provider.</param>
    public CgmBackfillPlanService(
        ICgmBackfillCapabilityService capabilityService,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(capabilityService);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _capabilityService = capabilityService;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Result<CgmBackfillPlan>> CreatePlanAsync(
        CgmBackfillPlanRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var validationResult = ValidateRequest(request);

        if (validationResult.IsFailure)
        {
            return Result<CgmBackfillPlan>.Failure(validationResult.Error);
        }

        var capabilityResult = await _capabilityService.GetCapabilityAsync(cancellationToken);

        if (capabilityResult.IsFailure)
        {
            return Result<CgmBackfillPlan>.Failure(capabilityResult.Error);
        }

        var capability = capabilityResult.Value;

        if (!capability.IsSupported)
        {
            return Result<CgmBackfillPlan>.Success(
                CreateUnsupportedPlan(
                    request,
                    capability.Message));
        }

        var maximumLookback = capability.MaximumLookback
            ?? throw new InvalidOperationException("Supported backfill capability must define a maximum lookback.");

        var minimumGapDuration = capability.MinimumGapDuration
            ?? throw new InvalidOperationException("Supported backfill capability must define a minimum gap duration.");

        var now = _timeProvider.GetLocalNow();
        var recoverableFrom = MaxDateTimeOffset(
            request.StartsAt,
            now.Subtract(maximumLookback));
        var recoverableTo = MinDateTimeOffset(
            request.EndsAt,
            now);

        if (recoverableTo <= recoverableFrom)
        {
            return Result<CgmBackfillPlan>.Success(
                new CgmBackfillPlan(
                    CanBackfill: false,
                    RequestedStartsAt: request.StartsAt,
                    RequestedEndsAt: request.EndsAt,
                    RecoverableFrom: recoverableFrom,
                    RecoverableTo: recoverableTo,
                    RecoverableGaps: [],
                    IgnoredGapsCount: request.DetectedGaps.Count,
                    Message: "No part of the requested window is currently recoverable."));
        }

        var recoverableGaps = new List<CgmBackfillPlanGap>();

        foreach (var gap in request.DetectedGaps.OrderBy(gap => gap.StartsAt))
        {
            var plannedGap = TryCreateRecoverableGap(
                gap,
                recoverableFrom,
                recoverableTo,
                minimumGapDuration);

            if (plannedGap is not null)
            {
                recoverableGaps.Add(plannedGap);
            }
        }

        return Result<CgmBackfillPlan>.Success(
            new CgmBackfillPlan(
                CanBackfill: recoverableGaps.Count > 0,
                RequestedStartsAt: request.StartsAt,
                RequestedEndsAt: request.EndsAt,
                RecoverableFrom: recoverableFrom,
                RecoverableTo: recoverableTo,
                RecoverableGaps: recoverableGaps,
                IgnoredGapsCount: request.DetectedGaps.Count - recoverableGaps.Count,
                Message: recoverableGaps.Count > 0
                    ? "One or more local history gaps can be backfilled."
                    : "No detected local history gap is currently eligible for backfill."));
    }

    #region Helpers

    /// <summary>
    /// Validates the supplied backfill plan request.
    /// </summary>
    /// <param name="request">The request to validate.</param>
    /// <returns>The validation result.</returns>
    private static Result ValidateRequest(CgmBackfillPlanRequest request)
    {
        if (request.EndsAt <= request.StartsAt)
        {
            return Result.Failure(
                new Error(
                    "Backfill.InvalidWindow",
                    "The backfill planning window must end after it starts."));
        }

        foreach (var gap in request.DetectedGaps)
        {
            if (gap.EndsAt <= gap.StartsAt)
            {
                return Result.Failure(
                    new Error(
                        "Backfill.InvalidGap",
                        "Detected backfill gaps must end after they start."));
            }
        }

        return Result.Success();
    }

    /// <summary>
    /// Creates a plan for a provider or configuration that does not support backfill.
    /// </summary>
    /// <param name="request">The original plan request.</param>
    /// <param name="message">The unsupported capability message.</param>
    /// <returns>The unsupported backfill plan.</returns>
    private static CgmBackfillPlan CreateUnsupportedPlan(
        CgmBackfillPlanRequest request,
        string message)
    {
        return new CgmBackfillPlan(
            CanBackfill: false,
            RequestedStartsAt: request.StartsAt,
            RequestedEndsAt: request.EndsAt,
            RecoverableFrom: null,
            RecoverableTo: null,
            RecoverableGaps: [],
            IgnoredGapsCount: request.DetectedGaps.Count,
            Message: message);
    }

    /// <summary>
    /// Tries to create a recoverable planned gap from a detected gap.
    /// </summary>
    /// <param name="gap">The detected gap.</param>
    /// <param name="recoverableFrom">The earliest recoverable timestamp.</param>
    /// <param name="recoverableTo">The latest recoverable timestamp.</param>
    /// <param name="minimumGapDuration">The minimum eligible gap duration.</param>
    /// <returns>The planned gap when recoverable; otherwise, null.</returns>
    private static CgmBackfillPlanGap? TryCreateRecoverableGap(
        CgmBackfillDetectedGap gap,
        DateTimeOffset recoverableFrom,
        DateTimeOffset recoverableTo,
        TimeSpan minimumGapDuration)
    {
        var plannedStartsAt = MaxDateTimeOffset(gap.StartsAt, recoverableFrom);
        var plannedEndsAt = MinDateTimeOffset(gap.EndsAt, recoverableTo);

        if (plannedEndsAt <= plannedStartsAt)
        {
            return null;
        }

        var plannedDuration = plannedEndsAt - plannedStartsAt;

        if (plannedDuration < minimumGapDuration)
        {
            return null;
        }

        return new CgmBackfillPlanGap(
            OriginalStartsAt: gap.StartsAt,
            OriginalEndsAt: gap.EndsAt,
            StartsAt: plannedStartsAt,
            EndsAt: plannedEndsAt,
            WasClampedByMaximumLookback: plannedStartsAt > gap.StartsAt);
    }

    /// <summary>
    /// Returns the latest of two timestamps.
    /// </summary>
    /// <param name="first">The first timestamp.</param>
    /// <param name="second">The second timestamp.</param>
    /// <returns>The latest timestamp.</returns>
    private static DateTimeOffset MaxDateTimeOffset(
        DateTimeOffset first,
        DateTimeOffset second)
    {
        return first >= second ? first : second;
    }

    /// <summary>
    /// Returns the earliest of two timestamps.
    /// </summary>
    /// <param name="first">The first timestamp.</param>
    /// <param name="second">The second timestamp.</param>
    /// <returns>The earliest timestamp.</returns>
    private static DateTimeOffset MinDateTimeOffset(
        DateTimeOffset first,
        DateTimeOffset second)
    {
        return first <= second ? first : second;
    }

    #endregion
}