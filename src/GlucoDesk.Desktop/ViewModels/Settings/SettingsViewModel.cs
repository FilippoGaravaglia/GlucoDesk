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
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.GlucoseAlerts.Services;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Nightscout.Services;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Services;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Settings.Providers;
using GlucoDesk.Desktop.ViewModels.Settings.Selections;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Services;
using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Diagnostics;
using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;

namespace GlucoDesk.Desktop.ViewModels.Settings;

/// <summary>
/// Represents the settings screen view model.
/// </summary>
public sealed partial class SettingsViewModel : ViewModelBase
{
    /// <summary>
    /// Gets native notification diagnostics for the current runtime environment.
    /// </summary>
    public string NativeNotificationDiagnosticsText { get; } =
        NativeNotificationDiagnosticsProvider
            .CreateDefault()
            .GetSettingsText();


    private static readonly IReadOnlyList<CgmProviderKind> SupportedProviderKinds =
    [
        CgmProviderKind.Mock,
        CgmProviderKind.Nightscout,
        CgmProviderKind.DexcomShare,
        CgmProviderKind.DexcomSandbox,
        CgmProviderKind.DexcomOfficial
    ];

    private readonly IApplicationSettingsService _settingsService;
    private readonly IReadOnlyCollection<ICgmMetadataProvider> _metadataProviders;
    private readonly IReadOnlyCollection<IDexcomConnectionStatusService> _dexcomConnectionStatusServices;
    private readonly IReadOnlyCollection<IDexcomDesktopConnectionService> _dexcomDesktopConnectionServices;
    private readonly IReadOnlyCollection<INightscoutDesktopConnectionService> _nightscoutDesktopConnectionServices;
    private readonly IGlucoseAlertNotificationTestService _glucoseAlertNotificationTestService;

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
    private string _targetRangeUnitLabel = "mg/dL";

    [ObservableProperty]
    private string _targetLowPlaceholderText = "70";

    [ObservableProperty]
    private string _targetHighPlaceholderText = "180";

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

    [ObservableProperty]
    private string _nightscoutConnectionStatusText = "Nightscout: status not checked";

    [ObservableProperty]
    private bool _canTestNightscoutConnection;

    [ObservableProperty]
    private bool _canUseNightscoutProvider;

    [ObservableProperty]
    private IReadOnlyList<ChartMaximumSelectionItem> _chartMaximumOptions =
        BuildChartMaximumOptions(GlucoseUnit.MgDl);

    [ObservableProperty]
    private string _chartMaximumUnitLabel = "mg/dL";

    [ObservableProperty]
    private int _selectedChartMaximumMgDl = 300;

    [ObservableProperty]
    private bool _glucoseAlertsEnabled = true;

    [ObservableProperty]
    private bool _lowGlucoseAlertsEnabled = true;

    [ObservableProperty]
    private bool _highGlucoseAlertsEnabled = true;

    [ObservableProperty]
    private bool _nativeGlucoseNotificationsEnabled;

    [ObservableProperty]
    private bool _glucoseAlertPrivacyModeEnabled = true;

    [ObservableProperty]
    private string _glucoseAlertRepeatIntervalMinutesText = "30";

    [ObservableProperty]
    private string _glucoseAlertRequiredConsecutiveReadingsText = "2";

    [ObservableProperty]
    private bool _canSendNativeGlucoseTestNotification;

    [ObservableProperty]
    private string _nativeGlucoseTestNotificationStatusText =
        "Enable native OS notifications to send a safe test notification.";

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsViewModel"/> class.
    /// </summary>
    /// <param name="settingsService">The application settings service.</param>
    /// <param name="metadataProviders">The registered CGM metadata providers.</param>
    /// <param name="dexcomConnectionStatusServices">The registered Dexcom connection status services.</param>
    /// <param name="dexcomDesktopConnectionServices">The registered desktop Dexcom connection services.</param>
    /// <param name="nightscoutDesktopConnectionServices">The registered desktop Nightscout connection services.</param>
    /// <param name="glucoseAlertNotificationTestService">The optional native glucose notification test service.</param>
    public SettingsViewModel(
        IApplicationSettingsService settingsService,
        IEnumerable<ICgmMetadataProvider>? metadataProviders = null,
        IEnumerable<IDexcomConnectionStatusService>? dexcomConnectionStatusServices = null,
        IEnumerable<IDexcomDesktopConnectionService>? dexcomDesktopConnectionServices = null,
        IEnumerable<INightscoutDesktopConnectionService>? nightscoutDesktopConnectionServices = null,
        IGlucoseAlertNotificationTestService? glucoseAlertNotificationTestService = null)
    {
        ArgumentNullException.ThrowIfNull(settingsService);

        _settingsService = settingsService;
        _metadataProviders = metadataProviders?.ToArray() ?? [];
        _dexcomConnectionStatusServices = dexcomConnectionStatusServices?.ToArray() ?? [];
        _dexcomDesktopConnectionServices = dexcomDesktopConnectionServices?.ToArray() ?? [];
        _nightscoutDesktopConnectionServices = nightscoutDesktopConnectionServices?.ToArray() ?? [];
        _glucoseAlertNotificationTestService = glucoseAlertNotificationTestService
            ?? new GlucoseAlertNotificationTestService(OperatingSystemGlucoseAlertNotificationService.Create());

        CanConnectDexcom = _dexcomDesktopConnectionServices.Count > 0;
        CanTestNightscoutConnection = _nightscoutDesktopConnectionServices.Count > 0;
        NightscoutConnectionStatusText = BuildInitialNightscoutConnectionStatusText();

        ProviderOptions = BuildProviderOptions(
            new HashSet<CgmProviderKind>
            {
                CgmProviderKind.Mock
            });

        PreferredUnitOptions = BuildPreferredUnitOptions();

        SelectedLiveProvider = FindAvailableProviderOptionOrFallback(CgmProviderKind.Mock);
        SelectedHistoricalProvider = FindAvailableProviderOptionOrFallback(CgmProviderKind.Mock);
        SelectedPreferredUnit = PreferredUnitOptions[0];
        UpdateTargetRangeUnitPresentation(SelectedPreferredUnit.Unit);
        UpdateChartMaximumPresentation(SelectedPreferredUnit.Unit);
        UpdateNativeGlucoseTestNotificationAvailability();

        ProviderAvailabilityStatusText = BuildProviderAvailabilityStatusText(ProviderOptions);
        UpdateNightscoutProviderActionAvailability();
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

            RefreshNightscoutConnectionStatus();

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
            var settingsToSave = await MergeSettingsForSaveAsync(
                    settings,
                    cancellationToken)
                .ConfigureAwait(false);

            var result = await _settingsService
                .SaveSettingsAsync(settingsToSave, cancellationToken)
                .ConfigureAwait(false);

            if (result.IsFailure)
            {
                ApplyFailure(result, "Unable to save settings");
                return;
            }

            StatusMessage = "Settings saved. Dashboard will use the selected provider on next refresh.";
            UpdateNativeGlucoseTestNotificationAvailability();
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

    /// <summary>
    /// Sends a privacy-safe native OS test notification.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private async Task SendNativeGlucoseTestNotificationAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        if (!GlucoseAlertsEnabled || !NativeGlucoseNotificationsEnabled)
        {
            NativeGlucoseTestNotificationStatusText =
                "Enable glucose awareness and native OS notifications before sending a test notification.";
            UpdateNativeGlucoseTestNotificationAvailability();
            return;
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = null;
        StatusMessage = "Sending native test notification...";
        NativeGlucoseTestNotificationStatusText = "Sending test notification...";

        try
        {
            var result = await _glucoseAlertNotificationTestService
                .SendTestNotificationAsync(cancellationToken)
                .ConfigureAwait(false);

            var presentation = NativeNotificationRequestResultPresentation.FromResult(result);

            ApplyNativeNotificationRequestResultPresentation(presentation);

            if (presentation.IsFailure)
            {
                return;
            }
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Native test notification cancelled";
            NativeGlucoseTestNotificationStatusText = "Test notification cancelled.";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, "Unexpected error while sending native test notification");
            NativeGlucoseTestNotificationStatusText =
                "Unexpected error while sending the test notification.";
        }
        finally
        {
            IsBusy = false;
            UpdateNativeGlucoseTestNotificationAvailability();
        }
    }

    /// <summary>
    /// Selects Nightscout as both live and historical provider and saves the settings.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private async Task UseNightscoutAsActiveProviderAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = null;
        StatusMessage = "Selecting Nightscout provider...";

        try
        {
            await RefreshProviderAvailabilityAsync(cancellationToken)
                .ConfigureAwait(false);

            var providerSelectionResult = SelectAvailableNightscoutProvider();

            if (providerSelectionResult.IsFailure)
            {
                ApplyFailure(providerSelectionResult, "Unable to select Nightscout provider");
                return;
            }

            var settings = CreateSettingsFromForm(out var validationMessage);

            if (settings is null)
            {
                ApplyValidationFailure(validationMessage ?? "Nightscout provider selection is invalid.");
                return;
            }

            var saveResult = await _settingsService
                .SaveSettingsAsync(settings, cancellationToken)
                .ConfigureAwait(false);

            if (saveResult.IsFailure)
            {
                ApplyFailure(saveResult, "Unable to save Nightscout provider settings");
                return;
            }

            RefreshNightscoutConnectionStatus();

            StatusMessage = "Nightscout selected. Dashboard will use Nightscout on next refresh.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Nightscout provider selection cancelled";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, "Unexpected error while selecting Nightscout provider");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Tests the configured Nightscout desktop connection.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private async Task TestNightscoutConnectionAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        var connectionService = _nightscoutDesktopConnectionServices.FirstOrDefault();

        if (connectionService is null)
        {
            ApplyValidationFailure("Nightscout is not configured in the current desktop runtime.");
            return;
        }

        IsBusy = true;
        HasError = false;
        ErrorMessage = null;
        StatusMessage = "Testing Nightscout connection...";

        try
        {
            var status = await connectionService
                .TestConnectionAsync(cancellationToken)
                .ConfigureAwait(false);

            NightscoutConnectionStatusText = status.Message;
            StatusMessage = status.IsConnected
                ? "Nightscout connection test succeeded."
                : "Nightscout connection test completed with warnings.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Nightscout connection test cancelled";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, "Unexpected error while testing Nightscout connection");
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Starts the Dexcom desktop connection flow and selects the official Dexcom provider when the connection succeeds.
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
    /// Sanitizes a target value input using the currently selected glucose unit.
    /// </summary>
    /// <param name="text">The raw target input text.</param>
    /// <returns>The sanitized target input text.</returns>
    public string SanitizeTargetValueInput(string text)
    {
        return SanitizeTargetValueText(
            text,
            SelectedPreferredUnit?.Unit ?? GlucoseUnit.MgDl);
    }

    /// <summary>
    /// Handles preferred glucose unit changes by updating labels, preserving chart scale selection and converting editable target values.
    /// </summary>
    /// <param name="oldValue">The previous selected glucose unit option.</param>
    /// <param name="newValue">The new selected glucose unit option.</param>
    partial void OnSelectedPreferredUnitChanged(
        GlucoseUnitSelectionItem? oldValue,
        GlucoseUnitSelectionItem? newValue)
    {
        var previousUnit = oldValue?.Unit ?? GlucoseUnit.MgDl;
        var nextUnit = newValue?.Unit ?? GlucoseUnit.MgDl;
        var currentChartMaximumMgDl = NormalizeChartMaximumMgDl(SelectedChartMaximumMgDl);

        UpdateTargetRangeUnitPresentation(nextUnit);
        UpdateChartMaximumPresentation(nextUnit, currentChartMaximumMgDl);

        if (previousUnit == nextUnit)
        {
            return;
        }

        TargetLowMgDlText = ConvertEditableTargetText(TargetLowMgDlText, previousUnit, nextUnit);
        TargetHighMgDlText = ConvertEditableTargetText(TargetHighMgDlText, previousUnit, nextUnit);
    }

    /// <summary>
    /// Sanitizes the target low input when the user edits the text.
    /// </summary>
    /// <param name="value">The edited target low text.</param>
    partial void OnTargetLowMgDlTextChanged(string value)
    {
        var sanitizedValue = SanitizeTargetValueText(
            value,
            SelectedPreferredUnit?.Unit ?? GlucoseUnit.MgDl);

        if (sanitizedValue != value)
        {
            TargetLowMgDlText = sanitizedValue;
        }
    }

    /// <summary>
    /// Sanitizes the target high input when the user edits the text.
    /// </summary>
    /// <param name="value">The edited target high text.</param>
    partial void OnTargetHighMgDlTextChanged(string value)
    {
        var sanitizedValue = SanitizeTargetValueText(
            value,
            SelectedPreferredUnit?.Unit ?? GlucoseUnit.MgDl);

        if (sanitizedValue != value)
        {
            TargetHighMgDlText = sanitizedValue;
        }
    }

    /// <summary>
    /// Sanitizes the alert repeat interval input when the user edits the text.
    /// </summary>
    /// <param name="value">The edited alert repeat interval text.</param>
    partial void OnGlucoseAlertRepeatIntervalMinutesTextChanged(string value)
    {
        var sanitizedValue = SanitizePositiveIntegerText(value);

        if (sanitizedValue != value)
        {
            GlucoseAlertRepeatIntervalMinutesText = sanitizedValue;
        }
    }

    /// <summary>
    /// Handles glucose alert stability input changes.
    /// </summary>
    /// <param name="value">The updated text value.</param>
    partial void OnGlucoseAlertRequiredConsecutiveReadingsTextChanged(string value)
    {
        var sanitizedValue = SanitizePositiveIntegerText(value);

        if (int.TryParse(sanitizedValue, NumberStyles.Integer, CultureInfo.InvariantCulture, out var parsedValue) &&
            parsedValue > ApplicationSettings.MaximumGlucoseAlertRequiredConsecutiveReadings)
        {
            sanitizedValue = ApplicationSettings.MaximumGlucoseAlertRequiredConsecutiveReadings
                .ToString(CultureInfo.InvariantCulture);
        }

        if (sanitizedValue != value)
        {
            GlucoseAlertRequiredConsecutiveReadingsText = sanitizedValue;
        }
    }

    /// <summary>
    /// Handles glucose alert toggle changes.
    /// </summary>
    /// <param name="value">The updated toggle value.</param>
    partial void OnGlucoseAlertsEnabledChanged(bool value)
    {
        UpdateNativeGlucoseTestNotificationAvailability();
    }

    /// <summary>
    /// Handles native glucose notification toggle changes.
    /// </summary>
    /// <param name="value">The updated toggle value.</param>
    partial void OnNativeGlucoseNotificationsEnabledChanged(bool value)
    {
        UpdateNativeGlucoseTestNotificationAvailability();
    }

    /// <summary>
    /// Handles busy state changes.
    /// </summary>
    /// <param name="value">The updated busy value.</param>
    partial void OnIsBusyChanged(bool value)
    {
        UpdateNativeGlucoseTestNotificationAvailability();
    }

    #region Helpers

    /// <summary>
    /// Updates native glucose test notification availability and helper text.
    /// </summary>
    private void UpdateNativeGlucoseTestNotificationAvailability()
    {
        CanSendNativeGlucoseTestNotification =
            GlucoseAlertsEnabled &&
            NativeGlucoseNotificationsEnabled &&
            !IsBusy;

        if (!GlucoseAlertsEnabled)
        {
            NativeGlucoseTestNotificationStatusText =
                "Enable glucose awareness notifications to send a safe test notification.";
            return;
        }

        if (!NativeGlucoseNotificationsEnabled)
        {
            NativeGlucoseTestNotificationStatusText =
                "Enable native OS notifications to send a safe test notification.";
            return;
        }

        if (IsBusy)
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(NativeGlucoseTestNotificationStatusText) ||
            NativeGlucoseTestNotificationStatusText.Contains("Enable ", StringComparison.OrdinalIgnoreCase))
        {
            NativeGlucoseTestNotificationStatusText =
                "Send a safe test notification to verify OS permissions.";
        }
    }

    /// <summary>
    /// Parses the glucose alert stability input.
    /// </summary>
    /// <returns>The parsed and bounded required consecutive readings value.</returns>
    private int ParseGlucoseAlertRequiredConsecutiveReadings()
    {
        if (!int.TryParse(
                GlucoseAlertRequiredConsecutiveReadingsText,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var requiredConsecutiveReadings))
        {
            return ApplicationSettings.DefaultGlucoseAlertRequiredConsecutiveReadings;
        }

        return Math.Clamp(
            requiredConsecutiveReadings,
            ApplicationSettings.MinimumGlucoseAlertRequiredConsecutiveReadings,
            ApplicationSettings.MaximumGlucoseAlertRequiredConsecutiveReadings);
    }

    /// <summary>
    /// Sanitizes a text value so it only contains digits.
    /// </summary>
    /// <param name="text">The raw input text.</param>
    /// <returns>The sanitized integer input text.</returns>
    private static string SanitizePositiveIntegerText(string text)
    {
        if (string.IsNullOrEmpty(text))
        {
            return string.Empty;
        }

        var builder = new StringBuilder(text.Length);

        foreach (var character in text)
        {
            if (char.IsDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    /// <summary>
    /// Updates the availability of the Nightscout activation action.
    /// </summary>
    private void UpdateNightscoutProviderActionAvailability()
    {
        CanUseNightscoutProvider = ProviderActivationSelector.CanActivate(
            ProviderOptions,
            CgmProviderKind.Nightscout);
    }

    /// <summary>
    /// Selects the available Nightscout provider option for both live and historical readings.
    /// </summary>
    /// <returns>The operation result.</returns>
    private Result SelectAvailableNightscoutProvider()
    {
        var nightscoutProviderResult = ProviderActivationSelector.SelectAvailableProvider(
            ProviderOptions,
            CgmProviderKind.Nightscout,
            "Nightscout.ProviderUnavailable",
            "Nightscout is not available in the current desktop runtime.");

        if (nightscoutProviderResult.IsFailure)
        {
            return Result.Failure(nightscoutProviderResult.Error);
        }

        SelectedLiveProvider = nightscoutProviderResult.Value;
        SelectedHistoricalProvider = nightscoutProviderResult.Value;

        return Result.Success();
    }

    /// <summary>
    /// Refreshes the Nightscout configuration status text.
    /// </summary>
    private void RefreshNightscoutConnectionStatus()
    {
        var connectionService = _nightscoutDesktopConnectionServices.FirstOrDefault();

        NightscoutConnectionStatusText = connectionService is null
            ? "Nightscout: not configured in this desktop runtime."
            : connectionService.GetConfigurationStatus().Message;
    }

    /// <summary>
    /// Builds the initial Nightscout connection status text.
    /// </summary>
    /// <returns>The initial Nightscout connection status text.</returns>
    private string BuildInitialNightscoutConnectionStatusText()
    {
        var connectionService = _nightscoutDesktopConnectionServices.FirstOrDefault();

        return connectionService is null
            ? "Nightscout: not configured in this desktop runtime."
            : connectionService.GetConfigurationStatus().Message;
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
        UpdateNightscoutProviderActionAvailability();
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
    /// Merges form settings with the currently persisted settings to avoid overwriting configured providers with a visual Mock fallback.
    /// </summary>
    /// <param name="formSettings">The settings created from the editable Settings form.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The settings that should be persisted.</returns>
    private async Task<ApplicationSettings> MergeSettingsForSaveAsync(
        ApplicationSettings formSettings,
        CancellationToken cancellationToken)
    {
        var persistedSettingsResult = await _settingsService
            .GetSettingsAsync(cancellationToken)
            .ConfigureAwait(false);

        if (persistedSettingsResult.IsFailure)
        {
            return formSettings;
        }

        var persistedSettings = persistedSettingsResult.Value;

        var activeLiveProvider = ResolveProviderKindForSave(
            selectedProvider: formSettings.ActiveLiveProvider,
            persistedProvider: persistedSettings.ActiveLiveProvider);

        var historicalProvider = ResolveProviderKindForSave(
            selectedProvider: formSettings.HistoricalProvider,
            persistedProvider: persistedSettings.HistoricalProvider);

        if (activeLiveProvider == formSettings.ActiveLiveProvider &&
            historicalProvider == formSettings.HistoricalProvider)
        {
            return formSettings;
        }

        return new ApplicationSettings(
            activeLiveProvider: activeLiveProvider,
            historicalProvider: historicalProvider,
            preferredUnit: formSettings.PreferredUnit,
            targetLowMgDl: formSettings.TargetLowMgDl,
            targetHighMgDl: formSettings.TargetHighMgDl,
            dashboardRefreshInterval: formSettings.DashboardRefreshInterval,
            chartMaximumMgDl: formSettings.ChartMaximumMgDl,
            glucoseAlertsEnabled: formSettings.GlucoseAlertsEnabled,
            lowGlucoseAlertsEnabled: formSettings.LowGlucoseAlertsEnabled,
            highGlucoseAlertsEnabled: formSettings.HighGlucoseAlertsEnabled,
            nativeGlucoseNotificationsEnabled: formSettings.NativeGlucoseNotificationsEnabled,
            glucoseAlertPrivacyModeEnabled: formSettings.GlucoseAlertPrivacyModeEnabled,
            glucoseAlertRepeatInterval: formSettings.GlucoseAlertRepeatInterval,
            glucoseAlertRequiredConsecutiveReadings: formSettings.GlucoseAlertRequiredConsecutiveReadings);
    }

    /// <summary>
    /// Resolves the provider kind to save when the Settings form selected Mock but a real provider is already persisted.
    /// </summary>
    /// <param name="selectedProvider">The provider selected by the Settings form.</param>
    /// <param name="persistedProvider">The provider currently persisted in settings.</param>
    /// <returns>The provider kind to persist.</returns>
    private static CgmProviderKind ResolveProviderKindForSave(
        CgmProviderKind selectedProvider,
        CgmProviderKind persistedProvider)
    {
        if (selectedProvider == CgmProviderKind.Mock &&
            persistedProvider is not CgmProviderKind.Mock and not CgmProviderKind.Unknown)
        {
            return persistedProvider;
        }

        return selectedProvider;
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

        var selectedUnit = NormalizeDisplayUnit(settings.PreferredUnit);
        var selectedChartMaximumMgDl = NormalizeChartMaximumMgDl(settings.ChartMaximumMgDl);

        SelectedLiveProvider = liveProvider;
        SelectedHistoricalProvider = historicalProvider;
        SelectedChartMaximumMgDl = selectedChartMaximumMgDl;
        SelectedPreferredUnit = FindPreferredUnitOption(selectedUnit);
        UpdateTargetRangeUnitPresentation(selectedUnit);
        UpdateChartMaximumPresentation(selectedUnit, selectedChartMaximumMgDl);

        TargetLowMgDlText = FormatTargetValueForUnit(settings.TargetLowMgDl, selectedUnit);
        TargetHighMgDlText = FormatTargetValueForUnit(settings.TargetHighMgDl, selectedUnit);
        DashboardRefreshIntervalSecondsText = ((int)settings.DashboardRefreshInterval.TotalSeconds)
            .ToString(CultureInfo.InvariantCulture);
        GlucoseAlertsEnabled = settings.GlucoseAlertsEnabled;
        LowGlucoseAlertsEnabled = settings.LowGlucoseAlertsEnabled;
        HighGlucoseAlertsEnabled = settings.HighGlucoseAlertsEnabled;
        NativeGlucoseNotificationsEnabled = settings.NativeGlucoseNotificationsEnabled;
        GlucoseAlertPrivacyModeEnabled = settings.GlucoseAlertPrivacyModeEnabled;
        GlucoseAlertRepeatIntervalMinutesText = ((int)settings.GlucoseAlertRepeatInterval.TotalMinutes)
            .ToString(CultureInfo.InvariantCulture);
        GlucoseAlertRequiredConsecutiveReadingsText = settings.GlucoseAlertRequiredConsecutiveReadings
            .ToString(CultureInfo.InvariantCulture);
        UpdateNativeGlucoseTestNotificationAvailability();

        return usedProviderFallback;
    }

    /// <summary>
    /// Normalizes chart maximum values to supported options.
    /// </summary>
    /// <param name="chartMaximumMgDl">The requested chart maximum value.</param>
    /// <returns>The normalized chart maximum value.</returns>
    private static int NormalizeChartMaximumMgDl(int chartMaximumMgDl)
    {
        return chartMaximumMgDl is 400
            ? 400
            : 300;
    }

    /// <summary>
    /// Normalizes unsupported glucose display units to the default display unit.
    /// </summary>
    /// <param name="unit">The requested glucose display unit.</param>
    /// <returns>The normalized glucose display unit.</returns>
    private static GlucoseUnit NormalizeDisplayUnit(GlucoseUnit unit)
    {
        return Enum.IsDefined(unit)
            ? unit
            : GlucoseUnit.MgDl;
    }

    /// <summary>
    /// Updates target range unit labels and placeholders for the selected glucose unit.
    /// </summary>
    /// <param name="unit">The selected glucose unit.</param>
    private void UpdateTargetRangeUnitPresentation(GlucoseUnit unit)
    {
        TargetRangeUnitLabel = FormatGlucoseUnitLabel(unit);

        if (unit == GlucoseUnit.MmolL)
        {
            TargetLowPlaceholderText = "3.9";
            TargetHighPlaceholderText = "10.0";
            return;
        }

        TargetLowPlaceholderText = "70";
        TargetHighPlaceholderText = "180";
    }

    /// <summary>
    /// Updates chart maximum options and unit label for the selected glucose unit while preserving the selected logical chart maximum.
    /// </summary>
    /// <param name="unit">The selected glucose unit.</param>
    /// <param name="chartMaximumMgDl">The chart maximum value to preserve, expressed in mg/dL.</param>
    private void UpdateChartMaximumPresentation(
        GlucoseUnit unit,
        int? chartMaximumMgDl = null)
    {
        var selectedChartMaximumMgDl = NormalizeChartMaximumMgDl(
            chartMaximumMgDl ?? SelectedChartMaximumMgDl);

        ChartMaximumUnitLabel = FormatGlucoseUnitLabel(unit);
        ChartMaximumOptions = BuildChartMaximumOptions(unit);

        SelectedChartMaximumMgDl = selectedChartMaximumMgDl;

        // The selected logical value can remain the same while the ItemsSource changes.
        // Raising the notification explicitly forces the ComboBox to re-match the new display option.
        OnPropertyChanged(nameof(SelectedChartMaximumMgDl));
    }

    /// <summary>
    /// Builds chart maximum selection options for the selected glucose unit.
    /// </summary>
    /// <param name="unit">The selected glucose unit.</param>
    /// <returns>The chart maximum options.</returns>
    private static IReadOnlyList<ChartMaximumSelectionItem> BuildChartMaximumOptions(GlucoseUnit unit)
    {
        return
        [
            new ChartMaximumSelectionItem(
                300,
                FormatChartMaximumValueForUnit(300, unit)),
            new ChartMaximumSelectionItem(
                400,
                FormatChartMaximumValueForUnit(400, unit))
        ];
    }

    /// <summary>
    /// Converts editable target text from one display unit to another.
    /// </summary>
    /// <param name="text">The editable target text.</param>
    /// <param name="sourceUnit">The source glucose unit.</param>
    /// <param name="targetUnit">The target glucose unit.</param>
    /// <returns>The converted editable target text, or the original sanitized text when conversion is not possible.</returns>
    private static string ConvertEditableTargetText(
        string text,
        GlucoseUnit sourceUnit,
        GlucoseUnit targetUnit)
    {
        if (!TryParseTargetValueMgDl(text, sourceUnit, out var valueMgDl))
        {
            return SanitizeTargetValueText(text, targetUnit);
        }

        return FormatTargetValueForUnit(valueMgDl, targetUnit);
    }

    /// <summary>
    /// Formats a target value stored in mg/dL for the selected display unit.
    /// </summary>
    /// <param name="valueMgDl">The target value expressed in mg/dL.</param>
    /// <param name="unit">The selected display unit.</param>
    /// <returns>The formatted target value.</returns>
    private static string FormatTargetValueForUnit(
        int valueMgDl,
        GlucoseUnit unit)
    {
        var value = new GlucoseValue(valueMgDl, GlucoseUnit.MgDl)
            .ConvertTo(unit);

        return unit switch
        {
            GlucoseUnit.MgDl => value.Amount.ToString("0", CultureInfo.InvariantCulture),
            GlucoseUnit.MmolL => value.Amount.ToString("0.0", CultureInfo.InvariantCulture),
            _ => valueMgDl.ToString(CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Formats a chart maximum value stored in mg/dL for the selected display unit.
    /// </summary>
    /// <param name="valueMgDl">The chart maximum value expressed in mg/dL.</param>
    /// <param name="unit">The selected display unit.</param>
    /// <returns>The formatted chart maximum value.</returns>
    private static string FormatChartMaximumValueForUnit(
        int valueMgDl,
        GlucoseUnit unit)
    {
        var value = new GlucoseValue(valueMgDl, GlucoseUnit.MgDl)
            .ConvertTo(unit);

        return unit switch
        {
            GlucoseUnit.MgDl => value.Amount.ToString("0", CultureInfo.InvariantCulture),
            GlucoseUnit.MmolL => value.Amount.ToString("0.0", CultureInfo.InvariantCulture),
            _ => valueMgDl.ToString(CultureInfo.InvariantCulture)
        };
    }

    /// <summary>
    /// Parses an editable target value and converts it to mg/dL.
    /// </summary>
    /// <param name="text">The editable target text.</param>
    /// <param name="unit">The unit used by the editable target text.</param>
    /// <param name="valueMgDl">The parsed value expressed in mg/dL.</param>
    /// <returns>True when parsing succeeds; otherwise false.</returns>
    private static bool TryParseTargetValueMgDl(
        string text,
        GlucoseUnit unit,
        out int valueMgDl)
    {
        valueMgDl = 0;

        var sanitizedText = SanitizeTargetValueText(text, unit);

        if (string.IsNullOrWhiteSpace(sanitizedText))
        {
            return false;
        }

        if (unit == GlucoseUnit.MgDl)
        {
            return int.TryParse(
                    sanitizedText,
                    NumberStyles.Integer,
                    CultureInfo.InvariantCulture,
                    out valueMgDl)
                && valueMgDl > 0;
        }

        if (!decimal.TryParse(
                sanitizedText,
                NumberStyles.Number,
                CultureInfo.InvariantCulture,
                out var displayValue)
            || displayValue <= 0)
        {
            return false;
        }

        var convertedValue = new GlucoseValue(displayValue, unit)
            .ConvertTo(GlucoseUnit.MgDl);

        if (convertedValue.Amount <= 0 || convertedValue.Amount > int.MaxValue)
        {
            return false;
        }

        valueMgDl = (int)convertedValue.Amount;
        return true;
    }

    /// <summary>
    /// Removes unsupported characters from target value input text.
    /// </summary>
    /// <param name="text">The raw input text.</param>
    /// <param name="unit">The selected glucose unit.</param>
    /// <returns>The sanitized input text.</returns>
    private static string SanitizeTargetValueText(
        string text,
        GlucoseUnit unit)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        if (unit != GlucoseUnit.MmolL)
        {
            return SanitizePositiveIntegerText(text);
        }

        var normalizedText = text.Replace(',', '.');
        var sanitizedText = string.Empty;
        var hasDecimalSeparator = false;

        foreach (var character in normalizedText)
        {
            if (char.IsDigit(character))
            {
                sanitizedText += character;
                continue;
            }

            if (character == '.' && !hasDecimalSeparator)
            {
                if (sanitizedText.Length == 0)
                {
                    sanitizedText = "0";
                }

                sanitizedText += ".";
                hasDecimalSeparator = true;
            }
        }

        return sanitizedText;
    }

    /// <summary>
    /// Formats glucose unit labels for user-facing UI.
    /// </summary>
    /// <param name="unit">The glucose unit.</param>
    /// <returns>The display label for the glucose unit.</returns>
    private static string FormatGlucoseUnitLabel(GlucoseUnit unit)
    {
        return unit switch
        {
            GlucoseUnit.MgDl => "mg/dL",
            GlucoseUnit.MmolL => "mmol/L",
            _ => FormatEnumName(unit.ToString())
        };
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

        var selectedUnit = SelectedPreferredUnit.Unit;

        if (!TryParseTargetValueMgDl(TargetLowMgDlText, selectedUnit, out var targetLowMgDl))
        {
            validationMessage = $"Target low must be a positive glucose value expressed in {FormatGlucoseUnitLabel(selectedUnit)}.";
            return null;
        }

        if (!TryParseTargetValueMgDl(TargetHighMgDlText, selectedUnit, out var targetHighMgDl))
        {
            validationMessage = $"Target high must be a positive glucose value expressed in {FormatGlucoseUnitLabel(selectedUnit)}.";
            return null;
        }

        if (!TryParsePositiveInteger(DashboardRefreshIntervalSecondsText, out var refreshIntervalSeconds))
        {
            validationMessage = "Refresh interval must be a positive integer.";
            return null;
        }

        if (!TryParsePositiveInteger(GlucoseAlertRepeatIntervalMinutesText, out var alertRepeatIntervalMinutes))
        {
            validationMessage = "Notification repeat interval must be a positive integer.";
            return null;
        }

        try
        {
            var glucoseAlertRequiredConsecutiveReadings = ParseGlucoseAlertRequiredConsecutiveReadings();

        return new ApplicationSettings(
                activeLiveProvider: SelectedLiveProvider.Kind,
                historicalProvider: SelectedHistoricalProvider.Kind,
                preferredUnit: SelectedPreferredUnit.Unit,
                targetLowMgDl: targetLowMgDl,
                targetHighMgDl: targetHighMgDl,
                dashboardRefreshInterval: TimeSpan.FromSeconds(refreshIntervalSeconds),
                chartMaximumMgDl: NormalizeChartMaximumMgDl(SelectedChartMaximumMgDl),
                glucoseAlertsEnabled: GlucoseAlertsEnabled,
                lowGlucoseAlertsEnabled: LowGlucoseAlertsEnabled,
                highGlucoseAlertsEnabled: HighGlucoseAlertsEnabled,
                nativeGlucoseNotificationsEnabled: NativeGlucoseNotificationsEnabled,
                glucoseAlertPrivacyModeEnabled: GlucoseAlertPrivacyModeEnabled,
                glucoseAlertRepeatInterval: TimeSpan.FromMinutes(alertRepeatIntervalMinutes),
                glucoseAlertRequiredConsecutiveReadings: glucoseAlertRequiredConsecutiveReadings);
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
    /// Applies a native notification request failure to the settings UI.
    /// </summary>
    /// <param name="result">The native notification request result.</param>
    /// <param name="statusMessage">The status message to display.</param>
    private void ApplyFailure(NativeNotificationRequestResult result, string statusMessage)
    {
        HasError = true;
        ErrorMessage = result.UserMessage;
        StatusMessage = statusMessage;

    }

    /// <summary>
    /// Applies a native notification request result presentation to the settings UI.
    /// </summary>
    /// <param name="presentation">The native notification request result presentation.</param>
    private void ApplyNativeNotificationRequestResultPresentation(
        NativeNotificationRequestResultPresentation presentation)
    {
        ArgumentNullException.ThrowIfNull(presentation);

        HasError = presentation.IsFailure;
        ErrorMessage = presentation.ErrorMessage;
        StatusMessage = presentation.StatusMessage;
        NativeGlucoseTestNotificationStatusText = presentation.NotificationStatusText;
    }

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
            .Select(unit => new GlucoseUnitSelectionItem(unit, FormatGlucoseUnitLabel(unit)))
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
    /// Selects the currently available official Dexcom provider for both live and historical readings.
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
    /// Finds the available official Dexcom provider option in the current provider options.
    /// </summary>
    /// <returns>The available official Dexcom provider option, or null when unavailable.</returns>
    private ProviderSelectionItem? FindAvailableDexcomProviderOption()
    {
        return ProviderOptions.FirstOrDefault(provider =>
            provider.IsAvailable && IsOfficialDexcomProviderKind(provider.Kind));
    }

    /// <summary>
    /// Checks whether the provider kind represents an official Dexcom OAuth provider.
    /// </summary>
    /// <param name="kind">The provider kind.</param>
    /// <returns>True when the provider kind is an official Dexcom OAuth provider; otherwise false.</returns>
    private static bool IsOfficialDexcomProviderKind(CgmProviderKind kind)
    {
        return kind is CgmProviderKind.DexcomSandbox or CgmProviderKind.DexcomOfficial;
    }

    #endregion
}

/// <summary>
/// Represents a selectable chart maximum option.
/// </summary>
/// <param name="ValueMgDl">The persisted chart maximum value expressed in mg/dL.</param>
/// <param name="DisplayName">The user-facing chart maximum label.</param>
public sealed record ChartMaximumSelectionItem(
    int ValueMgDl,
    string DisplayName)
{
    /// <inheritdoc />
    public override string ToString()
    {
        return DisplayName;
    }
}
