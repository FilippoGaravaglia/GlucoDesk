using GlucoDesk.Application.Cgm.Diary.Reviews.Requests;
using GlucoDesk.Application.Cgm.Diary.Reviews.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Diary.Reviews.Services.Abstractions;

/// <summary>
/// Generates comparison-based weekly glycemic diary reviews from local diary history.
/// </summary>
public interface IGlycemicDiaryWeeklyReviewGenerationService
{
    /// <summary>
    /// Generates a weekly glycemic diary review for the requested current period.
    /// </summary>
    /// <param name="request">The weekly review generation request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The generated weekly review result.</returns>
    Task<Result<GlycemicDiaryWeeklyReview>> GenerateAsync(
        GlycemicDiaryWeeklyReviewRequest request,
        CancellationToken cancellationToken);
}
