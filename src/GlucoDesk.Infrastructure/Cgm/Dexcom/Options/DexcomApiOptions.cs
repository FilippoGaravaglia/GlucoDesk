using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Options;

/// <summary>
/// Represents Dexcom Official API configuration options.
/// </summary>
public sealed record DexcomApiOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomApiOptions"/> class.
    /// </summary>
    /// <param name="environment">The Dexcom API environment.</param>
    /// <param name="clientId">The Dexcom application client id.</param>
    /// <param name="redirectUri">The OAuth redirect URI configured in the Dexcom developer application.</param>
    /// <param name="scopes">The optional OAuth scopes.</param>
    /// <exception cref="ArgumentException">Thrown when client id is invalid.</exception>
    /// <exception cref="ArgumentNullException">Thrown when redirect URI is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the environment is not supported.</exception>
    public DexcomApiOptions(
        DexcomApiEnvironment environment,
        string clientId,
        Uri redirectUri,
        IReadOnlyCollection<string>? scopes = null)
    {
        ValidateEnvironment(environment);

        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException("Dexcom client id must be specified.", nameof(clientId));
        }

        ArgumentNullException.ThrowIfNull(redirectUri);

        if (!redirectUri.IsAbsoluteUri)
        {
            throw new ArgumentException("Dexcom redirect URI must be absolute.", nameof(redirectUri));
        }

        Environment = environment;
        ClientId = clientId.Trim();
        RedirectUri = redirectUri;
        Scopes = NormalizeScopes(scopes);
    }

    /// <summary>
    /// Gets the Dexcom API environment.
    /// </summary>
    public DexcomApiEnvironment Environment { get; }

    /// <summary>
    /// Gets the Dexcom application client id.
    /// </summary>
    public string ClientId { get; }

    /// <summary>
    /// Gets the OAuth redirect URI configured in the Dexcom developer application.
    /// </summary>
    public Uri RedirectUri { get; }

    /// <summary>
    /// Gets the optional OAuth scopes.
    /// </summary>
    public IReadOnlyCollection<string> Scopes { get; }

    #region Helpers

    /// <summary>
    /// Validates that the Dexcom API environment is supported.
    /// </summary>
    /// <param name="environment">The Dexcom API environment.</param>
    private static void ValidateEnvironment(DexcomApiEnvironment environment)
    {
        if (!Enum.IsDefined(environment))
        {
            throw new ArgumentOutOfRangeException(
                nameof(environment),
                environment,
                "Dexcom API environment is not supported.");
        }
    }

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