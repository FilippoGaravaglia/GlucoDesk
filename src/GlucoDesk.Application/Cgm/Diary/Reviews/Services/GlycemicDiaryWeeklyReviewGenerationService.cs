using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Reviews.Requests;
using GlucoDesk.Application.Cgm.Diary.Reviews.Results;
using GlucoDesk.Application.Cgm.Diary.Reviews.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Diary.Reviews.Services;

/// <summary>
/// Generates comparison-based weekly glycemic diary reviews from local diary history.
/// </summary>
public sealed class GlycemicDiaryWeeklyReviewGenerationService : IGlycemicDiaryWeeklyReviewGenerationService
{
    private readonly IGlycemicDiaryService _diaryService;
    private readonly IGlycemicDiaryWeeklyReviewService _weeklyReviewService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryWeeklyReviewGenerationService"/> class.
    /// </summary>
    /// <param name="diaryService">The glycemic diary service.</param>
    /// <param name="weeklyReviewService">The weekly review comparison service.</param>
    public GlycemicDiaryWeeklyReviewGenerationService(
        IGlycemicDiaryService diaryService,
        IGlycemicDiaryWeeklyReviewService weeklyReviewService)
    {
        ArgumentNullException.ThrowIfNull(diaryService);
        ArgumentNullException.ThrowIfNull(weeklyReviewService);

        _diaryService = diaryService;
        _weeklyReviewService = weeklyReviewService;
    }

    /// <inheritdoc />
    public async Task<Result<GlycemicDiaryWeeklyReview>> GenerateAsync(
        GlycemicDiaryWeeklyReviewRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var currentDiaryResult = await _diaryService
            .CreateDiaryAsync(
                new GlycemicDiaryRequest(
                    request.CurrentPeriodStartsAt,
                    request.CurrentPeriodEndsAt),
                cancellationToken)
            .ConfigureAwait(false);

        if (currentDiaryResult.IsFailure)
        {
            return Result<GlycemicDiaryWeeklyReview>.Failure(currentDiaryResult.Error);
        }

        var previousPeriod = ResolvePreviousPeriod(request);

        var previousDiaryResult = await _diaryService
            .CreateDiaryAsync(
                new GlycemicDiaryRequest(
                    previousPeriod.StartsAt,
                    previousPeriod.EndsAt),
                cancellationToken)
            .ConfigureAwait(false);

        if (previousDiaryResult.IsFailure)
        {
            return Result<GlycemicDiaryWeeklyReview>.Failure(previousDiaryResult.Error);
        }

        var review = _weeklyReviewService.CreateReview(
            currentDiaryResult.Value,
            previousDiaryResult.Value);

        return Result<GlycemicDiaryWeeklyReview>.Success(review);
    }

    #region Helpers

    /// <summary>
    /// Resolves the previous comparison period.
    /// </summary>
    /// <param name="request">The weekly review request.</param>
    /// <returns>The resolved previous period.</returns>
    private static PreviousPeriod ResolvePreviousPeriod(
        GlycemicDiaryWeeklyReviewRequest request)
    {
        if (request.HasExplicitPreviousPeriod)
        {
            return new PreviousPeriod(
                request.PreviousPeriodStartsAt!.Value,
                request.PreviousPeriodEndsAt!.Value);
        }

        var currentDuration = request.CurrentPeriodEndsAt - request.CurrentPeriodStartsAt;
        var previousPeriodEndsAt = request.CurrentPeriodStartsAt.AddTicks(-1);
        var previousPeriodStartsAt = previousPeriodEndsAt - currentDuration;

        return new PreviousPeriod(
            previousPeriodStartsAt,
            previousPeriodEndsAt);
    }

    private sealed record PreviousPeriod(
        DateTimeOffset StartsAt,
        DateTimeOffset EndsAt);

    #endregion
}
