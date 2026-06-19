namespace GlucoDesk.Infrastructure.Cgm.Diary.Pdf.Options;

/// <summary>
/// Provides options for glycemic diary PDF export generation.
/// </summary>
public sealed record GlycemicDiaryPdfExportOptions
{
    /// <summary>
    /// Gets the PDF content type.
    /// </summary>
    public const string PdfContentType = "application/pdf";

    /// <summary>
    /// Gets the default glycemic diary PDF export options.
    /// </summary>
    public static GlycemicDiaryPdfExportOptions Default => new(
        "GlucoDesk",
        "GlucoDesk is not a medical device. This diary is for personal awareness and must not be used for treatment decisions.");

    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryPdfExportOptions"/> class.
    /// </summary>
    /// <param name="applicationName">The application name.</param>
    /// <param name="safetyDisclaimer">The safety disclaimer.</param>
    public GlycemicDiaryPdfExportOptions(
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