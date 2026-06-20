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

namespace GlucoDesk.Desktop.ViewModels.Account;

/// <summary>
/// Represents the account screen view model.
/// </summary>
public sealed partial class AccountViewModel : ViewModelBase
{
    private readonly IDexcomShareCredentialStore _credentialStore;
    private readonly IDexcomShareClient _dexcomShareClient;
    private readonly IApplicationSettingsService _settingsService;

    [ObservableProperty]
    private string _emailText = string.Empty;

    [ObservableProperty]
    private string _passwordText = string.Empty;

    [ObservableProperty]
    private DexcomShareRegionSelectionItem? _selectedRegion;

    [ObservableProperty]
    private string _statusMessage = "Account not loaded.";

    [ObservableProperty]
    private string? _errorMessage;

    [ObservableProperty]
    private bool _hasError;

    [ObservableProperty]
    private bool _isBusy;

    [ObservableProperty]
    private bool _hasStoredCredentials;

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

        SelectedRegion = RegionOptions[0];
    }

    /// <summary>
    /// Gets the supported Dexcom Share region options.
    /// </summary>
    public IReadOnlyList<DexcomShareRegionSelectionItem> RegionOptions { get; }

    /// <summary>
    /// Gets the user-facing secure credential storage state.
    /// </summary>
    public string CredentialStorageStatusText => HasStoredCredentials
        ? "Saved account available"
        : "No saved account";

    /// <summary>
    /// Gets the user-facing secure credential storage description.
    /// </summary>
    public string CredentialStorageDescriptionText => HasStoredCredentials
        ? "Dexcom Share credentials are available from secure local storage. The password is not shown in the form."
        : "Save your Dexcom Share account to enable automatic reconnect when GlucoDesk starts.";

    /// <summary>
    /// Gets the user-facing password help text.
    /// </summary>
    public string PasswordHelpText => HasStoredCredentials
        ? "Leave empty to keep the saved password. Enter a new password only if you want to replace it."
        : "Required to connect to Dexcom Share. It will be saved using the configured secure credential store.";

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
        StatusMessage = "Loading account...";

        try
        {
            var credentials = await _credentialStore
                .ReadAsync(cancellationToken)
                .ConfigureAwait(false);

            if (credentials is null)
            {
                HasStoredCredentials = false;
                EmailText = string.Empty;
                PasswordText = string.Empty;
                SelectedRegion = RegionOptions[0];
                StatusMessage = "No Dexcom Share account saved yet.";
                return;
            }

            HasStoredCredentials = true;
            EmailText = credentials.Username;
            PasswordText = string.Empty;
            SelectedRegion = FindRegion(credentials.Region);
            StatusMessage = "Dexcom Share account loaded. Password is kept hidden and remains in secure storage.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Account load cancelled.";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, "Unexpected error while loading account.");
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
        StatusMessage = "Saving account...";

        try
        {
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
            PasswordText = string.Empty;

            var providersConfigured = await EnsureDexcomShareProvidersConfiguredAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!providersConfigured)
            {
                return;
            }

            StatusMessage = "Dexcom Share account saved securely. Dexcom Share is now selected as the live and historical provider, so GlucoDesk can reconnect after restart.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Account save cancelled.";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, "Unexpected error while saving account.");
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
        StatusMessage = "Testing Dexcom Share connection...";

        try
        {
            var credentials = await BuildCredentialsFromCurrentFormAsync(cancellationToken)
                .ConfigureAwait(false);

            if (credentials is null)
            {
                return;
            }

            var options = CreateOptions(credentials);

            var result = await _dexcomShareClient
                .AuthenticateAsync(options, cancellationToken)
                .ConfigureAwait(false);

            if (result.IsFailure)
            {
                ApplyFailure(result, "Dexcom Share connection failed.");
                return;
            }

            StatusMessage = HasStoredCredentials
                ? "Dexcom Share connection successful. Save the account if you changed any field."
                : "Dexcom Share connection successful. Save the account to persist these credentials.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Connection test cancelled.";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, "Unexpected error while testing Dexcom Share connection.");
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
            await _credentialStore
                .ClearAsync(cancellationToken)
                .ConfigureAwait(false);

            HasStoredCredentials = false;
            EmailText = string.Empty;
            PasswordText = string.Empty;
            SelectedRegion = RegionOptions[0];

            StatusMessage = "Dexcom Share account removed from secure storage.";
        }
        catch (OperationCanceledException)
        {
            StatusMessage = "Clear account cancelled.";
        }
        catch (Exception exception)
        {
            ApplyUnexpectedFailure(exception, "Unexpected error while clearing account.");
        }
        finally
        {
            IsBusy = false;
        }
    }

    #region Helpers

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
        return HasStoredCredentials && !IsBusy;
    }

    /// <summary>
    /// Handles busy state changes and refreshes command availability.
    /// </summary>
    /// <param name="value">The new busy state.</param>
    partial void OnIsBusyChanged(bool value)
    {
        LoadCommand.NotifyCanExecuteChanged();
        SaveCommand.NotifyCanExecuteChanged();
        TestConnectionCommand.NotifyCanExecuteChanged();
        ClearCredentialsCommand.NotifyCanExecuteChanged();
    }

    /// <summary>
    /// Handles stored credential state changes and refreshes dependent UI state.
    /// </summary>
    /// <param name="value">The new stored credential state.</param>
    partial void OnHasStoredCredentialsChanged(bool value)
    {
        OnPropertyChanged(nameof(CredentialStorageStatusText));
        OnPropertyChanged(nameof(CredentialStorageDescriptionText));
        OnPropertyChanged(nameof(PasswordHelpText));

        ClearCredentialsCommand.NotifyCanExecuteChanged();
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
            ApplyValidationFailure("Select a Dexcom Share region.");
            return null;
        }

        if (string.IsNullOrWhiteSpace(EmailText))
        {
            ApplyValidationFailure("Enter your Dexcom account email.");
            return null;
        }

        var existingCredentials = await _credentialStore
            .ReadAsync(cancellationToken)
            .ConfigureAwait(false);

        var passwordToUse = string.IsNullOrWhiteSpace(PasswordText)
            ? existingCredentials?.Password
            : PasswordText;

        if (string.IsNullOrWhiteSpace(passwordToUse))
        {
            ApplyValidationFailure("Enter your Dexcom password.");
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
                "Dexcom Share account was saved, but GlucoDesk could not read application settings.");

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
                "Dexcom Share account was saved, but GlucoDesk could not update provider settings.");

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
}