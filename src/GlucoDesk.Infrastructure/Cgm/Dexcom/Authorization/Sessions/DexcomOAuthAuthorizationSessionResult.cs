using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Sessions;

/// <summary>
/// Represents a successfully completed Dexcom OAuth authorization session.
/// </summary>
public sealed record DexcomOAuthAuthorizationSessionResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomOAuthAuthorizationSessionResult"/> class.
    /// </summary>
    /// <param name="authorizationUri">The Dexcom authorization URI opened in the browser.</param>
    /// <param name="state">The generated OAuth state.</param>
    /// <param name="callbackUri">The callback URI received by the local listener.</param>
    /// <param name="tokenSet">The Dexcom OAuth token set.</param>
    public DexcomOAuthAuthorizationSessionResult(
        Uri authorizationUri,
        string state,
        Uri callbackUri,
        DexcomOAuthTokenSet tokenSet)
    {
        ArgumentNullException.ThrowIfNull(authorizationUri);
        ArgumentNullException.ThrowIfNull(callbackUri);
        ArgumentNullException.ThrowIfNull(tokenSet);

        if (!authorizationUri.IsAbsoluteUri)
        {
            throw new ArgumentException("Authorization URI must be absolute.", nameof(authorizationUri));
        }

        if (string.IsNullOrWhiteSpace(state))
        {
            throw new ArgumentException("OAuth state must be specified.", nameof(state));
        }

        if (!callbackUri.IsAbsoluteUri)
        {
            throw new ArgumentException("Callback URI must be absolute.", nameof(callbackUri));
        }

        AuthorizationUri = authorizationUri;
        State = state.Trim();
        CallbackUri = callbackUri;
        TokenSet = tokenSet;
    }

    /// <summary>
    /// Gets the Dexcom authorization URI opened in the browser.
    /// </summary>
    public Uri AuthorizationUri { get; }

    /// <summary>
    /// Gets the generated OAuth state.
    /// </summary>
    public string State { get; }

    /// <summary>
    /// Gets the callback URI received by the local listener.
    /// </summary>
    public Uri CallbackUri { get; }

    /// <summary>
    /// Gets the Dexcom OAuth token set.
    /// </summary>
    public DexcomOAuthTokenSet TokenSet { get; }
}