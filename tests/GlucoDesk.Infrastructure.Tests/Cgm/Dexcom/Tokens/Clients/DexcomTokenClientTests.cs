using System.Net;
using System.Text;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Clients;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Requests;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Tokens.Clients;

public sealed class DexcomTokenClientTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task ExchangeAuthorizationCodeAsync_ShouldPostExpectedFormAndReturnTokenSet()
    {
        var handler = new CapturingHttpMessageHandler(CreateSuccessResponse());
        var client = CreateClient(handler);

        var result = await client.ExchangeAuthorizationCodeAsync(
            new DexcomAuthorizationCodeTokenRequest("authorization-code", "client-secret"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value.AccessToken);
        Assert.Equal("refresh-token", result.Value.RefreshToken);
        Assert.Equal("Bearer", result.Value.TokenType);
        Assert.Equal(FixedNow, result.Value.IssuedAtUtc);
        Assert.Equal(FixedNow.AddSeconds(7200), result.Value.AccessTokenExpiresAtUtc);

        Assert.Equal(HttpMethod.Post, handler.LastRequest?.Method);
        Assert.Equal(new Uri("https://sandbox-api.dexcom.com/v3/oauth2/token"), handler.LastRequest?.RequestUri);

        var form = Uri.UnescapeDataString(handler.LastRequestContent ?? string.Empty);

        Assert.Contains("client_id=client-id", form, StringComparison.Ordinal);
        Assert.Contains("client_secret=client-secret", form, StringComparison.Ordinal);
        Assert.Contains("code=authorization-code", form, StringComparison.Ordinal);
        Assert.Contains("grant_type=authorization_code", form, StringComparison.Ordinal);
        Assert.Contains("redirect_uri=http://127.0.0.1:51234/callback", form, StringComparison.Ordinal);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_ShouldPostExpectedFormAndReturnTokenSet()
    {
        var handler = new CapturingHttpMessageHandler(CreateSuccessResponse());
        var client = CreateClient(handler);

        var result = await client.RefreshAccessTokenAsync(
            new DexcomRefreshTokenRequest("old-refresh-token", "client-secret"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("access-token", result.Value.AccessToken);
        Assert.Equal("refresh-token", result.Value.RefreshToken);

        var form = Uri.UnescapeDataString(handler.LastRequestContent ?? string.Empty);

        Assert.Contains("client_id=client-id", form, StringComparison.Ordinal);
        Assert.Contains("client_secret=client-secret", form, StringComparison.Ordinal);
        Assert.Contains("refresh_token=old-refresh-token", form, StringComparison.Ordinal);
        Assert.Contains("grant_type=refresh_token", form, StringComparison.Ordinal);
    }

    [Fact]
    public async Task ExchangeAuthorizationCodeAsync_ShouldReturnFailure_WhenDexcomReturnsErrorStatus()
    {
        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.BadRequest));

        var client = CreateClient(handler);

        var result = await client.ExchangeAuthorizationCodeAsync(
            new DexcomAuthorizationCodeTokenRequest("authorization-code", "client-secret"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.TokenExchangeFailed", result.Error.Code);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_ShouldReturnFailure_WhenDexcomReturnsInvalidJson()
    {
        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ invalid json", Encoding.UTF8, "application/json")
            });

        var client = CreateClient(handler);

        var result = await client.RefreshAccessTokenAsync(
            new DexcomRefreshTokenRequest("refresh-token", "client-secret"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.TokenInvalidResponse", result.Error.Code);
    }

    [Fact]
    public async Task RefreshAccessTokenAsync_ShouldReturnFailure_WhenRequiredFieldsAreMissing()
    {
        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    {
                      "access_token": "",
                      "expires_in": 7200,
                      "token_type": "Bearer",
                      "refresh_token": "refresh-token"
                    }
                    """,
                    Encoding.UTF8,
                    "application/json")
            });

        var client = CreateClient(handler);

        var result = await client.RefreshAccessTokenAsync(
            new DexcomRefreshTokenRequest("refresh-token", "client-secret"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.TokenInvalidResponse", result.Error.Code);
    }

    #region Helpers

    /// <summary>
    /// Creates a Dexcom token client for tests.
    /// </summary>
    /// <param name="handler">The HTTP message handler.</param>
    /// <returns>The Dexcom token client.</returns>
    private static DexcomTokenClient CreateClient(HttpMessageHandler handler)
    {
        return new DexcomTokenClient(
            new HttpClient(handler),
            CreateOptions(),
            new DexcomApiEndpointProvider(),
            new TestTimeProvider(FixedNow));
    }

    /// <summary>
    /// Creates Dexcom API options for tests.
    /// </summary>
    /// <returns>The Dexcom API options.</returns>
    private static DexcomApiOptions CreateOptions()
    {
        return new DexcomApiOptions(
            DexcomApiEnvironment.Sandbox,
            "client-id",
            new Uri("http://127.0.0.1:51234/callback"));
    }

    /// <summary>
    /// Creates a successful token response.
    /// </summary>
    /// <returns>The HTTP response message.</returns>
    private static HttpResponseMessage CreateSuccessResponse()
    {
        return new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {
                  "access_token": "access-token",
                  "expires_in": 7200,
                  "token_type": "Bearer",
                  "refresh_token": "refresh-token",
                  "refresh_expires_in": 0
                }
                """,
                Encoding.UTF8,
                "application/json")
        };
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="CapturingHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="response">The HTTP response to return.</param>
        public CapturingHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        /// <summary>
        /// Gets the last HTTP request.
        /// </summary>
        public HttpRequestMessage? LastRequest { get; private set; }

        /// <summary>
        /// Gets the last HTTP request content.
        /// </summary>
        public string? LastRequestContent { get; private set; }

        /// <inheritdoc />
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;
            LastRequestContent = request.Content is null
                ? null
                : await request.Content.ReadAsStringAsync(cancellationToken);

            return _response;
        }
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="TestTimeProvider"/> class.
        /// </summary>
        /// <param name="utcNow">The fixed UTC timestamp.</param>
        public TestTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        /// <inheritdoc />
        public override DateTimeOffset GetUtcNow()
        {
            return _utcNow;
        }
    }

    #endregion
}