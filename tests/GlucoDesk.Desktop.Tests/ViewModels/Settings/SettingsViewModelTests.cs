using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Settings;

namespace GlucoDesk.Desktop.Tests.ViewModels.Settings;

public sealed class SettingsViewModelTests
{
    [Fact]
    public async Task LoadCommand_ShouldPopulateFields_WhenSettingsLoadSucceeds()
    {
        var settings = new ApplicationSettings(
            activeLiveProvider: CgmProviderKind.Mock,
            historicalProvider: CgmProviderKind.Mock,
            preferredUnit: GlucoseUnit.MgDl,
            targetLowMgDl: 80,
            targetHighMgDl: 160,
            dashboardRefreshInterval: TimeSpan.FromSeconds(45));

        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(Result<ApplicationSettings>.Success(settings)));

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasError);
        Assert.Equal("Settings loaded", viewModel.StatusMessage);
        Assert.Equal(CgmProviderKind.Mock, viewModel.SelectedLiveProvider?.Kind);
        Assert.Equal(CgmProviderKind.Mock, viewModel.SelectedHistoricalProvider?.Kind);
        Assert.Equal(GlucoseUnit.MgDl, viewModel.SelectedPreferredUnit?.Unit);
        Assert.Equal("80", viewModel.TargetLowMgDlText);
        Assert.Equal("160", viewModel.TargetHighMgDlText);
        Assert.Equal("45", viewModel.DashboardRefreshIntervalSecondsText);
    }

    [Fact]
    public async Task SaveCommand_ShouldPersistSettings_WhenFormIsValid()
    {
        var settingsService = new FakeApplicationSettingsService();
        var viewModel = new SettingsViewModel(settingsService)
        {
            TargetLowMgDlText = "75",
            TargetHighMgDlText = "170",
            DashboardRefreshIntervalSecondsText = "60"
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasError);
        Assert.NotNull(settingsService.SavedSettings);
        Assert.Equal(75, settingsService.SavedSettings.TargetLowMgDl);
        Assert.Equal(170, settingsService.SavedSettings.TargetHighMgDl);
        Assert.Equal(TimeSpan.FromSeconds(60), settingsService.SavedSettings.DashboardRefreshInterval);
        Assert.Equal("Settings saved. Restart or reload the dashboard to apply all changes.", viewModel.StatusMessage);
    }

    [Fact]
    public async Task SaveCommand_ShouldExposeValidationError_WhenTargetLowIsInvalid()
    {
        var viewModel = new SettingsViewModel(new FakeApplicationSettingsService())
        {
            TargetLowMgDlText = "abc"
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.Equal("Settings validation failed", viewModel.StatusMessage);
        Assert.Equal("Target low must be a positive integer.", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadCommand_ShouldExposeError_WhenSettingsLoadFails()
    {
        var error = new Error("Settings.LoadFailed", "Unable to load settings.");
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(Result<ApplicationSettings>.Failure(error)));

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.Equal("Unable to load settings", viewModel.StatusMessage);
        Assert.Equal("Settings.LoadFailed: Unable to load settings.", viewModel.ErrorMessage);
    }

    #region Helpers

    private sealed class FakeApplicationSettingsService : IApplicationSettingsService
    {
        private readonly Result<ApplicationSettings> _loadResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeApplicationSettingsService"/> class.
        /// </summary>
        /// <param name="loadResult">The optional load result.</param>
        public FakeApplicationSettingsService(Result<ApplicationSettings>? loadResult = null)
        {
            _loadResult = loadResult ?? Result<ApplicationSettings>.Success(ApplicationSettings.Default);
        }

        /// <summary>
        /// Gets the last saved settings.
        /// </summary>
        public ApplicationSettings? SavedSettings { get; private set; }

        /// <inheritdoc />
        public Task<Result<ApplicationSettings>> GetSettingsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_loadResult);
        }

        /// <inheritdoc />
        public Task<Result> SaveSettingsAsync(
            ApplicationSettings settings,
            CancellationToken cancellationToken)
        {
            SavedSettings = settings;

            return Task.FromResult(Result.Success());
        }
    }

    #endregion
}