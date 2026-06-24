using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Reviews.Results;

namespace GlucoDesk.Application.Cgm.Diary.Reviews.Services.Abstractions;

/// <summary>
/// Builds comparison-based weekly glycemic diary reviews.
/// </summary>
public interface IGlycemicDiaryWeeklyReviewService
{
    /// <summary>
    /// Creates a weekly-style review by comparing a current diary report with a previous diary report.
    /// </summary>
    /// <param name="currentReport">The current period report.</param>
    /// <param name="previousReport">The previous period report.</param>
    /// <returns>The generated weekly review.</returns>
    GlycemicDiaryWeeklyReview CreateReview(
        GlycemicDiaryReport currentReport,
        GlycemicDiaryReport previousReport);
}
