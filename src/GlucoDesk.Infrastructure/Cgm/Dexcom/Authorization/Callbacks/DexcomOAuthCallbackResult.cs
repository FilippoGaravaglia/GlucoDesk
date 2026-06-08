namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;

/// <summary>
/// Represents a successfully parsed Dexcom OAuth callback.
/// </summary>
public sealed record DexcomOAuthCallbackResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomOAuthCallbackResult"/> class.
    /// </summary>
    /// <param name="authorizationCode">The authorization code returned by Dexcom.</param>
    /// <param name="state">The OAuth state returned by Dexcom.</param>
    public DexcomOAuthCallbackResult(
        string authorizationCode,
        string state)
    {
        if (string.IsNullOrWhiteSpace(authorizationCode))
        {
            throw new ArgumentException("Authorization code must be specified.", nameof(authorizationCode));
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("OAuth state must be specified.", nameof(state));
        }

        AuthorizationCode = authorizationCode.Trim();
        State = state.Trim();
    }

    /// <summary>
    /// Gets the authorization code returned by Dexcom.
    /// </summary>
    public string AuthorizationCode { get; }

    /// <summary>
    /// Gets the OAuth state returned by Dexcom.
    /// </summary>
    public string State { get; }
}