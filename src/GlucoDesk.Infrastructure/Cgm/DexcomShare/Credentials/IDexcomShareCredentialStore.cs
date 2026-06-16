namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Credentials;

/// <summary>
/// Defines secure Dexcom Share credential storage operations.
/// </summary>
public interface IDexcomShareCredentialStore
{
    /// <summary>
    /// Reads persisted Dexcom Share credentials.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The persisted credentials, or null when no credentials are available.</returns>
    Task<DexcomShareCredentials?> ReadAsync(CancellationToken cancellationToken);

    /// <summary>
    /// Saves Dexcom Share credentials.
    /// </summary>
    /// <param name="credentials">The credentials to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task SaveAsync(
        DexcomShareCredentials credentials,
        CancellationToken cancellationToken);

    /// <summary>
    /// Clears persisted Dexcom Share credentials.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    Task ClearAsync(CancellationToken cancellationToken);
}