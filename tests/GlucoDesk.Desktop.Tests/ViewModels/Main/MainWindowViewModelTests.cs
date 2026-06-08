using GlucoDesk.Application.Cgm.Dashboard.Requests;
using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Dashboard.Options;
using GlucoDesk.Desktop.ViewModels.Main;
using GlucoDesk.Desktop.ViewModels.Settings;

namespace GlucoDesk.Desktop.Tests.ViewModels.Main;

public sealed class MainWindowViewModelTests
{
    [Fact]
    public void Constructor_ShouldSelectDashboardByDefault()
    {
        var viewModel = CreateViewModel();

        Assert.True(viewModel.IsDashboardSelected);
        Assert.False(viewModel.IsSettingsSelected);
        Assert.Same(viewModel.Dashboard, viewModel.CurrentContent);
    }

    [Fact]
    public void ShowSettingsCommand_ShouldSelectSettings()
    {
        var viewModel = CreateViewModel();

        viewModel.ShowSettingsCommand.Execute(null);

        Assert.False(viewModel.IsDashboardSelected);
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
            new SettingsViewModel(settingsService));
    }

    private sealed class FakeGlucoseDataService : IGlucoseDataService
    {
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
            return Task.FromResult(Result<GlucoseDashboardSnapshot>.Success(CreateSnapshot()));
        }
    }

    private sealed class FakeApplicationSettingsService : IApplicationSettingsService
    {
        /// <inheritdoc />
        public Task<Result<ApplicationSettings>> GetSettingsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<ApplicationSettings>.Success(ApplicationSettings.Default));
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
}