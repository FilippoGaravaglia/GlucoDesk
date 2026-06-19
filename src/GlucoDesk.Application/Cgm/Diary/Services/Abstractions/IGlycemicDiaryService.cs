using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Diary.Services.Abstractions;

/// <summary>
/// Defines operations for generating glycemic diary reports from local glucose history.
/// </summary>
public interface IGlycemicDiaryService
{
    /// <summary>
    /// Creates a glycemic diary report for the requested period.
    /// </summary>
    /// <param name="request">The diary request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The glycemic diary report.</returns>
    Task<Result<GlycemicDiaryReport>> CreateDiaryAsync(
        GlycemicDiaryRequest request,
        CancellationToken cancellationToken);
}