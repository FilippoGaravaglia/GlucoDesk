using GlucoDesk.Application.Cgm.BackgroundSync.State.Services;
using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.Diary.Exports.Requests;
using GlucoDesk.Application.Cgm.Diary.Exports.Results;
using GlucoDesk.Application.Cgm.Diary.Exports.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Requests;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.BackgroundSync.Dispatching.Abstractions;
using GlucoDesk.Desktop.Cgm.History.Continuity.Results;
using GlucoDesk.Desktop.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Desktop.Cgm.History.Continuity.ViewModels;
using GlucoDesk.Desktop.Common.Dispatching.Abstractions;
using GlucoDesk.Desktop.Diary.Results;
using GlucoDesk.Desktop.Diary.Services.Abstractions;
using GlucoDesk.Desktop.ViewModels.Account;
using GlucoDesk.Desktop.ViewModels.BackgroundSync;
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Dashboard.Options;
using GlucoDesk.Desktop.ViewModels.Diary;
using GlucoDesk.Desktop.ViewModels.Main;
using GlucoDesk.Desktop.ViewModels.Settings;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Clients;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Credentials;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Readings;

namespace GlucoDesk.Desktop.Tests.ViewModels.Main;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_ShouldSelectDashboardByDefault()
    {
    var viewModel = CreateViewModel();
        Assert.True(viewModel.IsDashboardSelected);
        Assert.False(viewModel.IsAccountSelected);
        Assert.False(viewModel.IsSettingsSelected);
        Assert.Same(viewModel.Dashboard, viewModel.CurrentContent);
    }
    
    [Fact]
    public async Task ShowAccountCommand_ShouldSelectAccount()
    {
        var viewModel = CreateViewModel();
    
        await viewModel.ShowAccountCommand.ExecuteAsync(null);
    
        Assert.False(viewModel.IsDashboardSelected);
        Assert.True(viewModel.IsAccountSelected);
        Assert.False(viewModel.IsSettingsSelected);
        Assert.Same(viewModel.Account, viewModel.CurrentContent);
    }
    
    [Fact]
    public void ShowSettingsCommand_ShouldSelectSettings()
    {
        var viewModel = CreateViewModel();
    
        viewModel.ShowSettingsCommand.Execute(null);
    
        Assert.False(viewModel.IsDashboardSelected);
        Assert.False(viewModel.IsAccountSelected);
        Assert.True(viewModel.IsSettingsSelected);
        Assert.Same(viewModel.Settings, viewModel.CurrentContent);
    }
    
    [Fact]
    public void ShowDashboardCommand_ShouldSelectDashboard()
    {
        var viewModel = CreateViewModel();
    
        viewModel.ShowSettingsCommand.Execute(null);
        viewModel.ShowDashboardCommand.Execute(null);
    
        Assert.True(viewModel.IsDashboardSelected);
        Assert.False(viewModel.IsAccountSelected);
        Assert.False(viewModel.IsSettingsSelected);
        Assert.Same(viewModel.Dashboard, viewModel.CurrentContent);
    }
    
    #region Helpers
    
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);
    
    /// <summary>
    /// Creates a main window view model for navigation tests.
    /// </summary>
    /// <returns>The main window view model.</returns>
    private static MainWindowViewModel CreateViewModel()
    {
        var settingsService = new FakeApplicationSettingsService();
    
        return new MainWindowViewModel(
            new DashboardViewModel(
                new FakeGlucoseDataService(),
                settingsService,
                DashboardRefreshOptions.Default),
            new AccountViewModel(
                new FakeDexcomShareCredentialStore(),
                new FakeDexcomShareClient(),
                settingsService),
            new SettingsViewModel(settingsService),
            CreateBackgroundSyncStatusViewModel(),
            CreateDiaryViewModel(),
            CreateHistoryContinuitySyncStatusViewModel());
    }
    
    /// <summary>
    /// Creates a background sync status view model for navigation tests.
    /// </summary>
    /// <returns>The background sync status view model.</returns>
    private static BackgroundSyncStatusViewModel CreateBackgroundSyncStatusViewModel()
    {
        return new BackgroundSyncStatusViewModel(
            new BackgroundSyncStateService(),
            new ImmediateBackgroundSyncUiDispatcher());
    }
    
    /// <summary>
    /// Creates a diary view model for navigation tests.
    /// </summary>
    /// <returns>The diary view model.</returns>
    private static DiaryViewModel CreateDiaryViewModel()
    {
        return new DiaryViewModel(
            new FakeGlycemicDiaryService(),
            new FakeGlycemicDiaryExcelExportService(),
            new FakeGlycemicDiaryPdfExportService(),
            new FakeDiaryExportFileSaveService(),
            TimeProvider.System);
    }
    
    /// <summary>
    /// Creates a history continuity synchronization status ViewModel for navigation tests.
    /// </summary>
    /// <returns>The history continuity synchronization status ViewModel.</returns>
    private static DesktopHistoryContinuitySyncStatusViewModel CreateHistoryContinuitySyncStatusViewModel()
    {
        return new DesktopHistoryContinuitySyncStatusViewModel(
            new FakeDesktopHistoryContinuitySyncStatusStore(),
            new ImmediateDesktopUiDispatcher(),
            new FakeDesktopHistoryContinuitySyncCoordinator());
    }
    
    /// <summary>
    /// Creates a dashboard snapshot used by navigation tests.
    /// </summary>
    /// <returns>The dashboard snapshot.</returns>
    private static GlucoseDashboardSnapshot CreateSnapshot()
    {
        return new GlucoseDashboardSnapshot(
            CreateMetadata(),
            CreateReading(FixedNow),
            [
                CreateReading(FixedNow.AddMinutes(-10)),
                CreateReading(FixedNow.AddMinutes(-5))
            ],
            FixedNow,
            FixedNow,
            FixedNow,
            TimeSpan.FromMinutes(15));
    }
    
    /// <summary>
    /// Creates provider metadata used by navigation tests.
    /// </summary>
    /// <returns>The provider metadata.</returns>
    private static CgmProviderMetadata CreateMetadata()
    {
        return new CgmProviderMetadata(
            CgmProviderKind.Mock,
            "Mock CGM Provider",
            GlucoseDataFreshness.NearRealTime,
            supportsLiveReadings: true,
            supportsHistoricalReadings: true);
    }
    
    /// <summary>
    /// Creates a glucose reading for the supplied timestamp.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(DateTimeOffset timestamp)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(123, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.NearRealTime);
    }
    
    #endregion
    
    private sealed class FakeGlucoseDataService : IGlucoseDataService
    {
        /// <inheritdoc />
        public Task<Result<CgmProviderMetadata>> GetProviderMetadataAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<CgmProviderMetadata>.Success(CreateMetadata()));
        }
    
        /// <inheritdoc />
        public Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<LatestGlucoseReadingResult>.Success(
                new LatestGlucoseReadingResult(CreateReading(FixedNow), FixedNow)));
        }
    
        /// <inheritdoc />
        public Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<GlucoseReadingsResult>.Success(
                new GlucoseReadingsResult(
                    [
                        CreateReading(FixedNow.AddMinutes(-10)),
                        CreateReading(FixedNow.AddMinutes(-5))
                    ],
                    FixedNow)));
        }
    
        /// <inheritdoc />
        public Task<Result<GlucoseReadingsResult>> GetHistoricalReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            return GetRecentReadingsAsync(request, cancellationToken);
        }
    
        /// <inheritdoc />
        public Task<Result<GlucoseDashboardSnapshot>> GetDashboardSnapshotAsync(
            GlucoseDashboardRequest request,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<GlucoseDashboardSnapshot>.Success(CreateSnapshot()));
        }
    }
    
    private sealed class FakeApplicationSettingsService : IApplicationSettingsService
    {
        /// <inheritdoc />
        public Task<Result<ApplicationSettings>> GetSettingsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<ApplicationSettings>.Success(ApplicationSettings.Default));
        }
    
        /// <inheritdoc />
        public Task<Result> SaveSettingsAsync(
            ApplicationSettings settings,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result.Success());
        }
    }
    
    private sealed class FakeDexcomShareCredentialStore : IDexcomShareCredentialStore
    {
        /// <inheritdoc />
        public Task<DexcomShareCredentials?> ReadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult<DexcomShareCredentials?>(null);
        }
    
        /// <inheritdoc />
        public Task SaveAsync(
            DexcomShareCredentials credentials,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.CompletedTask;
        }
    
        /// <inheritdoc />
        public Task ClearAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.CompletedTask;
        }
    }
    
    private sealed class FakeDexcomShareClient : IDexcomShareClient
    {
        /// <inheritdoc />
        public Task<Result<string>> AuthenticateAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<string>.Success("test-session-id"));
        }
    
        /// <inheritdoc />
        public Task<Result<string>> AuthenticateAsync(
            DexcomShareOptions options,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(options);
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<string>.Success("test-session-id"));
        }
    
        /// <inheritdoc />
        public Task<Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>> GetLatestGlucoseValuesAsync(
            string sessionId,
            int minutes,
            int maxCount,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(
                Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>.Success([]));
        }
    
        /// <inheritdoc />
        public Task<Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>> GetLatestGlucoseValuesAsync(
            DexcomShareOptions options,
            int minutes,
            int maxCount,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(options);
    
            return GetLatestGlucoseValuesAsync(
                "fake-session",
                minutes,
                maxCount,
                cancellationToken);
        }
    
        /// <inheritdoc />
        public void InvalidateSession()
        {
        }
    }
    
    private sealed class FakeGlycemicDiaryService : IGlycemicDiaryService
    {
        /// <inheritdoc />
        public Task<Result<GlycemicDiaryReport>> CreateDiaryAsync(
            GlycemicDiaryRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
    
            throw new NotSupportedException("Preview generation is not used by navigation tests.");
        }
    }
    
    private sealed class FakeGlycemicDiaryExcelExportService : IGlycemicDiaryExcelExportService
    {
        /// <inheritdoc />
        public Task<Result<GlycemicDiaryExportFile>> ExportAsync(
            GlycemicDiaryExcelExportRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<GlycemicDiaryExportFile>.Success(
                new GlycemicDiaryExportFile(
                    "diary.xlsx",
                    "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
                    [1, 2, 3])));
        }
    }
    
    private sealed class FakeGlycemicDiaryPdfExportService : IGlycemicDiaryPdfExportService
    {
        /// <inheritdoc />
        public Task<Result<GlycemicDiaryExportFile>> ExportAsync(
            GlycemicDiaryPdfExportRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<GlycemicDiaryExportFile>.Success(
                new GlycemicDiaryExportFile(
                    "diary.pdf",
                    "application/pdf",
                    [1, 2, 3])));
        }
    }
    
    private sealed class FakeDiaryExportFileSaveService : IDiaryExportFileSaveService
    {
        /// <inheritdoc />
        public Task<Result<DiaryExportSaveResult>> SaveAsync(
            GlycemicDiaryExportFile file,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(file);
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<DiaryExportSaveResult>.Success(
                DiaryExportSaveResult.Saved(file.FileName)));
        }
    }
    
    private sealed class ImmediateBackgroundSyncUiDispatcher : IBackgroundSyncUiDispatcher
    {
        /// <inheritdoc />
        public void Post(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);
    
            action();
        }
    }
    
    private sealed class ImmediateDesktopUiDispatcher : IDesktopUiDispatcher
    {
        /// <inheritdoc />
        public void Post(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);
    
            action();
        }
    }
    
    private sealed class FakeDesktopHistoryContinuitySyncCoordinator : IDesktopHistoryContinuitySyncCoordinator
    {
        /// <inheritdoc />
        public Task<Result<DesktopHistoryContinuitySyncRunResult>> RunStartupSyncAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<DesktopHistoryContinuitySyncRunResult>.Success(
                DesktopHistoryContinuitySyncRunResult.Skipped(CgmHistoryContinuitySyncTrigger.Startup)));
        }
    
        /// <inheritdoc />
        public Task<Result<DesktopHistoryContinuitySyncRunResult>> RunResumeSyncAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<DesktopHistoryContinuitySyncRunResult>.Success(
                DesktopHistoryContinuitySyncRunResult.Skipped(CgmHistoryContinuitySyncTrigger.Resume)));
        }
    
        /// <inheritdoc />
        public Task<Result<DesktopHistoryContinuitySyncRunResult>> RunManualSyncAsync(
            TimeSpan lookback,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
    
            return Task.FromResult(Result<DesktopHistoryContinuitySyncRunResult>.Success(
                DesktopHistoryContinuitySyncRunResult.Skipped(CgmHistoryContinuitySyncTrigger.Manual)));
        }
    }
    
    private sealed class FakeDesktopHistoryContinuitySyncStatusStore : IDesktopHistoryContinuitySyncStatusStore
    {
        public event EventHandler<DesktopHistoryContinuitySyncStatusSnapshot>? StatusChanged
        {
            add
            {
            }
    
            remove
            {
            }
        }
    
        public DesktopHistoryContinuitySyncStatusSnapshot Current { get; private set; } =
            DesktopHistoryContinuitySyncStatusSnapshot.Idle;
    
        /// <inheritdoc />
        public void MarkRunning(CgmHistoryContinuitySyncTrigger trigger)
        {
            throw new NotSupportedException();
        }
    
        /// <inheritdoc />
        public void MarkSucceeded(
            CgmHistoryContinuitySyncTrigger trigger,
            DesktopHistoryContinuitySyncRunResult runResult)
        {
            throw new NotSupportedException();
        }
    
        /// <inheritdoc />
        public void MarkSkipped(
            CgmHistoryContinuitySyncTrigger trigger,
            string message)
        {
            throw new NotSupportedException();
        }
    
        /// <inheritdoc />
        public void MarkFailed(
            CgmHistoryContinuitySyncTrigger trigger,
            Error error)
        {
            throw new NotSupportedException();
        }
    
        /// <inheritdoc />
        public void MarkCanceled(CgmHistoryContinuitySyncTrigger trigger)
        {
            throw new NotSupportedException();
        }
    }
}
