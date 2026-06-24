using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Stories.Results;

namespace GlucoDesk.Application.Cgm.Diary.Stories.Services.Abstractions;

/// <summary>
/// Builds user-facing glycemic diary stories from diary reports.
/// </summary>
public interface IGlycemicDiaryStoryService
{
    /// <summary>
    /// Builds a user-facing glycemic diary story.
    /// </summary>
    /// <param name="report">The glycemic diary report.</param>
    /// <returns>The generated diary story.</returns>
    GlycemicDiaryStory CreateStory(GlycemicDiaryReport report);
}
