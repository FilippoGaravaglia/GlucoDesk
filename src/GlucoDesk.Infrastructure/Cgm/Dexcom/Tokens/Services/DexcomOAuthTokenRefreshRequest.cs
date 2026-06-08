namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;

/// <summary>
/// Represents a request to get a valid Dexcom OAuth access token.
/// </summary>
public sealed record DexcomOAuthTokenRefreshRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomOAuthTokenRefreshRequest"/> class.
    /// </summary>
    /// <param name="clientSecret">The Dexcom application client secret.</param>
    /// <param name="forceRefresh">Whether to force a token refresh even if the access token is still valid.</param>
    public DexcomOAuthTokenRefreshRequest(
        string clientSecret,
        bool forceRefresh = false)
    {
        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new ArgumentException("Dexcom client secret must be specified.", nameof(clientSecret));
        }

        ClientSecret = clientSecret.Trim();
        ForceRefresh = forceRefresh;
    }

    /// <summary>
    /// Gets the Dexcom application client secret.
    /// </summary>
    public string ClientSecret { get; }

    /// <summary>
    /// Gets a value indicating whether to force a token refresh even if the access token is still valid.
    /// </summary>
    public bool ForceRefresh { get; }
}