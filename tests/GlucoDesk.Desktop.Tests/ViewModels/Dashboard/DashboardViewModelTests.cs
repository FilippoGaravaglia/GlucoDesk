using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Dashboard.Options;

namespace GlucoDesk.Desktop.Tests.ViewModels.Dashboard;

public sealed class DashboardViewModelTests
{
    [Fact]
    public void Constructor_ShouldExposeConfiguredAutoRefreshInterval()
    {
        var options = new DashboardRefreshOptions(TimeSpan.FromSeconds(10));

        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(),
            options);

        Assert.Equal(TimeSpan.FromSeconds(10), viewModel.AutoRefreshInterval);
        Assert.Equal("Auto-refresh every 10 second(s)", viewModel.AutoRefreshStatusText);
    }

    [Fact]
    public async Task RefreshCommand_ShouldPopulateDashboardFields_WhenServiceSucceeds()
    {
        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(),
            new DashboardRefreshOptions(TimeSpan.FromSeconds(10)));

        await viewModel.RefreshCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasError);
        Assert.Equal("Mock CGM Provider", viewModel.ProviderDisplayName);
        Assert.Equal("123 mg/dL", viewModel.LatestValueText);
        Assert.Equal("→ Stable", viewModel.TrendText);
        Assert.Equal("Near real-time", viewModel.FreshnessText);
        Assert.Equal("In range", viewModel.StatusText);
        Assert.Equal("2 readings", viewModel.RecentReadingsCountText);
        Assert.StartsWith("Last refresh:", viewModel.AutoRefreshStatusText, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RefreshCommand_ShouldExposeError_WhenServiceFails()
    {
        var expectedError = new Error("Dashboard.Failed", "Unable to build dashboard.");

        var viewModel = new DashboardViewModel(
            new FakeGlucoseDataService(Result<GlucoseDashboardSnapshot>.Failure(expectedError)),
            DashboardRefreshOptions.Default);

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