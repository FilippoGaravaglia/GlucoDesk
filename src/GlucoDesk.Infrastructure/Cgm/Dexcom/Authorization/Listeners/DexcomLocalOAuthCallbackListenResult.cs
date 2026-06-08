using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;

/// <summary>
/// Represents a successfully received Dexcom local OAuth callback.
/// </summary>
public sealed record DexcomLocalOAuthCallbackListenResult
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomLocalOAuthCallbackListenResult"/> class.
    /// </summary>
    /// <param name="callbackUri">The received callback URI.</param>
    /// <param name="callbackResult">The parsed callback result.</param>
    public DexcomLocalOAuthCallbackListenResult(
        Uri callbackUri,
        DexcomOAuthCallbackResult callbackResult)
    {
        ArgumentNullException.ThrowIfNull(callbackUri);
        ArgumentNullException.ThrowIfNull(callbackResult);

        CallbackUri = callbackUri;
        CallbackResult = callbackResult;
    }

    /// <summary>
    /// Gets the received callback URI.
    /// </summary>
    public Uri CallbackUri { get; }

    /// <summary>
    /// Gets the parsed callback result.
    /// </summary>
    public DexcomOAuthCallbackResult CallbackResult { get; }

    /// <summary>
    /// Gets the authorization code returned by Dexcom.
    /// </summary>
    public string AuthorizationCode => CallbackResult.AuthorizationCode;

    /// <summary>
    /// Gets the OAuth state returned by Dexcom.
    /// </summary>
    public string State => CallbackResult.State;
}