using System.Net;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Nightscout.Enums;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Nightscout.Services;
using GlucoDesk.Desktop.Bootstrap.Providers.Options;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Enums;

namespace GlucoDesk.Desktop.Tests.Bootstrap.Providers.Connection.Nightscout.Services;

public sealed class NightscoutDesktopConnectionServiceTests
{
    [Fact]
    public void GetConfigurationStatus_ShouldReturnNotConfigured_WhenNightscoutIsDisabled()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, "[]");

        var service = new NightscoutDesktopConnectionService(
            new DesktopNightscoutProviderOptions(false, null),
            httpClient,
            TimeProvider.System);

        var status = service.GetConfigurationStatus();

        Assert.Equal(NightscoutConnectionState.NotConfigured, status.State);
        Assert.False(status.IsConfigured);
        Assert.False(status.IsConnected);
    }

    [Fact]
    public void GetConfigurationStatus_ShouldReturnConfigured_WhenNightscoutIsEnabled()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, "[]");

        var service = new NightscoutDesktopConnectionService(
            CreateEnabledOptions(),
            httpClient,
            TimeProvider.System);

        var status = service.GetConfigurationStatus();

        Assert.Equal(NightscoutConnectionState.Configured, status.State);
        Assert.True(status.IsConfigured);
        Assert.False(status.IsConnected);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldReturnConnected_WhenEntriesEndpointReturnsEntries()
    {
        using var httpClient = CreateHttpClient(
            HttpStatusCode.OK,
            """[{ "sgv": 120, "dateString": "2026-06-13T10:00:00.000Z" }]""");

        var service = new NightscoutDesktopConnectionService(
            CreateEnabledOptions(),
            httpClient,
            TimeProvider.System);

        var status = await service.TestConnectionAsync(CancellationToken.None);

        Assert.Equal(NightscoutConnectionState.Connected, status.State);
        Assert.True(status.IsConnected);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldReturnEmptyResponse_WhenEntriesEndpointReturnsEmptyArray()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, "[]");

        var service = new NightscoutDesktopConnectionService(
            CreateEnabledOptions(),
            httpClient,
            TimeProvider.System);

        var status = await service.TestConnectionAsync(CancellationToken.None);

        Assert.Equal(NightscoutConnectionState.EmptyResponse, status.State);
        Assert.False(status.IsConnected);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldReturnUnauthorized_WhenEndpointReturnsUnauthorized()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.Unauthorized, string.Empty);

        var service = new NightscoutDesktopConnectionService(
            CreateEnabledOptions(),
            httpClient,
            TimeProvider.System);

        var status = await service.TestConnectionAsync(CancellationToken.None);

        Assert.Equal(NightscoutConnectionState.Unauthorized, status.State);
        Assert.False(status.IsConnected);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldReturnForbidden_WhenEndpointReturnsForbidden()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.Forbidden, string.Empty);

        var service = new NightscoutDesktopConnectionService(
            CreateEnabledOptions(),
            httpClient,
            TimeProvider.System);

        var status = await service.TestConnectionAsync(CancellationToken.None);

        Assert.Equal(NightscoutConnectionState.Forbidden, status.State);
        Assert.False(status.IsConnected);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldReturnNotFound_WhenEndpointReturnsNotFound()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.NotFound, string.Empty);

        var service = new NightscoutDesktopConnectionService(
            CreateEnabledOptions(),
            httpClient,
            TimeProvider.System);

        var status = await service.TestConnectionAsync(CancellationToken.None);

        Assert.Equal(NightscoutConnectionState.NotFound, status.State);
        Assert.False(status.IsConnected);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldReturnServerUnavailable_WhenEndpointReturnsServerError()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.InternalServerError, string.Empty);

        var service = new NightscoutDesktopConnectionService(
            CreateEnabledOptions(),
            httpClient,
            TimeProvider.System);

        var status = await service.TestConnectionAsync(CancellationToken.None);

        Assert.Equal(NightscoutConnectionState.ServerUnavailable, status.State);
        Assert.False(status.IsConnected);
    }

    [Fact]
    public async Task TestConnectionAsync_ShouldReturnInvalidResponse_WhenEndpointReturnsObject()
    {
        using var httpClient = CreateHttpClient(HttpStatusCode.OK, """{ "status": "ok" }""");

        var service = new NightscoutDesktopConnectionService(
            CreateEnabledOptions(),
            httpClient,
            TimeProvider.System);

        var status = await service.TestConnectionAsync(CancellationToken.None);

        Assert.Equal(NightscoutConnectionState.InvalidResponse, status.State);
        Assert.False(status.IsConnected);
    }

    #region Helpers

    /// <summary>
    /// Creates enabled Nightscout desktop options for tests.
    /// </summary>
    /// <returns>The enabled Nightscout desktop options.</returns>
    private static DesktopNightscoutProviderOptions CreateEnabledOptions()
    {
        return new DesktopNightscoutProviderOptions(
            true,
            new Uri("https://example-nightscout.test"),
            "Nightscout",
            NightscoutAuthenticationMode.None);
    }

    /// <summary>
    /// Creates an HTTP client backed by a fake response handler.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <param name="content">The response content.</param>
    /// <returns>The HTTP client.</returns>
    private static HttpClient CreateHttpClient(
        HttpStatusCode statusCode,
        string content)
    {
        return new HttpClient(new FakeHttpMessageHandler(statusCode, content));
    }

    #endregion

    private sealed class FakeHttpMessageHandler : HttpMessageHandler
    {
        private readonly HttpStatusCode _statusCode;
        private readonly string _content;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeHttpMessageHandler"/> class.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <param name="content">The response content.</param>
        public FakeHttpMessageHandler(
            HttpStatusCode statusCode,
            string content)
        {
            _statusCode = statusCode;
            _content = content;
        }

        /// <inheritdoc />
        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var response = new HttpResponseMessage(_statusCode)
            {
                Content = new StringContent(_content)
            };

            return Task.FromResult(response);
        }
    }
}