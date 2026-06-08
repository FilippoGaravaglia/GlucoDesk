using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;

/// <summary>
/// Represents Dexcom Official API endpoints for a specific environment.
/// </summary>
public sealed record DexcomApiEndpoints
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomApiEndpoints"/> class.
    /// </summary>
    /// <param name="environment">The Dexcom API environment.</param>
    /// <param name="apiBaseUri">The API base URI.</param>
    /// <param name="authorizationUri">The OAuth authorization URI.</param>
    /// <param name="tokenUri">The OAuth token URI.</param>
    public DexcomApiEndpoints(
        DexcomApiEnvironment environment,
        Uri apiBaseUri,
        Uri authorizationUri,
        Uri tokenUri)
    {
        ArgumentNullException.ThrowIfNull(apiBaseUri);
        ArgumentNullException.ThrowIfNull(authorizationUri);
        ArgumentNullException.ThrowIfNull(tokenUri);

        Environment = environment;
        ApiBaseUri = apiBaseUri;
        AuthorizationUri = authorizationUri;
        TokenUri = tokenUri;
    }

    /// <summary>
    /// Gets the Dexcom API environment.
    /// </summary>
    public DexcomApiEnvironment Environment { get; }

    /// <summary>
    /// Gets the API base URI.
    /// </summary>
    public Uri ApiBaseUri { get; }

    /// <summary>
    /// Gets the OAuth authorization URI.
    /// </summary>
    public Uri AuthorizationUri { get; }

    /// <summary>
    /// Gets the OAuth token URI.
    /// </summary>
    public Uri TokenUri { get; }

    /// <summary>
    /// Gets the Dexcom EGV endpoint path.
    /// </summary>
    public string EgvsPath => "/v3/users/self/egvs";
}