namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization;

/// <summary>
/// Represents a Dexcom OAuth authorization URL build request.
/// </summary>
public sealed record DexcomAuthorizationRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomAuthorizationRequest"/> class.
    /// </summary>
    /// <param name="clientId">The Dexcom application client id.</param>
    /// <param name="redirectUri">The OAuth redirect URI.</param>
    /// <param name="scopes">The optional OAuth scopes.</param>
    /// <param name="state">The optional OAuth state value.</param>
    public DexcomAuthorizationRequest(
        string clientId,
        Uri redirectUri,
        IReadOnlyCollection<string>? scopes = null,
        string? state = null)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException("Dexcom client id must be specified.", nameof(clientId));
        }

        ArgumentNullException.ThrowIfNull(redirectUri);

        if (!redirectUri.IsAbsoluteUri)
        {
            throw new ArgumentException("Dexcom redirect URI must be absolute.", nameof(redirectUri));
        }

        ClientId = clientId.Trim();
        RedirectUri = redirectUri;
        Scopes = NormalizeScopes(scopes);
        State = string.IsNullOrWhiteSpace(state) ? null : state.Trim();
    }

    /// <summary>
    /// Gets the Dexcom application client id.
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    /// Gets the OAuth redirect URI.
    /// </summary>
    public Uri RedirectUri { get; }

    /// <summary>
    /// Gets the optional OAuth scopes.
    /// </summary>
    public IReadOnlyCollection<string> Scopes { get; }

    /// <summary>
    /// Gets the optional OAuth state value.
    /// </summary>
    public string? State { get; }

    #region Helpers

    /// <summary>
    /// Normalizes OAuth scopes by trimming values and removing empty entries.
    /// </summary>
    /// <param name="scopes">The optional scopes.</param>
    /// <returns>The normalized scopes.</returns>
    private static IReadOnlyCollection<string> NormalizeScopes(IReadOnlyCollection<string>? scopes)
    {
        if (scopes is null || scopes.Count == 0)
        {
            return [];
        }

        return scopes
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Select(scope => scope.Trim())
            .Distinct(StringComparer.Ordinal)
            .ToArray();
    }

    #endregion
}