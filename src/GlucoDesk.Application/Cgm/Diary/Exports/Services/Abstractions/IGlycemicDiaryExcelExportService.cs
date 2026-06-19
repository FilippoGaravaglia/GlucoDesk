using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;

/// <summary>
/// Defines operations for exporting glycemic diary reports to Excel.
/// </summary>
public interface IGlycemicDiaryExcelExportService
{
    /// <summary>
    /// Exports a glycemic diary report to Excel.
    /// </summary>
    /// <param name="request">The Excel export request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The exported glycemic diary file.</returns>
    Task<Result<GlycemicDiaryExportFile>> ExportAsync(
        GlycemicDiaryExcelExportRequest request,
        CancellationToken cancellationToken);
}