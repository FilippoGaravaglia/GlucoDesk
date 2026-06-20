using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.ViewModels.Account;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Clients;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Credentials;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Readings;

namespace GlucoDesk.Desktop.Tests.ViewModels.Account;

public sealed class AccountViewModelTests
{
    private const string PersonEmail = "person" + "@example.com";
    private const string OldEmail = "old" + "@example.com";
    private const string NewEmail = "new" + "@example.com";
    private const string SecretPassword = "secret-password";
    private const string ExistingPassword = "existing-password";

    [Fact]
    public async Task LoadCommand_ShouldLoadStoredCredentialsWithoutExposingPassword()
    {
        // Arrange
        var store = new FakeDexcomShareCredentialStore
        {
            StoredCredentials = new DexcomShareCredentials(
                PersonEmail,
                SecretPassword,
                DexcomShareRegion.Us)
        };

        var viewModel = CreateViewModel(store);

        // Act
        await viewModel.LoadCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasStoredCredentials);
        Assert.Equal(PersonEmail, viewModel.EmailText);
        Assert.Equal(string.Empty, viewModel.PasswordText);
        Assert.Equal(DexcomShareRegion.Us, viewModel.SelectedRegion?.Region);
        Assert.Equal("Saved account available", viewModel.CredentialStorageStatusText);
        Assert.Equal("Connection not verified", viewModel.ConnectionDiagnosticStatusText);
        Assert.Contains(
            "Password is kept hidden",
            viewModel.StatusMessage,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveCommand_ShouldSaveCredentialsAndClearPasswordField()
    {
        // Arrange
        var store = new FakeDexcomShareCredentialStore();
        var viewModel = CreateViewModel(store);

        viewModel.EmailText = PersonEmail;
        viewModel.PasswordText = SecretPassword;
        viewModel.SelectedRegion = viewModel.RegionOptions
            .Single(option => option.Region == DexcomShareRegion.OutsideUs);

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasStoredCredentials);
        Assert.Equal(string.Empty, viewModel.PasswordText);
        Assert.NotNull(store.StoredCredentials);
        Assert.Equal(PersonEmail, store.StoredCredentials.Username);
        Assert.Equal(SecretPassword, store.StoredCredentials.Password);
        Assert.Equal(DexcomShareRegion.OutsideUs, store.StoredCredentials.Region);
        Assert.Equal(1, store.SaveCallCount);
        Assert.Equal("Connection not verified", viewModel.ConnectionDiagnosticStatusText);
    }

    [Fact]
    public async Task SaveCommand_ShouldConfigureDexcomShareProvidersForRestartAutoReconnect()
    {
        // Arrange
        var store = new FakeDexcomShareCredentialStore();
        var settingsService = new FakeApplicationSettingsService();
        var viewModel = CreateViewModel(store, settingsService: settingsService);

        viewModel.EmailText = PersonEmail;
        viewModel.PasswordText = SecretPassword;
        viewModel.SelectedRegion = viewModel.RegionOptions
            .Single(option => option.Region == DexcomShareRegion.OutsideUs);

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(1, settingsService.SaveCallCount);
        Assert.NotNull(settingsService.SavedSettings);
        Assert.Equal(CgmProviderKind.DexcomShare, settingsService.SavedSettings.ActiveLiveProvider);
        Assert.Equal(CgmProviderKind.DexcomShare, settingsService.SavedSettings.HistoricalProvider);
        Assert.Contains("reconnect after restart", viewModel.StatusMessage, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SaveCommand_ShouldKeepExistingPassword_WhenPasswordFieldIsEmpty()
    {
        // Arrange
        var store = new FakeDexcomShareCredentialStore
        {
            StoredCredentials = new DexcomShareCredentials(
                OldEmail,
                ExistingPassword,
                DexcomShareRegion.OutsideUs)
        };

        var viewModel = CreateViewModel(store);

        viewModel.EmailText = NewEmail;
        viewModel.PasswordText = string.Empty;
        viewModel.SelectedRegion = viewModel.RegionOptions
            .Single(option => option.Region == DexcomShareRegion.Us);

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.NotNull(store.StoredCredentials);
        Assert.Equal(NewEmail, store.StoredCredentials.Username);
        Assert.Equal(ExistingPassword, store.StoredCredentials.Password);
        Assert.Equal(DexcomShareRegion.Us, store.StoredCredentials.Region);
    }

    [Fact]
    public async Task TestConnectionCommand_ShouldAuthenticateWithoutSavingCredentials()
    {
        // Arrange
        var store = new FakeDexcomShareCredentialStore();
        var client = new FakeDexcomShareClient();

        var viewModel = CreateViewModel(store, client);

        viewModel.EmailText = PersonEmail;
        viewModel.PasswordText = SecretPassword;
        viewModel.SelectedRegion = viewModel.RegionOptions
            .Single(option => option.Region == DexcomShareRegion.OutsideUs);

        // Act
        await viewModel.TestConnectionCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(1, client.AuthenticateWithOptionsCallCount);
        Assert.Equal(0, store.SaveCallCount);
        Assert.Null(store.StoredCredentials);
        Assert.Equal("Connection verified", viewModel.ConnectionDiagnosticStatusText);
        Assert.True(viewModel.HasSuccessfulConnectionTest);
        Assert.False(viewModel.HasFailedConnectionTest);
        Assert.Contains(
            "connection successful",
            viewModel.StatusMessage,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task TestConnectionCommand_ShouldShowConnectionDiagnostics_WhenAuthenticationFails()
    {
        // Arrange
        var store = new FakeDexcomShareCredentialStore();
        var client = new FakeDexcomShareClient
        {
            AuthenticationResult = Result<string>.Failure(
                new Error(
                    "DexcomShare.AuthenticationFailed",
                    "Invalid Dexcom Share credentials."))
        };

        var viewModel = CreateViewModel(store, client);

        viewModel.EmailText = PersonEmail;
        viewModel.PasswordText = SecretPassword;
        viewModel.SelectedRegion = viewModel.RegionOptions
            .Single(option => option.Region == DexcomShareRegion.OutsideUs);

        // Act
        await viewModel.TestConnectionCommand.ExecuteAsync(null);

        // Assert
        Assert.Equal(1, client.AuthenticateWithOptionsCallCount);
        Assert.Equal("Connection failed", viewModel.ConnectionDiagnosticStatusText);
        Assert.False(viewModel.HasSuccessfulConnectionTest);
        Assert.True(viewModel.HasFailedConnectionTest);
        Assert.True(viewModel.HasError);
        Assert.Equal("Invalid Dexcom Share credentials.", viewModel.ErrorMessage);
    }

    [Fact]
    public async Task EditingFormAfterSuccessfulConnectionTest_ShouldMarkDiagnosticsAsNotVerified()
    {
        // Arrange
        var viewModel = CreateViewModel();

        viewModel.EmailText = PersonEmail;
        viewModel.PasswordText = SecretPassword;
        viewModel.SelectedRegion = viewModel.RegionOptions
            .Single(option => option.Region == DexcomShareRegion.OutsideUs);

        await viewModel.TestConnectionCommand.ExecuteAsync(null);

        // Act
        viewModel.EmailText = NewEmail;

        // Assert
        Assert.Equal("Connection not verified", viewModel.ConnectionDiagnosticStatusText);
        Assert.False(viewModel.HasSuccessfulConnectionTest);
        Assert.False(viewModel.HasFailedConnectionTest);
        Assert.Contains(
            "form has changed",
            viewModel.ConnectionDiagnosticDescriptionText,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task ClearCredentialsCommand_ShouldClearStoredCredentialsAndResetForm()
    {
        // Arrange
        var store = new FakeDexcomShareCredentialStore
        {
            StoredCredentials = new DexcomShareCredentials(
                PersonEmail,
                SecretPassword,
                DexcomShareRegion.Us)
        };

        var viewModel = CreateViewModel(store);

        await viewModel.LoadCommand.ExecuteAsync(null);

        // Act
        await viewModel.ClearCredentialsCommand.ExecuteAsync(null);

        // Assert
        Assert.False(viewModel.HasStoredCredentials);
        Assert.Equal(string.Empty, viewModel.EmailText);
        Assert.Equal(string.Empty, viewModel.PasswordText);
        Assert.Equal(DexcomShareRegion.OutsideUs, viewModel.SelectedRegion?.Region);
        Assert.Null(store.StoredCredentials);
        Assert.Equal(1, store.ClearCallCount);
        Assert.Equal("Connection not tested", viewModel.ConnectionDiagnosticStatusText);
    }

    [Fact]
    public async Task SaveCommand_ShouldShowValidationError_WhenPasswordIsMissing()
    {
        // Arrange
        var store = new FakeDexcomShareCredentialStore();
        var viewModel = CreateViewModel(store);

        viewModel.EmailText = PersonEmail;
        viewModel.PasswordText = string.Empty;

        // Act
        await viewModel.SaveCommand.ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasError);
        Assert.Equal("Enter your Dexcom password.", viewModel.ErrorMessage);
        Assert.Equal(0, store.SaveCallCount);
    }

    #region Helpers

    /// <summary>
    /// Creates the Account ViewModel under test.
    /// </summary>
    /// <param name="credentialStore">The fake credential store.</param>
    /// <param name="dexcomShareClient">The fake Dexcom Share client.</param>
    /// <param name="settingsService">The fake application settings service.</param>
    /// <returns>The Account ViewModel.</returns>
    private static AccountViewModel CreateViewModel(
        FakeDexcomShareCredentialStore? credentialStore = null,
        FakeDexcomShareClient? dexcomShareClient = null,
        FakeApplicationSettingsService? settingsService = null)
    {
        return new AccountViewModel(
            credentialStore ?? new FakeDexcomShareCredentialStore(),
            dexcomShareClient ?? new FakeDexcomShareClient(),
            settingsService ?? new FakeApplicationSettingsService());
    }

    #endregion

    private sealed class FakeApplicationSettingsService : IApplicationSettingsService
    {
        public ApplicationSettings CurrentSettings { get; set; } = ApplicationSettings.Default;

        public ApplicationSettings? SavedSettings { get; private set; }

        public int GetCallCount { get; private set; }

        public int SaveCallCount { get; private set; }

        /// <inheritdoc />
        public Task<Result<ApplicationSettings>> GetSettingsAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            GetCallCount++;

            return Task.FromResult(Result<ApplicationSettings>.Success(CurrentSettings));
        }

        /// <inheritdoc />
        public Task<Result> SaveSettingsAsync(
            ApplicationSettings settings,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(settings);
            cancellationToken.ThrowIfCancellationRequested();

            SaveCallCount++;
            SavedSettings = settings;
            CurrentSettings = settings;

            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FakeDexcomShareCredentialStore : IDexcomShareCredentialStore
    {
        public DexcomShareCredentials? StoredCredentials { get; set; }

        public int ReadCallCount { get; private set; }

        public int SaveCallCount { get; private set; }

        public int ClearCallCount { get; private set; }

        /// <inheritdoc />
        public Task<DexcomShareCredentials?> ReadAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ReadCallCount++;

            return Task.FromResult(StoredCredentials);
        }

        /// <inheritdoc />
        public Task SaveAsync(
            DexcomShareCredentials credentials,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(credentials);
            cancellationToken.ThrowIfCancellationRequested();

            SaveCallCount++;
            StoredCredentials = credentials;

            return Task.CompletedTask;
        }

        /// <inheritdoc />
        public Task ClearAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            ClearCallCount++;
            StoredCredentials = null;

            return Task.CompletedTask;
        }
    }

    private sealed class FakeDexcomShareClient : IDexcomShareClient
    {
        public int AuthenticateWithOptionsCallCount { get; private set; }

        public Result<string> AuthenticationResult { get; set; } =
            Result<string>.Success("test-session-id");

        /// <inheritdoc />
        public Task<Result<string>> AuthenticateAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(AuthenticationResult);
        }

        /// <inheritdoc />
        public Task<Result<string>> AuthenticateAsync(
            DexcomShareOptions options,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(options);
            cancellationToken.ThrowIfCancellationRequested();

            AuthenticateWithOptionsCallCount++;

            return Task.FromResult(AuthenticationResult);
        }

        /// <inheritdoc />
        public Task<Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>> GetLatestGlucoseValuesAsync(
            string sessionId,
            int minutes,
            int maxCount,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>.Success([]));
        }

        /// <inheritdoc />
        public Task<Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>> GetLatestGlucoseValuesAsync(
            DexcomShareOptions options,
            int minutes,
            int maxCount,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(options);
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>.Success([]));
        }

        /// <inheritdoc />
        public void InvalidateSession()
        {
        }
    }
}