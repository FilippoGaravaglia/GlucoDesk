using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Cgm.WidgetState.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Dashboard.Options;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Desktop.Tests.Localization;

namespace GlucoDesk.Desktop.Tests.ViewModels.Dashboard;

public sealed class DashboardViewModelWidgetStateTests : EnglishLocalizationTestBase
{
    [Fact]
    public async Task RefreshCommand_ShouldPublishLatestReadingToWidgetState_WhenSnapshotHasLatestReading()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var latestReading = CreateReading(timestamp, 123m);

        var widgetStatePublisher = new FakeWidgetStatePublisher();

        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(
                Result<GlucoseDashboardSnapshot>.Success(
                    CreateSnapshot(latestReading, [latestReading]))),
            new FakeApplicationSettingsService(),
            DashboardRefreshOptions.Default,
            widgetStatePublisher: widgetStatePublisher);

        // Act
        await viewModel.RefreshCommand.ExecuteAsync(null);

        // Assert
        Assert.Same(latestReading, widgetStatePublisher.PublishedReading);
        Assert.False(widgetStatePublisher.PublishedUnavailable);
    }

    [Fact]
    public async Task RefreshCommand_ShouldPublishUnavailableWidgetState_WhenSnapshotHasNoLatestReading()
    {
        // Arrange
        var widgetStatePublisher = new FakeWidgetStatePublisher();

        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(
                Result<GlucoseDashboardSnapshot>.Success(
                    CreateSnapshot(null, []))),
            new FakeApplicationSettingsService(),
            DashboardRefreshOptions.Default,
            widgetStatePublisher: widgetStatePublisher);

        // Act
        await viewModel.RefreshCommand.ExecuteAsync(null);

        // Assert
        Assert.Null(widgetStatePublisher.PublishedReading);
        Assert.True(widgetStatePublisher.PublishedUnavailable);
        Assert.Equal(CgmProviderKind.DexcomShare, widgetStatePublisher.UnavailableProviderKind);
    }

    [Fact]
    public async Task RefreshCommand_ShouldNotFailDashboard_WhenWidgetStatePublishingFails()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var latestReading = CreateReading(timestamp, 123m);

        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(
                Result<GlucoseDashboardSnapshot>.Success(
                    CreateSnapshot(latestReading, [latestReading]))),
            new FakeApplicationSettingsService(),
            DashboardRefreshOptions.Default,
            widgetStatePublisher: new FailingWidgetStatePublisher());

        // Act
        await viewModel.RefreshCommand.ExecuteAsync(null);

        // Assert
        Assert.False(viewModel.HasError);
        Assert.Equal("In range", viewModel.StatusText);
    }

    [Fact]
    public async Task RefreshCommand_ShouldPublishUnavailableWidgetState_WhenDashboardRefreshFails()
    {
        // Arrange
        var widgetStatePublisher = new FakeWidgetStatePublisher();

        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(
                Result<GlucoseDashboardSnapshot>.Failure(
                    new Error("Dashboard.Failed", "Dashboard refresh failed."))),
            new FakeApplicationSettingsService(),
            DashboardRefreshOptions.Default,
            widgetStatePublisher: widgetStatePublisher);

        // Act
        await viewModel.RefreshCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.True(widgetStatePublisher.PublishedUnavailable);
        Assert.Equal(CgmProviderKind.Unknown, widgetStatePublisher.UnavailableProviderKind);
    }

    #region Helpers

    /// <summary>
    /// Creates a dashboard snapshot for widget state tests.
    /// </summary>
    /// <param name="latestReading">The latest glucose reading.</param>
    /// <param name="recentReadings">The recent glucose readings.</param>
    /// <returns>The dashboard snapshot.</returns>
    private static GlucoseDashboardSnapshot CreateSnapshot(
        GlucoseReading? latestReading,
        IReadOnlyCollection<GlucoseReading> recentReadings)
    {
        var snapshotCreatedAt = new DateTimeOffset(
            2026,
            6,
            19,
            10,
            1,
            0,
            TimeSpan.Zero);

        return new GlucoseDashboardSnapshot(
            new CgmProviderMetadata(
                CgmProviderKind.DexcomShare,
                "Dexcom Share",
                GlucoseDataFreshness.NearRealTime,
                true,
                true),
            latestReading,
            recentReadings,
            snapshotCreatedAt,
            snapshotCreatedAt,
            snapshotCreatedAt,
            TimeSpan.FromMinutes(15));
    }

    /// <summary>
    /// Creates a glucose reading for tests.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <param name="valueMgDl">The glucose value in mg/dL.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(
        DateTimeOffset timestamp,
        decimal valueMgDl)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(valueMgDl, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.DexcomShare,
            GlucoseDataFreshness.NearRealTime);
    }

    #endregion

   private sealed class FakeGlucoseDataService : IGlucoseDataService
    {
        private readonly Result<GlucoseDashboardSnapshot> _result;

        public FakeGlucoseDataService(Result<GlucoseDashboardSnapshot> result)
        {
            _result = result;
        }

        /// <inheritdoc />
        public Task<Result<CgmProviderMetadata>> GetProviderMetadataAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_result.IsFailure)
            {
                return Task.FromResult(Result<CgmProviderMetadata>.Failure(_result.Error));
            }

            return Task.FromResult(Result<CgmProviderMetadata>.Success(_result.Value.Metadata));
        }

        /// <inheritdoc />
        public Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (_result.IsFailure)
            {
                return Task.FromResult(Result<LatestGlucoseReadingResult>.Failure(_result.Error));
            }

            return Task.FromResult(Result<LatestGlucoseReadingResult>.Success(
                new LatestGlucoseReadingResult(
                    _result.Value.LatestReading,
                    _result.Value.LatestReadingRetrievedAt)));
        }

        /// <inheritdoc />
        public Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            if (_result.IsFailure)
            {
                return Task.FromResult(Result<GlucoseReadingsResult>.Failure(_result.Error));
            }

            return Task.FromResult(Result<GlucoseReadingsResult>.Success(
                new GlucoseReadingsResult(
                    _result.Value.RecentReadings,
                    _result.Value.RecentReadingsRetrievedAt)));
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
            ArgumentNullException.ThrowIfNull(request);
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(_result);
        }
    }

    private sealed class FakeApplicationSettingsService : IApplicationSettingsService
    {
        public Task<Result<ApplicationSettings>> GetSettingsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(
                Result<ApplicationSettings>.Success(ApplicationSettings.Default));
        }

        public Task<Result> SaveSettingsAsync(
            ApplicationSettings settings,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FakeWidgetStatePublisher : IWidgetStatePublisher
    {
        public GlucoseReading? PublishedReading { get; private set; }

        public bool PublishedUnavailable { get; private set; }

        public CgmProviderKind UnavailableProviderKind { get; private set; }

        public Task<Result> PublishReadingAsync(
            GlucoseReading reading,
            CancellationToken cancellationToken)
        {
            PublishedReading = reading;
            return Task.FromResult(Result.Success());
        }

        public Task<Result> PublishLatestReadingAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CgmProviderKind providerKind,
            CancellationToken cancellationToken)
        {
            PublishedReading = readings
                .OrderByDescending(reading => reading.Timestamp)
                .FirstOrDefault();

            return Task.FromResult(Result.Success());
        }

        public Task<Result> PublishUnavailableAsync(
            CgmProviderKind providerKind,
            string? statusMessage,
            CancellationToken cancellationToken)
        {
            PublishedUnavailable = true;
            UnavailableProviderKind = providerKind;

            return Task.FromResult(Result.Success());
        }

        public Task<Result> ClearAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FailingWidgetStatePublisher : IWidgetStatePublisher
    {
        public Task<Result> PublishReadingAsync(
            GlucoseReading reading,
            CancellationToken cancellationToken)
        {
            throw new IOException("Widget state file is unavailable.");
        }

        public Task<Result> PublishLatestReadingAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CgmProviderKind providerKind,
            CancellationToken cancellationToken)
        {
            throw new IOException("Widget state file is unavailable.");
        }

        public Task<Result> PublishUnavailableAsync(
            CgmProviderKind providerKind,
            string? statusMessage,
            CancellationToken cancellationToken)
        {
            throw new IOException("Widget state file is unavailable.");
        }

        public Task<Result> ClearAsync(CancellationToken cancellationToken)
        {
            throw new IOException("Widget state file is unavailable.");
        }
    }
}