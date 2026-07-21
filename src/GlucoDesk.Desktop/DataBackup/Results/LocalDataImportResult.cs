namespace GlucoDesk.Desktop.DataBackup.Results;

/// <summary>
/// Describes the result of importing a portable GlucoDesk backup.
/// </summary>
public sealed record LocalDataImportResult(
    int IncomingReadingsCount,
    int AddedReadingsCount,
    int DuplicateReadingsCount,
    int StoredReadingsCount,
    bool SettingsImported,
    bool LanguageImported,
    bool PrivacyPreferenceImported)
{
    /// <summary>
    /// Gets whether at least one glucose reading was added.
    /// </summary>
    public bool HasNewReadings => AddedReadingsCount > 0;
}
