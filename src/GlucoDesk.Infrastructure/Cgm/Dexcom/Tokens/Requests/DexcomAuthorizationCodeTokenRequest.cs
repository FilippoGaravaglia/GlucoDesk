namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Requests;

/// <summary>
/// Represents a request to exchange a Dexcom authorization code for OAuth tokens.
/// </summary>
public sealed record DexcomAuthorizationCodeTokenRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomAuthorizationCodeTokenRequest"/> class.
    /// </summary>
    /// <param name="authorizationCode">The authorization code returned by Dexcom OAuth.</param>
    /// <param name="clientSecret">The Dexcom application client secret.</param>
    public DexcomAuthorizationCodeTokenRequest(
        string authorizationCode,
        string clientSecret)
    {
        if (string.IsNullOrWhiteSpace(authorizationCode))
        {
            throw new ArgumentException("Authorization code must be specified.", nameof(authorizationCode));
        }

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new ArgumentException("Dexcom client secret must be specified.", nameof(clientSecret));
        }

        AuthorizationCode = authorizationCode.Trim();
        ClientSecret = clientSecret.Trim();
    }

    /// <summary>
    /// Gets the authorization code returned by Dexcom OAuth.
    /// </summary>
    public string AuthorizationCode { get; }

    /// <summary>
    /// Gets the Dexcom application client secret.
    /// </summary>
    public string ClientSecret { get; }
}