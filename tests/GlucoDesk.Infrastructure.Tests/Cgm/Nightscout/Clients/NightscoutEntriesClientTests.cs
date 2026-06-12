using System.Net;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Clients;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Enums;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Options;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Requests;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Nightscout.Clients;

public sealed class NightscoutEntriesClientTests
{
    private static readonly DateTimeOffset FixedFrom = DateTimeOffset.Parse("2026-06-12T08:00:00Z");
    private static readonly DateTimeOffset FixedTo = DateTimeOffset.Parse("2026-06-12T09:00:00Z");

    [Fact]
    public async Task GetEntriesAsync_ShouldReturnEntries_WhenResponseIsSuccessful()
    {
        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent(
                    """
                    [
                      {
                        "_id": "entry-1",
                        "sgv": 123,
                        "dateString": "2026-06-12T08:00:00.000Z",
                        "direction": "Flat",
                        "device": "xDrip"
                      }
                    ]
                    """)
            });

        var client = CreateClient(handler);

        var result = await client.GetEntriesAsync(
            new NightscoutEntriesRequest(FixedFrom, FixedTo, 12),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Value);
        Assert.Equal(123, result.Value[0].Sgv);
        Assert.Contains("/api/v1/entries/sgv.json", handler.RequestUri!.AbsoluteUri);
        Assert.Contains("count=12", handler.RequestUri.AbsoluteUri);
    }

    [Theory]
    [InlineData(HttpStatusCode.Unauthorized, "Nightscout.EntriesUnauthorized")]
    [InlineData(HttpStatusCode.Forbidden, "Nightscout.EntriesForbidden")]
    [InlineData(HttpStatusCode.TooManyRequests, "Nightscout.EntriesRateLimited")]
    [InlineData(HttpStatusCode.InternalServerError, "Nightscout.EntriesServerUnavailable")]
    [InlineData(HttpStatusCode.BadGateway, "Nightscout.EntriesServerUnavailable")]
    [InlineData(HttpStatusCode.BadRequest, "Nightscout.EntriesRequestFailed")]
    public async Task GetEntriesAsync_ShouldReturnSpecificFailure_WhenHttpResponseIsNotSuccessful(
        HttpStatusCode statusCode,
        string expectedErrorCode)
    {
        var handler = new CapturingHttpMessageHandler(new HttpResponseMessage(statusCode));
        var client = CreateClient(handler);

        var result = await client.GetEntriesAsync(
            new NightscoutEntriesRequest(FixedFrom, FixedTo, 12),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal(expectedErrorCode, result.Error.Code);
    }

    [Fact]
    public async Task GetEntriesAsync_ShouldSendApiSecretHeader_WhenConfigured()
    {
        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            });

        var client = CreateClient(
            handler,
            new NightscoutOptions(
                new Uri("https://example-nightscout.test"),
                authenticationMode: NightscoutAuthenticationMode.ApiSecretSha1Header,
                apiSecretSha1: "hashed-secret"));

        var result = await client.GetEntriesAsync(
            new NightscoutEntriesRequest(FixedFrom, FixedTo, 12),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(handler.RequestHeaders.TryGetValues("api-secret", out var values));
        Assert.Equal("hashed-secret", Assert.Single(values));
    }

    [Fact]
    public async Task GetEntriesAsync_ShouldAppendTokenQueryParameter_WhenConfigured()
    {
        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("[]")
            });

        var client = CreateClient(
            handler,
            new NightscoutOptions(
                new Uri("https://example-nightscout.test"),
                authenticationMode: NightscoutAuthenticationMode.AccessTokenQueryString,
                accessToken: "access-token"));

        var result = await client.GetEntriesAsync(
            new NightscoutEntriesRequest(FixedFrom, FixedTo, 12),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Contains("token=access-token", handler.RequestUri!.AbsoluteUri);
    }

    [Fact]
    public async Task GetEntriesAsync_ShouldReturnFailure_WhenResponsePayloadIsInvalid()
    {
        var handler = new CapturingHttpMessageHandler(
            new HttpResponseMessage(HttpStatusCode.OK)
            {
                Content = new StringContent("{ invalid json")
            });

        var client = CreateClient(handler);

        var result = await client.GetEntriesAsync(
            new NightscoutEntriesRequest(FixedFrom, FixedTo, 12),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Nightscout.EntriesInvalidResponse", result.Error.Code);
    }

    [Fact]
    public async Task GetEntriesAsync_ShouldReturnFailure_WhenNetworkFails()
    {
        var handler = new CapturingHttpMessageHandler(
            new HttpRequestException("Network failure."));

        var client = CreateClient(handler);

        var result = await client.GetEntriesAsync(
            new NightscoutEntriesRequest(FixedFrom, FixedTo, 12),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Nightscout.EntriesNetworkError", result.Error.Code);
    }

    private static NightscoutEntriesClient CreateClient(
        CapturingHttpMessageHandler handler,
        NightscoutOptions? options = null)
    {
        var httpClient = new HttpClient(handler)
        {
            Timeout = TimeSpan.FromSeconds(5)
        };

        return new NightscoutEntriesClient(
            httpClient,
            options ?? new NightscoutOptions(new Uri("https://example-nightscout.test")));
    }

    private sealed class CapturingHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpResponseMessage? _response;
        private readonly Exception? _exception;

        public CapturingHttpMessageHandler(HttpResponseMessage response)
        {
            _response = response;
        }

        public CapturingHttpMessageHandler(Exception exception)
        {
            _exception = exception;
        }

        public Uri? RequestUri { get; private set; }

        public HttpRequestHeadersSnapshot RequestHeaders { get; } = new();

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            RequestUri = request.RequestUri;

            foreach (var header in request.Headers)
            {
                RequestHeaders.Add(header.Key, header.Value);
            }

            if (_exception is not null)
            {
                throw _exception;
            }

            return Task.FromResult(_response!);
        }
    }

    private sealed class HttpRequestHeadersSnapshot
    {
        private readonly Dictionary<string, IReadOnlyCollection<string>> _headers = new(StringComparer.OrdinalIgnoreCase);

        public void Add(string key, IEnumerable<string> values)
        {
            _headers[key] = values.ToArray();
        }

        public bool TryGetValues(string key, out IReadOnlyCollection<string> values)
        {
            return _headers.TryGetValue(key, out values!);
        }
    }
}