using System.IO.Compression;
using System.Text.Json;
using System.Text.Json.Serialization;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.DataBackup.Models;
using GlucoDesk.Desktop.DataBackup.Results;
using GlucoDesk.Desktop.DataBackup.Services.Abstractions;
using GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.DataBackup.Services;

/// <summary>
/// Creates and restores versioned portable GlucoDesk backup archives.
/// </summary>
public sealed class LocalDataBackupService : ILocalDataBackupService
{
    private const int CurrentSchemaVersion = 1;

    private const string ManifestEntryName = "manifest.json";
    private const string HistoryEntryName = "glucose-history.json";
    private const string SettingsEntryName = "application-settings.json";
    private const string PreferencesEntryName = "preferences.json";

    private const string BackupContentType =
        "application/vnd.glucodesk.backup";

    private static readonly JsonSerializerOptions SerializerOptions =
        CreateSerializerOptions();

    private readonly IGlucoseHistoryService _historyService;
    private readonly IApplicationSettingsService _settingsService;
    private readonly IDesktopPresencePrivacyModeStore _privacyModeStore;

    /// <summary>
    /// Initializes the portable backup service.
    /// </summary>
    public LocalDataBackupService(
        IGlucoseHistoryService historyService,
        IApplicationSettingsService settingsService,
        IDesktopPresencePrivacyModeStore privacyModeStore)
    {
        ArgumentNullException.ThrowIfNull(historyService);
        ArgumentNullException.ThrowIfNull(settingsService);
        ArgumentNullException.ThrowIfNull(privacyModeStore);

        _historyService = historyService;
        _settingsService = settingsService;
        _privacyModeStore = privacyModeStore;
    }

    /// <inheritdoc />
    public async Task<Result<LocalDataBackupFile>> ExportAsync(
        CancellationToken cancellationToken)
    {
        try
        {
            var historyResult = await _historyService
                .GetReadingsAsync(
                    new GlucoseHistoryRequest(
                        DateTimeOffset.MinValue,
                        DateTimeOffset.MaxValue),
                    cancellationToken)
                .ConfigureAwait(false);

            if (historyResult.IsFailure)
            {
                return Result<LocalDataBackupFile>.Failure(
                    new Error(
                        "LocalBackup.HistoryReadFailed",
                        historyResult.Error.Message));
            }

            var settingsResult = await _settingsService
                .GetSettingsAsync(cancellationToken)
                .ConfigureAwait(false);

            if (settingsResult.IsFailure)
            {
                return Result<LocalDataBackupFile>.Failure(
                    new Error(
                        "LocalBackup.SettingsReadFailed",
                        settingsResult.Error.Message));
            }

            var exportedAtUtc = DateTimeOffset.UtcNow;

            var manifest = new BackupManifestDocument
            {
                SchemaVersion = CurrentSchemaVersion,
                Product = "GlucoDesk",
                ExportedAtUtc = exportedAtUtc,
                ContainsCredentials = false,
                HistoryReadingsCount =
                    historyResult.Value.Readings.Count
            };

            var history = historyResult.Value.Readings
                .OrderBy(reading => reading.Timestamp)
                .ThenBy(
                    reading => reading.Provider.ToString(),
                    StringComparer.Ordinal)
                .Select(BackupGlucoseReadingDocument.FromReading)
                .ToArray();

            var settings =
                BackupApplicationSettingsDocument.FromSettings(
                    settingsResult.Value);

            var preferences = new BackupPreferencesDocument
            {
                LanguageCode =
                    LocalizationManager.CurrentLanguageCode,
                DesktopPrivacyModeEnabled =
                    _privacyModeStore.Load()
            };

            await using var output = new MemoryStream();

            using (var archive = new ZipArchive(
                       output,
                       ZipArchiveMode.Create,
                       leaveOpen: true))
            {
                await WriteJsonEntryAsync(
                        archive,
                        ManifestEntryName,
                        manifest,
                        cancellationToken)
                    .ConfigureAwait(false);

                await WriteJsonEntryAsync(
                        archive,
                        HistoryEntryName,
                        history,
                        cancellationToken)
                    .ConfigureAwait(false);

                await WriteJsonEntryAsync(
                        archive,
                        SettingsEntryName,
                        settings,
                        cancellationToken)
                    .ConfigureAwait(false);

                await WriteJsonEntryAsync(
                        archive,
                        PreferencesEntryName,
                        preferences,
                        cancellationToken)
                    .ConfigureAwait(false);
            }

            var fileName =
                $"GlucoDesk-backup-{exportedAtUtc:yyyy-MM-dd-HHmmss}.glucodesk-backup";

            return Result<LocalDataBackupFile>.Success(
                new LocalDataBackupFile(
                    fileName,
                    BackupContentType,
                    output.ToArray()));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
            when (IsSupportedBackupException(exception))
        {
            return Result<LocalDataBackupFile>.Failure(
                new Error(
                    "LocalBackup.ExportFailed",
                    "Unable to create the local-data backup."));
        }
    }

    /// <inheritdoc />
    public async Task<Result<LocalDataImportResult>> ImportAsync(
        Stream backupStream,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(backupStream);

        if (!backupStream.CanRead)
        {
            return Result<LocalDataImportResult>.Failure(
                new Error(
                    "LocalBackup.StreamUnreadable",
                    "The selected backup file cannot be read."));
        }

        try
        {
            using var archive = new ZipArchive(
                backupStream,
                ZipArchiveMode.Read,
                leaveOpen: true);

            var manifest =
                await ReadRequiredJsonEntryAsync<BackupManifestDocument>(
                        archive,
                        ManifestEntryName,
                        cancellationToken)
                    .ConfigureAwait(false);

            var manifestValidationResult =
                ValidateManifest(manifest);

            if (manifestValidationResult.IsFailure)
            {
                return Result<LocalDataImportResult>.Failure(
                    manifestValidationResult.Error);
            }

            var history =
                await ReadRequiredJsonEntryAsync<
                        IReadOnlyCollection<BackupGlucoseReadingDocument>>(
                        archive,
                        HistoryEntryName,
                        cancellationToken)
                    .ConfigureAwait(false);

            var settings =
                await ReadRequiredJsonEntryAsync<
                        BackupApplicationSettingsDocument>(
                        archive,
                        SettingsEntryName,
                        cancellationToken)
                    .ConfigureAwait(false);

            var preferences =
                await ReadRequiredJsonEntryAsync<
                        BackupPreferencesDocument>(
                        archive,
                        PreferencesEntryName,
                        cancellationToken)
                    .ConfigureAwait(false);

            var readings = history
                .Select(document => document.ToReading())
                .ToArray();

            var historySaveResult = await _historyService
                .SaveReadingsWithSummaryAsync(
                    readings,
                    cancellationToken)
                .ConfigureAwait(false);

            if (historySaveResult.IsFailure)
            {
                return Result<LocalDataImportResult>.Failure(
                    new Error(
                        "LocalBackup.HistoryImportFailed",
                        historySaveResult.Error.Message));
            }

            var importedSettings = settings.ToSettings();

            var settingsSaveResult = await _settingsService
                .SaveSettingsAsync(
                    importedSettings,
                    cancellationToken)
                .ConfigureAwait(false);

            if (settingsSaveResult.IsFailure)
            {
                return Result<LocalDataImportResult>.Failure(
                    new Error(
                        "LocalBackup.SettingsImportFailed",
                        settingsSaveResult.Error.Message));
            }

            var languageImported =
                TryImportLanguagePreference(
                    preferences.LanguageCode);

            _privacyModeStore.Save(
                preferences.DesktopPrivacyModeEnabled);

            return Result<LocalDataImportResult>.Success(
                new LocalDataImportResult(
                    historySaveResult.Value.IncomingReadingsCount,
                    historySaveResult.Value.AddedReadingsCount,
                    historySaveResult.Value.DuplicateReadingsCount,
                    historySaveResult.Value.StoredReadingsCount,
                    SettingsImported: true,
                    LanguageImported: languageImported,
                    PrivacyPreferenceImported: true));
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (InvalidDataException)
        {
            return Result<LocalDataImportResult>.Failure(
                new Error(
                    "LocalBackup.InvalidArchive",
                    "The selected file is not a valid GlucoDesk backup."));
        }
        catch (JsonException)
        {
            return Result<LocalDataImportResult>.Failure(
                new Error(
                    "LocalBackup.InvalidJson",
                    "The GlucoDesk backup contains invalid JSON."));
        }
        catch (ArgumentException)
        {
            return Result<LocalDataImportResult>.Failure(
                new Error(
                    "LocalBackup.InvalidContent",
                    "The GlucoDesk backup contains invalid data."));
        }
        catch (Exception exception)
            when (IsSupportedBackupException(exception))
        {
            return Result<LocalDataImportResult>.Failure(
                new Error(
                    "LocalBackup.ImportFailed",
                    "Unable to import the local-data backup."));
        }
    }

    private static Result ValidateManifest(
        BackupManifestDocument manifest)
    {
        if (!string.Equals(
                manifest.Product,
                "GlucoDesk",
                StringComparison.Ordinal))
        {
            return Result.Failure(
                new Error(
                    "LocalBackup.WrongProduct",
                    "The selected backup was not created by GlucoDesk."));
        }

        if (manifest.SchemaVersion != CurrentSchemaVersion)
        {
            return Result.Failure(
                new Error(
                    "LocalBackup.UnsupportedVersion",
                    "The selected backup version is not supported."));
        }

        if (manifest.ContainsCredentials)
        {
            return Result.Failure(
                new Error(
                    "LocalBackup.UnsafeArchive",
                    "The selected backup declares credential content and cannot be imported."));
        }

        return Result.Success();
    }

    private static async Task WriteJsonEntryAsync<TValue>(
        ZipArchive archive,
        string entryName,
        TValue value,
        CancellationToken cancellationToken)
    {
        var entry = archive.CreateEntry(
            entryName,
            CompressionLevel.Optimal);

        await using var stream = entry.Open();

        await JsonSerializer.SerializeAsync(
                stream,
                value,
                SerializerOptions,
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<TValue> ReadRequiredJsonEntryAsync<TValue>(
        ZipArchive archive,
        string entryName,
        CancellationToken cancellationToken)
        where TValue : class
    {
        var entry = archive.GetEntry(entryName);

        if (entry is null)
        {
            throw new InvalidDataException(
                $"Required backup entry '{entryName}' is missing.");
        }

        await using var stream = entry.Open();

        var value = await JsonSerializer.DeserializeAsync<TValue>(
                stream,
                SerializerOptions,
                cancellationToken)
            .ConfigureAwait(false);

        return value
            ?? throw new InvalidDataException(
                $"Required backup entry '{entryName}' is empty.");
    }

    private static bool TryImportLanguagePreference(
        string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(languageCode))
        {
            return false;
        }

        var supportedLanguage =
            TranslationCatalog.SupportedLanguages.FirstOrDefault(
                language => string.Equals(
                    language.Code,
                    languageCode,
                    StringComparison.OrdinalIgnoreCase));

        if (supportedLanguage is null)
        {
            return false;
        }

        LanguagePreferenceStore.SaveLanguageCode(
            supportedLanguage.Code);

        LocalizationManager.SetLanguage(
            supportedLanguage.Code);

        return true;
    }

    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options =
            new JsonSerializerOptions(
                JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            };

        options.Converters.Add(
            new JsonStringEnumConverter<GlucoseUnit>());

        options.Converters.Add(
            new JsonStringEnumConverter<TrendDirection>());

        options.Converters.Add(
            new JsonStringEnumConverter<CgmProviderKind>());

        options.Converters.Add(
            new JsonStringEnumConverter<GlucoseDataFreshness>());

        return options;
    }

    private static bool IsSupportedBackupException(
        Exception exception)
    {
        return exception is
            IOException
            or UnauthorizedAccessException
            or NotSupportedException;
    }

    private sealed record BackupManifestDocument
    {
        public int SchemaVersion { get; init; }

        public string Product { get; init; } = string.Empty;

        public DateTimeOffset ExportedAtUtc { get; init; }

        public bool ContainsCredentials { get; init; }

        public int HistoryReadingsCount { get; init; }
    }

    private sealed record BackupPreferencesDocument
    {
        public string LanguageCode { get; init; } =
            TranslationCatalog.DefaultLanguageCode;

        public bool DesktopPrivacyModeEnabled { get; init; }
    }

    private sealed record BackupGlucoseReadingDocument
    {
        public DateTimeOffset Timestamp { get; init; }

        public decimal Amount { get; init; }

        public GlucoseUnit Unit { get; init; }

        public TrendDirection Trend { get; init; }

        public CgmProviderKind ProviderKind { get; init; }

        public GlucoseDataFreshness Freshness { get; init; }

        public static BackupGlucoseReadingDocument FromReading(
            GlucoseReading reading)
        {
            ArgumentNullException.ThrowIfNull(reading);

            return new BackupGlucoseReadingDocument
            {
                Timestamp = reading.Timestamp,
                Amount = reading.Value.Amount,
                Unit = reading.Value.Unit,
                Trend = reading.Trend,
                ProviderKind = reading.Provider,
                Freshness = reading.Freshness
            };
        }

        public GlucoseReading ToReading()
        {
            return new GlucoseReading(
                Timestamp,
                new GlucoseValue(Amount, Unit),
                Trend,
                ProviderKind,
                Freshness);
        }
    }

    private sealed record BackupApplicationSettingsDocument
    {
        public CgmProviderKind ActiveLiveProvider { get; init; }

        public CgmProviderKind HistoricalProvider { get; init; }

        public GlucoseUnit PreferredUnit { get; init; }

        public int TargetLowMgDl { get; init; }

        public int TargetHighMgDl { get; init; }

        public int ChartMaximumMgDl { get; init; }

        public TimeSpan DashboardRefreshInterval { get; init; }

        public bool GlucoseAlertsEnabled { get; init; }

        public bool LowGlucoseAlertsEnabled { get; init; }

        public bool HighGlucoseAlertsEnabled { get; init; }

        public bool NativeGlucoseNotificationsEnabled { get; init; }

        public bool GlucoseAlertPrivacyModeEnabled { get; init; }

        public TimeSpan GlucoseAlertRepeatInterval { get; init; }

        public int GlucoseAlertRequiredConsecutiveReadings { get; init; }

        public static BackupApplicationSettingsDocument FromSettings(
            ApplicationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            return new BackupApplicationSettingsDocument
            {
                ActiveLiveProvider =
                    settings.ActiveLiveProvider,
                HistoricalProvider =
                    settings.HistoricalProvider,
                PreferredUnit =
                    settings.PreferredUnit,
                TargetLowMgDl =
                    settings.TargetLowMgDl,
                TargetHighMgDl =
                    settings.TargetHighMgDl,
                ChartMaximumMgDl =
                    settings.ChartMaximumMgDl,
                DashboardRefreshInterval =
                    settings.DashboardRefreshInterval,
                GlucoseAlertsEnabled =
                    settings.GlucoseAlertsEnabled,
                LowGlucoseAlertsEnabled =
                    settings.LowGlucoseAlertsEnabled,
                HighGlucoseAlertsEnabled =
                    settings.HighGlucoseAlertsEnabled,
                NativeGlucoseNotificationsEnabled =
                    settings.NativeGlucoseNotificationsEnabled,
                GlucoseAlertPrivacyModeEnabled =
                    settings.GlucoseAlertPrivacyModeEnabled,
                GlucoseAlertRepeatInterval =
                    settings.GlucoseAlertRepeatInterval,
                GlucoseAlertRequiredConsecutiveReadings =
                    settings.GlucoseAlertRequiredConsecutiveReadings
            };
        }

        public ApplicationSettings ToSettings()
        {
            return new ApplicationSettings(
                activeLiveProvider:
                    ActiveLiveProvider,
                historicalProvider:
                    HistoricalProvider,
                preferredUnit:
                    PreferredUnit,
                targetLowMgDl:
                    TargetLowMgDl,
                targetHighMgDl:
                    TargetHighMgDl,
                dashboardRefreshInterval:
                    DashboardRefreshInterval,
                chartMaximumMgDl:
                    ChartMaximumMgDl,
                glucoseAlertsEnabled:
                    GlucoseAlertsEnabled,
                lowGlucoseAlertsEnabled:
                    LowGlucoseAlertsEnabled,
                highGlucoseAlertsEnabled:
                    HighGlucoseAlertsEnabled,
                nativeGlucoseNotificationsEnabled:
                    NativeGlucoseNotificationsEnabled,
                glucoseAlertPrivacyModeEnabled:
                    GlucoseAlertPrivacyModeEnabled,
                glucoseAlertRepeatInterval:
                    GlucoseAlertRepeatInterval,
                glucoseAlertRequiredConsecutiveReadings:
                    GlucoseAlertRequiredConsecutiveReadings);
        }
    }
}
