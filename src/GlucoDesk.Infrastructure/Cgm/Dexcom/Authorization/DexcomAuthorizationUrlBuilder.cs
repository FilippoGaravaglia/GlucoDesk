using GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization;

/// <summary>
/// Builds Dexcom OAuth authorization URLs.
/// </summary>
public sealed class DexcomAuthorizationUrlBuilder : IDexcomAuthorizationUrlBuilder
{
    private readonly IDexcomApiEndpointProvider _endpointProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomAuthorizationUrlBuilder"/> class.
    /// </summary>
    /// <param name="endpointProvider">The Dexcom endpoint provider.</param>
    public DexcomAuthorizationUrlBuilder(IDexcomApiEndpointProvider endpointProvider)
    {
        ArgumentNullException.ThrowIfNull(endpointProvider);

        _endpointProvider = endpointProvider;
    }

    /// <inheritdoc />
    public Uri BuildAuthorizationUri(
        DexcomApiEnvironment environment,
        DexcomAuthorizationRequest request)
    {
        ArgumentNullException.ThrowIfNull(request);

        var endpoints = _endpointProvider.GetEndpoints(environment);

        var queryParameters = new Dictionary<string, string>
        {
            ["response_type"] = "code",
            ["client_id"] = request.ClientId,
            ["redirect_uri"] = request.RedirectUri.ToString()
        };

        if (request.Scopes.Count > 0)
        {
            queryParameters["scope"] = string.Join(" ", request.Scopes);
        }

        if (!string.IsNullOrWhiteSpace(request.State))
        {
            queryParameters["state"] = request.State;
        }

        var uriBuilder = new UriBuilder(endpoints.AuthorizationUri)
        {
            Query = BuildQueryString(queryParameters)
        };

        return uriBuilder.Uri;
    }

    #region Helpers

    /// <summary>
    /// Builds a URL query string from key/value parameters.
    /// </summary>
    /// <param name="parameters">The query parameters.</param>
    /// <returns>The URL query string.</returns>
    private static string BuildQueryString(IReadOnlyDictionary<string, string> parameters)
    {
        return string.Join(
            "&",
            parameters.Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));
    }

    #endregion
}