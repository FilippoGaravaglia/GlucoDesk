using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Credentials;

/// <summary>
/// Reads Dexcom Share credentials from environment variables for development and smoke testing.
/// </summary>
public sealed class EnvironmentDexcomShareCredentialStore : IDexcomShareCredentialStore
{
    private const string UsernameEnvironmentVariable = "GLUCODESK_DEXCOM_SHARE__USERNAME";
    private const string PasswordEnvironmentVariable = "GLUCODESK_DEXCOM_SHARE__PASSWORD";
    private const string RegionEnvironmentVariable = "GLUCODESK_DEXCOM_SHARE__REGION";

    /// <inheritdoc />
    public Task<DexcomShareCredentials?> ReadAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var username = Environment.GetEnvironmentVariable(UsernameEnvironmentVariable);
        var password = Environment.GetEnvironmentVariable(PasswordEnvironmentVariable);
        var region = ParseRegion(Environment.GetEnvironmentVariable(RegionEnvironmentVariable));

        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(password)
            || region is DexcomShareRegion.Unknown)
        {
            return Task.FromResult<DexcomShareCredentials?>(null);
        }

        return Task.FromResult<DexcomShareCredentials?>(
            new DexcomShareCredentials(
                username,
                password,
                region));
    }

    /// <inheritdoc />
    public Task SaveAsync(
        DexcomShareCredentials credentials,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        throw new NotSupportedException("Environment variables are read-only credential sources.");
    }

    /// <inheritdoc />
    public Task ClearAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        throw new NotSupportedException("Environment variables are read-only credential sources.");
    }

    #region Helpers

    /// <summary>
    /// Parses a Dexcom Share region token.
    /// </summary>
    /// <param name="value">The region token.</param>
    /// <returns>The parsed Dexcom Share region.</returns>
    private static DexcomShareRegion ParseRegion(string? value)
    {
        return value?.Trim().ToLowerInvariant() switch
        {
            "us" => DexcomShareRegion.Us,
            "ous" => DexcomShareRegion.OutsideUs,
            "outsideus" => DexcomShareRegion.OutsideUs,
            "outside-us" => DexcomShareRegion.OutsideUs,
            "eu" => DexcomShareRegion.OutsideUs,
            "europe" => DexcomShareRegion.OutsideUs,
            _ => DexcomShareRegion.Unknown
        };
    }

    #endregion
}