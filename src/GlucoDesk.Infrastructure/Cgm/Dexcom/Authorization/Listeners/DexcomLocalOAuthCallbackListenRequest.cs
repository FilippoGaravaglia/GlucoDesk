namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;

/// <summary>
/// Represents a request to listen for a Dexcom local OAuth callback.
/// </summary>
public sealed record DexcomLocalOAuthCallbackListenRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomLocalOAuthCallbackListenRequest"/> class.
    /// </summary>
    /// <param name="redirectUri">The local OAuth redirect URI.</param>
    /// <param name="expectedState">The expected OAuth state.</param>
    /// <param name="timeout">The optional callback timeout.</param>
    public DexcomLocalOAuthCallbackListenRequest(
        Uri redirectUri,
        string expectedState,
        TimeSpan? timeout = null)
    {
        ArgumentNullException.ThrowIfNull(redirectUri);

        if (!redirectUri.IsAbsoluteUri)
        {
            throw new ArgumentException("Dexcom OAuth redirect URI must be absolute.", nameof(redirectUri));
        }

        if (!string.Equals(redirectUri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Dexcom local OAuth redirect URI must use HTTP.", nameof(redirectUri));
        }

        if (!redirectUri.IsLoopback)
        {
            throw new ArgumentException("Dexcom local OAuth redirect URI must use a loopback host.", nameof(redirectUri));
        }

        if (string.IsNullOrWhiteSpace(redirectUri.AbsolutePath) ||
            string.Equals(redirectUri.AbsolutePath, "/", StringComparison.Ordinal))
        {
            throw new ArgumentException("Dexcom local OAuth redirect URI must include a callback path.", nameof(redirectUri));
        }

        if (string.IsNullOrWhiteSpace(expectedState))
        {
            throw new ArgumentException("Expected OAuth state must be specified.", nameof(expectedState));
        }

        if (timeout is not null && timeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(timeout),
                timeout,
                "Dexcom OAuth callback timeout must be greater than zero.");
        }

        RedirectUri = redirectUri;
        ExpectedState = expectedState.Trim();
        Timeout = timeout;
    }

    /// <summary>
    /// Gets the local OAuth redirect URI.
    /// </summary>
    public Uri RedirectUri { get; }

    /// <summary>
    /// Gets the expected OAuth state.
    /// </summary>
    public string ExpectedState { get; }

    /// <summary>
    /// Gets the optional callback timeout.
    /// </summary>
    public TimeSpan? Timeout { get; }
}