using System.Globalization;
using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Dtos;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Enums;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Options;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Requests;

namespace GlucoDesk.Infrastructure.Cgm.Nightscout.Clients;

/// <summary>
/// Provides Nightscout entries API operations.
/// </summary>
public sealed class NightscoutEntriesClient : INightscoutEntriesClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly NightscoutOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="NightscoutEntriesClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="options">The Nightscout options.</param>
    public NightscoutEntriesClient(
        HttpClient httpClient,
        NightscoutOptions options)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);

        _httpClient = httpClient;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyList<NightscoutEntryDto>>> GetEntriesAsync(
        NightscoutEntriesRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var httpRequest = BuildHttpRequest(request);

        try
        {
            using var response = await _httpClient
                .SendAsync(httpRequest, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return Result<IReadOnlyList<NightscoutEntryDto>>.Failure(BuildHttpFailure(response));
            }

            var entries = await response.Content
                .ReadFromJsonAsync<IReadOnlyList<NightscoutEntryDto>>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (entries is null)
            {
                return Result<IReadOnlyList<NightscoutEntryDto>>.Failure(BuildInvalidResponseFailure());
            }

            return Result<IReadOnlyList<NightscoutEntryDto>>.Success(entries);
        }
        catch (JsonException)
        {
            return Result<IReadOnlyList<NightscoutEntryDto>>.Failure(BuildInvalidResponseFailure());
        }
        catch (NotSupportedException)
        {
            return Result<IReadOnlyList<NightscoutEntryDto>>.Failure(BuildInvalidResponseFailure());
        }
        catch (HttpRequestException)
        {
            return Result<IReadOnlyList<NightscoutEntryDto>>.Failure(new Error(
                "Nightscout.EntriesNetworkError",
                "Unable to complete the Nightscout entries request due to a network error."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<IReadOnlyList<NightscoutEntryDto>>.Failure(new Error(
                "Nightscout.EntriesRequestTimeout",
                "The Nightscout entries request timed out."));
        }
    }

    #region Helpers

    /// <summary>
    /// Builds the Nightscout HTTP request.
    /// </summary>
    /// <param name="request">The entries request.</param>
    /// <returns>The HTTP request message.</returns>
    private HttpRequestMessage BuildHttpRequest(NightscoutEntriesRequest request)
    {
        var requestUri = BuildRequestUri(request);
        var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);

        if (_options.AuthenticationMode == NightscoutAuthenticationMode.ApiSecretSha1Header)
        {
            httpRequest.Headers.TryAddWithoutValidation("api-secret", _options.ApiSecretSha1);
        }

        return httpRequest;
    }

    /// <summary>
    /// Builds the Nightscout entries request URI.
    /// </summary>
    /// <param name="request">The entries request.</param>
    /// <returns>The entries request URI.</returns>
    private Uri BuildRequestUri(NightscoutEntriesRequest request)
    {
        var entriesUri = BuildEntriesEndpointUri();
        var queryParameters = new List<KeyValuePair<string, string>>
        {
            new("find[date][$gte]", ToEpochMilliseconds(request.From).ToString(CultureInfo.InvariantCulture)),
            new("find[date][$lte]", ToEpochMilliseconds(request.To).ToString(CultureInfo.InvariantCulture)),
            new("count", request.Count.ToString(CultureInfo.InvariantCulture))
        };

        if (_options.AuthenticationMode == NightscoutAuthenticationMode.AccessTokenQueryString)
        {
            queryParameters.Add(new KeyValuePair<string, string>("token", _options.AccessToken!));
        }

        var query = string.Join(
            "&",
            queryParameters.Select(parameter =>
                $"{Uri.EscapeDataString(parameter.Key)}={Uri.EscapeDataString(parameter.Value)}"));

        return new UriBuilder(entriesUri)
        {
            Query = query
        }.Uri;
    }

    /// <summary>
    /// Builds the Nightscout entries endpoint URI from the configured base URI.
    /// </summary>
    /// <returns>The Nightscout entries endpoint URI.</returns>
    private Uri BuildEntriesEndpointUri()
    {
        var normalizedBaseUri = EnsureTrailingSlash(_options.BaseUri);

        var relativePath = normalizedBaseUri.AbsolutePath.TrimEnd('/').EndsWith(
            "/api/v1",
            StringComparison.OrdinalIgnoreCase)
            ? "entries/sgv.json"
            : "api/v1/entries/sgv.json";

        return new Uri(normalizedBaseUri, relativePath);
    }

    /// <summary>
    /// Converts a timestamp to Unix epoch milliseconds.
    /// </summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <returns>The Unix epoch milliseconds.</returns>
    private static long ToEpochMilliseconds(DateTimeOffset timestamp)
    {
        return timestamp.ToUniversalTime().ToUnixTimeMilliseconds();
    }

    /// <summary>
    /// Ensures that the URI path ends with a trailing slash.
    /// </summary>
    /// <param name="uri">The source URI.</param>
    /// <returns>The normalized URI.</returns>
    private static Uri EnsureTrailingSlash(Uri uri)
    {
        var uriText = uri.ToString();

        return uriText.EndsWith("/", StringComparison.Ordinal)
            ? uri
            : new Uri($"{uriText}/");
    }

    /// <summary>
    /// Builds an HTTP failure error.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <returns>The application error.</returns>
    private static Error BuildHttpFailure(HttpResponseMessage response)
    {
        return response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new Error(
                "Nightscout.EntriesUnauthorized",
                "Nightscout rejected the current authorization."),

            HttpStatusCode.Forbidden => new Error(
                "Nightscout.EntriesForbidden",
                "Nightscout denied access to entries data."),

            HttpStatusCode.TooManyRequests => new Error(
                "Nightscout.EntriesRateLimited",
                "Nightscout rate limit was reached."),

            _ when IsServerError(response.StatusCode) => new Error(
                "Nightscout.EntriesServerUnavailable",
                $"Nightscout entries service returned HTTP status code {(int)response.StatusCode}."),

            _ => new Error(
                "Nightscout.EntriesRequestFailed",
                $"Nightscout entries request failed with HTTP status code {(int)response.StatusCode}.")
        };
    }

    /// <summary>
    /// Checks whether the status code represents a server-side failure.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>True when the status code is a server-side failure; otherwise false.</returns>
    private static bool IsServerError(HttpStatusCode statusCode)
    {
        return (int)statusCode >= 500;
    }

    /// <summary>
    /// Builds an invalid response error.
    /// </summary>
    /// <returns>The application error.</returns>
    private static Error BuildInvalidResponseFailure()
    {
        return new Error(
            "Nightscout.EntriesInvalidResponse",
            "Nightscout entries response payload is invalid.");
    }

    #endregion
}