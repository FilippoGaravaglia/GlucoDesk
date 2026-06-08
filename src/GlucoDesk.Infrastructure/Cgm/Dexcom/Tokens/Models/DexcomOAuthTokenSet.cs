namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;

/// <summary>
/// Represents a Dexcom OAuth token set returned by the Official API.
/// </summary>
public sealed record DexcomOAuthTokenSet
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomOAuthTokenSet"/> class.
    /// </summary>
    /// <param name="accessToken">The OAuth access token.</param>
    /// <param name="refreshToken">The OAuth refresh token.</param>
    /// <param name="tokenType">The OAuth token type.</param>
    /// <param name="issuedAtUtc">The UTC timestamp when the token set was issued locally.</param>
    /// <param name="accessTokenExpiresAtUtc">The UTC timestamp when the access token expires.</param>
    /// <param name="refreshTokenExpiresAtUtc">The optional UTC timestamp when the refresh token expires.</param>
    public DexcomOAuthTokenSet(
        string accessToken,
        string refreshToken,
        string tokenType,
        DateTimeOffset issuedAtUtc,
        DateTimeOffset accessTokenExpiresAtUtc,
        DateTimeOffset? refreshTokenExpiresAtUtc)
    {
        if (string.IsNullOrWhiteSpace(accessToken))
        {
            throw new ArgumentException("Access token must be specified.", nameof(accessToken));
        }

        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token must be specified.", nameof(refreshToken));
        }

        if (string.IsNullOrWhiteSpace(tokenType))
        {
            throw new ArgumentException("Token type must be specified.", nameof(tokenType));
        }

        if (accessTokenExpiresAtUtc <= issuedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(accessTokenExpiresAtUtc),
                accessTokenExpiresAtUtc,
                "Access token expiration must be greater than issued timestamp.");
        }

        if (refreshTokenExpiresAtUtc is not null && refreshTokenExpiresAtUtc <= issuedAtUtc)
        {
            throw new ArgumentOutOfRangeException(
                nameof(refreshTokenExpiresAtUtc),
                refreshTokenExpiresAtUtc,
                "Refresh token expiration must be greater than issued timestamp.");
        }

        AccessToken = accessToken.Trim();
        RefreshToken = refreshToken.Trim();
        TokenType = tokenType.Trim();
        IssuedAtUtc = issuedAtUtc;
        AccessTokenExpiresAtUtc = accessTokenExpiresAtUtc;
        RefreshTokenExpiresAtUtc = refreshTokenExpiresAtUtc;
    }

    /// <summary>
    /// Gets the OAuth access token.
    /// </summary>
    public string AccessToken { get; }

    /// <summary>
    /// Gets the OAuth refresh token.
    /// </summary>
    public string RefreshToken { get; }

    /// <summary>
    /// Gets the OAuth token type.
    /// </summary>
    public string TokenType { get; }

    /// <summary>
    /// Gets the UTC timestamp when the token set was issued locally.
    /// </summary>
    public DateTimeOffset IssuedAtUtc { get; }

    /// <summary>
    /// Gets the UTC timestamp when the access token expires.
    /// </summary>
    public DateTimeOffset AccessTokenExpiresAtUtc { get; }

    /// <summary>
    /// Gets the optional UTC timestamp when the refresh token expires.
    /// </summary>
    public DateTimeOffset? RefreshTokenExpiresAtUtc { get; }

    /// <summary>
    /// Gets a value indicating whether the token type is Bearer.
    /// </summary>
    public bool IsBearerToken => string.Equals(TokenType, "Bearer", StringComparison.OrdinalIgnoreCase);
}