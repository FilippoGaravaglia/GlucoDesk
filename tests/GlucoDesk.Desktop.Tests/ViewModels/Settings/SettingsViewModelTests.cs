using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Settings;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Services;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Models;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Services;

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
            new FakeApplicationSettingsService(Result<ApplicationSettings>.Success(settings)),
            [new FakeMetadataProvider(CgmProviderKind.Mock, "Mock")]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasError);
        Assert.Equal("Settings loaded", viewModel.StatusMessage);
        Assert.Equal(CgmProviderKind.Mock, viewModel.SelectedLiveProvider?.Kind);
        Assert.Equal(CgmProviderKind.Mock, viewModel.SelectedHistoricalProvider?.Kind);
        Assert.Equal(GlucoseUnit.MgDl, viewModel.SelectedPreferredUnit?.Unit);
        Assert.Equal("80", viewModel.TargetLowMgDlText);
        Assert.Equal("160", viewModel.TargetHighMgDlText);
        Assert.Equal("45", viewModel.DashboardRefreshIntervalSecondsText);
        Assert.Contains("Mock", viewModel.ProviderAvailabilityStatusText);
    }

    [Fact]
    public async Task LoadCommand_ShouldMarkDexcomAsAvailable_WhenDexcomMetadataProviderIsRegistered()
    {
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(),
            [
                new FakeMetadataProvider(CgmProviderKind.Mock, "Mock"),
                new FakeMetadataProvider(CgmProviderKind.DexcomSandbox, "Dexcom Sandbox")
            ]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        var dexcomOption = viewModel.ProviderOptions.Single(option =>
            option.Kind == CgmProviderKind.DexcomSandbox);

        Assert.True(dexcomOption.IsAvailable);
        Assert.Equal("Dexcom Sandbox", dexcomOption.DisplayName);
        Assert.Equal("Dexcom Sandbox", dexcomOption.DisplayLabel);
        Assert.Contains("Dexcom Sandbox", viewModel.ProviderAvailabilityStatusText);
    }

    [Fact]
    public async Task LoadCommand_ShouldMarkDexcomAsUnavailable_WhenDexcomMetadataProviderIsNotRegistered()
    {
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(),
            [new FakeMetadataProvider(CgmProviderKind.Mock, "Mock")]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        var dexcomOption = viewModel.ProviderOptions.Single(option =>
            option.Kind == CgmProviderKind.DexcomOfficial);

        Assert.False(dexcomOption.IsAvailable);
        Assert.Equal("Dexcom Official", dexcomOption.DisplayName);
        Assert.Equal("Dexcom Official (not configured)", dexcomOption.DisplayLabel);
    }

    [Fact]
    public async Task LoadCommand_ShouldFallbackToMock_WhenSavedProviderIsUnavailable()
    {
        var settings = new ApplicationSettings(
            activeLiveProvider: CgmProviderKind.DexcomOfficial,
            historicalProvider: CgmProviderKind.DexcomOfficial,
            preferredUnit: GlucoseUnit.MgDl,
            targetLowMgDl: 80,
            targetHighMgDl: 160,
            dashboardRefreshInterval: TimeSpan.FromSeconds(45));

        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(Result<ApplicationSettings>.Success(settings)),
            [new FakeMetadataProvider(CgmProviderKind.Mock, "Mock")]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasError);
        Assert.Equal("Settings loaded. Unavailable providers were replaced with Mock.", viewModel.StatusMessage);
        Assert.Equal(CgmProviderKind.Mock, viewModel.SelectedLiveProvider?.Kind);
        Assert.Equal(CgmProviderKind.Mock, viewModel.SelectedHistoricalProvider?.Kind);
    }

    [Fact]
    public async Task SaveCommand_ShouldPersistSettings_WhenFormIsValid()
    {
        var settingsService = new FakeApplicationSettingsService();
        var viewModel = new SettingsViewModel(
            settingsService,
            [new FakeMetadataProvider(CgmProviderKind.Mock, "Mock")]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.TargetLowMgDlText = "75";
        viewModel.TargetHighMgDlText = "170";
        viewModel.DashboardRefreshIntervalSecondsText = "60";

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasError);
        Assert.NotNull(settingsService.SavedSettings);
        Assert.Equal(CgmProviderKind.Mock, settingsService.SavedSettings.ActiveLiveProvider);
        Assert.Equal(CgmProviderKind.Mock, settingsService.SavedSettings.HistoricalProvider);
        Assert.Equal(75, settingsService.SavedSettings.TargetLowMgDl);
        Assert.Equal(170, settingsService.SavedSettings.TargetHighMgDl);
        Assert.Equal(TimeSpan.FromSeconds(60), settingsService.SavedSettings.DashboardRefreshInterval);
        Assert.Equal("Settings saved. Dashboard will use the selected provider on next refresh.", viewModel.StatusMessage);
    }

    [Fact]
    public async Task SaveCommand_ShouldExposeValidationError_WhenSelectedLiveProviderIsUnavailable()
    {
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(),
            [new FakeMetadataProvider(CgmProviderKind.Mock, "Mock")]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.SelectedLiveProvider = viewModel.ProviderOptions.Single(option =>
            option.Kind == CgmProviderKind.DexcomOfficial);

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.Equal("Settings validation failed", viewModel.StatusMessage);
        Assert.Equal(
            "Live provider 'Dexcom Official' is not available. Configure it before selecting it.",
            viewModel.ErrorMessage);
    }

    [Fact]
    public async Task SaveCommand_ShouldExposeValidationError_WhenSelectedHistoricalProviderIsUnavailable()
    {
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(),
            [new FakeMetadataProvider(CgmProviderKind.Mock, "Mock")]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        viewModel.SelectedHistoricalProvider = viewModel.ProviderOptions.Single(option =>
            option.Kind == CgmProviderKind.DexcomOfficial);

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.Equal("Settings validation failed", viewModel.StatusMessage);
        Assert.Equal(
            "Historical provider 'Dexcom Official' is not available. Configure it before selecting it.",
            viewModel.ErrorMessage);
    }

    [Fact]
    public async Task SaveCommand_ShouldExposeValidationError_WhenTargetLowIsInvalid()
    {
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(),
            [new FakeMetadataProvider(CgmProviderKind.Mock, "Mock")])
        {
            TargetLowMgDlText = "abc"
        };

        await viewModel.SaveCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.Equal("Settings validation failed", viewModel.StatusMessage);
        Assert.Equal(
            "Target low must be a positive glucose value expressed in mg/dL.",
            viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadCommand_ShouldExposeError_WhenSettingsLoadFails()
    {
        var error = new Error("Settings.LoadFailed", "Unable to load settings.");
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(Result<ApplicationSettings>.Failure(error)),
            [new FakeMetadataProvider(CgmProviderKind.Mock, "Mock")]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.Equal("Unable to load settings", viewModel.StatusMessage);
        Assert.Equal("Settings.LoadFailed: Unable to load settings.", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task LoadCommand_ShouldShowDexcomNotConfigured_WhenConnectionStatusServiceIsNotRegistered()
    {
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(),
            [new FakeMetadataProvider(CgmProviderKind.Mock, "Mock")]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(
            "Dexcom: not configured in this desktop runtime.",
            viewModel.DexcomConnectionStatusText);
    }

    [Theory]
    [InlineData(DexcomConnectionState.TokenMissing, "Dexcom: configured, not connected.")]
    [InlineData(DexcomConnectionState.Connected, "Dexcom: connected.")]
    [InlineData(DexcomConnectionState.AccessTokenRefreshRequired, "Dexcom: token refresh required before reading data.")]
    [InlineData(DexcomConnectionState.RefreshTokenExpired, "Dexcom: authorization expired. Reconnect Dexcom.")]
    [InlineData(DexcomConnectionState.TokenStoreUnavailable, "Dexcom: token store unavailable.")]
    public async Task LoadCommand_ShouldShowDexcomConnectionStatus_WhenConnectionStatusServiceIsRegistered(
        DexcomConnectionState state,
        string expectedStatusText)
    {
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(),
            [
                new FakeMetadataProvider(CgmProviderKind.Mock, "Mock"),
                new FakeMetadataProvider(CgmProviderKind.DexcomSandbox, "Dexcom Sandbox")
            ],
            [new FakeDexcomConnectionStatusService(state)]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.Equal(expectedStatusText, viewModel.DexcomConnectionStatusText);
    }

    [Fact]
    public async Task LoadCommand_ShouldDisableDexcomConnect_WhenDesktopConnectionServiceIsNotRegistered()
    {
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(),
            [new FakeMetadataProvider(CgmProviderKind.Mock, "Mock")]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.False(viewModel.CanConnectDexcom);
    }

    [Fact]
    public async Task LoadCommand_ShouldEnableDexcomConnect_WhenDesktopConnectionServiceIsRegistered()
    {
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(),
            [
                new FakeMetadataProvider(CgmProviderKind.Mock, "Mock"),
                new FakeMetadataProvider(CgmProviderKind.DexcomSandbox, "Dexcom Sandbox")
            ],
            [new FakeDexcomConnectionStatusService(DexcomConnectionState.TokenMissing)],
            [new FakeDexcomDesktopConnectionService()]);

        await viewModel.LoadCommand.ExecuteAsync(null);

        Assert.True(viewModel.CanConnectDexcom);
    }

    [Fact]
    public async Task ConnectDexcomCommand_ShouldExposeValidationError_WhenDexcomConnectionServiceIsNotRegistered()
    {
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(),
            [new FakeMetadataProvider(CgmProviderKind.Mock, "Mock")]);

        await viewModel.ConnectDexcomCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.Equal("Settings validation failed", viewModel.StatusMessage);
        Assert.Equal("Dexcom is not configured in the current desktop runtime.", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task ConnectDexcomCommand_ShouldConnectSelectAndSaveDexcom_WhenDexcomConnectionServiceSucceeds()
    {
        var settingsService = new FakeApplicationSettingsService();
        var connectionService = new FakeDexcomDesktopConnectionService();

        var viewModel = new SettingsViewModel(
            settingsService,
            [
                new FakeMetadataProvider(CgmProviderKind.Mock, "Mock"),
                new FakeMetadataProvider(CgmProviderKind.DexcomSandbox, "Dexcom Sandbox")
            ],
            [new FakeDexcomConnectionStatusService(DexcomConnectionState.Connected)],
            [connectionService]);

        await viewModel.ConnectDexcomCommand.ExecuteAsync(null);

        Assert.False(viewModel.HasError);
        Assert.True(connectionService.WasCalled);
        Assert.Equal(CgmProviderKind.DexcomSandbox, viewModel.SelectedLiveProvider?.Kind);
        Assert.Equal(CgmProviderKind.DexcomSandbox, viewModel.SelectedHistoricalProvider?.Kind);
        Assert.NotNull(settingsService.SavedSettings);
        Assert.Equal(CgmProviderKind.DexcomSandbox, settingsService.SavedSettings.ActiveLiveProvider);
        Assert.Equal(CgmProviderKind.DexcomSandbox, settingsService.SavedSettings.HistoricalProvider);
        Assert.Equal("Dexcom: connected.", viewModel.DexcomConnectionStatusText);
        Assert.Equal(
            "Dexcom connected and selected. Dashboard will use Dexcom on next refresh.",
            viewModel.StatusMessage);
    }

    [Fact]
    public async Task ConnectDexcomCommand_ShouldExposeFailure_WhenDexcomConnectionServiceFails()
    {
        var connectionService = new FakeDexcomDesktopConnectionService
        {
            Result = Result<DexcomDesktopConnectionResult>.Failure(
                new Error("Dexcom.BrowserOpenFailed", "Unable to open browser."))
        };  

        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService(),
            [
                new FakeMetadataProvider(CgmProviderKind.Mock, "Mock"),
                new FakeMetadataProvider(CgmProviderKind.DexcomSandbox, "Dexcom Sandbox")
            ],
            [new FakeDexcomConnectionStatusService(DexcomConnectionState.TokenMissing)],
            [connectionService]);   

        await viewModel.ConnectDexcomCommand.ExecuteAsync(null);    

        Assert.True(viewModel.HasError);
        Assert.True(connectionService.WasCalled);
        Assert.Equal("Unable to connect Dexcom", viewModel.StatusMessage);
        Assert.Equal("Dexcom.BrowserOpenFailed: Unable to open browser.", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task ConnectDexcomCommand_ShouldExposeFailure_WhenDexcomConnectSucceedsButProviderIsUnavailable()
    {
        var settingsService = new FakeApplicationSettingsService();
        var connectionService = new FakeDexcomDesktopConnectionService();

        var viewModel = new SettingsViewModel(
            settingsService,
            [new FakeMetadataProvider(CgmProviderKind.Mock, "Mock")],
            [new FakeDexcomConnectionStatusService(DexcomConnectionState.Connected)],
            [connectionService]);

        await viewModel.ConnectDexcomCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.True(connectionService.WasCalled);
        Assert.Null(settingsService.SavedSettings);
        Assert.Equal("Unable to select Dexcom provider", viewModel.StatusMessage);
        Assert.Equal(
            "Dexcom.ProviderUnavailable: Dexcom is connected but no Dexcom provider is available in the current desktop runtime.",
            viewModel.ErrorMessage);
    }

    [Fact]
    public async Task ConnectDexcomCommand_ShouldExposeFailure_WhenSavingDexcomSettingsFails()
    {
        var settingsService = new FakeApplicationSettingsService
        {
            SaveResult = Result.Failure(
                new Error("Settings.SaveFailed", "Unable to save settings."))
        };

        var connectionService = new FakeDexcomDesktopConnectionService();

        var viewModel = new SettingsViewModel(
            settingsService,
            [
                new FakeMetadataProvider(CgmProviderKind.Mock, "Mock"),
                new FakeMetadataProvider(CgmProviderKind.DexcomSandbox, "Dexcom Sandbox")
            ],
            [new FakeDexcomConnectionStatusService(DexcomConnectionState.Connected)],
            [connectionService]);

        await viewModel.ConnectDexcomCommand.ExecuteAsync(null);

        Assert.True(viewModel.HasError);
        Assert.True(connectionService.WasCalled);
        Assert.NotNull(settingsService.SavedSettings);
        Assert.Equal(CgmProviderKind.DexcomSandbox, settingsService.SavedSettings.ActiveLiveProvider);
        Assert.Equal("Unable to save Dexcom provider settings", viewModel.StatusMessage);
        Assert.Equal("Settings.SaveFailed: Unable to save settings.", viewModel.ErrorMessage);
    }

    #region Helpers

    private sealed class FakeDexcomDesktopConnectionService : IDexcomDesktopConnectionService
    {
        /// <summary>
        /// Gets or sets the connection result.
        /// </summary>
        public Result<DexcomDesktopConnectionResult> Result { get; set; } =
            Result<DexcomDesktopConnectionResult>.Success(
                new DexcomDesktopConnectionResult(
                    DateTimeOffset.Parse("2026-01-01T10:00:00Z"),
                    DateTimeOffset.Parse("2026-01-01T11:00:00Z"),
                    DateTimeOffset.Parse("2026-01-31T10:00:00Z")));

        /// <summary>
        /// Gets a value indicating whether the service was called.
        /// </summary>
        public bool WasCalled { get; private set; }

        /// <inheritdoc />
        public Task<Result<DexcomDesktopConnectionResult>> ConnectAsync(
            CancellationToken cancellationToken)
        {
            WasCalled = true;

            return Task.FromResult(Result);
        }
    }

    private sealed class FakeDexcomConnectionStatusService : IDexcomConnectionStatusService
    {
        private readonly DexcomConnectionState _state;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeDexcomConnectionStatusService"/> class.
        /// </summary>
        /// <param name="state">The Dexcom connection state.</param>
        public FakeDexcomConnectionStatusService(DexcomConnectionState state)
        {
            _state = state;
        }

        /// <inheritdoc />
        public Task<Result<DexcomConnectionStatus>> GetConnectionStatusAsync(
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<DexcomConnectionStatus>.Success(
                new DexcomConnectionStatus(
                    _state,
                    DateTimeOffset.Parse("2026-01-01T10:00:00Z"),
                    "Dexcom test status.")));
        }
    }

    private sealed class FakeApplicationSettingsService : IApplicationSettingsService
    {
        private readonly Result<ApplicationSettings> _loadResult;
        public Result SaveResult { get; set; } = Result.Success();

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
        
            return Task.FromResult(SaveResult);
        }
    }

    private sealed class FakeMetadataProvider : ICgmMetadataProvider
    {
        private readonly CgmProviderKind _providerKind;
        private readonly string _displayName;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeMetadataProvider"/> class.
        /// </summary>
        /// <param name="providerKind">The provider kind.</param>
        /// <param name="displayName">The provider display name.</param>
        public FakeMetadataProvider(
            CgmProviderKind providerKind,
            string displayName)
        {
            _providerKind = providerKind;
            _displayName = displayName;
        }

        /// <inheritdoc />
        public Task<Result<CgmProviderMetadata>> GetMetadataAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<CgmProviderMetadata>.Success(
                new CgmProviderMetadata(
                    _providerKind,
                    _displayName,
                    GlucoseDataFreshness.NearRealTime,
                    supportsLiveReadings: true,
                    supportsHistoricalReadings: true)));
        }
    }

    #endregion
}