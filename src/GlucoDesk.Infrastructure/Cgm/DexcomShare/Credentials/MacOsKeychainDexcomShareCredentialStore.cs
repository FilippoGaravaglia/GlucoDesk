using System.Diagnostics;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Credentials;

/// <summary>
/// Stores Dexcom Share credentials in the macOS Keychain.
/// </summary>
public sealed class MacOsKeychainDexcomShareCredentialStore : IDexcomShareCredentialStore
{
    private const string ServiceName = "GlucoDesk.DexcomShare";
    private const string UsernameAccountName = "username";
    private const string PasswordAccountName = "password";
    private const string RegionAccountName = "region";

    /// <inheritdoc />
    public async Task<DexcomShareCredentials?> ReadAsync(CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return null;
        }

        var username = await ReadSecretAsync(UsernameAccountName, cancellationToken)
            .ConfigureAwait(false);

        var password = await ReadSecretAsync(PasswordAccountName, cancellationToken)
            .ConfigureAwait(false);

        var regionText = await ReadSecretAsync(RegionAccountName, cancellationToken)
            .ConfigureAwait(false);

        var region = ParseRegion(regionText);

        if (string.IsNullOrWhiteSpace(username)
            || string.IsNullOrWhiteSpace(password)
            || region is DexcomShareRegion.Unknown)
        {
            return null;
        }

        return new DexcomShareCredentials(
            username,
            password,
            region);
    }

    /// <inheritdoc />
    public async Task SaveAsync(
        DexcomShareCredentials credentials,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(credentials);

        if (!OperatingSystem.IsMacOS())
        {
            throw new PlatformNotSupportedException("macOS Keychain credential storage is available only on macOS.");
        }

        await SaveSecretAsync(UsernameAccountName, credentials.Username, cancellationToken)
            .ConfigureAwait(false);

        await SaveSecretAsync(PasswordAccountName, credentials.Password, cancellationToken)
            .ConfigureAwait(false);

        await SaveSecretAsync(RegionAccountName, FormatRegion(credentials.Region), cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task ClearAsync(CancellationToken cancellationToken)
    {
        if (!OperatingSystem.IsMacOS())
        {
            return;
        }

        await DeleteSecretAsync(UsernameAccountName, cancellationToken)
            .ConfigureAwait(false);

        await DeleteSecretAsync(PasswordAccountName, cancellationToken)
            .ConfigureAwait(false);

        await DeleteSecretAsync(RegionAccountName, cancellationToken)
            .ConfigureAwait(false);
    }

    #region Helpers

    /// <summary>
    /// Reads a secret from the macOS Keychain.
    /// </summary>
    /// <param name="accountName">The Keychain account name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The secret value, or null when missing.</returns>
    private static async Task<string?> ReadSecretAsync(
        string accountName,
        CancellationToken cancellationToken)
    {
        var result = await RunSecurityCommandAsync(
                [
                    "find-generic-password",
                    "-a",
                    accountName,
                    "-s",
                    ServiceName,
                    "-w"
                ],
                cancellationToken)
            .ConfigureAwait(false);

        return result.ExitCode == 0
            ? result.StandardOutput.Trim()
            : null;
    }

    /// <summary>
    /// Saves a secret in the macOS Keychain.
    /// </summary>
    /// <param name="accountName">The Keychain account name.</param>
    /// <param name="secret">The secret value.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static async Task SaveSecretAsync(
        string accountName,
        string secret,
        CancellationToken cancellationToken)
    {
        await DeleteSecretAsync(accountName, cancellationToken)
            .ConfigureAwait(false);

        var result = await RunSecurityCommandAsync(
                [
                    "add-generic-password",
                    "-a",
                    accountName,
                    "-s",
                    ServiceName,
                    "-w",
                    secret
                ],
                cancellationToken)
            .ConfigureAwait(false);

        if (result.ExitCode != 0)
        {
            throw new InvalidOperationException(
                $"Unable to save Dexcom Share credential '{accountName}' in macOS Keychain. {result.StandardError}");
        }
    }

    /// <summary>
    /// Deletes a secret from the macOS Keychain.
    /// </summary>
    /// <param name="accountName">The Keychain account name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static async Task DeleteSecretAsync(
        string accountName,
        CancellationToken cancellationToken)
    {
        await RunSecurityCommandAsync(
                [
                    "delete-generic-password",
                    "-a",
                    accountName,
                    "-s",
                    ServiceName
                ],
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Runs the macOS security command-line tool.
    /// </summary>
    /// <param name="arguments">The command arguments.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The command result.</returns>
    private static async Task<SecurityCommandResult> RunSecurityCommandAsync(
        IReadOnlyList<string> arguments,
        CancellationToken cancellationToken)
    {
        using var process = new Process();

        process.StartInfo = new ProcessStartInfo
        {
            FileName = "/usr/bin/security",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false
        };

        foreach (var argument in arguments)
        {
            process.StartInfo.ArgumentList.Add(argument);
        }

        process.Start();

        var standardOutputTask = process.StandardOutput.ReadToEndAsync(cancellationToken);
        var standardErrorTask = process.StandardError.ReadToEndAsync(cancellationToken);

        await process.WaitForExitAsync(cancellationToken)
            .ConfigureAwait(false);

        var standardOutput = await standardOutputTask.ConfigureAwait(false);
        var standardError = await standardErrorTask.ConfigureAwait(false);

        return new SecurityCommandResult(
            process.ExitCode,
            standardOutput,
            standardError);
    }

    /// <summary>
    /// Parses a persisted Dexcom Share region value.
    /// </summary>
    /// <param name="value">The persisted region value.</param>
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

    /// <summary>
    /// Formats a Dexcom Share region for persistence.
    /// </summary>
    /// <param name="region">The Dexcom Share region.</param>
    /// <returns>The formatted region value.</returns>
    private static string FormatRegion(DexcomShareRegion region)
    {
        return region switch
        {
            DexcomShareRegion.Us => "us",
            DexcomShareRegion.OutsideUs => "ous",
            _ => "unknown"
        };
    }

    private sealed record SecurityCommandResult(
        int ExitCode,
        string StandardOutput,
        string StandardError);

    #endregion
}