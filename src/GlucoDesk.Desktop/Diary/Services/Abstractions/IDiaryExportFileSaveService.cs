using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.Diary.Results;

namespace GlucoDesk.Desktop.Diary.Services.Abstractions;

/// <summary>
/// Defines operations for saving exported glycemic diary files from the desktop application.
/// </summary>
public interface IDiaryExportFileSaveService
{
    /// <summary>
    /// Saves an exported glycemic diary file.
    /// </summary>
    /// <param name="file">The exported diary file.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The save result.</returns>
    Task<Result<DiaryExportSaveResult>> SaveAsync(
        GlycemicDiaryExportFile file,
        CancellationToken cancellationToken);
}