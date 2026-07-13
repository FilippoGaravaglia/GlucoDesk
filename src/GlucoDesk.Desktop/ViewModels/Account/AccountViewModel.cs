using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Clients;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Credentials;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;
using Avalonia.Threading;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.ViewModels.Account;

/// <summary>
/// Represents the account screen view model.
/// </summary>
public sealed partial class AccountViewModel : ViewModelBase
{
    private static string UnsupportedAccountConnectionMessage =>

        T("AccountUnsupportedPlatformMessage");

    private readonly IDexcomShareCredentialStore _credentialStore;
    private readonly IDexcomShareClient _dexcomShareClient;
    private readonly IApplicationSettingsService _settingsService;

    private bool _suppressFormChangeDiagnostics;

    [ObservableProperty]
    private string _emailText = string.Empty;

    [ObservableProperty]
    private string _passwordText = string.Empty;

    [ObservableProperty]
    private DexcomShareRegionSelectionItem? _selectedRegion;

    [ObservableProperty]
    private string _statusMessage = T("AccountNotLoaded");

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasStoredCredentials;

    [ObservableProperty]
    private string _connectionDiagnosticStatusText = T("AccountConnectionNotTested");

    [ObservableProperty]
    private string _connectionDiagnosticDescriptionText = T("AccountInitialDiagnosticsDescription");

    [ObservableProperty]
    private bool _hasSuccessfulConnectionTest;

    [ObservableProperty]
    private bool _hasFailedConnectionTest;

    /// <summary>
    /// Initializes a new instance of the <see cref="AccountViewModel"/> class.
    /// </summary>
    /// <param name="credentialStore">The Dexcom Share credential store.</param>
    /// <param name="dexcomShareClient">The Dexcom Share client.</param>
    /// <param name="settingsService">The application settings service.</param>
    public AccountViewModel(
        IDexcomShareCredentialStore credentialStore,
        IDexcomShareClient dexcomShareClient,
        IApplicationSettingsService settingsService)
    {
        ArgumentNullException.ThrowIfNull(credentialStore);
        ArgumentNullException.ThrowIfNull(dexcomShareClient);
        ArgumentNullException.ThrowIfNull(settingsService);

        _credentialStore = credentialStore;
        _dexcomShareClient = dexcomShareClient;
        _settingsService = settingsService;

        RegionOptions =
        [
            new DexcomShareRegionSelectionItem(
                DexcomShareRegion.OutsideUs,
                "Outside US / Europe"),

            new DexcomShareRegionSelectionItem(
                DexcomShareRegion.Us,
                "United States")
        ];

        _suppressFormChangeDiagnostics = true;
        SelectedRegion = RegionOptions[0];
        _suppressFormChangeDiagnostics = false;
    }

    /// <summary>
    /// Gets the supported Dexcom Share region options.
    /// </summary>
    public IReadOnlyList<DexcomShareRegionSelectionItem> RegionOptions { get; }

    /// <summary>
    /// Gets the user-facing secure credential storage state.
    /// </summary>
    public string CredentialStorageStatusText
    {
        get
        {
            if (!IsDexcomShareAccountConnectionSupportedOnCurrentPlatform())
            {
                return T("AccountConnectionUnavailable");
            }

            return HasStoredCredentials
                ? T("AccountSavedAvailable")
                : T("AccountNoSavedAccount");
        }
    }

    /// <summary>
    /// Gets the user-facing secure credential storage description.
    /// </summary>
    public string CredentialStorageDescriptionText
    {
        get
        {
            if (!IsDexcomShareAccountConnectionSupportedOnCurrentPlatform())
            {
                return T("AccountSecureStorageUnavailable");
            }

            return HasStoredCredentials
                ? T("AccountStoredCredentialsAvailable")
                : T("AccountSaveToEnableReconnect");
        }
    }

    /// <summary>
    /// Gets the user-facing password help text.
    /// </summary>
    public string PasswordHelpText
    {
        get
        {
            if (!IsDexcomShareAccountConnectionSupportedOnCurrentPlatform())
            {
                return T("AccountPlatformUnavailable");
            }

            return HasStoredCredentials
                ? T("AccountPasswordKeepExisting")
                : T("AccountPasswordRequiredHelp");
        }
    }

    /// <summary>
    /// Loads persisted Dexcom Share account details.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand(CanExecute = nameof(CanRunAccountOperation))]
    public async Task LoadAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        StatusMessage = T("AccountLoading");

        try
        {
            if (!IsDexcomShareAccountConnectionSupportedOnCurrentPlatform())
            {
                ApplyUnsupportedAccountConnectionFailure();
                return;
            }

            var credentials = await _credentialStore
                .ReadAsync(cancellationToken)
                .ConfigureAwait(false);

            if (credentials is null)
            {
                HasStoredCredentials = false;
                ApplyAccountFormState(
                    email: string.Empty,
                    password: string.Empty,
                    region: RegionOptions[0]);

                ResetConnectionDiagnostics(
                    T("AccountNoSavedCredentialsDescription"));

                StatusMessage = T("AccountNoAccountSavedStatus");
                return;
            }

            HasStoredCredentials = true;
            ApplyAccountFormState(
                credentials.Username,
                password: string.Empty,
                FindRegion(credentials.Region));

            SetConnectionDiagnosticsPending(
                T("AccountLoadedDiagnostics"));

            StatusMessage = T("AccountLoadedStatus");
        }
        catch (OperationCanceledException)
        {
            StatusMessage = T("AccountLoadCancelled");
        }
        catch (PlatformNotSupportedException exception)
        {
            ApplyPlatformNotSupportedFailure(exception);
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, T("AccountUnexpectedLoadError"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Saves Dexcom Share account details.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand(CanExecute = nameof(CanRunAccountOperation))]
    private async Task SaveAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        StatusMessage = T("AccountSaving");

        try
        {
            if (!IsDexcomShareAccountConnectionSupportedOnCurrentPlatform())
            {
                ApplyUnsupportedAccountConnectionFailure();
                return;
            }

            var credentials = await BuildCredentialsFromCurrentFormAsync(cancellationToken)
                .ConfigureAwait(false);

            if (credentials is null)
            {
                return;
            }

            await _credentialStore
                .SaveAsync(credentials, cancellationToken)
                .ConfigureAwait(false);

            HasStoredCredentials = true;

            ApplyAccountFormState(
                credentials.Username,
                password: string.Empty,
                FindRegion(credentials.Region));

            var providersConfigured = await EnsureDexcomShareProvidersConfiguredAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!providersConfigured)
            {
                SetConnectionDiagnosticsPending(
                    T("AccountSavedSettingsFailedDiagnostics"));
                return;
            }

            SetConnectionDiagnosticsPending(
                T("AccountSavedPendingTestDiagnostics"));

            StatusMessage = T("AccountSavedSuccessfullyStatus");
        }
        catch (OperationCanceledException)
        {
            StatusMessage = T("AccountSaveCancelled");
        }
        catch (PlatformNotSupportedException exception)
        {
            ApplyPlatformNotSupportedFailure(exception);
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, T("AccountUnexpectedSaveError"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Tests the current Dexcom Share account without saving it.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand(CanExecute = nameof(CanRunAccountOperation))]
    private async Task TestConnectionAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        StatusMessage = T("AccountTestingConnection");
        SetConnectionDiagnosticsPending(T("AccountTestingCredentials"));

        try
        {
            if (!IsDexcomShareAccountConnectionSupportedOnCurrentPlatform())
            {
                ApplyUnsupportedAccountConnectionFailure();
                return;
            }

            var credentials = await BuildCredentialsFromCurrentFormAsync(cancellationToken)
                .ConfigureAwait(false);

            if (credentials is null)
            {
                SetConnectionDiagnosticsFailed(T("AccountIncompleteForm"));
                return;
            }

            var options = CreateOptions(credentials);

            var result = await _dexcomShareClient
                .AuthenticateAsync(options, cancellationToken)
                .ConfigureAwait(false);

            if (result.IsFailure)
            {
                var message = string.IsNullOrWhiteSpace(result.Error.Message)
                    ? T("AccountCredentialsRejected")
                    : result.Error.Message;

                SetConnectionDiagnosticsFailed(message);
                ApplyFailure(result, T("AccountConnectionFailedFallback"));
                return;
            }

            SetConnectionDiagnosticsSucceeded(
                T("AccountCredentialsAccepted"));

            StatusMessage = HasStoredCredentials
                ? T("AccountConnectionSuccessfulSaved")
                : T("AccountConnectionSuccessfulUnsaved");
        }
        catch (OperationCanceledException)
        {
            StatusMessage = T("AccountConnectionTestCancelled");
            SetConnectionDiagnosticsPending(T("AccountConnectionTestCancelledDiagnostics"));
        }
        catch (PlatformNotSupportedException exception)
        {
            ApplyPlatformNotSupportedFailure(exception);
        }
        catch (Exception exception)
        {
            SetConnectionDiagnosticsFailed(exception.Message);
            ApplyUnexpectedFailure(exception, T("AccountUnexpectedConnectionTestError"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    /// <summary>
    /// Clears saved Dexcom Share account details.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand(CanExecute = nameof(CanClearStoredCredentials))]
    private async Task ClearCredentialsAsync(CancellationToken cancellationToken)
    {
        if (IsBusy)
        {
            return;
        }

        IsBusy = true;
        ClearError();
        StatusMessage = "Clearing account...";

        try
        {
            if (!IsDexcomShareAccountConnectionSupportedOnCurrentPlatform())
            {
                ApplyUnsupportedAccountConnectionFailure();
                return;
            }

            await _credentialStore
                .ClearAsync(cancellationToken)
                .ConfigureAwait(false);

            HasStoredCredentials = false;

            ApplyAccountFormState(
                email: string.Empty,
                password: string.Empty,
                region: RegionOptions[0]);

            ResetConnectionDiagnostics(
                T("AccountCredentialsRemovedDiagnostics"));

            StatusMessage = T("AccountCredentialsRemovedStatus");
        }
        catch (OperationCanceledException)
        {
            StatusMessage = T("AccountClearCancelled");
        }
        catch (PlatformNotSupportedException exception)
        {
            ApplyPlatformNotSupportedFailure(exception);
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, T("AccountUnexpectedClearError"));
        }
        finally
        {
            IsBusy = false;
        }
    }

    #region Helpers

    /// <summary>
    /// Refreshes account command states on the UI thread.
    /// </summary>
    private void RefreshAccountCommandStates()
    {
        RunOnUiThread(() =>
        {
            LoadCommand.NotifyCanExecuteChanged();
            SaveCommand.NotifyCanExecuteChanged();
            TestConnectionCommand.NotifyCanExecuteChanged();
            ClearCredentialsCommand.NotifyCanExecuteChanged();
        });
    }
    
    /// <summary>
    /// Refreshes credential storage presentation properties on the UI thread.
    /// </summary>
    private void RefreshCredentialStorageState()
    {
        RunOnUiThread(() =>
        {
            OnPropertyChanged(nameof(CredentialStorageStatusText));
            OnPropertyChanged(nameof(CredentialStorageDescriptionText));
            OnPropertyChanged(nameof(PasswordHelpText));
        });
    }

    /// <summary>
    /// Runs the specified action on the Avalonia UI thread.
    /// </summary>
    /// <param name="action">The action to run.</param>
    private static void RunOnUiThread(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.UIThread.Post(action);
    }

    /// <summary>
    /// Indicates whether the Dexcom Share account flow is enabled for this ViewModel.
    /// </summary>
    /// <remarks>
    /// Platform-specific credential persistence is handled by the concrete credential store registered by dependency injection.
    /// The ViewModel intentionally stays platform-independent so it can be tested on Linux CI with fake stores.
    /// </remarks>
    private static bool IsDexcomShareAccountConnectionSupportedOnCurrentPlatform()
    {
        return true;
    }

    /// <summary>
    /// Applies a controlled failure when the Dexcom Share account flow is not supported on the current platform.
    /// </summary>
    private void ApplyUnsupportedAccountConnectionFailure()
    {
        HasStoredCredentials = false;
        HasError = true;
        ErrorMessage = UnsupportedAccountConnectionMessage;
        StatusMessage = T("AccountPlatformUnavailable");

        SetConnectionDiagnosticsFailed(UnsupportedAccountConnectionMessage);

        OnPropertyChanged(nameof(CredentialStorageStatusText));
        OnPropertyChanged(nameof(CredentialStorageDescriptionText));
        OnPropertyChanged(nameof(PasswordHelpText));
    }

    /// <summary>
    /// Applies a controlled platform-not-supported failure.
    /// </summary>
    /// <param name="exception">The platform exception.</param>
    private void ApplyPlatformNotSupportedFailure(PlatformNotSupportedException exception)
    {
        ArgumentNullException.ThrowIfNull(exception);

        var message = string.IsNullOrWhiteSpace(exception.Message)
            ? UnsupportedAccountConnectionMessage
            : exception.Message;

        HasError = true;
        ErrorMessage = message;
        StatusMessage = T("AccountPlatformUnavailable");

        SetConnectionDiagnosticsFailed(message);

        OnPropertyChanged(nameof(CredentialStorageStatusText));
        OnPropertyChanged(nameof(CredentialStorageDescriptionText));
        OnPropertyChanged(nameof(PasswordHelpText));
    }

    /// <summary>
    /// Determines whether an account operation can run.
    /// </summary>
    /// <returns>True when no account operation is already running; otherwise false.</returns>
    private bool CanRunAccountOperation()
    {
        return !IsBusy;
    }

    /// <summary>
    /// Determines whether stored credentials can be cleared.
    /// </summary>
    /// <returns>True when stored credentials exist and no operation is running; otherwise false.</returns>
    private bool CanClearStoredCredentials()
    {
        return HasStoredCredentials
            && !IsBusy
            && IsDexcomShareAccountConnectionSupportedOnCurrentPlatform();
    }

    /// <summary>
    /// Handles email changes and marks diagnostics as stale when the user edits the form.
    /// </summary>
    /// <param name="value">The new email value.</param>
    partial void OnEmailTextChanged(string value)
    {
        MarkDiagnosticsStaleAfterUserChange();
    }

    /// <summary>
    /// Handles password changes and marks diagnostics as stale when the user edits the form.
    /// </summary>
    /// <param name="value">The new password value.</param>
    partial void OnPasswordTextChanged(string value)
    {
        MarkDiagnosticsStaleAfterUserChange();
    }

    /// <summary>
    /// Handles region changes and marks diagnostics as stale when the user edits the form.
    /// </summary>
    /// <param name="value">The new selected region.</param>
    partial void OnSelectedRegionChanged(DexcomShareRegionSelectionItem? value)
    {
        MarkDiagnosticsStaleAfterUserChange();
    }

    /// <summary>
    /// Handles busy state changes and refreshes command availability.
    /// </summary>
    /// <param name="value">The new busy state.</param>
    partial void OnIsBusyChanged(bool value)
    {
        RefreshAccountCommandStates();
    }

    /// <summary>
    /// Handles stored credential state changes and refreshes dependent UI state.
    /// </summary>
    /// <param name="value">The new stored credential state.</param>
    partial void OnHasStoredCredentialsChanged(bool value)
    {
        RefreshCredentialStorageState();
        RefreshAccountCommandStates();
    }

    /// <summary>
    /// Applies account form state while suppressing stale diagnostics generated by programmatic changes.
    /// </summary>
    /// <param name="email">The account email.</param>
    /// <param name="password">The account password field value.</param>
    /// <param name="region">The selected Dexcom Share region.</param>
    private void ApplyAccountFormState(
        string email,
        string password,
        DexcomShareRegionSelectionItem region)
    {
        ArgumentNullException.ThrowIfNull(region);

        _suppressFormChangeDiagnostics = true;

        try
        {
            EmailText = email;
            PasswordText = password;
            SelectedRegion = region;
        }
        finally
        {
            _suppressFormChangeDiagnostics = false;
        }
    }

    /// <summary>
    /// Marks connection diagnostics as stale after a user-driven form change.
    /// </summary>
    private void MarkDiagnosticsStaleAfterUserChange()
    {
        if (_suppressFormChangeDiagnostics || IsBusy)
        {
            return;
        }

        SetConnectionDiagnosticsPending(
            T("AccountFormChangedDiagnostics"));
    }

    /// <summary>
    /// Resets connection diagnostics to the not-tested state.
    /// </summary>
    /// <param name="description">The diagnostic description.</param>
    private void ResetConnectionDiagnostics(string description)
    {
        HasSuccessfulConnectionTest = false;
        HasFailedConnectionTest = false;
        ConnectionDiagnosticStatusText = T("AccountConnectionNotTested");
        ConnectionDiagnosticDescriptionText = description;
    }

    /// <summary>
    /// Sets connection diagnostics to a pending or stale state.
    /// </summary>
    /// <param name="description">The diagnostic description.</param>
    private void SetConnectionDiagnosticsPending(string description)
    {
        HasSuccessfulConnectionTest = false;
        HasFailedConnectionTest = false;
        ConnectionDiagnosticStatusText = T("AccountConnectionNotVerified");
        ConnectionDiagnosticDescriptionText = description;
    }

    /// <summary>
    /// Sets connection diagnostics to a successful state.
    /// </summary>
    /// <param name="description">The diagnostic description.</param>
    private void SetConnectionDiagnosticsSucceeded(string description)
    {
        HasSuccessfulConnectionTest = true;
        HasFailedConnectionTest = false;
        ConnectionDiagnosticStatusText = T("AccountConnectionVerified");
        ConnectionDiagnosticDescriptionText = description;
    }

    /// <summary>
    /// Sets connection diagnostics to a failed state.
    /// </summary>
    /// <param name="description">The diagnostic description.</param>
    private void SetConnectionDiagnosticsFailed(string description)
    {
        HasSuccessfulConnectionTest = false;
        HasFailedConnectionTest = true;
        ConnectionDiagnosticStatusText = T("AccountConnectionFailed");
        ConnectionDiagnosticDescriptionText = description;
    }

    /// <summary>
    /// Builds Dexcom Share credentials from the current form values without saving them.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The credentials, or null when the form is invalid.</returns>
    private async Task<DexcomShareCredentials?> BuildCredentialsFromCurrentFormAsync(CancellationToken cancellationToken)
    {
        if (SelectedRegion is null)
        {
            ApplyValidationFailure(T("AccountSelectRegionValidation"));
            return null;
        }

        if (string.IsNullOrWhiteSpace(EmailText))
        {
            ApplyValidationFailure(T("AccountEnterEmailValidation"));
            return null;
        }

        if (!string.IsNullOrWhiteSpace(PasswordText))
        {
            return new DexcomShareCredentials(
                EmailText,
                PasswordText,
                SelectedRegion.Region);
        }

        var existingCredentials = await _credentialStore
            .ReadAsync(cancellationToken)
            .ConfigureAwait(false);

        var passwordToUse = existingCredentials?.Password;

        if (string.IsNullOrWhiteSpace(passwordToUse))
        {
            ApplyValidationFailure(T("AccountEnterPasswordValidation"));
            return null;
        }

        return new DexcomShareCredentials(
            EmailText,
            passwordToUse,
            SelectedRegion.Region);
    }

    /// <summary>
    /// Ensures Dexcom Share is selected as the active live and historical provider.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>True when provider settings are configured successfully; otherwise false.</returns>
    private async Task<bool> EnsureDexcomShareProvidersConfiguredAsync(CancellationToken cancellationToken)
    {
        var settingsResult = await _settingsService
            .GetSettingsAsync(cancellationToken)
            .ConfigureAwait(false);

        if (settingsResult.IsFailure)
        {
            ApplyFailure(
                settingsResult,
                T("AccountSettingsReadFailed"));

            return false;
        }

        var currentSettings = settingsResult.Value;

        if (currentSettings.ActiveLiveProvider == CgmProviderKind.DexcomShare &&
            currentSettings.HistoricalProvider == CgmProviderKind.DexcomShare)
        {
            return true;
        }

        var updatedSettings = CreateDexcomShareProviderSettings(currentSettings);

        var saveSettingsResult = await _settingsService
            .SaveSettingsAsync(updatedSettings, cancellationToken)
            .ConfigureAwait(false);

        if (saveSettingsResult.IsFailure)
        {
            ApplyFailure(
                saveSettingsResult,
                T("AccountSettingsUpdateFailed"));

            return false;
        }

        return true;
    }

    /// <summary>
    /// Creates application settings with Dexcom Share selected as live and historical provider.
    /// </summary>
    /// <param name="currentSettings">The current application settings.</param>
    /// <returns>The updated application settings.</returns>
    private static ApplicationSettings CreateDexcomShareProviderSettings(ApplicationSettings currentSettings)
    {
        ArgumentNullException.ThrowIfNull(currentSettings);

        return new ApplicationSettings(
            CgmProviderKind.DexcomShare,
            CgmProviderKind.DexcomShare,
            currentSettings.PreferredUnit,
            currentSettings.TargetLowMgDl,
            currentSettings.TargetHighMgDl,
            currentSettings.DashboardRefreshInterval);
    }

    /// <summary>
    /// Creates Dexcom Share options from credentials.
    /// </summary>
    /// <param name="credentials">The Dexcom Share credentials.</param>
    /// <returns>The Dexcom Share options.</returns>
    private static DexcomShareOptions CreateOptions(DexcomShareCredentials credentials)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        return new DexcomShareOptions(
            credentials.Username,
            credentials.Password,
            credentials.Region,
            displayName: "Dexcom Share");
    }

    /// <summary>
    /// Finds a region selection item by region.
    /// </summary>
    /// <param name="region">The Dexcom Share region.</param>
    /// <returns>The matching region selection item.</returns>
    private DexcomShareRegionSelectionItem FindRegion(DexcomShareRegion region)
    {
        return RegionOptions.FirstOrDefault(option => option.Region == region)
            ?? RegionOptions[0];
    }

    /// <summary>
    /// Clears the current error state.
    /// </summary>
    private void ClearError()
    {
        HasError = false;
        ErrorMessage = null;
    }

    /// <summary>
    /// Applies a validation failure.
    /// </summary>
    /// <param name="message">The validation message.</param>
    private void ApplyValidationFailure(string message)
    {
        HasError = true;
        ErrorMessage = message;
        StatusMessage = message;
    }

    /// <summary>
    /// Applies an operation failure.
    /// </summary>
    /// <param name="result">The failed result.</param>
    /// <param name="fallbackMessage">The fallback message.</param>
    private void ApplyFailure(
        Result result,
        string fallbackMessage)
    {
        HasError = true;
        ErrorMessage = string.IsNullOrWhiteSpace(result.Error.Message)
            ? fallbackMessage
            : result.Error.Message;
        StatusMessage = fallbackMessage;
    }

    /// <summary>
    /// Applies an operation failure.
    /// </summary>
    /// <typeparam name="TValue">The result value type.</typeparam>
    /// <param name="result">The failed result.</param>
    /// <param name="fallbackMessage">The fallback message.</param>
    private void ApplyFailure<TValue>(
        Result<TValue> result,
        string fallbackMessage)
        where TValue : notnull
    {
        HasError = true;
        ErrorMessage = string.IsNullOrWhiteSpace(result.Error.Message)
            ? fallbackMessage
            : result.Error.Message;
        StatusMessage = fallbackMessage;
    }

    /// <summary>
    /// Applies an unexpected exception failure.
    /// </summary>
    /// <param name="exception">The exception.</param>
    /// <param name="fallbackMessage">The fallback message.</param>
    private void ApplyUnexpectedFailure(
        Exception exception,
        string fallbackMessage)
    {
        HasError = true;
        ErrorMessage = exception.Message;
        StatusMessage = fallbackMessage;
    }

    #endregion

    private static string T(string key)
    {
        return LocalizationManager.GetString(key);
    }

}