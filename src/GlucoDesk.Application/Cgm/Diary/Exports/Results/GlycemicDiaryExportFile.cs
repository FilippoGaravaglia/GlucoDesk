namespace GlucoDesk.Application.Cgm.Diary.Exports.Results;

/// <summary>
/// Represents an exported glycemic diary file.
/// </summary>
public sealed record GlycemicDiaryExportFile
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryExportFile"/> class.
    /// </summary>
    /// <param name="fileName">The exported file name.</param>
    /// <param name="contentType">The exported file content type.</param>
    /// <param name="content">The exported file content.</param>
    public GlycemicDiaryExportFile(
        string fileName,
        string contentType,
        byte[] content)
    {
        if (string.IsNullOrWhiteSpace(fileName))
        {
            throw new ArgumentException(
                "Export file name cannot be empty.",
                nameof(fileName));
        }

        if (string.IsNullOrWhiteSpace(contentType))
        {
            throw new ArgumentException(
                "Export file content type cannot be empty.",
                nameof(contentType));
        }

        ArgumentNullException.ThrowIfNull(content);

        if (content.Length == 0)
        {
            throw new ArgumentException(
                "Export file content cannot be empty.",
                nameof(content));
        }

        FileName = fileName;
        ContentType = contentType;
        Content = content;
    }

    /// <summary>
    /// Gets the exported file name.
    /// </summary>
    public string FileName { get; }

    /// <summary>
    /// Gets the exported file content type.
    /// </summary>
    public string ContentType { get; }

    /// <summary>
    /// Gets the exported file content.
    /// </summary>
    public byte[] Content { get; }
}