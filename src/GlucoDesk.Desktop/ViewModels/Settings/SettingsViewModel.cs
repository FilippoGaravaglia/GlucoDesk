using System.Globalization;
using System.Text;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Services;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Settings.Selections;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Services;

namespace GlucoDesk.Desktop.ViewModels.Settings;

/// <summary>
/// Represents the settings screen view model.
/// </summary>
public sealed partial class SettingsViewModel : ViewModelBase
{
    private static readonly IReadOnlyList<CgmProviderKind> SupportedProviderKinds =
    [
        CgmProviderKind.Mock,
        CgmProviderKind.Nightscout,
        CgmProviderKind.DexcomSandbox,
        CgmProviderKind.DexcomOfficial
    ];

    private readonly IApplicationSettingsService _settingsService;
    private readonly IReadOnlyCollection<ICgmMetadataProvider> _metadataProviders;
    private readonly IReadOnlyCollection<IDexcomConnectionStatusService> _dexcomConnectionStatusServices;
    private readonly IReadOnlyCollection<IDexcomDesktopConnectionService> _dexcomDesktopConnectionServices;

    [ObservableProperty]
    private IReadOnlyList<ProviderSelectionItem> _providerOptions = [];

    [ObservableProperty]
    private ProviderSelectionItem? _selectedLiveProvider;

    [ObservableProperty]
    private ProviderSelectionItem? _selectedHistoricalProvider;

    [ObservableProperty]
    private GlucoseUnitSelectionItem? _selectedPreferredUnit;

    [ObservableProperty]
    private string _providerAvailabilityStatusText = "Provider availability not checked";

    [ObservableProperty]
    private string _targetLowMgDlText = "70";

    [ObservableProperty]
    private string _targetHighMgDlText = "180";

    [ObservableProperty]
    private string _dashboardRefreshIntervalSecondsText = "30";

    [ObservableProperty]
    private string _statusMessage = "Settings not loaded";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private string _dexcomConnectionStatusText = "Dexcom: status not checked";

    [ObservableProperty]
    private bool _canConnectDexcom;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="settingsService">The application settings service.</param>
    /// <param name="metadataProviders">The registered CGM metadata providers.</param>
    /// <param name="dexcomConnectionStatusServices">The registered Dexcom connection status services.</param>
    /// <param name="dexcomDesktopConnectionServices">The registered desktop Dexcom connection services.</param>
    public SettingsViewModel(
        IApplicationSettingsService settingsService,
        IEnumerable<ICgmMetadataProvider>? metadataProviders = null,
        IEnumerable<IDexcomConnectionStatusService>? dexcomConnectionStatusServices = null,
        IEnumerable<IDexcomDesktopConnectionService>? dexcomDesktopConnectionServices = null)
    {
        ArgumentNullException.ThrowIfNull(settingsService);

        _settingsService = settingsService;
        _metadataProviders = metadataProviders?.ToArray() ?? [];
        _dexcomConnectionStatusServices = dexcomConnectionStatusServices?.ToArray() ?? [];
        _dexcomDesktopConnectionServices = dexcomDesktopConnectionServices?.ToArray() ?? [];

        CanConnectDexcom = _dexcomDesktopConnectionServices.Count > 0;

        ProviderOptions = BuildProviderOptions(
            new HashSet<CgmProviderKind>
            {
                CgmProviderKind.Mock
            });

        PreferredUnitOptions = BuildPreferredUnitOptions();

        SelectedLiveProvider = FindAvailableProviderOptionOrFallback(CgmProviderKind.Mock);
        SelectedHistoricalProvider = FindAvailableProviderOptionOrFallback(CgmProviderKind.Mock);
        SelectedPreferredUnit = PreferredUnitOptions[0];

        ProviderAvailabilityStatusText = BuildProviderAvailabilityStatusText(ProviderOptions);
    }

    /// <summary>
    /// Gets the available glucose unit options.
    /// </summary>
    public IReadOnlyList<GlucoseUnitSelectionItem> PreferredUnitOptions { get; }

    /// <summary>
    /// Loads settings from the configured application settings service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = null;
        StatusMessage = "Loading settings...";

        try
        {
            await RefreshProviderAvailabilityAsync(cancellationToken)
                .ConfigureAwait(false);

            await RefreshDexcomConnectionStatusAsync(cancellationToken)
                .ConfigureAwait(false);

            var result = await _settingsService
                .GetSettingsAsync(cancellationToken)
                .ConfigureAwait(false);

            if (result.IsFailure)
            {
                ApplyFailure(result, "Unable to load settings");
                return;
            }

            var usedProviderFallback = ApplySettings(result.Value);

            StatusMessage = usedProviderFallback
                ? "Settings loaded. Unavailable providers were replaced with Mock."
                : "Settings loaded";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Settings load cancelled";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, "Unexpected error while loading settings");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Saves settings through the configured application settings service.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        var settings = CreateSettingsFromForm(out var validationMessage);

        if (settings is null)
        {
            ApplyValidationFailure(validationMessage ?? "Invalid settings.");
            return;
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = null;
        StatusMessage = "Saving settings...";

        try
        {
            var result = await _settingsService
                .SaveSettingsAsync(settings, cancellationToken)
                .ConfigureAwait(false);

            if (result.IsFailure)
            {
                ApplyFailure(result, "Unable to save settings");
                return;
            }

            StatusMessage = "Settings saved. Dashboard will use the selected provider on next refresh.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Settings save cancelled";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, "Unexpected error while saving settings");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #region Helpers

    /// <summary>
    /// Starts the Dexcom desktop connection flow and selects Dexcom as the active provider when the connection succeeds.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private async Task ConnectDexcomAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        var connectionService = _dexcomDesktopConnectionServices.FirstOrDefault();

        if (connectionService is null)
        {
            ApplyValidationFailure("Dexcom is not configured in the current desktop runtime.");
            return;
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = null;
        StatusMessage = "Starting Dexcom connection...";

        try
        {
            var connectionResult = await connectionService
                .ConnectAsync(cancellationToken)
                .ConfigureAwait(false);

            if (connectionResult.IsFailure)
            {
                ApplyFailure(connectionResult, "Unable to connect Dexcom");
                return;
            }

            await RefreshProviderAvailabilityAsync(cancellationToken)
                .ConfigureAwait(false);

            var providerSelectionResult = SelectAvailableDexcomProvider();

            if (providerSelectionResult.IsFailure)
            {
                ApplyFailure(providerSelectionResult, "Unable to select Dexcom provider");
                return;
            }

            var settings = CreateSettingsFromForm(out var validationMessage);

            if (settings is null)
            {
                ApplyValidationFailure(validationMessage ?? "Dexcom provider selection is invalid.");
                return;
            }

            var saveResult = await _settingsService
                .SaveSettingsAsync(settings, cancellationToken)
                .ConfigureAwait(false);

            if (saveResult.IsFailure)
            {
                ApplyFailure(saveResult, "Unable to save Dexcom provider settings");
                return;
            }

            await RefreshDexcomConnectionStatusAsync(cancellationToken)
                .ConfigureAwait(false);

            StatusMessage = "Dexcom connected and selected. Dashboard will use Dexcom on next refresh.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Dexcom connection cancelled";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, "Unexpected error while connecting Dexcom");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Refreshes the provider availability options from registered metadata providers.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task RefreshProviderAvailabilityAsync(CancellationToken cancellationToken)
    {
        var availableProviders = await GetAvailableProviderKindsAsync(cancellationToken)
            .ConfigureAwait(false);

        ProviderOptions = BuildProviderOptions(availableProviders);
        ProviderAvailabilityStatusText = BuildProviderAvailabilityStatusText(ProviderOptions);
    }

    /// <summary>
    /// Refreshes the Dexcom connection status text.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task RefreshDexcomConnectionStatusAsync(CancellationToken cancellationToken)
    {
        var connectionStatusService = _dexcomConnectionStatusServices.FirstOrDefault();

        if (connectionStatusService is null)
        {
            DexcomConnectionStatusText = "Dexcom: not configured in this desktop runtime.";
            return;
        }

        var statusResult = await connectionStatusService
            .GetConnectionStatusAsync(cancellationToken)
            .ConfigureAwait(false);

        DexcomConnectionStatusText = statusResult.IsSuccess
            ? BuildDexcomConnectionStatusText(statusResult.Value)
            : $"Dexcom: status unavailable ({statusResult.Error.Code}).";
    }

    /// <summary>
    /// Builds the Dexcom connection status text shown in the settings screen.
    /// </summary>
    /// <param name="status">The Dexcom connection status.</param>
    /// <returns>The Dexcom connection status text.</returns>
    private static string BuildDexcomConnectionStatusText(DexcomConnectionStatus status)
    {
        return status.State switch
        {
            DexcomConnectionState.ProviderNotRegistered =>
                "Dexcom: not configured in this desktop runtime.",

            DexcomConnectionState.TokenMissing =>
                "Dexcom: configured, not connected.",

            DexcomConnectionState.Connected =>
                "Dexcom: connected.",

            DexcomConnectionState.AccessTokenRefreshRequired =>
                "Dexcom: token refresh required before reading data.",

            DexcomConnectionState.RefreshTokenExpired =>
                "Dexcom: authorization expired. Reconnect Dexcom.",

            DexcomConnectionState.TokenStoreUnavailable =>
                "Dexcom: token store unavailable.",

            _ =>
                "Dexcom: status unknown."
        };
    }

    /// <summary>
    /// Applies loaded application settings to the editable form.
    /// </summary>
    /// <param name="settings">The loaded application settings.</param>
    /// <returns>True when an unavailable provider was replaced by the Mock provider; otherwise false.</returns>
    private bool ApplySettings(ApplicationSettings settings)
    {
        var liveProvider = FindAvailableProviderOptionOrFallback(settings.ActiveLiveProvider);
        var historicalProvider = FindAvailableProviderOptionOrFallback(settings.HistoricalProvider);

        var usedProviderFallback =
            liveProvider.Kind != settings.ActiveLiveProvider
            || historicalProvider.Kind != settings.HistoricalProvider;

        SelectedLiveProvider = liveProvider;
        SelectedHistoricalProvider = historicalProvider;
        SelectedPreferredUnit = FindPreferredUnitOption(settings.PreferredUnit);

        TargetLowMgDlText = settings.TargetLowMgDl.ToString(CultureInfo.InvariantCulture);
        TargetHighMgDlText = settings.TargetHighMgDl.ToString(CultureInfo.InvariantCulture);
        DashboardRefreshIntervalSecondsText = ((int)settings.DashboardRefreshInterval.TotalSeconds)
            .ToString(CultureInfo.InvariantCulture);

        return usedProviderFallback;
    }

    /// <summary>
    /// Creates application settings from the editable form values.
    /// </summary>
    /// <param name="validationMessage">The validation error message when settings cannot be created.</param>
    /// <returns>The application settings when valid; otherwise null.</returns>
    private ApplicationSettings? CreateSettingsFromForm(out string? validationMessage)
    {
        validationMessage = null;

        if (SelectedLiveProvider is null)
        {
            validationMessage = "Select a live provider.";
            return null;
        }

        if (!SelectedLiveProvider.IsAvailable)
        {
            validationMessage = $"Live provider '{SelectedLiveProvider.DisplayName}' is not available. Configure it before selecting it.";
            return null;
        }

        if (SelectedHistoricalProvider is null)
        {
            validationMessage = "Select a historical provider.";
            return null;
        }

        if (!SelectedHistoricalProvider.IsAvailable)
        {
            validationMessage = $"Historical provider '{SelectedHistoricalProvider.DisplayName}' is not available. Configure it before selecting it.";
            return null;
        }

        if (SelectedPreferredUnit is null)
        {
            validationMessage = "Select a preferred glucose unit.";
            return null;
        }

        if (!TryParsePositiveInteger(TargetLowMgDlText, out var targetLowMgDl))
        {
            validationMessage = "Target low must be a positive integer.";
            return null;
        }

        if (!TryParsePositiveInteger(TargetHighMgDlText, out var targetHighMgDl))
        {
            validationMessage = "Target high must be a positive integer.";
            return null;
        }

        if (!TryParsePositiveInteger(DashboardRefreshIntervalSecondsText, out var refreshIntervalSeconds))
        {
            validationMessage = "Refresh interval must be a positive integer.";
            return null;
        }

        try
        {
            return new ApplicationSettings(
                SelectedLiveProvider.Kind,
                SelectedHistoricalProvider.Kind,
                SelectedPreferredUnit.Unit,
                targetLowMgDl,
                targetHighMgDl,
                TimeSpan.FromSeconds(refreshIntervalSeconds));
        }
        catch (ArgumentException exception)
        {
            validationMessage = exception.Message;
            return null;
        }
    }

    /// <summary>
    /// Gets currently available provider kinds from registered metadata providers.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The available provider kinds.</returns>
    private async Task<IReadOnlySet<CgmProviderKind>> GetAvailableProviderKindsAsync(
        CancellationToken cancellationToken)
    {
        var availableProviderKinds = new HashSet<CgmProviderKind>
        {
            CgmProviderKind.Mock
        };

        foreach (var metadataProvider in _metadataProviders)
        {
            var metadataResult = await GetMetadataSafelyAsync(metadataProvider, cancellationToken)
                .ConfigureAwait(false);

            if (metadataResult.IsFailure)
            {
                continue;
            }

            availableProviderKinds.Add(metadataResult.Value.ProviderKind);
        }

        return availableProviderKinds;
    }

    /// <summary>
    /// Gets provider metadata while ignoring provider metadata failures.
    /// </summary>
    /// <param name="metadataProvider">The metadata provider.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The provider metadata result.</returns>
    private static async Task<Result<CgmProviderMetadata>> GetMetadataSafelyAsync(
        ICgmMetadataProvider metadataProvider,
        CancellationToken cancellationToken)
    {
        try
        {
            return await metadataProvider
                .GetMetadataAsync(cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<CgmProviderMetadata>.Failure(
                new Error(
                    "Settings.ProviderMetadataUnavailable",
                    exception.Message));
        }
    }

    /// <summary>
    /// Applies a failed application result to the settings view model.
    /// </summary>
    /// <typeparam name="TValue">The result value type.</typeparam>
    /// <param name="result">The failed result.</param>
    /// <param name="statusMessage">The status message.</param>
    private void ApplyFailure<TValue>(
        Result<TValue> result,
        string statusMessage)
        where TValue : notnull
    {
        HasError = true;
        ErrorMessage = $"{result.Error.Code}: {result.Error.Message}";
        StatusMessage = statusMessage;
    }

    /// <summary>
    /// Applies a failed application result to the settings view model.
    /// </summary>
    /// <param name="result">The failed result.</param>
    /// <param name="statusMessage">The status message.</param>
    private void ApplyFailure(
        Result result,
        string statusMessage)
    {
        HasError = true;
        ErrorMessage = $"{result.Error.Code}: {result.Error.Message}";
        StatusMessage = statusMessage;
    }

    /// <summary>
    /// Applies a validation failure to the settings view model.
    /// </summary>
    /// <param name="message">The validation message.</param>
    private void ApplyValidationFailure(string message)
    {
        HasError = true;
        ErrorMessage = message;
        StatusMessage = "Settings validation failed";
    }

    /// <summary>
    /// Applies an unexpected exception to the settings view model.
    /// </summary>
    /// <param name="exception">The unexpected exception.</param>
    /// <param name="statusMessage">The status message.</param>
    private void ApplyUnexpectedFailure(
        Exception exception,
        string statusMessage)
    {
        HasError = true;
        ErrorMessage = exception.Message;
        StatusMessage = statusMessage;
    }

    /// <summary>
    /// Finds an available provider option by provider kind or falls back to Mock.
    /// </summary>
    /// <param name="kind">The provider kind.</param>
    /// <returns>The matching available provider option, or Mock.</returns>
    private ProviderSelectionItem FindAvailableProviderOptionOrFallback(CgmProviderKind kind)
    {
        var option = ProviderOptions.FirstOrDefault(candidate => candidate.Kind == kind);

        if (option is { IsAvailable: true })
        {
            return option;
        }

        return ProviderOptions.First(candidate => candidate.Kind == CgmProviderKind.Mock);
    }

    /// <summary>
    /// Finds a glucose unit option by unit.
    /// </summary>
    /// <param name="unit">The glucose unit.</param>
    /// <returns>The matching glucose unit option.</returns>
    private GlucoseUnitSelectionItem FindPreferredUnitOption(GlucoseUnit unit)
    {
        return PreferredUnitOptions.FirstOrDefault(option => option.Unit == unit)
            ?? PreferredUnitOptions[0];
    }

    /// <summary>
    /// Parses a positive integer using invariant culture.
    /// </summary>
    /// <param name="value">The text value to parse.</param>
    /// <param name="result">The parsed integer value.</param>
    /// <returns>True when parsing succeeds and the value is positive; otherwise false.</returns>
    private static bool TryParsePositiveInteger(
        string value,
        out int result)
    {
        return int.TryParse(
                value,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out result)
            && result > 0;
    }

    /// <summary>
    /// Builds selectable provider options from the supported CGM provider kinds.
    /// </summary>
    /// <param name="availableProviderKinds">The available provider kinds.</param>
    /// <returns>The selectable provider options.</returns>
    private static IReadOnlyList<ProviderSelectionItem> BuildProviderOptions(
        IReadOnlySet<CgmProviderKind> availableProviderKinds)
    {
        return SupportedProviderKinds
            .Select(kind => BuildProviderOption(kind, availableProviderKinds))
            .ToArray();
    }

    /// <summary>
    /// Builds a selectable provider option.
    /// </summary>
    /// <param name="kind">The provider kind.</param>
    /// <param name="availableProviderKinds">The available provider kinds.</param>
    /// <returns>The selectable provider option.</returns>
    private static ProviderSelectionItem BuildProviderOption(
        CgmProviderKind kind,
        IReadOnlySet<CgmProviderKind> availableProviderKinds)
    {
        var isAvailable = availableProviderKinds.Contains(kind);

        return new ProviderSelectionItem(
            kind,
            FormatEnumName(kind.ToString()),
            isAvailable,
            isAvailable
                ? "Provider is available."
                : "Provider is not configured in the current desktop runtime.");
    }

    /// <summary>
    /// Builds a display-friendly provider availability status text.
    /// </summary>
    /// <param name="providerOptions">The provider options.</param>
    /// <returns>The provider availability status text.</returns>
    private static string BuildProviderAvailabilityStatusText(
        IReadOnlyCollection<ProviderSelectionItem> providerOptions)
    {
        var availableProviders = providerOptions
            .Where(provider => provider.IsAvailable)
            .Select(provider => provider.DisplayName)
            .ToArray();

        return availableProviders.Length == 0
            ? "No CGM provider is currently available."
            : $"Available providers: {string.Join(", ", availableProviders)}.";
    }

    /// <summary>
    /// Builds selectable glucose unit options from the glucose unit enum.
    /// </summary>
    /// <returns>The selectable glucose unit options.</returns>
    private static IReadOnlyList<GlucoseUnitSelectionItem> BuildPreferredUnitOptions()
    {
        return Enum
            .GetValues<GlucoseUnit>()
            .Select(unit => new GlucoseUnitSelectionItem(unit, FormatEnumName(unit.ToString())))
            .ToArray();
    }

    /// <summary>
    /// Formats enum names into display-friendly text.
    /// </summary>
    /// <param name="value">The enum name.</param>
    /// <returns>The display-friendly enum name.</returns>
    private static string FormatEnumName(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var builder = new StringBuilder();

        for (var index = 0; index < value.Length; index++)
        {
            var character = value[index];

            if (index > 0 && char.IsUpper(character))
            {
                builder.Append(' ');
            }

            builder.Append(character);
        }

        return builder.ToString();
    }

    /// <summary>
    /// Selects the currently available Dexcom provider for both live and historical readings.
    /// </summary>
    /// <returns>The operation result.</returns>
    private Result SelectAvailableDexcomProvider()
    {
        var dexcomProvider = FindAvailableDexcomProviderOption();

        if (dexcomProvider is null)
        {
            return Result.Failure(
                new Error(
                    "Dexcom.ProviderUnavailable",
                    "Dexcom is connected but no Dexcom provider is available in the current desktop runtime."));
        }

        SelectedLiveProvider = dexcomProvider;
        SelectedHistoricalProvider = dexcomProvider;

        return Result.Success();
    }

    /// <summary>
    /// Finds the available Dexcom provider option in the current provider options.
    /// </summary>
    /// <returns>The available Dexcom provider option, or null when unavailable.</returns>
    private ProviderSelectionItem? FindAvailableDexcomProviderOption()
    {
        return ProviderOptions.FirstOrDefault(provider =>
            provider.IsAvailable && IsDexcomProviderKind(provider.Kind));
    }

    /// <summary>
    /// Checks whether the provider kind represents a Dexcom provider.
    /// </summary>
    /// <param name="kind">The provider kind.</param>
    /// <returns>True when the provider kind is a Dexcom provider; otherwise false.</returns>
    private static bool IsDexcomProviderKind(CgmProviderKind kind)
    {
        return kind is CgmProviderKind.DexcomSandbox or CgmProviderKind.DexcomOfficial;
    }

    #endregion
}