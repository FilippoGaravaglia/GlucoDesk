using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;

/// <summary>
/// Provides endpoint resolution for Dexcom Official API environments.
/// </summary>
public sealed class DexcomApiEndpointProvider : IDexcomApiEndpointProvider
{
    /// <inheritdoc />
    public DexcomApiEndpoints GetEndpoints(DexcomApiEnvironment environment)
    {
        return environment switch
        {
            DexcomApiEnvironment.Sandbox => CreateEndpoints(
                environment,
                "https://sandbox-api.dexcom.com"),

            DexcomApiEnvironment.ProductionUs => CreateEndpoints(
                environment,
                "https://api.dexcom.com"),

            DexcomApiEnvironment.ProductionEu => CreateEndpoints(
                environment,
                "https://api.dexcom.eu"),

            DexcomApiEnvironment.ProductionJapan => CreateEndpoints(
                environment,
                "https://api.dexcom.jp"),

            _ => throw new ArgumentOutOfRangeException(
                nameof(environment),
                environment,
                "Dexcom API environment is not supported.")
        };
    }

    #region Helpers

    /// <summary>
    /// Creates Dexcom API endpoints from an API base URL.
    /// </summary>
    /// <param name="environment">The Dexcom API environment.</param>
    /// <param name="apiBaseUrl">The API base URL.</param>
    /// <returns>The Dexcom API endpoints.</returns>
    private static DexcomApiEndpoints CreateEndpoints(
        DexcomApiEnvironment environment,
        string apiBaseUrl)
    {
        var apiBaseUri = new Uri(apiBaseUrl, UriKind.Absolute);

        return new DexcomApiEndpoints(
            environment,
            apiBaseUri,
            new Uri(apiBaseUri, "/v3/oauth2/login"),
            new Uri(apiBaseUri, "/v3/oauth2/token"));
    }

    #endregion
}