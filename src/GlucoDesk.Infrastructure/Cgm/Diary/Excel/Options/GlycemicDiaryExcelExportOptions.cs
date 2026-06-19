namespace GlucoDesk.Infrastructure.Cgm.Diary.Excel.Options;

/// <summary>
/// Provides options for glycemic diary Excel export generation.
/// </summary>
public sealed record GlycemicDiaryExcelExportOptions
{
    /// <summary>
    /// Gets the default Excel content type.
    /// </summary>
    public const string ExcelContentType = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    /// <summary>
    /// Gets the default glycemic diary Excel export options.
    /// </summary>
    public static GlycemicDiaryExcelExportOptions Default => new(
        "GlucoDesk",
        "GlucoDesk is not a medical device. This diary is for personal awareness and must not be used for treatment decisions.");

    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryExcelExportOptions"/> class.
    /// </summary>
    /// <param name="applicationName">The application name.</param>
    /// <param name="safetyDisclaimer">The safety disclaimer.</param>
    public GlycemicDiaryExcelExportOptions(
        string applicationName,
        string safetyDisclaimer)
    {
        if (string.IsNullOrWhiteSpace(applicationName))
        {
            throw new ArgumentException(
                "Application name cannot be empty.",
                nameof(applicationName));
        }

        if (string.IsNullOrWhiteSpace(safetyDisclaimer))
        {
            throw new ArgumentException(
                "Safety disclaimer cannot be empty.",
                nameof(safetyDisclaimer));
        }

        ApplicationName = applicationName;
        SafetyDisclaimer = safetyDisclaimer;
    }

    /// <summary>
    /// Gets the application name.
    /// </summary>
    public string ApplicationName { get; }

    /// <summary>
    /// Gets the safety disclaimer.
    /// </summary>
    public string SafetyDisclaimer { get; }
}