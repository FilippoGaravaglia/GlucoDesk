using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Services;
using GlucoDesk.Desktop.Bootstrap.Providers.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Sessions;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;

namespace GlucoDesk.Desktop.Tests.Bootstrap.Providers.Connection.Services;

public sealed class DexcomDesktopConnectionServiceTests
{
    private static readonly DateTimeOffset FixedNow = DateTimeOffset.Parse("2026-01-01T10:00:00Z");

    [Fact]
    public async Task ConnectAsync_ShouldStartAuthorizationSession_WhenDexcomIsConfigured()
    {
        var authorizationSessionService = new FakeAuthorizationSessionService();
        var timeProvider = new FakeTimeProvider(FixedNow);

        var service = new DexcomDesktopConnectionService(
            authorizationSessionService,
            CreateEnabledOptions(),
            timeProvider);

        var result = await service.ConnectAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(authorizationSessionService.WasCalled);
        Assert.NotNull(authorizationSessionService.LastRequest);
        Assert.Equal("client-secret", authorizationSessionService.LastRequest.ClientSecret);
        Assert.Equal(FixedNow, result.Value.ConnectedAtUtc);
        Assert.Equal(FixedNow.AddHours(1), result.Value.AccessTokenExpiresAtUtc);
        Assert.Equal(FixedNow.AddDays(30), result.Value.RefreshTokenExpiresAtUtc);
    }

    [Fact]
    public async Task ConnectAsync_ShouldReturnFailure_WhenDexcomIsDisabled()
    {
        var service = new DexcomDesktopConnectionService(
            new FakeAuthorizationSessionService(),
            DesktopDexcomProviderOptions.Disabled,
            new FakeTimeProvider(FixedNow));

        var result = await service.ConnectAsync(CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.DesktopConnectionDisabled", result.Error.Code);
    }

    [Fact]
    public async Task ConnectAsync_ShouldPropagateAuthorizationSessionFailure()
    {
        var authorizationSessionService = new FakeAuthorizationSessionService
        {
            Result = Result<DexcomOAuthAuthorizationSessionResult>.Failure(
                new Error("Dexcom.BrowserOpenFailed", "Unable to open browser."))
        };

        var service = new DexcomDesktopConnectionService(
            authorizationSessionService,
            CreateEnabledOptions(),
            new FakeTimeProvider(FixedNow));

        var result = await service.ConnectAsync(CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.BrowserOpenFailed", result.Error.Code);
    }

    #region Helpers

    /// <summary>
    /// Creates enabled Dexcom options for tests.
    /// </summary>
    /// <returns>The enabled Dexcom options.</returns>
    private static DesktopDexcomProviderOptions CreateEnabledOptions()
    {
        return new DesktopDexcomProviderOptions(
            isEnabled: true,
            environment: DexcomApiEnvironment.Sandbox,
            clientId: "client-id",
            clientSecret: "client-secret",
            redirectUri: new Uri("http://127.0.0.1:51234/callback"),
            scopes: ["egv", "offline_access"]);
    }

    /// <summary>
    /// Creates a Dexcom OAuth token set for tests.
    /// </summary>
    /// <returns>The Dexcom OAuth token set.</returns>
    private static DexcomOAuthTokenSet CreateTokenSet()
    {
        return new DexcomOAuthTokenSet(
            "access-token",
            "refresh-token",
            "Bearer",
            FixedNow.AddMinutes(-1),
            FixedNow.AddHours(1),
            FixedNow.AddDays(30));
    }

    private sealed class FakeAuthorizationSessionService : IDexcomOAuthAuthorizationSessionService
    {
        /// <summary>
        /// Gets or sets the authorization session result.
        /// </summary>
        public Result<DexcomOAuthAuthorizationSessionResult> Result { get; set; } =
            Result<DexcomOAuthAuthorizationSessionResult>.Success(
                new DexcomOAuthAuthorizationSessionResult(
                    new Uri("https://sandbox-api.dexcom.com/v3/oauth2/login"),
                    "state",
                    new Uri("http://127.0.0.1:51234/callback?code=code&state=state"),
                    CreateTokenSet()));

        /// <summary>
        /// Gets a value indicating whether the service was called.
        /// </summary>
        public bool WasCalled { get; private set; }

        /// <summary>
        /// Gets the last authorization session request.
        /// </summary>
        public DexcomOAuthAuthorizationSessionRequest? LastRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result<DexcomOAuthAuthorizationSessionResult>> StartAuthorizationSessionAsync(
            DexcomOAuthAuthorizationSessionRequest request,
            CancellationToken cancellationToken)
        {
            WasCalled = true;
            LastRequest = request;

            return Task.FromResult(Result);
        }
    }

    private sealed class FakeTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeTimeProvider"/> class.
        /// </summary>
        /// <param name="utcNow">The current UTC timestamp.</param>
        public FakeTimeProvider(DateTimeOffset utcNow)
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