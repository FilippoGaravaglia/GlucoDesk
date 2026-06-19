using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;

/// <summary>
/// Defines operations for exporting glycemic diary reports to PDF.
/// </summary>
public interface IGlycemicDiaryPdfExportService
{
    /// <summary>
    /// Exports a glycemic diary report to PDF.
    /// </summary>
    /// <param name="request">The PDF export request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The exported glycemic diary file.</returns>
    Task<Result<GlycemicDiaryExportFile>> ExportAsync(
        GlycemicDiaryPdfExportRequest request,
        CancellationToken cancellationToken);
}