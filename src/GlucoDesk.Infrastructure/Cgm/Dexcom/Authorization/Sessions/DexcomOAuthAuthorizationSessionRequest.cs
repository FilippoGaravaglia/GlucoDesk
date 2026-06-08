namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Sessions;

/// <summary>
/// Represents a Dexcom OAuth authorization session request.
/// </summary>
public sealed record DexcomOAuthAuthorizationSessionRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomOAuthAuthorizationSessionRequest"/> class.
    /// </summary>
    /// <param name="clientSecret">The Dexcom application client secret.</param>
    /// <param name="callbackTimeout">The optional local OAuth callback timeout.</param>
    public DexcomOAuthAuthorizationSessionRequest(
        string clientSecret,
        TimeSpan? callbackTimeout = null)
    {
        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new ArgumentException("Dexcom client secret must be specified.", nameof(clientSecret));
        }

        if (callbackTimeout is not null && callbackTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(callbackTimeout),
                callbackTimeout,
                "Dexcom OAuth callback timeout must be greater than zero.");
        }

        if (callbackTimeout is not null && callbackTimeout > TimeSpan.FromMinutes(10))
        {
            throw new ArgumentOutOfRangeException(
                nameof(callbackTimeout),
                callbackTimeout,
                "Dexcom OAuth callback timeout cannot exceed 10 minutes.");
        }

        ClientSecret = clientSecret.Trim();
        CallbackTimeout = callbackTimeout;
    }

    /// <summary>
    /// Gets the Dexcom application client secret.
    /// </summary>
    public string ClientSecret { get; }

    /// <summary>
    /// Gets the optional local OAuth callback timeout.
    /// </summary>
    public TimeSpan? CallbackTimeout { get; }
}