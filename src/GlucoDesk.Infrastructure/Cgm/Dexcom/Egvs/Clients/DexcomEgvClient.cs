using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Dtos;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Requests;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Clients;

/// <summary>
/// Provides Dexcom EGV API operations.
/// </summary>
public sealed class DexcomEgvClient : IDexcomEgvClient
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly DexcomApiOptions _options;
    private readonly IDexcomApiEndpointProvider _endpointProvider;
    private readonly IDexcomOAuthTokenService _tokenService;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomEgvClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="options">The Dexcom API options.</param>
    /// <param name="endpointProvider">The Dexcom API endpoint provider.</param>
    /// <param name="tokenService">The Dexcom OAuth token service.</param>
    public DexcomEgvClient(
        HttpClient httpClient,
        DexcomApiOptions options,
        IDexcomApiEndpointProvider endpointProvider,
        IDexcomOAuthTokenService tokenService)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(endpointProvider);
        ArgumentNullException.ThrowIfNull(tokenService);

        _httpClient = httpClient;
        _options = options;
        _endpointProvider = endpointProvider;
        _tokenService = tokenService;
    }

    /// <inheritdoc />
    public async Task<Result<DexcomEgvResponseDto>> GetEgvsAsync(
        DexcomEgvRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var accessTokenResult = await _tokenService
            .GetValidAccessTokenAsync(
                new DexcomOAuthTokenRefreshRequest(
                    request.ClientSecret,
                    request.ForceTokenRefresh),
                cancellationToken)
            .ConfigureAwait(false);

        if (accessTokenResult.IsFailure)
        {
            return Result<DexcomEgvResponseDto>.Failure(accessTokenResult.Error);
        }

        using var httpRequest = BuildHttpRequest(
            request,
            accessTokenResult.Value.TokenType,
            accessTokenResult.Value.AccessToken);

        try
        {
            using var response = await _httpClient
                .SendAsync(httpRequest, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return Result<DexcomEgvResponseDto>.Failure(BuildHttpFailure(response));
            }

            var responsePayload = await response.Content
                .ReadFromJsonAsync<DexcomEgvResponseDto>(JsonOptions, cancellationToken)
                .ConfigureAwait(false);

            if (responsePayload is null || responsePayload.Records is null)
            {
                return Result<DexcomEgvResponseDto>.Failure(BuildInvalidResponseFailure());
            }

            return Result<DexcomEgvResponseDto>.Success(responsePayload);
        }
        catch (JsonException)
        {
            return Result<DexcomEgvResponseDto>.Failure(BuildInvalidResponseFailure());
        }
        catch (NotSupportedException)
        {
            return Result<DexcomEgvResponseDto>.Failure(BuildInvalidResponseFailure());
        }
        catch (HttpRequestException)
        {
            return Result<DexcomEgvResponseDto>.Failure(new Error(
                "Dexcom.EgvNetworkError",
                "Unable to complete the Dexcom EGV request due to a network error."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<DexcomEgvResponseDto>.Failure(new Error(
                "Dexcom.EgvRequestTimeout",
                "The Dexcom EGV request timed out."));
        }
    }

    #region Helpers

    /// <summary>
    /// Builds the Dexcom EGV HTTP request.
    /// </summary>
    /// <param name="request">The EGV request.</param>
    /// <param name="tokenType">The OAuth token type.</param>
    /// <param name="accessToken">The OAuth access token.</param>
    /// <returns>The HTTP request message.</returns>
    private HttpRequestMessage BuildHttpRequest(
        DexcomEgvRequest request,
        string tokenType,
        string accessToken)
    {
        var requestUri = BuildRequestUri(request);

        var httpRequest = new HttpRequestMessage(HttpMethod.Get, requestUri);
        httpRequest.Headers.Authorization = new AuthenticationHeaderValue(tokenType, accessToken);
        httpRequest.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        return httpRequest;
    }

    /// <summary>
    /// Builds the Dexcom EGV request URI.
    /// </summary>
    /// <param name="request">The EGV request.</param>
    /// <returns>The EGV request URI.</returns>
    private Uri BuildRequestUri(DexcomEgvRequest request)
    {
        var endpoints = _endpointProvider.GetEndpoints(_options.Environment);
        var egvsUri = new Uri(endpoints.ApiBaseUri, endpoints.EgvsPath);

        var startDate = FormatDateTimeOffset(request.StartDateUtc);
        var endDate = FormatDateTimeOffset(request.EndDateUtc);

        var query = string.Create(
            CultureInfo.InvariantCulture,
            $"startDate={Uri.EscapeDataString(startDate)}&endDate={Uri.EscapeDataString(endDate)}");

        var builder = new UriBuilder(egvsUri)
        {
            Query = query
        };

        return builder.Uri;
    }

    /// <summary>
    /// Formats a UTC timestamp for Dexcom query string usage.
    /// </summary>
    /// <param name="value">The UTC timestamp.</param>
    /// <returns>The formatted timestamp.</returns>
    private static string FormatDateTimeOffset(DateTimeOffset value)
    {
        return value
            .ToUniversalTime()
            .UtcDateTime
            .ToString("O", CultureInfo.InvariantCulture);
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
                "Dexcom.EgvUnauthorized",
                "Dexcom rejected the current authorization."),
    
            HttpStatusCode.Forbidden => new Error(
                "Dexcom.EgvForbidden",
                "Dexcom denied access to EGV data."),
    
            HttpStatusCode.TooManyRequests => new Error(
                "Dexcom.EgvRateLimited",
                "Dexcom rate limit was reached."),
    
            _ when IsServerError(response.StatusCode) => new Error(
                "Dexcom.EgvServerUnavailable",
                $"Dexcom EGV service returned HTTP status code {(int)response.StatusCode}."),
    
            _ => new Error(
                "Dexcom.EgvRequestFailed",
                $"Dexcom EGV request failed with HTTP status code {(int)response.StatusCode}.")
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
            "Dexcom.EgvInvalidResponse",
            "Dexcom EGV response payload is invalid.");
    }

    #endregion
}