using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Authentication;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Endpoints;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Readings;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Clients;

/// <summary>
/// Provides Dexcom Share HTTP operations.
/// </summary>
public sealed class DexcomShareClient : IDexcomShareClient
{
    private const int MinimumLookbackMinutes = 1;
    private const int MaximumLookbackMinutes = 1440;
    private const int MinimumReadingCount = 1;
    private const int MaximumReadingCount = 144;
    private const string UserAgent = "GlucoDesk/0.1";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly DexcomShareOptions _options;
    private readonly DexcomShareEndpointProvider _endpointProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomShareClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="options">The Dexcom Share options.</param>
    /// <param name="endpointProvider">The endpoint provider.</param>
    public DexcomShareClient(
        HttpClient httpClient,
        DexcomShareOptions options,
        DexcomShareEndpointProvider endpointProvider)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(endpointProvider);

        _httpClient = httpClient;
        _options = options;
        _endpointProvider = endpointProvider;
    }

    /// <inheritdoc />
    public async Task<Result<string>> AuthenticateAsync(CancellationToken cancellationToken)
    {
        if (!_options.IsConfigured)
        {
            return Result<string>.Failure(
                new Error(
                    "DexcomShare.NotConfigured",
                    "Dexcom Share is not configured. Set username, password and region through environment variables."));
        }

        var accountIdResult = await AuthenticatePublisherAccountAsync(cancellationToken)
            .ConfigureAwait(false);

        if (accountIdResult.IsFailure)
        {
            return Result<string>.Failure(accountIdResult.Error);
        }

        return await LoginPublisherAccountByIdAsync(accountIdResult.Value, cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>> GetLatestGlucoseValuesAsync(
        string sessionId,
        int minutes,
        int maxCount,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(sessionId))
        {
            return Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>.Failure(
                new Error(
                    "DexcomShare.SessionMissing",
                    "Dexcom Share session identifier is missing."));
        }

        var normalizedMinutes = Math.Clamp(
            minutes,
            MinimumLookbackMinutes,
            MaximumLookbackMinutes);

        var normalizedMaxCount = Math.Clamp(
            maxCount,
            MinimumReadingCount,
            MaximumReadingCount);

        var baseUri = _endpointProvider.GetBaseUri(_options.Region);
        var requestUri = new Uri(
            baseUri,
            $"/ShareWebServices/Services/Publisher/ReadPublisherLatestGlucoseValues?sessionID={Uri.EscapeDataString(sessionId)}&minutes={normalizedMinutes}&maxCount={normalizedMaxCount}");

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        ApplyDefaultHeaders(request);

        try
        {
            using var response = await _httpClient
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>.Failure(
                    new Error(
                        "DexcomShare.SessionRejected",
                        "Dexcom Share rejected the current session. Authentication may have expired."));
            }

            if (!response.IsSuccessStatusCode)
            {
                return Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>.Failure(
                    new Error(
                        "DexcomShare.ReadingsHttpFailed",
                        $"Dexcom Share readings request failed with HTTP {(int)response.StatusCode}."));
            }

            var values = await response.Content
                .ReadFromJsonAsync<IReadOnlyCollection<DexcomShareGlucoseValueDto>>(SerializerOptions, cancellationToken)
                .ConfigureAwait(false);

            return Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>.Success(values ?? []);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>.Failure(
                new Error(
                    "DexcomShare.ReadingsNetworkError",
                    exception.Message));
        }
    }

    #region Helpers

    /// <summary>
    /// Authenticates the configured Dexcom Share publisher account and returns its account identifier.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Dexcom Share account identifier.</returns>
    private async Task<Result<string>> AuthenticatePublisherAccountAsync(CancellationToken cancellationToken)
    {
        var baseUri = _endpointProvider.GetBaseUri(_options.Region);
        var requestUri = new Uri(
            baseUri,
            "/ShareWebServices/Services/General/AuthenticatePublisherAccount");

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(
                new DexcomShareAuthenticationRequest(
                    _options.Username,
                    _options.Password,
                    _options.ApplicationId))
        };

        ApplyDefaultHeaders(request);

        return await SendStringRequestAsync(
                request,
                "DexcomShare.AuthenticationFailed",
                "Dexcom Share authentication failed. Check username, password, region and Share status.",
                "DexcomShare.AuthenticationHttpFailed",
                "Dexcom Share authentication failed",
                "DexcomShare.EmptyAccountId",
                "Dexcom Share returned an empty account identifier.",
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Logs in the configured Dexcom Share publisher account and returns a session identifier.
    /// </summary>
    /// <param name="accountId">The Dexcom Share account identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Dexcom Share session identifier.</returns>
    private async Task<Result<string>> LoginPublisherAccountByIdAsync(
        string accountId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(accountId))
        {
            return Result<string>.Failure(
                new Error(
                    "DexcomShare.EmptyAccountId",
                    "Dexcom Share account identifier is missing."));
        }

        var baseUri = _endpointProvider.GetBaseUri(_options.Region);
        var requestUri = new Uri(
            baseUri,
            "/ShareWebServices/Services/General/LoginPublisherAccountById");

        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri)
        {
            Content = JsonContent.Create(
                new DexcomShareLoginByAccountIdRequest(
                    accountId,
                    _options.Password,
                    _options.ApplicationId))
        };

        ApplyDefaultHeaders(request);

        return await SendStringRequestAsync(
                request,
                "DexcomShare.LoginFailed",
                "Dexcom Share login failed after account authentication.",
                "DexcomShare.LoginHttpFailed",
                "Dexcom Share login failed",
                "DexcomShare.EmptySession",
                "Dexcom Share returned an empty session identifier.",
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Sends a Dexcom Share request that returns a JSON string payload.
    /// </summary>
    /// <param name="request">The HTTP request.</param>
    /// <param name="authorizationErrorCode">The authorization error code.</param>
    /// <param name="authorizationErrorMessage">The authorization error message.</param>
    /// <param name="httpErrorCode">The HTTP error code.</param>
    /// <param name="httpErrorMessage">The HTTP error message prefix.</param>
    /// <param name="emptyPayloadErrorCode">The empty payload error code.</param>
    /// <param name="emptyPayloadErrorMessage">The empty payload error message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The JSON string payload.</returns>
    private async Task<Result<string>> SendStringRequestAsync(
        HttpRequestMessage request,
        string authorizationErrorCode,
        string authorizationErrorMessage,
        string httpErrorCode,
        string httpErrorMessage,
        string emptyPayloadErrorCode,
        string emptyPayloadErrorMessage,
        CancellationToken cancellationToken)
    {
        try
        {
            using var response = await _httpClient
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            if (response.StatusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
            {
                return Result<string>.Failure(
                    new Error(
                        authorizationErrorCode,
                        authorizationErrorMessage));
            }

            if (!response.IsSuccessStatusCode)
            {
                return Result<string>.Failure(
                    new Error(
                        httpErrorCode,
                        $"{httpErrorMessage} with HTTP {(int)response.StatusCode}."));
            }

            var value = await response.Content
                .ReadFromJsonAsync<string>(SerializerOptions, cancellationToken)
                .ConfigureAwait(false);

            if (string.IsNullOrWhiteSpace(value))
            {
                return Result<string>.Failure(
                    new Error(
                        emptyPayloadErrorCode,
                        emptyPayloadErrorMessage));
            }

            return Result<string>.Success(value);
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return Result<string>.Failure(
                new Error(
                    "DexcomShare.NetworkError",
                    exception.Message));
        }
    }

    /// <summary>
    /// Applies default headers required by Dexcom Share requests.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    private static void ApplyDefaultHeaders(HttpRequestMessage request)
    {
        request.Headers.TryAddWithoutValidation("User-Agent", UserAgent);
    }

    #endregion
}