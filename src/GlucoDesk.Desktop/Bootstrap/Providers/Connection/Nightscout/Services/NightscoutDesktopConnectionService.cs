using System.Net;
using System.Text.Json;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Nightscout.Enums;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Nightscout.Models;
using GlucoDesk.Desktop.Bootstrap.Providers.Options;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Enums;

namespace GlucoDesk.Desktop.Bootstrap.Providers.Connection.Nightscout.Services;

/// <summary>
/// Provides Nightscout desktop connection diagnostics.
/// </summary>
public sealed class NightscoutDesktopConnectionService : INightscoutDesktopConnectionService
{
    private const string ApiSecretHeaderName = "api-secret";
    private const string TokenQueryStringName = "token";
    private const string DiagnosticEntriesPath = "api/v1/entries/sgv.json";

    private readonly DesktopNightscoutProviderOptions _options;
    private readonly HttpClient _httpClient;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="NightscoutDesktopConnectionService"/> class.
    /// </summary>
    /// <param name="options">The desktop Nightscout provider options.</param>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="timeProvider">The time provider.</param>
    public NightscoutDesktopConnectionService(
        DesktopNightscoutProviderOptions options,
        HttpClient httpClient,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _options = options;
        _httpClient = httpClient;
        _timeProvider = timeProvider;

        _httpClient.Timeout = options.RequestTimeout;
    }

    /// <inheritdoc />
    public NightscoutConnectionStatus GetConfigurationStatus()
    {
        if (!_options.IsEnabled || _options.BaseUri is null)
        {
            return CreateStatus(
                NightscoutConnectionState.NotConfigured,
                "Nightscout: not configured in this desktop runtime.");
        }

        return CreateStatus(
            NightscoutConnectionState.Configured,
            $"Nightscout: configured ({_options.AuthenticationMode}).");
    }

    /// <inheritdoc />
    public async Task<NightscoutConnectionStatus> TestConnectionAsync(CancellationToken cancellationToken)
    {
        if (!_options.IsEnabled || _options.BaseUri is null)
        {
            return CreateStatus(
                NightscoutConnectionState.NotConfigured,
                "Nightscout: not configured in this desktop runtime.");
        }

        try
        {
            using var request = CreateDiagnosticRequest();

            using var response = await _httpClient
                .SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return CreateHttpFailureStatus(response.StatusCode);
            }

            await using var stream = await response.Content
                .ReadAsStreamAsync(cancellationToken)
                .ConfigureAwait(false);

            return await CreateSuccessStatusFromResponseAsync(stream, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return CreateStatus(
                NightscoutConnectionState.RequestTimeout,
                "Nightscout: connection test timed out.");
        }
        catch (HttpRequestException exception)
        {
            return CreateStatus(
                NightscoutConnectionState.NetworkError,
                $"Nightscout: network error while testing connection ({exception.Message}).");
        }
        catch (JsonException)
        {
            return CreateStatus(
                NightscoutConnectionState.InvalidResponse,
                "Nightscout: the endpoint returned unreadable JSON.");
        }
    }

    #region Helpers

    /// <summary>
    /// Creates the Nightscout diagnostic HTTP request.
    /// </summary>
    /// <returns>The diagnostic HTTP request.</returns>
    private HttpRequestMessage CreateDiagnosticRequest()
    {
        var requestUri = BuildDiagnosticRequestUri();

        var request = new HttpRequestMessage(HttpMethod.Get, requestUri);

        if (_options.AuthenticationMode is NightscoutAuthenticationMode.ApiSecretSha1Header
            && !string.IsNullOrWhiteSpace(_options.ApiSecretSha1))
        {
            request.Headers.TryAddWithoutValidation(ApiSecretHeaderName, _options.ApiSecretSha1);
        }

        return request;
    }

    /// <summary>
    /// Builds the Nightscout diagnostic request URI.
    /// </summary>
    /// <returns>The diagnostic request URI.</returns>
    private Uri BuildDiagnosticRequestUri()
    {
        var baseUri = _options.BaseUri
            ?? throw new InvalidOperationException("Nightscout base URI is required.");

        var query = _options.AuthenticationMode is NightscoutAuthenticationMode.AccessTokenQueryString
                    && !string.IsNullOrWhiteSpace(_options.AccessToken)
            ? $"count=1&{TokenQueryStringName}={Uri.EscapeDataString(_options.AccessToken)}"
            : "count=1";

        return new Uri(baseUri, $"{DiagnosticEntriesPath}?{query}");
    }

    /// <summary>
    /// Creates a Nightscout connection status from a successful HTTP response.
    /// </summary>
    /// <param name="responseStream">The response stream.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Nightscout connection status.</returns>
    private async Task<NightscoutConnectionStatus> CreateSuccessStatusFromResponseAsync(
        Stream responseStream,
        CancellationToken cancellationToken)
    {
        using var document = await JsonDocument
            .ParseAsync(responseStream, cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (document.RootElement.ValueKind is not JsonValueKind.Array)
        {
            return CreateStatus(
                NightscoutConnectionState.InvalidResponse,
                "Nightscout: the entries endpoint returned an unexpected response shape.");
        }

        if (document.RootElement.GetArrayLength() == 0)
        {
            return CreateStatus(
                NightscoutConnectionState.EmptyResponse,
                "Nightscout: connected, but no glucose entries were returned.");
        }

        return CreateStatus(
            NightscoutConnectionState.Connected,
            "Nightscout: connected. Latest glucose entry is available.");
    }

    /// <summary>
    /// Creates a Nightscout failure status from an HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The Nightscout connection status.</returns>
    private NightscoutConnectionStatus CreateHttpFailureStatus(HttpStatusCode statusCode)
    {
        return statusCode switch
        {
            HttpStatusCode.Unauthorized => CreateStatus(
                NightscoutConnectionState.Unauthorized,
                "Nightscout: unauthorized. Check the configured secret or access token."),

            HttpStatusCode.Forbidden => CreateStatus(
                NightscoutConnectionState.Forbidden,
                "Nightscout: access denied. Check Nightscout permissions."),

            HttpStatusCode.NotFound => CreateStatus(
                NightscoutConnectionState.NotFound,
                "Nightscout: endpoint not found. Check the configured base URL."),

            _ when IsServerError(statusCode) => CreateStatus(
                NightscoutConnectionState.ServerUnavailable,
                "Nightscout: server unavailable or returned a server error."),

            _ => CreateStatus(
                NightscoutConnectionState.UnexpectedError,
                $"Nightscout: connection test failed with HTTP {(int)statusCode}.")
        };
    }

    /// <summary>
    /// Creates a Nightscout connection status.
    /// </summary>
    /// <param name="state">The connection state.</param>
    /// <param name="message">The status message.</param>
    /// <returns>The Nightscout connection status.</returns>
    private NightscoutConnectionStatus CreateStatus(
        NightscoutConnectionState state,
        string message)
    {
        return new NightscoutConnectionStatus(
            state,
            message,
            _options.BaseUri,
            _timeProvider.GetUtcNow());
    }

    /// <summary>
    /// Checks whether an HTTP status code is a server error.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>True when the status code is a server error; otherwise false.</returns>
    private static bool IsServerError(HttpStatusCode statusCode)
    {
        return (int)statusCode is >= 500 and <= 599;
    }

    #endregion
}