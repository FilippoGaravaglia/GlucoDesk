using System.Net;
using System.Text.Json;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Dtos;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Requests;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Clients;

/// <summary>
/// Provides Dexcom OAuth token operations.
/// </summary>
public sealed class DexcomTokenClient : IDexcomTokenClient
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly DexcomApiOptions _options;
    private readonly IDexcomApiEndpointProvider _endpointProvider;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomTokenClient"/> class.
    /// </summary>
    /// <param name="httpClient">The HTTP client.</param>
    /// <param name="options">The Dexcom API options.</param>
    /// <param name="endpointProvider">The Dexcom API endpoint provider.</param>
    /// <param name="timeProvider">The time provider.</param>
    public DexcomTokenClient(
        HttpClient httpClient,
        DexcomApiOptions options,
        IDexcomApiEndpointProvider endpointProvider,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(httpClient);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(endpointProvider);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _httpClient = httpClient;
        _options = options;
        _endpointProvider = endpointProvider;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Result<DexcomOAuthTokenSet>> ExchangeAuthorizationCodeAsync(
        DexcomAuthorizationCodeTokenRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var formValues = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = request.ClientSecret,
            ["code"] = request.AuthorizationCode,
            ["grant_type"] = "authorization_code",
            ["redirect_uri"] = _options.RedirectUri.ToString()
        };

        return await SendTokenRequestAsync(
                formValues,
                failureCode: "Dexcom.TokenExchangeFailed",
                cancellationToken)
            .ConfigureAwait(false);
    }

    /// <inheritdoc />
    public async Task<Result<DexcomOAuthTokenSet>> RefreshAccessTokenAsync(
        DexcomRefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        var formValues = new Dictionary<string, string>
        {
            ["client_id"] = _options.ClientId,
            ["client_secret"] = request.ClientSecret,
            ["refresh_token"] = request.RefreshToken,
            ["grant_type"] = "refresh_token"
        };

        return await SendTokenRequestAsync(
                formValues,
                failureCode: "Dexcom.TokenRefreshFailed",
                cancellationToken)
            .ConfigureAwait(false);
    }

    #region Helpers

    /// <summary>
    /// Sends a Dexcom OAuth token request.
    /// </summary>
    /// <param name="formValues">The OAuth form values.</param>
    /// <param name="failureCode">The failure error code.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Dexcom OAuth token set.</returns>
    private async Task<Result<DexcomOAuthTokenSet>> SendTokenRequestAsync(
        IReadOnlyDictionary<string, string> formValues,
        string failureCode,
        CancellationToken cancellationToken)
    {
        try
        {
            var endpoints = _endpointProvider.GetEndpoints(_options.Environment);

            using var content = new FormUrlEncodedContent(formValues);
            using var response = await _httpClient
                .PostAsync(endpoints.TokenUri, content, cancellationToken)
                .ConfigureAwait(false);

            if (!response.IsSuccessStatusCode)
            {
                return Result<DexcomOAuthTokenSet>.Failure(
                    BuildHttpFailure(failureCode, response.StatusCode));
            }

            return await ReadTokenResponseAsync(response, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenInvalidResponse", "Dexcom returned an invalid token response."));
        }
        catch (HttpRequestException)
        {
            return Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenRequestFailed", "Unable to reach Dexcom token endpoint."));
        }
        catch (TaskCanceledException) when (!cancellationToken.IsCancellationRequested)
        {
            return Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenRequestTimeout", "Dexcom token request timed out."));
        }
    }

    /// <summary>
    /// Reads and maps a Dexcom OAuth token response.
    /// </summary>
    /// <param name="response">The HTTP response.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The Dexcom OAuth token set.</returns>
    private async Task<Result<DexcomOAuthTokenSet>> ReadTokenResponseAsync(
        HttpResponseMessage response,
        CancellationToken cancellationToken)
    {
        await using var stream = await response.Content
            .ReadAsStreamAsync(cancellationToken)
            .ConfigureAwait(false);

        var dto = await JsonSerializer
            .DeserializeAsync<DexcomTokenResponseDto>(
                stream,
                SerializerOptions,
                cancellationToken)
            .ConfigureAwait(false);

        if (dto is null)
        {
            return Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenInvalidResponse", "Dexcom returned an empty token response."));
        }

        return MapTokenResponse(dto);
    }

    /// <summary>
    /// Maps a Dexcom token response DTO to a token set.
    /// </summary>
    /// <param name="dto">The Dexcom token response DTO.</param>
    /// <returns>The Dexcom OAuth token set.</returns>
    private Result<DexcomOAuthTokenSet> MapTokenResponse(DexcomTokenResponseDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.AccessToken) ||
            string.IsNullOrWhiteSpace(dto.RefreshToken) ||
            string.IsNullOrWhiteSpace(dto.TokenType) ||
            dto.ExpiresIn <= 0)
        {
            return Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenInvalidResponse", "Dexcom token response is missing required fields."));
        }

        var issuedAtUtc = _timeProvider.GetUtcNow();

        var refreshTokenExpiresAtUtc = dto.RefreshExpiresIn > 0
            ? issuedAtUtc.AddSeconds(dto.RefreshExpiresIn)
            : (DateTimeOffset?)null;

        var tokenSet = new DexcomOAuthTokenSet(
            dto.AccessToken,
            dto.RefreshToken,
            dto.TokenType,
            issuedAtUtc,
            issuedAtUtc.AddSeconds(dto.ExpiresIn),
            refreshTokenExpiresAtUtc);

        return Result<DexcomOAuthTokenSet>.Success(tokenSet);
    }

    /// <summary>
    /// Builds an HTTP failure error.
    /// </summary>
    /// <param name="failureCode">The failure error code.</param>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The application error.</returns>
    private static Error BuildHttpFailure(
        string failureCode,
        HttpStatusCode statusCode)
    {
        return new Error(
            failureCode,
            $"Dexcom token request failed with HTTP status {(int)statusCode}.");
    }

    #endregion
}