using System.IO.Compression;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.DataBackup.Services;
using GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.Tests.DataBackup.Services;

public sealed class LocalDataBackupServiceTests
{
    [Fact]
    public async Task ExportAsync_ShouldCreateVersionedArchiveWithoutCredentials()
    {
        var historyService = new FakeHistoryService
        {
            Readings =
            [
                CreateReading(
                    new DateTimeOffset(
                        2026,
                        7,
                        21,
                        10,
                        0,
                        0,
                        TimeSpan.Zero),
                    118m)
            ]
        };

        var settingsService = new FakeSettingsService(
            new ApplicationSettings(
                preferredUnit: GlucoseUnit.MgDl,
                targetLowMgDl: 70,
                targetHighMgDl: 180));

        var privacyStore = new FakePrivacyModeStore
        {
            Value = true
        };

        var service = new LocalDataBackupService(
            historyService,
            settingsService,
            privacyStore);

        var result = await service.ExportAsync(
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.EndsWith(
            ".glucodesk-backup",
            result.Value.FileName,
            StringComparison.OrdinalIgnoreCase);

        Assert.NotEmpty(result.Value.Content);

        await using var stream =
            new MemoryStream(result.Value.Content);

        using var archive = new ZipArchive(
            stream,
            ZipArchiveMode.Read);

        Assert.NotNull(
            archive.GetEntry("manifest.json"));

        Assert.NotNull(
            archive.GetEntry("glucose-history.json"));

        Assert.NotNull(
            archive.GetEntry("application-settings.json"));

        Assert.NotNull(
            archive.GetEntry("preferences.json"));

        var manifestText = await ReadEntryTextAsync(
            archive,
            "manifest.json");

        Assert.Contains(
            "\"containsCredentials\": false",
            manifestText,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ImportAsync_ShouldMergeHistoryAndRestorePreferences()
    {
        LocalizationManager.SetLanguageForCurrentProcess("en");

        var exportHistoryService = new FakeHistoryService
        {
            Readings =
            [
                CreateReading(
                    new DateTimeOffset(
                        2026,
                        7,
                        21,
                        10,
                        0,
                        0,
                        TimeSpan.Zero),
                    118m),

                CreateReading(
                    new DateTimeOffset(
                        2026,
                        7,
                        21,
                        10,
                        5,
                        0,
                        TimeSpan.Zero),
                    124m)
            ]
        };

        var exportSettings = new ApplicationSettings(
            preferredUnit: GlucoseUnit.MmolL,
            targetLowMgDl: 75,
            targetHighMgDl: 170);

        var exportService = new LocalDataBackupService(
            exportHistoryService,
            new FakeSettingsService(exportSettings),
            new FakePrivacyModeStore
            {
                Value = true
            });

        var exportResult = await exportService.ExportAsync(
            CancellationToken.None);

        Assert.True(exportResult.IsSuccess);

        var importHistoryService = new FakeHistoryService
        {
            SaveSummaryResult =
                Result<GlucoseHistorySaveResult>.Success(
                    new GlucoseHistorySaveResult(
                        CgmProviderKind.DexcomShare,
                        incomingReadingsCount: 2,
                        addedReadingsCount: 1,
                        duplicateReadingsCount: 1,
                        storedReadingsCount: 3))
        };

        var importSettingsService =
            new FakeSettingsService(
                ApplicationSettings.Default);

        var importPrivacyStore =
            new FakePrivacyModeStore();

        var importService = new LocalDataBackupService(
            importHistoryService,
            importSettingsService,
            importPrivacyStore);

        await using var backupStream =
            new MemoryStream(
                exportResult.Value.Content);

        var importResult = await importService.ImportAsync(
            backupStream,
            CancellationToken.None);

        Assert.True(importResult.IsSuccess);

        Assert.Equal(
            2,
            importResult.Value.IncomingReadingsCount);

        Assert.Equal(
            1,
            importResult.Value.AddedReadingsCount);

        Assert.Equal(
            1,
            importResult.Value.DuplicateReadingsCount);

        Assert.Equal(
            3,
            importResult.Value.StoredReadingsCount);

        Assert.True(
            importResult.Value.SettingsImported);

        Assert.True(
            importResult.Value.PrivacyPreferenceImported);

        Assert.NotNull(
            importSettingsService.SavedSettings);

        Assert.Equal(
            GlucoseUnit.MmolL,
            importSettingsService
                .SavedSettings!
                .PreferredUnit);

        Assert.True(
            importPrivacyStore.Value);

        Assert.Equal(
            2,
            importHistoryService
                .SavedReadings
                .Count);
    }

    [Fact]
    public async Task ImportAsync_ShouldRejectInvalidArchive()
    {
        var service = new LocalDataBackupService(
            new FakeHistoryService(),
            new FakeSettingsService(
                ApplicationSettings.Default),
            new FakePrivacyModeStore());

        await using var stream =
            new MemoryStream(
                "not-a-valid-backup"u8.ToArray());

        var result = await service.ImportAsync(
            stream,
            CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "LocalBackup.InvalidArchive",
            result.Error.Code);
    }

    [Fact]
    public async Task ImportAsync_ShouldRejectUnreadableStream()
    {
        var service = new LocalDataBackupService(
            new FakeHistoryService(),
            new FakeSettingsService(
                ApplicationSettings.Default),
            new FakePrivacyModeStore());

        await using var stream =
            new WriteOnlyStream();

        var result = await service.ImportAsync(
            stream,
            CancellationToken.None);

        Assert.True(result.IsFailure);

        Assert.Equal(
            "LocalBackup.StreamUnreadable",
            result.Error.Code);
    }

    private static GlucoseReading CreateReading(
        DateTimeOffset timestamp,
        decimal amount)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(
                amount,
                GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.DexcomShare,
            GlucoseDataFreshness.NearRealTime);
    }

    private static async Task<string> ReadEntryTextAsync(
        ZipArchive archive,
        string entryName)
    {
        var entry = archive.GetEntry(entryName);

        Assert.NotNull(entry);

        await using var stream = entry.Open();

        using var reader = new StreamReader(stream);

        return await reader.ReadToEndAsync();
    }

    private sealed class FakeHistoryService :
        IGlucoseHistoryService
    {
        public IReadOnlyCollection<GlucoseReading> Readings
        {
            get;
            init;
        } = [];

        public IReadOnlyCollection<GlucoseReading> SavedReadings
        {
            get;
            private set;
        } = [];

        public Result<GlucoseHistorySaveResult>
            SaveSummaryResult
        {
            get;
            init;
        } =
            Result<GlucoseHistorySaveResult>.Success(
                new GlucoseHistorySaveResult(
                    CgmProviderKind.Unknown,
                    incomingReadingsCount: 0,
                    addedReadingsCount: 0,
                    duplicateReadingsCount: 0,
                    storedReadingsCount: 0));

        public Task<Result> SaveReadingsAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            SavedReadings = readings;

            return Task.FromResult(
                Result.Success());
        }

        public Task<Result<GlucoseHistorySaveResult>>
            SaveReadingsWithSummaryAsync(
                IReadOnlyCollection<GlucoseReading> readings,
                CancellationToken cancellationToken)
        {
            SavedReadings = readings;

            return Task.FromResult(
                SaveSummaryResult);
        }

        public Task<Result<GlucoseHistoryResult>>
            GetReadingsAsync(
                GlucoseHistoryRequest request,
                CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Result<GlucoseHistoryResult>.Success(
                    new GlucoseHistoryResult(
                        Readings)));
        }
    }

    private sealed class FakeSettingsService :
        IApplicationSettingsService
    {
        private readonly ApplicationSettings _settings;

        public FakeSettingsService(
            ApplicationSettings settings)
        {
            _settings = settings;
        }

        public ApplicationSettings? SavedSettings
        {
            get;
            private set;
        }

        public Task<Result<ApplicationSettings>>
            GetSettingsAsync(
                CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Result<ApplicationSettings>.Success(
                    _settings));
        }

        public Task<Result> SaveSettingsAsync(
            ApplicationSettings settings,
            CancellationToken cancellationToken)
        {
            SavedSettings = settings;

            return Task.FromResult(
                Result.Success());
        }
    }

    private sealed class FakePrivacyModeStore :
        IDesktopPresencePrivacyModeStore
    {
        public bool Value { get; set; }

        public bool Load()
        {
            return Value;
        }

        public void Save(bool isEnabled)
        {
            Value = isEnabled;
        }
    }

    private sealed class WriteOnlyStream : Stream
    {
        public override bool CanRead => false;

        public override bool CanSeek => false;

        public override bool CanWrite => true;

        public override long Length =>
            throw new NotSupportedException();

        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush()
        {
        }

        public override int Read(
            byte[] buffer,
            int offset,
            int count)
        {
            throw new NotSupportedException();
        }

        public override long Seek(
            long offset,
            SeekOrigin origin)
        {
            throw new NotSupportedException();
        }

        public override void SetLength(
            long value)
        {
        }

        public override void Write(
            byte[] buffer,
            int offset,
            int count)
        {
        }
    }
}
