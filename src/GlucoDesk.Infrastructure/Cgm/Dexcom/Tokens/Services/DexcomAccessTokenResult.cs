using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;

/// <summary>
/// Represents an access token result returned by the Dexcom OAuth token service.
/// </summary>
public sealed record DexcomAccessTokenResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomAccessTokenResult"/> class.
    /// </summary>
    /// <param name="tokenSet">The Dexcom OAuth token set.</param>
    /// <param name="wasRefreshed">Whether the token set was refreshed.</param>
    public DexcomAccessTokenResult(
        DexcomOAuthTokenSet tokenSet,
        bool wasRefreshed)
    {
        ArgumentNullException.ThrowIfNull(tokenSet);

        TokenSet = tokenSet;
        WasRefreshed = wasRefreshed;
    }

    /// <summary>
    /// Gets the Dexcom OAuth token set.
    /// </summary>
    public DexcomOAuthTokenSet TokenSet { get; }

    /// <summary>
    /// Gets a value indicating whether the token set was refreshed.
    /// </summary>
    public bool WasRefreshed { get; }

    /// <summary>
    /// Gets the OAuth access token.
    /// </summary>
    public string AccessToken => TokenSet.AccessToken;

    /// <summary>
    /// Gets the OAuth token type.
    /// </summary>
    public string TokenType => TokenSet.TokenType;

    /// <summary>
    /// Gets the access token expiration timestamp in UTC.
    /// </summary>
    public DateTimeOffset AccessTokenExpiresAtUtc => TokenSet.AccessTokenExpiresAtUtc;
}