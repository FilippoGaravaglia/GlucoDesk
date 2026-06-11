using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Stores;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Services;

/// <summary>
/// Inspects the current Dexcom connection status from the configured OAuth token store.
/// </summary>
public sealed class DexcomConnectionStatusService : IDexcomConnectionStatusService
{
    private const string TokenStoreEmptyErrorCode = "Dexcom.TokenStoreEmpty";

    private readonly IDexcomOAuthTokenStore _tokenStore;
    private readonly DexcomOAuthTokenRefreshOptions _refreshOptions;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomConnectionStatusService"/> class.
    /// </summary>
    /// <param name="tokenStore">The Dexcom OAuth token store.</param>
    /// <param name="refreshOptions">The Dexcom token refresh options.</param>
    /// <param name="timeProvider">The time provider.</param>
    public DexcomConnectionStatusService(
        IDexcomOAuthTokenStore tokenStore,
        DexcomOAuthTokenRefreshOptions refreshOptions,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(tokenStore);
        ArgumentNullException.ThrowIfNull(refreshOptions);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _tokenStore = tokenStore;
        _refreshOptions = refreshOptions;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Result<DexcomConnectionStatus>> GetConnectionStatusAsync(
        CancellationToken cancellationToken)
    {
        var checkedAtUtc = _timeProvider.GetUtcNow();

        var tokenResult = await _tokenStore
            .GetTokenSetAsync(cancellationToken)
            .ConfigureAwait(false);

        if (tokenResult.IsFailure)
        {
            return Result<DexcomConnectionStatus>.Success(
                BuildFailureStatus(tokenResult, checkedAtUtc));
        }

        return Result<DexcomConnectionStatus>.Success(
            BuildTokenStatus(tokenResult.Value, checkedAtUtc));
    }

    #region Helpers

    /// <summary>
    /// Builds a connection status from a failed token store read.
    /// </summary>
    /// <param name="tokenResult">The failed token result.</param>
    /// <param name="checkedAtUtc">The UTC timestamp when the status was checked.</param>
    /// <returns>The Dexcom connection status.</returns>
    private static DexcomConnectionStatus BuildFailureStatus(
        Result<DexcomOAuthTokenSet> tokenResult,
        DateTimeOffset checkedAtUtc)
    {
        if (tokenResult.Error.Code == TokenStoreEmptyErrorCode)
        {
            return new DexcomConnectionStatus(
                DexcomConnectionState.TokenMissing,
                checkedAtUtc,
                "Dexcom is configured, but no OAuth token is currently stored.");
        }

        return new DexcomConnectionStatus(
            DexcomConnectionState.TokenStoreUnavailable,
            checkedAtUtc,
            $"Dexcom token store is unavailable: {tokenResult.Error.Message}");
    }

    /// <summary>
    /// Builds a connection status from the stored token set.
    /// </summary>
    /// <param name="tokenSet">The stored token set.</param>
    /// <param name="checkedAtUtc">The UTC timestamp when the status was checked.</param>
    /// <returns>The Dexcom connection status.</returns>
    private DexcomConnectionStatus BuildTokenStatus(
        DexcomOAuthTokenSet tokenSet,
        DateTimeOffset checkedAtUtc)
    {
        if (IsRefreshTokenExpired(tokenSet, checkedAtUtc))
        {
            return new DexcomConnectionStatus(
                DexcomConnectionState.RefreshTokenExpired,
                checkedAtUtc,
                "Dexcom authorization expired. Reconnect Dexcom to continue.",
                tokenSet.AccessTokenExpiresAtUtc,
                tokenSet.RefreshTokenExpiresAtUtc);
        }

        if (IsAccessTokenUsable(tokenSet, checkedAtUtc))
        {
            return new DexcomConnectionStatus(
                DexcomConnectionState.Connected,
                checkedAtUtc,
                "Dexcom is connected.",
                tokenSet.AccessTokenExpiresAtUtc,
                tokenSet.RefreshTokenExpiresAtUtc);
        }

        return new DexcomConnectionStatus(
            DexcomConnectionState.AccessTokenRefreshRequired,
            checkedAtUtc,
            "Dexcom access token needs refresh before data can be read.",
            tokenSet.AccessTokenExpiresAtUtc,
            tokenSet.RefreshTokenExpiresAtUtc);
    }

    /// <summary>
    /// Checks whether the access token can be used without refreshing it.
    /// </summary>
    /// <param name="tokenSet">The token set.</param>
    /// <param name="checkedAtUtc">The UTC timestamp used for the check.</param>
    /// <returns>True when the access token is usable; otherwise false.</returns>
    private bool IsAccessTokenUsable(
        DexcomOAuthTokenSet tokenSet,
        DateTimeOffset checkedAtUtc)
    {
        var refreshThresholdUtc = tokenSet.AccessTokenExpiresAtUtc.Subtract(
            _refreshOptions.RefreshSafetyWindow);

        return checkedAtUtc < refreshThresholdUtc;
    }

    /// <summary>
    /// Checks whether the refresh token is expired.
    /// </summary>
    /// <param name="tokenSet">The token set.</param>
    /// <param name="checkedAtUtc">The UTC timestamp used for the check.</param>
    /// <returns>True when the refresh token is expired; otherwise false.</returns>
    private static bool IsRefreshTokenExpired(
        DexcomOAuthTokenSet tokenSet,
        DateTimeOffset checkedAtUtc)
    {
        return tokenSet.RefreshTokenExpiresAtUtc is not null
            && checkedAtUtc >= tokenSet.RefreshTokenExpiresAtUtc;
    }

    #endregion
}