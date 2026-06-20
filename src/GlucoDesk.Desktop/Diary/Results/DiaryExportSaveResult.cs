namespace GlucoDesk.Desktop.Diary.Results;

/// <summary>
/// Represents the result of saving an exported diary file.
/// </summary>
public sealed record DiaryExportSaveResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DiaryExportSaveResult"/> class.
    /// </summary>
    /// <param name="wasCanceled">A value indicating whether the save operation was canceled.</param>
    /// <param name="savedFileName">The saved file name.</param>
    public DiaryExportSaveResult(
        bool wasCanceled,
        string? savedFileName)
    {
        WasCanceled = wasCanceled;
        SavedFileName = savedFileName;
    }

    /// <summary>
    /// Gets a value indicating whether the save operation was canceled.
    /// </summary>
    public bool WasCanceled { get; }

    /// <summary>
    /// Gets the saved file name.
    /// </summary>
    public string? SavedFileName { get; }

    /// <summary>
    /// Creates a canceled save result.
    /// </summary>
    /// <returns>The canceled save result.</returns>
    public static DiaryExportSaveResult Canceled()
    {
        return new DiaryExportSaveResult(true, null);
    }

    /// <summary>
    /// Creates a saved file result.
    /// </summary>
    /// <param name="savedFileName">The saved file name.</param>
    /// <returns>The saved file result.</returns>
    public static DiaryExportSaveResult Saved(string savedFileName)
    {
        return new DiaryExportSaveResult(false, savedFileName);
    }
}