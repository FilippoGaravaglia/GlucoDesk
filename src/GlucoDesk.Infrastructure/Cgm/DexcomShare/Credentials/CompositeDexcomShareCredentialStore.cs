namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Credentials;

/// <summary>
/// Reads Dexcom Share credentials from the platform secure store first and falls back to development environment variables.
/// </summary>
public sealed class CompositeDexcomShareCredentialStore : IDexcomShareCredentialStore
{
    private readonly MacOsKeychainDexcomShareCredentialStore _macOsSecureStore;
    private readonly WindowsCredentialManagerDexcomShareCredentialStore _windowsSecureStore;
    private readonly EnvironmentDexcomShareCredentialStore _environmentStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeDexcomShareCredentialStore"/> class.
    /// </summary>
    /// <param name="macOsSecureStore">The macOS secure credential store.</param>
    /// <param name="windowsSecureStore">The Windows secure credential store.</param>
    /// <param name="environmentStore">The environment credential store.</param>
    public CompositeDexcomShareCredentialStore(
        MacOsKeychainDexcomShareCredentialStore macOsSecureStore,
        WindowsCredentialManagerDexcomShareCredentialStore windowsSecureStore,
        EnvironmentDexcomShareCredentialStore environmentStore)
    {
        ArgumentNullException.ThrowIfNull(macOsSecureStore);
        ArgumentNullException.ThrowIfNull(windowsSecureStore);
        ArgumentNullException.ThrowIfNull(environmentStore);

        _macOsSecureStore = macOsSecureStore;
        _windowsSecureStore = windowsSecureStore;
        _environmentStore = environmentStore;
    }

    /// <inheritdoc />
    public async Task<DexcomShareCredentials?> ReadAsync(CancellationToken cancellationToken)
    {
        var secureStore = GetCurrentPlatformSecureStore();

        if (secureStore is not null)
        {
            var storedCredentials = await secureStore
                .ReadAsync(cancellationToken)
                .ConfigureAwait(false);

            if (storedCredentials?.IsConfigured == true)
            {
                return storedCredentials;
            }
        }

        return await _environmentStore
            .ReadAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public Task SaveAsync(
        DexcomShareCredentials credentials,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        var secureStore = GetCurrentPlatformSecureStore()
            ?? throw new PlatformNotSupportedException(
                "Secure Dexcom Share credential storage is currently supported only on macOS and Windows.");

        return secureStore.SaveAsync(credentials, cancellationToken);
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken)
    {
        var secureStore = GetCurrentPlatformSecureStore()
            ?? throw new PlatformNotSupportedException(
                "Secure Dexcom Share credential storage is currently supported only on macOS and Windows.");

        return secureStore.ClearAsync(cancellationToken);
    }

    #region Helpers

    /// <summary>
    /// Gets the secure credential store for the current platform.
    /// </summary>
    /// <returns>The current platform secure store, or null when unsupported.</returns>
    private IDexcomShareCredentialStore? GetCurrentPlatformSecureStore()
    {
        if (OperatingSystem.IsMacOS())
        {
            return _macOsSecureStore;
        }

        if (OperatingSystem.IsWindows())
        {
            return _windowsSecureStore;
        }

        return null;
    }

    #endregion
}