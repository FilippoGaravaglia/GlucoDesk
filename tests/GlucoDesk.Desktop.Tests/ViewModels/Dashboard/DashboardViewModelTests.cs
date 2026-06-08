using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
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
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Dashboard.Options;

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
        Assert.Equal("2 readings · 123-123 mg/dL", viewModel.ChartSummaryText);
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
        Assert.Equal("Dashboard.Failed: Unable to build dashboard.", viewModel.ErrorMessage);
        Assert.Equal("Unable to refresh glucose data", viewModel.StatusText);
    }

    #region Helpers

    private static readonly DateTimeOffset FixedNow = new(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

    private sealed class FakeGlucoseDataService : IGlucoseDataService
    {
        private readonly Result<GlucoseDashboardSnapshot> _dashboardResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeGlucoseDataService"/> class.
        /// </summary>
        /// <param name="dashboardResult">The optional dashboard result returned by the fake service.</param>
        public FakeGlucoseDataService(Result<GlucoseDashboardSnapshot>? dashboardResult = null)
        {
            _dashboardResult = dashboardResult ?? Result<GlucoseDashboardSnapshot>.Success(CreateSnapshot());
        }

        /// <inheritdoc />
        public Task<Result<CgmProviderMetadata>> GetProviderMetadataAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<CgmProviderMetadata>.Success(CreateMetadata()));
        }

        /// <inheritdoc />
        public Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<LatestGlucoseReadingResult>.Success(
                new LatestGlucoseReadingResult(CreateReading(FixedNow), FixedNow)));
        }

        /// <inheritdoc />
        public Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
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
            return Task.FromResult(_dashboardResult);
        }
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

    #endregion
}