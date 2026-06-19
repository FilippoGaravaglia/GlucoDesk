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
/// Provides Dexcom Share HTTP operations with managed in-memory session reuse.
/// </summary>
public sealed class DexcomShareClient : IDexcomShareClient
{
    private const int MinimumLookbackMinutes = 1;
    private const int MaximumLookbackMinutes = 1440;
    private const int MinimumReadingCount = 1;
    private const int MaximumReadingCount = 288;
    private const string UserAgent = "GlucoDesk/0.1";

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly IDexcomShareOptionsProvider _optionsProvider;
    private readonly DexcomShareEndpointProvider _endpointProvider;
    private readonly TimeProvider _timeProvider;
    private readonly SemaphoreSlim _authenticationLock = new(1, 1);

    private DexcomShareSession? _cachedSession;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomShareClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="optionsProvider">The Dexcom Share options provider.</param>
    /// <param name="endpointProvider">The endpoint provider.</param>
    /// <param name="timeProvider">The time provider.</param>
    public DexcomShareClient(
        HttpClient httpClient,
        IDexcomShareOptionsProvider optionsProvider,
        DexcomShareEndpointProvider endpointProvider,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(optionsProvider);
        ArgumentNullException.ThrowIfNull(endpointProvider);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _httpClient = httpClient;
        _optionsProvider = optionsProvider;
        _endpointProvider = endpointProvider;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Result<string>> AuthenticateAsync(CancellationToken cancellationToken)
    {
        var optionsResult = await _optionsProvider
            .GetOptionsAsync(cancellationToken)
            .ConfigureAwait(false);

        if (optionsResult.IsFailure)
        {
            return Result<string>.Failure(optionsResult.Error);
        }

        var sessionResult = await GetOrAuthenticateSessionAsync(
                optionsResult.Value,
                cancellationToken)
            .ConfigureAwait(false);

        return sessionResult.IsFailure
            ? Result<string>.Failure(sessionResult.Error)
            : Result<string>.Success(sessionResult.Value.SessionId);
    }

    /// <inheritdoc />
    public async Task<Result<string>> AuthenticateAsync(
        DexcomShareOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.IsConfigured)
        {
            return Result<string>.Failure(CreateNotConfiguredError());
        }

        var sessionResult = await AuthenticateFreshSessionAsync(
                options,
                cacheSession: false,
                cancellationToken)
            .ConfigureAwait(false);

        return sessionResult.IsFailure
            ? Result<string>.Failure(sessionResult.Error)
            : Result<string>.Success(sessionResult.Value.SessionId);
    }

    /// <inheritdoc />
    public async Task<Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>> GetLatestGlucoseValuesAsync(
        DexcomShareOptions options,
        int minutes,
        int maxCount,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.IsConfigured)
        {
            return Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>.Failure(CreateNotConfiguredError());
        }

        var sessionResult = await GetOrAuthenticateSessionAsync(
                options,
                cancellationToken)
            .ConfigureAwait(false);

        if (sessionResult.IsFailure)
        {
            return Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>.Failure(sessionResult.Error);
        }

        var firstAttemptResult = await GetLatestGlucoseValuesAsync(
                sessionResult.Value.SessionId,
                minutes,
                maxCount,
                cancellationToken)
            .ConfigureAwait(false);

        if (firstAttemptResult.IsSuccess)
        {
            TouchCachedSession(sessionResult.Value);
            return firstAttemptResult;
        }

        if (!IsSessionExpiredError(firstAttemptResult.Error))
        {
            return firstAttemptResult;
        }

        InvalidateSession();

        var freshSessionResult = await AuthenticateFreshSessionAsync(
                options,
                cacheSession: true,
                cancellationToken)
            .ConfigureAwait(false);

        if (freshSessionResult.IsFailure)
        {
            return Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>.Failure(freshSessionResult.Error);
        }

        var retryResult = await GetLatestGlucoseValuesAsync(
                freshSessionResult.Value.SessionId,
                minutes,
                maxCount,
                cancellationToken)
            .ConfigureAwait(false);

        if (retryResult.IsSuccess)
        {
            TouchCachedSession(freshSessionResult.Value);
        }

        return retryResult;
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

        var optionsResult = await _optionsProvider
            .GetOptionsAsync(cancellationToken)
            .ConfigureAwait(false);

        if (optionsResult.IsFailure)
        {
            return Result<IReadOnlyCollection<DexcomShareGlucoseValueDto>>.Failure(optionsResult.Error);
        }

        var options = optionsResult.Value;
        var normalizedMinutes = Math.Clamp(minutes, MinimumLookbackMinutes, MaximumLookbackMinutes);
        var normalizedMaxCount = Math.Clamp(maxCount, MinimumReadingCount, MaximumReadingCount);
        var baseUri = _endpointProvider.GetBaseUri(options.Region);

        var requestUri = new Uri(
            baseUri,
            $"/ShareWebServices/Services/Publisher/ReadPublisherLatestGlucoseValues?sessionID={Uri.EscapeDataString(sessionId)}&minutes={normalizedMinutes}&maxCount={normalizedMaxCount}");

        var result = await SendDexcomPostAsync<IReadOnlyCollection<DexcomShareGlucoseValueDto>>(
                requestUri,
                payload: null,
                DexcomShareHttpOperation.GlucoseReadings,
                cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc />
    public void InvalidateSession()
    {
        _cachedSession = null;
    }

    #region Helpers

    /// <summary>
    /// Gets a valid cached session or authenticates a new one.
    /// </summary>
    /// <param name="options">The Dexcom Share options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Dexcom Share session.</returns>
    private async Task<Result<DexcomShareSession>> GetOrAuthenticateSessionAsync(
        DexcomShareOptions options,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);
    
        if (!options.IsConfigured)
        {
            return Result<DexcomShareSession>.Failure(CreateNotConfiguredError());
        }
    
        var accountKey = BuildAccountKey(options);
        var cachedSession = _cachedSession;
    
        if (cachedSession is not null && IsCachedSessionUsable(cachedSession, accountKey, options))
        {
            TouchCachedSession(cachedSession);
            return Result<DexcomShareSession>.Success(cachedSession);
        }
    
        await _authenticationLock
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);
    
        try
        {
            cachedSession = _cachedSession;
    
            if (cachedSession is not null && IsCachedSessionUsable(cachedSession, accountKey, options))
            {
                TouchCachedSession(cachedSession);
                return Result<DexcomShareSession>.Success(cachedSession);
            }
    
            return await AuthenticateFreshSessionAsync(
                    options,
                    cacheSession: true,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        finally
        {
            _authenticationLock.Release();
        }
    }

    /// <summary>
    /// Authenticates a new Dexcom Share session.
    /// </summary>
    /// <param name="options">The Dexcom Share options.</param>
    /// <param name="cacheSession">A value indicating whether the session should be cached.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The authenticated Dexcom Share session.</returns>
    private async Task<Result<DexcomShareSession>> AuthenticateFreshSessionAsync(
        DexcomShareOptions options,
        bool cacheSession,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        if (!options.IsConfigured)
        {
            return Result<DexcomShareSession>.Failure(CreateNotConfiguredError());
        }

        var accountIdResult = await AuthenticatePublisherAccountAsync(options, cancellationToken)
            .ConfigureAwait(false);

        if (accountIdResult.IsFailure)
        {
            return Result<DexcomShareSession>.Failure(accountIdResult.Error);
        }

        var sessionIdResult = await LoginPublisherAccountByIdAsync(
                options,
                accountIdResult.Value,
                cancellationToken)
            .ConfigureAwait(false);

        if (sessionIdResult.IsFailure)
        {
            return Result<DexcomShareSession>.Failure(sessionIdResult.Error);
        }

        var now = _timeProvider.GetUtcNow();
        var session = new DexcomShareSession(
            sessionIdResult.Value,
            BuildAccountKey(options),
            now,
            now);

        if (cacheSession)
        {
            _cachedSession = session;
        }

        return Result<DexcomShareSession>.Success(session);
    }

    /// <summary>
    /// Authenticates the Dexcom Share publisher account and returns the account identifier.
    /// </summary>
    /// <param name="options">The Dexcom Share options.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Dexcom Share account identifier.</returns>
    private async Task<Result<string>> AuthenticatePublisherAccountAsync(
        DexcomShareOptions options,
        CancellationToken cancellationToken)
    {
        var baseUri = _endpointProvider.GetBaseUri(options.Region);
        var requestUri = new Uri(
            baseUri,
            "/ShareWebServices/Services/General/AuthenticatePublisherAccount");

        var payload = new DexcomShareAuthenticationRequest(
            options.Username,
            options.Password,
            options.ApplicationId);

        var result = await SendDexcomPostAsync<string>(
                requestUri,
                payload,
                DexcomShareHttpOperation.Authentication,
                cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result;
        }

        return ValidateDexcomIdentifier(
            result.Value,
            "DexcomShare.AuthenticationRejected",
            "Dexcom Share rejected the configured account credentials.");
    }

    /// <summary>
    /// Logs into Dexcom Share using an authenticated account identifier.
    /// </summary>
    /// <param name="options">The Dexcom Share options.</param>
    /// <param name="accountId">The Dexcom Share account identifier.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Dexcom Share session identifier.</returns>
    private async Task<Result<string>> LoginPublisherAccountByIdAsync(
        DexcomShareOptions options,
        string accountId,
        CancellationToken cancellationToken)
    {
        var baseUri = _endpointProvider.GetBaseUri(options.Region);
        var requestUri = new Uri(
            baseUri,
            "/ShareWebServices/Services/General/LoginPublisherAccountById");

        var payload = new DexcomShareLoginByAccountIdRequest(
            accountId,
            options.Password,
            options.ApplicationId);

        var result = await SendDexcomPostAsync<string>(
                requestUri,
                payload,
                DexcomShareHttpOperation.Authentication,
                cancellationToken)
            .ConfigureAwait(false);

        if (result.IsFailure)
        {
            return result;
        }

        return ValidateDexcomIdentifier(
            result.Value,
            "DexcomShare.AuthenticationRejected",
            "Dexcom Share did not return a valid session identifier.");
    }

    /// <summary>
    /// Sends a Dexcom Share POST request and deserializes the response.
    /// </summary>
    /// <typeparam name="TResponse">The response type.</typeparam>
    /// <param name="requestUri">The request URI.</param>
    /// <param name="payload">The optional JSON payload.</param>
    /// <param name="operation">The Dexcom Share operation.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The deserialized response.</returns>
    private async Task<Result<TResponse>> SendDexcomPostAsync<TResponse>(
        Uri requestUri,
        object? payload,
        DexcomShareHttpOperation operation,
        CancellationToken cancellationToken)
        where TResponse : notnull
    {
        using var request = new HttpRequestMessage(HttpMethod.Post, requestUri);

        ApplyDefaultHeaders(request);

        if (payload is not null)
        {
            request.Content = JsonContent.Create(payload, options: SerializerOptions);
        }

        try
        {
            using var response = await _httpClient
                .SendAsync(request, cancellationToken)
                .ConfigureAwait(false);

            var responseContent = await response.Content
                .ReadAsStringAsync(cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return Result<TResponse>.Failure(
                    BuildHttpError(response.StatusCode, responseContent, operation));
            }

            if (string.IsNullOrWhiteSpace(responseContent))
            {
                return Result<TResponse>.Failure(
                    new Error(
                        "DexcomShare.EmptyResponse",
                        "Dexcom Share returned an empty response."));
            }

            var value = JsonSerializer.Deserialize<TResponse>(
                responseContent,
                SerializerOptions);

            if (value is null)
            {
                return Result<TResponse>.Failure(
                    new Error(
                        "DexcomShare.InvalidResponse",
                        "Dexcom Share returned a response that could not be parsed."));
            }

            return Result<TResponse>.Success(value);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (TaskCanceledException)
        {
            return Result<TResponse>.Failure(
                new Error(
                    "DexcomShare.Timeout",
                    "Dexcom Share did not respond before the request timeout."));
        }
        catch (HttpRequestException)
        {
            return Result<TResponse>.Failure(
                new Error(
                    "DexcomShare.NetworkError",
                    "Unable to reach Dexcom Share. Check your network connection and try again."));
        }
        catch (JsonException)
        {
            return Result<TResponse>.Failure(
                new Error(
                    "DexcomShare.InvalidResponse",
                    "Dexcom Share returned a response that could not be parsed."));
        }
    }

    /// <summary>
    /// Applies default Dexcom Share HTTP headers.
    /// </summary>
    /// <param name="request">The HTTP request message.</param>
    private static void ApplyDefaultHeaders(HttpRequestMessage request)
    {
        request.Headers.UserAgent.ParseAdd(UserAgent);
        request.Headers.Accept.ParseAdd("application/json");
    }

    /// <summary>
    /// Validates a Dexcom Share identifier returned by the API.
    /// </summary>
    /// <param name="identifier">The identifier.</param>
    /// <param name="errorCode">The error code used when validation fails.</param>
    /// <param name="errorMessage">The error message used when validation fails.</param>
    /// <returns>The validated identifier.</returns>
    private static Result<string> ValidateDexcomIdentifier(
        string? identifier,
        string errorCode,
        string errorMessage)
    {
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return Result<string>.Failure(new Error(errorCode, errorMessage));
        }

        var normalizedIdentifier = identifier.Trim();

        if (Guid.TryParse(normalizedIdentifier, out var guid) && guid == Guid.Empty)
        {
            return Result<string>.Failure(new Error(errorCode, errorMessage));
        }

        return Result<string>.Success(normalizedIdentifier);
    }

    /// <summary>
    /// Builds a Dexcom Share HTTP error.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="responseContent">The response content.</param>
    /// <param name="operation">The Dexcom Share operation.</param>
    /// <returns>The mapped error.</returns>
    private static Error BuildHttpError(
        HttpStatusCode statusCode,
        string responseContent,
        DexcomShareHttpOperation operation)
    {
        if (operation is DexcomShareHttpOperation.GlucoseReadings
            && IsSessionInvalidResponse(statusCode, responseContent))
        {
            return new Error(
                "DexcomShare.SessionExpired",
                "Dexcom Share session expired. GlucoDesk will try to reconnect.");
        }

        if (operation is DexcomShareHttpOperation.Authentication)
        {
            return new Error(
                "DexcomShare.AuthenticationRejected",
                "Dexcom Share rejected the configured account credentials or is temporarily unavailable.");
        }

        return new Error(
            "DexcomShare.RequestFailed",
            $"Dexcom Share request failed with HTTP {(int)statusCode}.");
    }

    /// <summary>
    /// Determines whether an HTTP response indicates an invalid Dexcom Share session.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="responseContent">The response content.</param>
    /// <returns><see langword="true"/> when the response indicates an invalid session; otherwise, <see langword="false"/>.</returns>
    private static bool IsSessionInvalidResponse(
        HttpStatusCode statusCode,
        string responseContent)
    {
        if (statusCode is HttpStatusCode.Unauthorized or HttpStatusCode.Forbidden)
        {
            return true;
        }

        if (string.IsNullOrWhiteSpace(responseContent))
        {
            return false;
        }

        return responseContent.Contains("session", StringComparison.OrdinalIgnoreCase)
            && (responseContent.Contains("invalid", StringComparison.OrdinalIgnoreCase)
                || responseContent.Contains("expired", StringComparison.OrdinalIgnoreCase)
                || responseContent.Contains("not found", StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Determines whether an error represents an expired Dexcom Share session.
    /// </summary>
    /// <param name="error">The error.</param>
    /// <returns><see langword="true"/> when the error represents an expired session; otherwise, <see langword="false"/>.</returns>
    private static bool IsSessionExpiredError(Error error)
    {
        return string.Equals(
            error.Code,
            "DexcomShare.SessionExpired",
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Determines whether a cached session can be reused.
    /// </summary>
    /// <param name="session">The cached session.</param>
    /// <param name="accountKey">The current account key.</param>
    /// <param name="options">The Dexcom Share options.</param>
    /// <returns><see langword="true"/> when the cached session can be reused; otherwise, <see langword="false"/>.</returns>
    private bool IsCachedSessionUsable(
        DexcomShareSession? session,
        string accountKey,
        DexcomShareOptions options)
    {
        if (session is null)
        {
            return false;
        }

        if (!string.Equals(session.AccountKey, accountKey, StringComparison.Ordinal))
        {
            return false;
        }

        var now = _timeProvider.GetUtcNow();
        var age = now - session.AuthenticatedAt;

        return age >= TimeSpan.Zero && age <= options.SessionCacheDuration;
    }

    /// <summary>
    /// Updates the last-used timestamp for the cached session.
    /// </summary>
    /// <param name="session">The Dexcom Share session.</param>
    private void TouchCachedSession(DexcomShareSession session)
    {
        if (_cachedSession is null)
        {
            return;
        }

        if (!string.Equals(_cachedSession.SessionId, session.SessionId, StringComparison.Ordinal))
        {
            return;
        }

        _cachedSession = _cachedSession with
        {
            LastUsedAt = _timeProvider.GetUtcNow()
        };
    }

    /// <summary>
    /// Builds the in-memory session account key without exposing the password.
    /// </summary>
    /// <param name="options">The Dexcom Share options.</param>
    /// <returns>The session account key.</returns>
    private static string BuildAccountKey(DexcomShareOptions options)
    {
        return string.Join(
            "|",
            options.Username.Trim().ToUpperInvariant(),
            options.Region.ToString(),
            options.ApplicationId.Trim());
    }

    /// <summary>
    /// Creates the not-configured Dexcom Share error.
    /// </summary>
    /// <returns>The not-configured error.</returns>
    private static Error CreateNotConfiguredError()
    {
        return new Error(
            "DexcomShare.NotConfigured",
            "Dexcom Share account is not configured. Open Account and enter your Dexcom Share credentials.");
    }

    private enum DexcomShareHttpOperation
    {
        Authentication,
        GlucoseReadings
    }

    private sealed record DexcomShareSession(
        string SessionId,
        string AccountKey,
        DateTimeOffset AuthenticatedAt,
        DateTimeOffset LastUsedAt);

    #endregion
}