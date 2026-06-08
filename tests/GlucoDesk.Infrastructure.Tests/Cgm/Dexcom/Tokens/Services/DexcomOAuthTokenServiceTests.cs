using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Clients;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Requests;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Stores;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Tokens.Services;

public sealed class DexcomOAuthTokenServiceTests
{
    private static readonly DateTimeOffset FixedNow = new(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);

    [Fact]
    public async Task GetValidAccessTokenAsync_ShouldReturnStoredToken_WhenTokenIsStillUsable()
    {
        var storedTokenSet = CreateTokenSet(
            accessToken: "stored-access-token",
            refreshToken: "stored-refresh-token",
            accessTokenExpiresAtUtc: FixedNow.AddMinutes(30));

        var tokenStore = new FakeTokenStore
        {
            TokenSetResult = Result<DexcomOAuthTokenSet>.Success(storedTokenSet)
        };

        var tokenClient = new FakeTokenClient();
        var service = CreateService(tokenStore, tokenClient);

        var result = await service.GetValidAccessTokenAsync(
            new DexcomOAuthTokenRefreshRequest("client-secret"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value.WasRefreshed);
        Assert.Same(storedTokenSet, result.Value.TokenSet);
        Assert.Null(tokenClient.LastRefreshRequest);
        Assert.Null(tokenStore.LastSavedTokenSet);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_ShouldRefreshToken_WhenTokenIsInsideSafetyWindow()
    {
        var storedTokenSet = CreateTokenSet(
            accessToken: "stored-access-token",
            refreshToken: "stored-refresh-token",
            accessTokenExpiresAtUtc: FixedNow.AddMinutes(4));

        var refreshedTokenSet = CreateTokenSet(
            accessToken: "refreshed-access-token",
            refreshToken: "refreshed-refresh-token",
            accessTokenExpiresAtUtc: FixedNow.AddHours(2));

        var tokenStore = new FakeTokenStore
        {
            TokenSetResult = Result<DexcomOAuthTokenSet>.Success(storedTokenSet)
        };

        var tokenClient = new FakeTokenClient
        {
            RefreshResult = Result<DexcomOAuthTokenSet>.Success(refreshedTokenSet)
        };

        var service = CreateService(tokenStore, tokenClient);

        var result = await service.GetValidAccessTokenAsync(
            new DexcomOAuthTokenRefreshRequest("client-secret"),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.WasRefreshed);
        Assert.Same(refreshedTokenSet, result.Value.TokenSet);

        Assert.NotNull(tokenClient.LastRefreshRequest);
        Assert.Equal("stored-refresh-token", tokenClient.LastRefreshRequest.RefreshToken);
        Assert.Equal("client-secret", tokenClient.LastRefreshRequest.ClientSecret);

        Assert.Same(refreshedTokenSet, tokenStore.LastSavedTokenSet);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_ShouldRefreshToken_WhenForceRefreshIsTrue()
    {
        var storedTokenSet = CreateTokenSet(
            accessToken: "stored-access-token",
            refreshToken: "stored-refresh-token",
            accessTokenExpiresAtUtc: FixedNow.AddHours(2));

        var refreshedTokenSet = CreateTokenSet(
            accessToken: "refreshed-access-token",
            refreshToken: "refreshed-refresh-token",
            accessTokenExpiresAtUtc: FixedNow.AddHours(3));

        var tokenStore = new FakeTokenStore
        {
            TokenSetResult = Result<DexcomOAuthTokenSet>.Success(storedTokenSet)
        };

        var tokenClient = new FakeTokenClient
        {
            RefreshResult = Result<DexcomOAuthTokenSet>.Success(refreshedTokenSet)
        };

        var service = CreateService(tokenStore, tokenClient);

        var result = await service.GetValidAccessTokenAsync(
            new DexcomOAuthTokenRefreshRequest("client-secret", forceRefresh: true),
            CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value.WasRefreshed);
        Assert.Same(refreshedTokenSet, result.Value.TokenSet);
        Assert.NotNull(tokenClient.LastRefreshRequest);
        Assert.Same(refreshedTokenSet, tokenStore.LastSavedTokenSet);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_ShouldReturnFailure_WhenTokenStoreIsEmpty()
    {
        var tokenStore = new FakeTokenStore
        {
            TokenSetResult = Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenStoreEmpty", "No token set is stored."))
        };

        var tokenClient = new FakeTokenClient();
        var service = CreateService(tokenStore, tokenClient);

        var result = await service.GetValidAccessTokenAsync(
            new DexcomOAuthTokenRefreshRequest("client-secret"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.TokenStoreEmpty", result.Error.Code);
        Assert.Null(tokenClient.LastRefreshRequest);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_ShouldReturnFailure_WhenRefreshFails()
    {
        var storedTokenSet = CreateTokenSet(
            accessToken: "stored-access-token",
            refreshToken: "stored-refresh-token",
            accessTokenExpiresAtUtc: FixedNow.AddMinutes(4));

        var tokenStore = new FakeTokenStore
        {
            TokenSetResult = Result<DexcomOAuthTokenSet>.Success(storedTokenSet)
        };

        var tokenClient = new FakeTokenClient
        {
            RefreshResult = Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenRefreshFailed", "Token refresh failed."))
        };

        var service = CreateService(tokenStore, tokenClient);

        var result = await service.GetValidAccessTokenAsync(
            new DexcomOAuthTokenRefreshRequest("client-secret"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.TokenRefreshFailed", result.Error.Code);
        Assert.Null(tokenStore.LastSavedTokenSet);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_ShouldReturnFailure_WhenSavingRefreshedTokenFails()
    {
        var storedTokenSet = CreateTokenSet(
            accessToken: "stored-access-token",
            refreshToken: "stored-refresh-token",
            accessTokenExpiresAtUtc: FixedNow.AddMinutes(4));

        var refreshedTokenSet = CreateTokenSet(
            accessToken: "refreshed-access-token",
            refreshToken: "refreshed-refresh-token",
            accessTokenExpiresAtUtc: FixedNow.AddHours(2));

        var tokenStore = new FakeTokenStore
        {
            TokenSetResult = Result<DexcomOAuthTokenSet>.Success(storedTokenSet),
            SaveResult = Result.Failure(
                new Error("Dexcom.TokenStoreSaveFailed", "Unable to save token set."))
        };

        var tokenClient = new FakeTokenClient
        {
            RefreshResult = Result<DexcomOAuthTokenSet>.Success(refreshedTokenSet)
        };

        var service = CreateService(tokenStore, tokenClient);

        var result = await service.GetValidAccessTokenAsync(
            new DexcomOAuthTokenRefreshRequest("client-secret"),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.TokenStoreSaveFailed", result.Error.Code);
        Assert.Same(refreshedTokenSet, tokenStore.LastSavedTokenSet);
    }

    [Fact]
    public async Task GetValidAccessTokenAsync_ShouldRejectNullRequest()
    {
        var service = CreateService(new FakeTokenStore(), new FakeTokenClient());

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => service.GetValidAccessTokenAsync(null!, CancellationToken.None));

        Assert.Equal("request", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates a Dexcom OAuth token service.
    /// </summary>
    /// <param name="tokenStore">The fake token store.</param>
    /// <param name="tokenClient">The fake token client.</param>
    /// <returns>The Dexcom OAuth token service.</returns>
    private static DexcomOAuthTokenService CreateService(
        FakeTokenStore tokenStore,
        FakeTokenClient tokenClient)
    {
        return new DexcomOAuthTokenService(
            tokenStore,
            tokenClient,
            DexcomOAuthTokenRefreshOptions.Default,
            new TestTimeProvider(FixedNow));
    }

    /// <summary>
    /// Creates a valid Dexcom OAuth token set.
    /// </summary>
    /// <param name="accessToken">The access token.</param>
    /// <param name="refreshToken">The refresh token.</param>
    /// <param name="accessTokenExpiresAtUtc">The access token expiration timestamp.</param>
    /// <returns>The Dexcom OAuth token set.</returns>
    private static DexcomOAuthTokenSet CreateTokenSet(
        string accessToken,
        string refreshToken,
        DateTimeOffset accessTokenExpiresAtUtc)
    {
        return new DexcomOAuthTokenSet(
            accessToken,
            refreshToken,
            "Bearer",
            FixedNow.AddHours(-1),
            accessTokenExpiresAtUtc,
            null);
    }

    private sealed class FakeTokenStore : IDexcomOAuthTokenStore
    {
        /// <summary>
        /// Gets or sets the token set result.
        /// </summary>
        public Result<DexcomOAuthTokenSet> TokenSetResult { get; set; } =
            Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenStoreEmpty", "No token set is stored."));

        /// <summary>
        /// Gets or sets the save result.
        /// </summary>
        public Result SaveResult { get; set; } = Result.Success();

        /// <summary>
        /// Gets the last saved token set.
        /// </summary>
        public DexcomOAuthTokenSet? LastSavedTokenSet { get; private set; }

        /// <inheritdoc />
        public Task<Result> SaveTokenSetAsync(
            DexcomOAuthTokenSet tokenSet,
            CancellationToken cancellationToken)
        {
            LastSavedTokenSet = tokenSet;

            return Task.FromResult(SaveResult);
        }

        /// <inheritdoc />
        public Task<Result<DexcomOAuthTokenSet>> GetTokenSetAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(TokenSetResult);
        }

        /// <inheritdoc />
        public Task<Result<bool>> HasTokenSetAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<bool>.Success(TokenSetResult.IsSuccess));
        }

        /// <inheritdoc />
        public Task<Result> ClearTokenSetAsync(CancellationToken cancellationToken)
        {
            TokenSetResult = Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenStoreEmpty", "No token set is stored."));

            LastSavedTokenSet = null;

            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FakeTokenClient : IDexcomTokenClient
    {
        /// <summary>
        /// Gets or sets the refresh result.
        /// </summary>
        public Result<DexcomOAuthTokenSet> RefreshResult { get; set; } =
            Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenRefreshFailed", "Token refresh failed."));

        /// <summary>
        /// Gets the last refresh request.
        /// </summary>
        public DexcomRefreshTokenRequest? LastRefreshRequest { get; private set; }

        /// <inheritdoc />
        public Task<Result<DexcomOAuthTokenSet>> ExchangeAuthorizationCodeAsync(
            DexcomAuthorizationCodeTokenRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenExchangeNotSupported", "Token exchange is not supported by this fake.")));
        }

        /// <inheritdoc />
        public Task<Result<DexcomOAuthTokenSet>> RefreshAccessTokenAsync(
            DexcomRefreshTokenRequest request,
            CancellationToken cancellationToken)
        {
            LastRefreshRequest = request;

            return Task.FromResult(RefreshResult);
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