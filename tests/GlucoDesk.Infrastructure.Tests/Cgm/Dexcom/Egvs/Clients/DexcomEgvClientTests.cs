using System.Net;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Clients;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Requests;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Egvs.Clients;

public sealed class DexcomEgvClientTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetEgvsAsync_ShouldSendAuthorizedRequestAndDeserializeResponse_WhenRequestSucceeds()
    {
        const string json = """
        {
          "recordType": "egv",
          "recordVersion": "3.0",
          "userId": "user-id",
          "records": [
            {
              "recordId": "record-id",
              "systemTime": "2025-01-30T23:49:55Z",
              "displayTime": "2025-01-30T15:49:55-08:00",
              "transmitterId": "transmitter-id",
              "transmitterTicks": 85273,
              "value": 101,
              "status": null,
              "trend": "flat",
              "trendRate": 0,
              "unit": "mg/dL",
              "rateUnit": "mg/dL/min",
              "displayDevice": "iOS",
              "transmitterGeneration": "g7",
              "displayApp": "G7"
            }
          ]
        }
        """;

        var tokenService = new FakeTokenService
        {
            AccessTokenResult = Result<DexcomAccessTokenResult>.Success(
                new DexcomAccessTokenResult(CreateTokenSet("access-token"), wasRefreshed: false))
        };

        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(json)
            });

        var client = CreateClient(handler, tokenService);

        var result = await client.GetEgvsAsync(
            new DexcomEgvRequest(
                "client-secret",
                FixedNow.AddHours(-2),
                FixedNow),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal("egv", result.Value.RecordType);

        var record = Assert.Single(result.Value.Records!);
        Assert.Equal("record-id", record.RecordId);
        Assert.Equal(101, record.Value);

        Assert.NotNull(handler.LastRequest);
        Assert.Equal(HttpMethod.Get, handler.LastRequest.Method);
        Assert.Equal("Bearer", handler.LastRequest.Headers.Authorization?.Scheme);
        Assert.Equal("access-token", handler.LastRequest.Headers.Authorization?.Parameter);
        Assert.Contains("/v3/users/self/egvs", handler.LastRequest.RequestUri?.AbsolutePath);
        Assert.Contains("startDate=", handler.LastRequest.RequestUri?.Query);
        Assert.Contains("endDate=", handler.LastRequest.RequestUri?.Query);

        Assert.NotNull(tokenService.LastRequest);
        Assert.Equal("client-secret", tokenService.LastRequest.ClientSecret);
        Assert.False(tokenService.LastRequest.ForceRefresh);
    }

    [Fact]
    public async Task GetEgvsAsync_ShouldPropagateForcedTokenRefresh()
    {
        var tokenService = new FakeTokenService
        {
            AccessTokenResult = Result<DexcomAccessTokenResult>.Success(
                new DexcomAccessTokenResult(CreateTokenSet("access-token"), wasRefreshed: true))
        };

        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"recordType":"egv","recordVersion":"3.0","userId":"user-id","records":[]}""")
            });

        var client = CreateClient(handler, tokenService);

        var result = await client.GetEgvsAsync(
            new DexcomEgvRequest(
                "client-secret",
                FixedNow.AddHours(-2),
                FixedNow,
                forceTokenRefresh: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.NotNull(tokenService.LastRequest);
        Assert.True(tokenService.LastRequest.ForceRefresh);
    }

    [Fact]
    public async Task GetEgvsAsync_ShouldReturnFailure_WhenTokenServiceFails()
    {
        var tokenService = new FakeTokenService
        {
            AccessTokenResult = Result<DexcomAccessTokenResult>.Failure(
                new Error("Dexcom.TokenStoreEmpty", "No token is stored."))
        };

        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK));

        var client = CreateClient(handler, tokenService);

        var result = await client.GetEgvsAsync(
            new DexcomEgvRequest(
                "client-secret",
                FixedNow.AddHours(-2),
                FixedNow),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.TokenStoreEmpty", result.Error.Code);
        Assert.Null(handler.LastRequest);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "Dexcom.EgvUnauthorized")]
    [InlineData(HttpStatusCode.Forbidden, "Dexcom.EgvForbidden")]
    [InlineData(HttpStatusCode.TooManyRequests, "Dexcom.EgvRateLimited")]
    [InlineData(HttpStatusCode.InternalServerError, "Dexcom.EgvServerUnavailable")]
    [InlineData(HttpStatusCode.BadGateway, "Dexcom.EgvServerUnavailable")]
    [InlineData(HttpStatusCode.ServiceUnavailable, "Dexcom.EgvServerUnavailable")]
    [InlineData(HttpStatusCode.BadRequest, "Dexcom.EgvRequestFailed")]
    public async Task GetEgvsAsync_ShouldReturnSpecificFailure_WhenHttpResponseIsNotSuccessful(
        HttpStatusCode statusCode,
        string expectedErrorCode)
    {
        var tokenService = new FakeTokenService
        {
            AccessTokenResult = Result<DexcomAccessTokenResult>.Success(
                new DexcomAccessTokenResult(CreateTokenSet("access-token"), wasRefreshed: false))
        };
    
        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(statusCode));
    
        var client = CreateClient(handler, tokenService);
    
        var result = await client.GetEgvsAsync(
            new DexcomEgvRequest(
                "client-secret",
                FixedNow.AddHours(-2),
                FixedNow),
            CancellationToken.None);
    
        Assert.True(result.IsFailure);
        Assert.Equal(expectedErrorCode, result.Error.Code);
    }

    [Fact]
    public async Task GetEgvsAsync_ShouldReturnFailure_WhenJsonIsInvalid()
    {
        var tokenService = new FakeTokenService
        {
            AccessTokenResult = Result<DexcomAccessTokenResult>.Success(
                new DexcomAccessTokenResult(CreateTokenSet("access-token"), wasRefreshed: false))
        };

        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ invalid-json")
            });

        var client = CreateClient(handler, tokenService);

        var result = await client.GetEgvsAsync(
            new DexcomEgvRequest(
                "client-secret",
                FixedNow.AddHours(-2),
                FixedNow),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvInvalidResponse", result.Error.Code);
    }

    [Fact]
    public async Task GetEgvsAsync_ShouldReturnFailure_WhenRecordsAreMissing()
    {
        var tokenService = new FakeTokenService
        {
            AccessTokenResult = Result<DexcomAccessTokenResult>.Success(
                new DexcomAccessTokenResult(CreateTokenSet("access-token"), wasRefreshed: false))
        };

        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("""{"recordType":"egv","recordVersion":"3.0","userId":"user-id"}""")
            });

        var client = CreateClient(handler, tokenService);

        var result = await client.GetEgvsAsync(
            new DexcomEgvRequest(
                "client-secret",
                FixedNow.AddHours(-2),
                FixedNow),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvInvalidResponse", result.Error.Code);
    }

    [Fact]
    public async Task GetEgvsAsync_ShouldRejectNullRequest()
    {
        var client = CreateClient(
            new CapturingHttpMessageHandler(new HttpResponseMessage(HttpStatusCode.OK)),
            new FakeTokenService());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => client.GetEgvsAsync(null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates a Dexcom EGV client for tests.
    /// </summary>
    /// <param name="handler">The capturing HTTP message handler.</param>
    /// <param name="tokenService">The fake token service.</param>
    /// <returns>The Dexcom EGV client.</returns>
    private static DexcomEgvClient CreateClient(
        CapturingHttpMessageHandler handler,
        FakeTokenService tokenService)
    {
        var httpClient = new HttpClient(handler)
        {
            BaseAddress = new Uri("https://sandbox-api.dexcom.com")
        };

        return new DexcomEgvClient(
            httpClient,
            CreateOptions(),
            new DexcomApiEndpointProvider(),
            tokenService);
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
            new Uri("http://127.0.0.1:51234/callback"),
            ["egv", "offline_access"]);
    }

    /// <summary>
    /// Creates a valid access token set.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <returns>The Dexcom OAuth token set.</returns>
    private static DexcomOAuthTokenSet CreateTokenSet(string accessToken)
    {
        return new DexcomOAuthTokenSet(
            accessToken,
            "refresh-token",
            "Bearer",
            FixedNow.AddHours(-1),
            FixedNow.AddHours(1),
            null);
    }

    private sealed class FakeTokenService : IDexcomOAuthTokenService
    {
        /// <summary>
        /// Gets or sets the access token result.
        /// </summary>
        public Result<DexcomAccessTokenResult> AccessTokenResult { get; set; } =
            Result<DexcomAccessTokenResult>.Failure(
                new Error("Dexcom.TokenStoreEmpty", "No token is stored."));

        /// <summary>
        /// Gets the last token refresh request.
        /// </summary>
        public DexcomOAuthTokenRefreshRequest? LastRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result<DexcomAccessTokenResult>> GetValidAccessTokenAsync(
            DexcomOAuthTokenRefreshRequest request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(AccessTokenResult);
        }
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage _response;

        /// <summary>
        /// Initializes a new instance of the <see cref="CapturingHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="response">The response to return.</param>
        public CapturingHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        /// <summary>
        /// Gets the last HTTP request.
        /// </summary>
        public HttpRequestMessage? LastRequest { get; private set; }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(_response);
        }
    }

    #endregion
}