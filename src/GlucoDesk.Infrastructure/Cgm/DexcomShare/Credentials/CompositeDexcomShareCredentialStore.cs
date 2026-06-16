namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Credentials;

/// <summary>
/// Reads Dexcom Share credentials from secure storage first and falls back to development environment variables.
/// </summary>
public sealed class CompositeDexcomShareCredentialStore : IDexcomShareCredentialStore
{
    private readonly MacOsKeychainDexcomShareCredentialStore _secureStore;
    private readonly EnvironmentDexcomShareCredentialStore _environmentStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="CompositeDexcomShareCredentialStore"/> class.
    /// </summary>
    /// <param name="secureStore">The secure credential store.</param>
    /// <param name="environmentStore">The environment credential store.</param>
    public CompositeDexcomShareCredentialStore(
        MacOsKeychainDexcomShareCredentialStore secureStore,
        EnvironmentDexcomShareCredentialStore environmentStore)
    {
        ArgumentNullException.ThrowIfNull(secureStore);
        ArgumentNullException.ThrowIfNull(environmentStore);

        _secureStore = secureStore;
        _environmentStore = environmentStore;
    }

    /// <inheritdoc />
    public async Task<DexcomShareCredentials?> ReadAsync(CancellationToken cancellationToken)
    {
        var storedCredentials = await _secureStore
            .ReadAsync(cancellationToken)
            .ConfigureAwait(false);

        if (storedCredentials?.IsConfigured == true)
        {
            return storedCredentials;
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
        return _secureStore.SaveAsync(credentials, cancellationToken);
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken)
    {
        return _secureStore.ClearAsync(cancellationToken);
    }
}