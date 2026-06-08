namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Requests;

/// <summary>
/// Represents a request to refresh Dexcom OAuth tokens.
/// </summary>
public sealed record DexcomRefreshTokenRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomRefreshTokenRequest"/> class.
    /// </summary>
    /// <param name="refreshToken">The current refresh token.</param>
    /// <param name="clientSecret">The Dexcom application client secret.</param>
    public DexcomRefreshTokenRequest(
        string refreshToken,
        string clientSecret)
    {
        if (string.IsNullOrWhiteSpace(refreshToken))
        {
            throw new ArgumentException("Refresh token must be specified.", nameof(refreshToken));
        }

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new ArgumentException("Dexcom client secret must be specified.", nameof(clientSecret));
        }

        RefreshToken = refreshToken.Trim();
        ClientSecret = clientSecret.Trim();
    }

    /// <summary>
    /// Gets the current refresh token.
    /// </summary>
    public string RefreshToken { get; }

    /// <summary>
    /// Gets the Dexcom application client secret.
    /// </summary>
    public string ClientSecret { get; }
}