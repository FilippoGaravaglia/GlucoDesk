using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Application.Settings.Services;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Dashboard.Options;
using GlucoDesk.Application.Cgm.Statistics.Requests;
using GlucoDesk.Application.Cgm.Statistics.Results;
using GlucoDesk.Application.Cgm.Statistics.Services.Abstractions;

namespace GlucoDesk.Desktop.Tests.ViewModels.Dashboard;

public sealed class DashboardViewModelTests
{
    [Fact]
    public void Constructor_ShouldExposeDefaultFallbackAutoRefreshInterval()
    {
        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(),
            new FakeApplicationSettingsService(),
            new DashboardRefreshOptions(TimeSpan.FromSeconds(10)));

        Assert.Equal(TimeSpan.FromSeconds(10), viewModel.AutoRefreshInterval);
        Assert.Equal("Auto-refresh every 10 second(s)", viewModel.AutoRefreshStatusText);
    }

    [Fact]
    public async Task InitializeCommand_ShouldLoadDashboardConfigurationFromSettings()
    {
        var settings = new ApplicationSettings(
            targetLowMgDl: 80,
            targetHighMgDl: 160,
            dashboardRefreshInterval: TimeSpan.FromSeconds(15));

        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(),
            new FakeApplicationSettingsService(Result<ApplicationSettings>.Success(settings)),
            DashboardRefreshOptions.Default);

        await viewModel.InitializeCommand.ExecuteAsync(null);

        Assert.Equal(TimeSpan.FromSeconds(15), viewModel.AutoRefreshInterval);
        Assert.Equal(80, viewModel.TargetLowMgDl);
        Assert.Equal(160, viewModel.TargetHighMgDl);
        Assert.Equal("Target range: 80-160 mg/dL", viewModel.TargetRangeText);
        Assert.Equal("Settings loaded", viewModel.SettingsStatusText);
        Assert.Equal("Auto-refresh every 15 second(s)", viewModel.AutoRefreshStatusText);
    }

    [Fact]
    public async Task InitializeCommand_ShouldUseFallbackDashboardConfiguration_WhenSettingsFail()
    {
        var expectedError = new Error("Settings.Failed", "Unable to load settings.");

        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(),
            new FakeApplicationSettingsService(Result<ApplicationSettings>.Failure(expectedError)),
            new DashboardRefreshOptions(TimeSpan.FromSeconds(20)));

        await viewModel.InitializeCommand.ExecuteAsync(null);

        Assert.Equal(TimeSpan.FromSeconds(20), viewModel.AutoRefreshInterval);
        Assert.Equal(70, viewModel.TargetLowMgDl);
        Assert.Equal(180, viewModel.TargetHighMgDl);
        Assert.Equal("Target range: 70-180 mg/dL", viewModel.TargetRangeText);
        Assert.Equal("Using default settings · Settings.Failed", viewModel.SettingsStatusText);
        Assert.Equal("Auto-refresh every 20 second(s)", viewModel.AutoRefreshStatusText);
    }

    [Fact]
    public async Task RefreshCommand_ShouldPopulateDashboardFields_WhenServiceSucceeds()
    {
        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(),
            new FakeApplicationSettingsService(),
            new DashboardRefreshOptions(TimeSpan.FromSeconds(10)));
    
        await viewModel.InitializeCommand.ExecuteAsync(null);
        await viewModel.RefreshCommand.ExecuteAsync(null);
    
        Assert.False(viewModel.HasError);
        Assert.Equal("Mock CGM Provider", viewModel.ProviderDisplayName);
        Assert.Equal("123 mg/dL", viewModel.LatestValueText);
        Assert.Equal("→ Stable", viewModel.TrendText);
        Assert.Equal("Near real-time", viewModel.FreshnessText);
        Assert.Equal("In range", viewModel.StatusText);
        Assert.Equal("2 readings", viewModel.RecentReadingsCountText);
        Assert.Equal(2, viewModel.ChartPoints.Count);
        Assert.Equal("Last 3H · 2 readings · 123-123 mg/dL", viewModel.ChartSummaryText);
        Assert.StartsWith("Last refresh:", viewModel.AutoRefreshStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RefreshCommand_ShouldExposeError_WhenServiceFails()
    {
        var expectedError = new Error("Dashboard.Failed", "Unable to build dashboard.");

        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(Result<GlucoseDashboardSnapshot>.Failure(expectedError)),
            new FakeApplicationSettingsService(),
            DashboardRefreshOptions.Default);

        await viewModel.InitializeCommand.ExecuteAsync(null);
        await viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.Equal(
            "Unable to build dashboard. (Dashboard.Failed)",
            viewModel.ErrorMessage);
        Assert.Equal("Unable to refresh glucose data", viewModel.StatusText);
    }

    [Fact]
    public void SettingsChanged_ShouldApplyDashboardConfiguration()
    {
        var notifier = new ApplicationSettingsChangeNotifier();
        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(),
            new FakeApplicationSettingsService(),
            new DashboardRefreshOptions(TimeSpan.FromSeconds(10)),
            notifier);
    
        var settings = new ApplicationSettings(
            targetLowMgDl: 85,
            targetHighMgDl: 155,
            dashboardRefreshInterval: TimeSpan.FromSeconds(20));
    
        notifier.NotifySettingsChanged(settings);
    
        Assert.Equal(TimeSpan.FromSeconds(20), viewModel.AutoRefreshInterval);
        Assert.Equal(85, viewModel.TargetLowMgDl);
        Assert.Equal(155, viewModel.TargetHighMgDl);
        Assert.Equal("Target range: 85-155 mg/dL", viewModel.TargetRangeText);
        Assert.Equal("Settings updated", viewModel.SettingsStatusText);
        Assert.Equal("Auto-refresh every 20 second(s)", viewModel.AutoRefreshStatusText);
    }

    [Fact]
    public async Task RefreshCommand_ShouldPersistDashboardReadingsToHistory_WhenHistoryServiceIsConfigured()
    {
        var historyService = new FakeGlucoseHistoryService();

        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(),
            new FakeApplicationSettingsService(),
            DashboardRefreshOptions.Default,
            settingsChangeNotifier: null,
            historyService);

        await viewModel.InitializeCommand.ExecuteAsync(null);
        await viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.Equal(3, historyService.SavedReadings.Count);
        Assert.Equal("History updated: 3 new reading(s), 0 duplicate(s), 3 stored.", viewModel.HistoryStatusText);
    }

    [Fact]
    public async Task RefreshCommand_ShouldNotFailDashboard_WhenHistoryPersistenceFails()
    {
        var historyService = new FakeGlucoseHistoryService
        {
            SaveReadingsWithSummaryResult =
                Result<GlucoseHistorySaveResult>.Failure(
                    new Error("History.SaveFailed", "Unable to save glucose history."))
        };

        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(),
            new FakeApplicationSettingsService(),
            DashboardRefreshOptions.Default,
            settingsChangeNotifier: null,
            historyService);

        await viewModel.InitializeCommand.ExecuteAsync(null);
        await viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasError);
        Assert.Equal("History update failed · History.SaveFailed", viewModel.HistoryStatusText);
        Assert.Equal("Mock CGM Provider", viewModel.ProviderDisplayName);
    }

    [Fact]
    public async Task RefreshCommand_ShouldUpdateStatistics_WhenStatisticsServiceIsConfigured()
    {
        var glucoseDataService = new FakeGlucoseDataService();
        var historyService = new FakeGlucoseHistoryService();
        var statisticsService = new FakeGlucoseStatisticsService();

        var viewModel = new DashboardViewModel(
            glucoseDataService: glucoseDataService,
            settingsService: new FakeApplicationSettingsService(),
            refreshOptions: DashboardRefreshOptions.Default,
            settingsChangeNotifier: null,
            glucoseHistoryService: historyService,
            glucoseStatisticsService: statisticsService);

        await viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.True(viewModel.IsStatisticsEnabled);
        Assert.True(viewModel.HasStatisticsData);
        Assert.Equal("120 mg/dL", viewModel.StatisticsAverageGlucoseText);
        Assert.Equal("100%", viewModel.StatisticsTimeInRangeText);
        Assert.Equal("0%", viewModel.StatisticsBelowRangeText);
        Assert.Equal("0%", viewModel.StatisticsAboveRangeText);
        Assert.Equal("3 analyzed", viewModel.StatisticsReadingsAnalyzedText);
        Assert.NotNull(statisticsService.LastRequest);
    }

    #region Helpers

    /// <summary>
    /// Fake glucose statistics service used by dashboard view model tests.
    /// </summary>
    private sealed class FakeGlucoseStatisticsService : IGlucoseStatisticsService
    {
        /// <summary>
        /// Gets or sets the statistics result returned by the fake service.
        /// </summary>
        public Result<GlucoseStatisticsResult> StatisticsResult { get; set; } =
            Result<GlucoseStatisticsResult>.Success(
                new GlucoseStatisticsResult(
                    new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero),
                    new DateTimeOffset(2026, 6, 14, 9, 0, 0, TimeSpan.Zero),
                    GlucoseUnit.MgDl,
                    includeMockData: true,
                    loadedReadingsCount: 3,
                    analyzedReadingsCount: 3,
                    ignoredMockReadingsCount: 0,
                    ignoredDifferentUnitReadingsCount: 0,
                    averageGlucose: 120,
                    minimumGlucose: 100,
                    maximumGlucose: 140,
                    belowRangeCount: 0,
                    inRangeCount: 3,
                    aboveRangeCount: 0,
                    firstReadingAt: new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero),
                    lastReadingAt: new DateTimeOffset(2026, 6, 14, 9, 0, 0, TimeSpan.Zero)));

        /// <summary>
        /// Gets the last statistics request.
        /// </summary>
        public GlucoseStatisticsRequest? LastRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result<GlucoseStatisticsResult>> CalculateAsync(
            GlucoseStatisticsRequest request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(StatisticsResult);
        }
    }

    private static readonly DateTimeOffset FixedNow = new(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

   private sealed class FakeGlucoseDataService : IGlucoseDataService
{
    private readonly Result<GlucoseDashboardSnapshot>? _dashboardResult;

    /// <summary>
    /// Initializes a new instance of the <see cref="FakeGlucoseDataService"/> class.
    /// </summary>
    /// <param name="dashboardResult">The optional dashboard result returned by the fake service.</param>
    public FakeGlucoseDataService(Result<GlucoseDashboardSnapshot>? dashboardResult = null)
    {
        _dashboardResult = dashboardResult;
    }

    /// <inheritdoc />
    public Task<Result<CgmProviderMetadata>> GetProviderMetadataAsync(CancellationToken cancellationToken)
    {
        return Task.FromResult(Result<CgmProviderMetadata>.Success(CreateMetadata()));
    }

    public Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;
    
        return Task.FromResult(Result<LatestGlucoseReadingResult>.Success(
            new LatestGlucoseReadingResult(CreateReading(now), now)));
    }

    /// <inheritdoc />
    public Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        return Task.FromResult(Result<GlucoseReadingsResult>.Success(
            new GlucoseReadingsResult(
                [
                    CreateReading(now.AddMinutes(-10)),
                    CreateReading(now.AddMinutes(-5))
                ],
                now)));
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
        if (_dashboardResult is not null)
        {
            return Task.FromResult(_dashboardResult);
        }

        return Task.FromResult(Result<GlucoseDashboardSnapshot>.Success(CreateLiveSnapshot()));
    }

    #region Helpers

    /// <summary>
    /// Creates a dashboard snapshot with timestamps relative to the current time.
    /// </summary>
    /// <returns>The dashboard snapshot.</returns>
    private static GlucoseDashboardSnapshot CreateLiveSnapshot()
    {
        var now = DateTimeOffset.UtcNow;

        return new GlucoseDashboardSnapshot(
            metadata: CreateMetadata(),
            latestReading: CreateReading(now),
            recentReadings:
            [
                CreateReading(now.AddMinutes(-10)),
                CreateReading(now.AddMinutes(-5))
            ],
            latestReadingRetrievedAt: now,
            recentReadingsRetrievedAt: now,
            snapshotCreatedAt: now,
            staleThreshold: TimeSpan.FromMilliseconds(50));
    }

        #endregion
    }

    private sealed class FakeApplicationSettingsService : IApplicationSettingsService
    {
        private readonly Result<ApplicationSettings> _settingsResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeApplicationSettingsService"/> class.
        /// </summary>
        /// <param name="settingsResult">The optional settings result returned by the fake service.</param>
        public FakeApplicationSettingsService(Result<ApplicationSettings>? settingsResult = null)
        {
            _settingsResult = settingsResult ?? Result<ApplicationSettings>.Success(ApplicationSettings.Default);
        }

        /// <inheritdoc />
        public Task<Result<ApplicationSettings>> GetSettingsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_settingsResult);
        }

        /// <inheritdoc />
        public Task<Result> SaveSettingsAsync(
            ApplicationSettings settings,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }
    }

    /// <summary>
    /// Creates a dashboard snapshot used by view model tests.
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
    /// Creates provider metadata used by view model tests.
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

    private sealed class FakeGlucoseHistoryService : IGlucoseHistoryService
    {
        private readonly Result _saveResult;

        /// <summary>
        /// Gets the readings saved through the detailed history save method.
        /// </summary>
        public IReadOnlyCollection<GlucoseReading> SavedReadingsWithSummary { get; private set; } = [];

        /// <summary>
        /// Gets or sets the detailed history save result returned by the fake service.
        /// </summary>
        public Result<GlucoseHistorySaveResult>? SaveReadingsWithSummaryResult { get; set; }

        /// <summary>
        /// Gets the saved readings.
        /// </summary>
        public IReadOnlyCollection<GlucoseReading> SavedReadings { get; private set; } = [];
        
        /// <summary>
        /// Gets or sets the save result returned by the fake service.
        /// </summary>
        public Result SaveResult { get; set; } = Result.Success();
    
        /// <inheritdoc />
        public Task<Result<GlucoseHistorySaveResult>> SaveReadingsWithSummaryAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            SavedReadings = readings;
            SavedReadingsWithSummary = readings;

            if (SaveReadingsWithSummaryResult is not null)
            {
                return Task.FromResult(SaveReadingsWithSummaryResult);
            }

            if (SaveResult.IsFailure)
            {
                return Task.FromResult(Result<GlucoseHistorySaveResult>.Failure(SaveResult.Error));
            }

            var result = new GlucoseHistorySaveResult(
                readings.FirstOrDefault()?.Provider ?? CgmProviderKind.Unknown,
                readings.Count,
                readings.Count,
                0,
                readings.Count);

            return Task.FromResult(Result<GlucoseHistorySaveResult>.Success(result));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeGlucoseHistoryService"/> class.
        /// </summary>
        /// <param name="saveResult">The optional save result.</param>
        public FakeGlucoseHistoryService(Result? saveResult = null)
        {
            _saveResult = saveResult ?? Result.Success();
        }
       
        /// <inheritdoc />
        public Task<Result> SaveReadingsAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            SavedReadings = readings;

            return Task.FromResult(SaveResult);
        }
    
        /// <inheritdoc />
        public Task<Result<GlucoseHistoryResult>> GetReadingsAsync(
            GlucoseHistoryRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<GlucoseHistoryResult>.Success(new GlucoseHistoryResult([])));
        }
    }

    #endregion
}