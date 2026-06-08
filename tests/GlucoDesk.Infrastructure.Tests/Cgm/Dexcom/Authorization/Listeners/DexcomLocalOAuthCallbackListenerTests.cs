using System.Net;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization.Listeners;

public sealed class DexcomLocalOAuthCallbackListenerTests
{
    [Fact]
    public async Task ListenForCallbackAsync_ShouldReturnSuccess_WhenCallbackIsReceived()
    {
        var port = GetAvailablePort();
        var redirectUri = new Uri($"http://127.0.0.1:{port}/callback");

        var listener = CreateListener();

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var listenTask = listener.ListenForCallbackAsync(
            new DexcomLocalOAuthCallbackListenRequest(
                redirectUri,
                "state-value",
                TimeSpan.FromSeconds(5)),
            cancellationTokenSource.Token);

        await Task.Delay(150, cancellationTokenSource.Token);

        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync(
            new Uri($"{redirectUri}?code=authorization-code&state=state-value"),
            cancellationTokenSource.Token);

        var result = await listenTask;

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.True(result.IsSuccess);
        Assert.Equal("authorization-code", result.Value.AuthorizationCode);
        Assert.Equal("state-value", result.Value.State);
    }

    [Fact]
    public async Task ListenForCallbackAsync_ShouldReturnFailure_WhenStateDoesNotMatch()
    {
        var port = GetAvailablePort();
        var redirectUri = new Uri($"http://127.0.0.1:{port}/callback");

        var listener = CreateListener();

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var listenTask = listener.ListenForCallbackAsync(
            new DexcomLocalOAuthCallbackListenRequest(
                redirectUri,
                "expected-state",
                TimeSpan.FromSeconds(5)),
            cancellationTokenSource.Token);

        await Task.Delay(150, cancellationTokenSource.Token);

        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync(
            new Uri($"{redirectUri}?code=authorization-code&state=returned-state"),
            cancellationTokenSource.Token);

        var result = await listenTask;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.OAuthStateMismatch", result.Error.Code);
    }

    [Fact]
    public async Task ListenForCallbackAsync_ShouldReturnFailure_WhenCallbackTimesOut()
    {
        var port = GetAvailablePort();
        var redirectUri = new Uri($"http://127.0.0.1:{port}/callback");

        var listener = CreateListener();

        var result = await listener.ListenForCallbackAsync(
            new DexcomLocalOAuthCallbackListenRequest(
                redirectUri,
                "state-value",
                TimeSpan.FromMilliseconds(50)),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.OAuthCallbackTimeout", result.Error.Code);
    }

    [Fact]
    public async Task ListenForCallbackAsync_ShouldReturnFailure_WhenCallbackPathIsUnexpected()
    {
        var port = GetAvailablePort();
        var redirectUri = new Uri($"http://127.0.0.1:{port}/callback");

        var listener = CreateListener();

        using var cancellationTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(10));

        var listenTask = listener.ListenForCallbackAsync(
            new DexcomLocalOAuthCallbackListenRequest(
                redirectUri,
                "state-value",
                TimeSpan.FromSeconds(5)),
            cancellationTokenSource.Token);

        await Task.Delay(150, cancellationTokenSource.Token);

        using var httpClient = new HttpClient();

        var response = await httpClient.GetAsync(
            new Uri($"http://127.0.0.1:{port}/unexpected?code=authorization-code&state=state-value"),
            cancellationTokenSource.Token);

        var result = await listenTask;

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.OAuthUnexpectedCallbackPath", result.Error.Code);
    }

    #region Helpers

    /// <summary>
    /// Creates a local OAuth callback listener.
    /// </summary>
    /// <returns>The local OAuth callback listener.</returns>
    private static DexcomLocalOAuthCallbackListener CreateListener()
    {
        return new DexcomLocalOAuthCallbackListener(
            new DexcomOAuthCallbackParser(),
            new DexcomLocalOAuthCallbackOptions(TimeSpan.FromSeconds(5)));
    }

    /// <summary>
    /// Gets an available local TCP port.
    /// </summary>
    /// <returns>The available port.</returns>
    private static int GetAvailablePort()
    {
        using var tcpListener = new System.Net.Sockets.TcpListener(IPAddress.Loopback, port: 0);

        tcpListener.Start();

        var port = ((IPEndPoint)tcpListener.LocalEndpoint).Port;

        tcpListener.Stop();

        return port;
    }

    #endregion
}