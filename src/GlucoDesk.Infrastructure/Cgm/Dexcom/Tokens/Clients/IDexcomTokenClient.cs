using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Requests;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Clients;

/// <summary>
/// Defines Dexcom OAuth token operations.
/// </summary>
public interface IDexcomTokenClient
{
    /// <summary>
    /// Exchanges a Dexcom authorization code for an OAuth token set.
    /// </summary>
    /// <param name="request">The authorization code token request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Dexcom OAuth token set.</returns>
    Task<Result<DexcomOAuthTokenSet>> ExchangeAuthorizationCodeAsync(
        DexcomAuthorizationCodeTokenRequest request,
        CancellationToken cancellationToken);

    /// <summary>
    /// Refreshes a Dexcom OAuth token set using the current refresh token.
    /// </summary>
    /// <param name="request">The refresh token request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The refreshed Dexcom OAuth token set.</returns>
    Task<Result<DexcomOAuthTokenSet>> RefreshAccessTokenAsync(
        DexcomRefreshTokenRequest request,
        CancellationToken cancellationToken);
}