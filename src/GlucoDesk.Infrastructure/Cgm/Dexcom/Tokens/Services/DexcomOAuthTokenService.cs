using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Clients;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Requests;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Stores;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;

/// <summary>
/// Provides Dexcom OAuth token retrieval and refresh operations.
/// </summary>
public sealed class DexcomOAuthTokenService : IDexcomOAuthTokenService
{
    private readonly IDexcomOAuthTokenStore _tokenStore;
    private readonly IDexcomTokenClient _tokenClient;
    private readonly DexcomOAuthTokenRefreshOptions _options;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomOAuthTokenService"/> class.
    /// </summary>
    /// <param name="tokenStore">The Dexcom OAuth token store.</param>
    /// <param name="tokenClient">The Dexcom token client.</param>
    /// <param name="options">The token refresh options.</param>
    /// <param name="timeProvider">The time provider.</param>
    public DexcomOAuthTokenService(
        IDexcomOAuthTokenStore tokenStore,
        IDexcomTokenClient tokenClient,
        DexcomOAuthTokenRefreshOptions options,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(tokenStore);
        ArgumentNullException.ThrowIfNull(tokenClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _tokenStore = tokenStore;
        _tokenClient = tokenClient;
        _options = options;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Result<DexcomAccessTokenResult>> GetValidAccessTokenAsync(
        DexcomOAuthTokenRefreshRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var storedTokenResult = await _tokenStore
            .GetTokenSetAsync(cancellationToken)
            .ConfigureAwait(false);

        if (storedTokenResult.IsFailure)
        {
            return Result<DexcomAccessTokenResult>.Failure(storedTokenResult.Error);
        }

        var storedTokenSet = storedTokenResult.Value;

        if (!request.ForceRefresh && IsAccessTokenUsable(storedTokenSet))
        {
            return Result<DexcomAccessTokenResult>.Success(
                new DexcomAccessTokenResult(storedTokenSet, wasRefreshed: false));
        }

        var refreshedTokenResult = await _tokenClient
            .RefreshAccessTokenAsync(
                new DexcomRefreshTokenRequest(
                    storedTokenSet.RefreshToken,
                    request.ClientSecret),
                cancellationToken)
            .ConfigureAwait(false);

        if (refreshedTokenResult.IsFailure)
        {
            return Result<DexcomAccessTokenResult>.Failure(refreshedTokenResult.Error);
        }

        var saveResult = await _tokenStore
            .SaveTokenSetAsync(refreshedTokenResult.Value, cancellationToken)
            .ConfigureAwait(false);

        if (saveResult.IsFailure)
        {
            return Result<DexcomAccessTokenResult>.Failure(saveResult.Error);
        }

        return Result<DexcomAccessTokenResult>.Success(
            new DexcomAccessTokenResult(refreshedTokenResult.Value, wasRefreshed: true));
    }

    #region Helpers

    /// <summary>
    /// Checks whether an access token can be used without refreshing it.
    /// </summary>
    /// <param name="tokenSet">The token set to inspect.</param>
    /// <returns>True when the access token can still be used; otherwise, false.</returns>
    private bool IsAccessTokenUsable(DexcomOAuthTokenSet tokenSet)
    {
        var refreshThresholdUtc = tokenSet.AccessTokenExpiresAtUtc.Subtract(_options.RefreshSafetyWindow);

        return _timeProvider.GetUtcNow() < refreshThresholdUtc;
    }

    #endregion
}