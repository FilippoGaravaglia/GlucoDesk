using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Cgm.Diary.Exports.Requests;

/// <summary>
/// Represents a request for exporting a glycemic diary to Excel.
/// </summary>
public sealed record GlycemicDiaryExcelExportRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryExcelExportRequest"/> class.
    /// </summary>
    /// <param name="diaryRequest">The diary generation request.</param>
    /// <param name="fileName">The optional exported file name.</param>
    /// <param name="preferredUnit">The preferred glucose display unit for exported values.</param>
    /// <param name="languageCode">The language code used to localize the generated document.</param>
    public GlycemicDiaryExcelExportRequest(
        GlycemicDiaryRequest diaryRequest,
        string? fileName = null,
        GlucoseUnit preferredUnit = GlucoseUnit.MgDl,
        string languageCode = "en")
    {
        ArgumentNullException.ThrowIfNull(diaryRequest);

        if (fileName is not null && string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "Export file name cannot be empty.",
                nameof(fileName));
        }

        if (!Enum.IsDefined(preferredUnit))
        {
            throw new ArgumentException(
                "Preferred glucose unit is not valid.",
                nameof(preferredUnit));
        }

        if (string.IsNullOrWhiteSpace(languageCode))
        {
            throw new ArgumentException(
                "Export language code cannot be empty.",
                nameof(languageCode));
        }

        DiaryRequest = diaryRequest;
        FileName = fileName;
        PreferredUnit = preferredUnit;
        LanguageCode = NormalizeLanguageCode(languageCode);
    }

    /// <summary>
    /// Gets the diary generation request.
    /// </summary>
    public GlycemicDiaryRequest DiaryRequest { get; }

    /// <summary>
    /// Gets the optional exported file name.
    /// </summary>
    public string? FileName { get; }

    /// <summary>
    /// Gets the preferred glucose display unit for exported values.
    /// </summary>
    public GlucoseUnit PreferredUnit { get; }

    /// <summary>
    /// Gets the normalized language code used by the generated document.
    /// </summary>
    public string LanguageCode { get; }

    private static string NormalizeLanguageCode(string languageCode)
    {
        var normalizedLanguageCode = languageCode
            .Trim()
            .ToLowerInvariant();

        return normalizedLanguageCode.StartsWith(
            "it",
            StringComparison.Ordinal)
            ? "it"
            : "en";
    }
}