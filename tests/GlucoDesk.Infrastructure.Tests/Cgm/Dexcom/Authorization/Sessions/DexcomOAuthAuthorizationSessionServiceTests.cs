using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Browsers;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Sessions;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.States;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Clients;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Requests;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization.Sessions;

public sealed class DexcomOAuthAuthorizationSessionServiceTests
{
    private static readonly Uri AuthorizationUri = new("https://sandbox-api.dexcom.com/v3/oauth2/login?client_id=client-id");
    private static readonly Uri CallbackUri = new("http://127.0.0.1:51234/callback?code=authorization-code&state=state-value");
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task StartAuthorizationSessionAsync_ShouldCompleteAuthorizationFlow_WhenDependenciesSucceed()
    {
        var dependencies = CreateDependencies();

        var service = CreateService(dependencies);

        var result = await service.StartAuthorizationSessionAsync(
            new DexcomOAuthAuthorizationSessionRequest("client-secret", TimeSpan.FromSeconds(30)),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(AuthorizationUri, result.Value.AuthorizationUri);
        Assert.Equal("state-value", result.Value.State);
        Assert.Equal(CallbackUri, result.Value.CallbackUri);
        Assert.Equal("access-token", result.Value.TokenSet.AccessToken);

        Assert.Equal(DexcomApiEnvironment.Sandbox, dependencies.AuthorizationUrlBuilder.LastEnvironment);
        Assert.Equal("client-id", dependencies.AuthorizationUrlBuilder.LastRequest?.ClientId);
        Assert.Equal(new Uri("http://127.0.0.1:51234/callback"), dependencies.AuthorizationUrlBuilder.LastRequest?.RedirectUri);
        Assert.Equal("state-value", dependencies.AuthorizationUrlBuilder.LastRequest?.State);

        Assert.Equal(AuthorizationUri, dependencies.Browser.LastAuthorizationUri);
        Assert.Equal("state-value", dependencies.CallbackListener.LastRequest?.ExpectedState);
        Assert.Equal(TimeSpan.FromSeconds(30), dependencies.CallbackListener.LastRequest?.Timeout);

        Assert.Equal("authorization-code", dependencies.TokenClient.LastAuthorizationCodeRequest?.AuthorizationCode);
        Assert.Equal("client-secret", dependencies.TokenClient.LastAuthorizationCodeRequest?.ClientSecret);
    }

    [Fact]
    public async Task StartAuthorizationSessionAsync_ShouldReturnFailure_WhenBrowserOpenFails()
    {
        var dependencies = CreateDependencies();
        dependencies.Browser.Result = Result<Uri>.Failure(
            new Error("Dexcom.BrowserOpenFailed", "Browser failed."));

        var service = CreateService(dependencies);

        var result = await service.StartAuthorizationSessionAsync(
            new DexcomOAuthAuthorizationSessionRequest("client-secret"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.BrowserOpenFailed", result.Error.Code);
        Assert.Null(dependencies.CallbackListener.LastRequest);
        Assert.Null(dependencies.TokenClient.LastAuthorizationCodeRequest);
    }

    [Fact]
    public async Task StartAuthorizationSessionAsync_ShouldReturnFailure_WhenCallbackFails()
    {
        var dependencies = CreateDependencies();
        dependencies.CallbackListener.Result = Result<DexcomLocalOAuthCallbackListenResult>.Failure(
            new Error("Dexcom.OAuthStateMismatch", "State mismatch."));

        var service = CreateService(dependencies);

        var result = await service.StartAuthorizationSessionAsync(
            new DexcomOAuthAuthorizationSessionRequest("client-secret"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.OAuthStateMismatch", result.Error.Code);
        Assert.Null(dependencies.TokenClient.LastAuthorizationCodeRequest);
    }

    [Fact]
    public async Task StartAuthorizationSessionAsync_ShouldReturnFailure_WhenTokenExchangeFails()
    {
        var dependencies = CreateDependencies();
        dependencies.TokenClient.AuthorizationCodeResult = Result<DexcomOAuthTokenSet>.Failure(
            new Error("Dexcom.TokenExchangeFailed", "Token exchange failed."));

        var service = CreateService(dependencies);

        var result = await service.StartAuthorizationSessionAsync(
            new DexcomOAuthAuthorizationSessionRequest("client-secret"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.TokenExchangeFailed", result.Error.Code);
    }

    [Fact]
    public async Task StartAuthorizationSessionAsync_ShouldRejectNullRequest()
    {
        var service = CreateService(CreateDependencies());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.StartAuthorizationSessionAsync(null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates the Dexcom OAuth authorization session service.
    /// </summary>
    /// <param name="dependencies">The fake dependencies.</param>
    /// <returns>The Dexcom OAuth authorization session service.</returns>
    private static DexcomOAuthAuthorizationSessionService CreateService(
        FakeDependencies dependencies)
    {
        return new DexcomOAuthAuthorizationSessionService(
            CreateOptions(),
            dependencies.StateGenerator,
            dependencies.AuthorizationUrlBuilder,
            dependencies.Browser,
            dependencies.CallbackListener,
            dependencies.TokenClient);
    }

    /// <summary>
    /// Creates fake dependencies for the authorization session service.
    /// </summary>
    /// <returns>The fake dependencies.</returns>
    private static FakeDependencies CreateDependencies()
    {
        return new FakeDependencies(
            new FakeStateGenerator(),
            new FakeAuthorizationUrlBuilder(),
            new FakeSystemBrowser(),
            new FakeCallbackListener(),
            new FakeTokenClient());
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
    /// Creates a valid token set.
    /// </summary>
    /// <returns>The Dexcom OAuth token set.</returns>
    private static DexcomOAuthTokenSet CreateTokenSet()
    {
        return new DexcomOAuthTokenSet(
            "access-token",
            "refresh-token",
            "Bearer",
            FixedNow,
            FixedNow.AddHours(2),
            null);
    }

    private sealed record FakeDependencies(
        FakeStateGenerator StateGenerator,
        FakeAuthorizationUrlBuilder AuthorizationUrlBuilder,
        FakeSystemBrowser Browser,
        FakeCallbackListener CallbackListener,
        FakeTokenClient TokenClient);

    private sealed class FakeStateGenerator : IDexcomOAuthStateGenerator
    {
        /// <inheritdoc />
        public string GenerateState()
        {
            return "state-value";
        }
    }

    private sealed class FakeAuthorizationUrlBuilder : IDexcomAuthorizationUrlBuilder
    {
        /// <summary>
        /// Gets the last requested Dexcom environment.
        /// </summary>
        public DexcomApiEnvironment? LastEnvironment { get; private set; }

        /// <summary>
        /// Gets the last authorization request.
        /// </summary>
        public DexcomAuthorizationRequest? LastRequest { get; private set; }

        /// <inheritdoc />
        public Uri BuildAuthorizationUri(
            DexcomApiEnvironment environment,
            DexcomAuthorizationRequest request)
        {
            LastEnvironment = environment;
            LastRequest = request;

            return AuthorizationUri;
        }
    }

    private sealed class FakeSystemBrowser : IDexcomSystemBrowser
    {
        /// <summary>
        /// Gets or sets the browser opening result.
        /// </summary>
        public Result<Uri> Result { get; set; } = Result<Uri>.Success(AuthorizationUri);

        /// <summary>
        /// Gets the last authorization URI.
        /// </summary>
        public Uri? LastAuthorizationUri { get; private set; }

        /// <inheritdoc />
        public Task<Result<Uri>> OpenAsync(
            Uri authorizationUri,
            CancellationToken cancellationToken)
        {
            LastAuthorizationUri = authorizationUri;

            return Task.FromResult(Result);
        }
    }

    private sealed class FakeCallbackListener : IDexcomLocalOAuthCallbackListener
    {
        /// <summary>
        /// Gets or sets the callback listener result.
        /// </summary>
        public Result<DexcomLocalOAuthCallbackListenResult> Result { get; set; } =
            Result<DexcomLocalOAuthCallbackListenResult>.Success(
                new DexcomLocalOAuthCallbackListenResult(
                    CallbackUri,
                    new DexcomOAuthCallbackResult("authorization-code", "state-value")));

        /// <summary>
        /// Gets the last listen request.
        /// </summary>
        public DexcomLocalOAuthCallbackListenRequest? LastRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result<DexcomLocalOAuthCallbackListenResult>> ListenForCallbackAsync(
            DexcomLocalOAuthCallbackListenRequest request,
            CancellationToken cancellationToken)
        {
            LastRequest = request;

            return Task.FromResult(Result);
        }
    }

    private sealed class FakeTokenClient : IDexcomTokenClient
    {
        /// <summary>
        /// Gets or sets the authorization code exchange result.
        /// </summary>
        public Result<DexcomOAuthTokenSet> AuthorizationCodeResult { get; set; } =
            Result<DexcomOAuthTokenSet>.Success(CreateTokenSet());

        /// <summary>
        /// Gets the last authorization code token request.
        /// </summary>
        public DexcomAuthorizationCodeTokenRequest? LastAuthorizationCodeRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result<DexcomOAuthTokenSet>> ExchangeAuthorizationCodeAsync(
            DexcomAuthorizationCodeTokenRequest request,
            CancellationToken cancellationToken)
        {
            LastAuthorizationCodeRequest = request;

            return Task.FromResult(AuthorizationCodeResult);
        }

        /// <inheritdoc />
        public Task<Result<DexcomOAuthTokenSet>> RefreshAccessTokenAsync(
            DexcomRefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<DexcomOAuthTokenSet>.Success(CreateTokenSet()));
        }
    }

    #endregion
}