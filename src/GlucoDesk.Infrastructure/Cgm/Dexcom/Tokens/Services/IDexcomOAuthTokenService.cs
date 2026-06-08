using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;

/// <summary>
/// Defines Dexcom OAuth token retrieval and refresh operations.
/// </summary>
public interface IDexcomOAuthTokenService
{
    /// <summary>
    /// Gets a valid Dexcom OAuth access token, refreshing it when necessary.
    /// </summary>
    /// <param name="request">The token refresh request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A valid Dexcom access token result.</returns>
    Task<Result<DexcomAccessTokenResult>> GetValidAccessTokenAsync(
        DexcomOAuthTokenRefreshRequest request,
        CancellationToken cancellationToken);
}