using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Services;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Stores;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Connection.Services;

public sealed class DexcomConnectionStatusServiceTests
{
    [Fact]
    public async Task GetConnectionStatusAsync_ShouldReturnTokenMissing_WhenTokenStoreIsEmpty()
    {
        var timeProvider = new FakeTimeProvider(DateTimeOffset.Parse("2026-01-01T10:00:00Z"));
        var tokenStore = new FakeTokenStore(
            Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenStoreEmpty", "No token set.")));

        var service = CreateService(tokenStore, timeProvider);

        var result = await service.GetConnectionStatusAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DexcomConnectionState.TokenMissing, result.Value.State);
        Assert.False(result.Value.IsConnected);
        Assert.False(result.Value.CanAttemptRefresh);
    }

    [Fact]
    public async Task GetConnectionStatusAsync_ShouldReturnConnected_WhenAccessTokenIsUsable()
    {
        var now = DateTimeOffset.Parse("2026-01-01T10:00:00Z");
        var timeProvider = new FakeTimeProvider(now);
        var tokenStore = new FakeTokenStore(
            Result<DexcomOAuthTokenSet>.Success(
                CreateTokenSet(
                    issuedAtUtc: now.AddMinutes(-10),
                    accessTokenExpiresAtUtc: now.AddMinutes(30),
                    refreshTokenExpiresAtUtc: now.AddDays(30))));

        var service = CreateService(tokenStore, timeProvider);

        var result = await service.GetConnectionStatusAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DexcomConnectionState.Connected, result.Value.State);
        Assert.True(result.Value.IsConnected);
        Assert.False(result.Value.CanAttemptRefresh);
        Assert.Equal(now.AddMinutes(30), result.Value.AccessTokenExpiresAtUtc);
        Assert.Equal(now.AddDays(30), result.Value.RefreshTokenExpiresAtUtc);
    }

    [Fact]
    public async Task GetConnectionStatusAsync_ShouldReturnAccessTokenRefreshRequired_WhenAccessTokenIsInsideSafetyWindow()
    {
        var now = DateTimeOffset.Parse("2026-01-01T10:00:00Z");
        var timeProvider = new FakeTimeProvider(now);
        var tokenStore = new FakeTokenStore(
            Result<DexcomOAuthTokenSet>.Success(
                CreateTokenSet(
                    issuedAtUtc: now.AddHours(-1),
                    accessTokenExpiresAtUtc: now.AddMinutes(4),
                    refreshTokenExpiresAtUtc: now.AddDays(30))));

        var service = CreateService(tokenStore, timeProvider);

        var result = await service.GetConnectionStatusAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DexcomConnectionState.AccessTokenRefreshRequired, result.Value.State);
        Assert.False(result.Value.IsConnected);
        Assert.True(result.Value.CanAttemptRefresh);
    }

    [Fact]
    public async Task GetConnectionStatusAsync_ShouldReturnAccessTokenRefreshRequired_WhenAccessTokenIsExpiredButRefreshTokenIsValid()
    {
        var now = DateTimeOffset.Parse("2026-01-01T10:00:00Z");
        var timeProvider = new FakeTimeProvider(now);
        var tokenStore = new FakeTokenStore(
            Result<DexcomOAuthTokenSet>.Success(
                CreateTokenSet(
                    issuedAtUtc: now.AddHours(-2),
                    accessTokenExpiresAtUtc: now.AddHours(-1),
                    refreshTokenExpiresAtUtc: now.AddDays(30))));

        var service = CreateService(tokenStore, timeProvider);

        var result = await service.GetConnectionStatusAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DexcomConnectionState.AccessTokenRefreshRequired, result.Value.State);
        Assert.False(result.Value.IsConnected);
        Assert.True(result.Value.CanAttemptRefresh);
    }

    [Fact]
    public async Task GetConnectionStatusAsync_ShouldReturnRefreshTokenExpired_WhenRefreshTokenIsExpired()
    {
        var now = DateTimeOffset.Parse("2026-01-01T10:00:00Z");
        var timeProvider = new FakeTimeProvider(now);
        var tokenStore = new FakeTokenStore(
            Result<DexcomOAuthTokenSet>.Success(
                CreateTokenSet(
                    issuedAtUtc: now.AddDays(-31),
                    accessTokenExpiresAtUtc: now.AddDays(-30),
                    refreshTokenExpiresAtUtc: now.AddMinutes(-1))));

        var service = CreateService(tokenStore, timeProvider);

        var result = await service.GetConnectionStatusAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DexcomConnectionState.RefreshTokenExpired, result.Value.State);
        Assert.False(result.Value.IsConnected);
        Assert.False(result.Value.CanAttemptRefresh);
    }

    [Fact]
    public async Task GetConnectionStatusAsync_ShouldReturnTokenStoreUnavailable_WhenTokenStoreFails()
    {
        var now = DateTimeOffset.Parse("2026-01-01T10:00:00Z");
        var timeProvider = new FakeTimeProvider(now);
        var tokenStore = new FakeTokenStore(
            Result<DexcomOAuthTokenSet>.Failure(
                new Error("Dexcom.TokenStoreReadFailed", "Unable to read token store.")));

        var service = CreateService(tokenStore, timeProvider);

        var result = await service.GetConnectionStatusAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(DexcomConnectionState.TokenStoreUnavailable, result.Value.State);
        Assert.False(result.Value.IsConnected);
        Assert.False(result.Value.CanAttemptRefresh);
    }

    #region Helpers

    /// <summary>
    /// Creates the Dexcom connection status service under test.
    /// </summary>
    /// <param name="tokenStore">The token store.</param>
    /// <param name="timeProvider">The time provider.</param>
    /// <returns>The Dexcom connection status service.</returns>
    private static DexcomConnectionStatusService CreateService(
        IDexcomOAuthTokenStore tokenStore,
        TimeProvider timeProvider)
    {
        return new DexcomConnectionStatusService(
            tokenStore,
            DexcomOAuthTokenRefreshOptions.Default,
            timeProvider);
    }

    /// <summary>
    /// Creates a Dexcom OAuth token set for tests.
    /// </summary>
    /// <param name="issuedAtUtc">The issued timestamp.</param>
    /// <param name="accessTokenExpiresAtUtc">The access token expiration timestamp.</param>
    /// <param name="refreshTokenExpiresAtUtc">The refresh token expiration timestamp.</param>
    /// <returns>The Dexcom OAuth token set.</returns>
    private static DexcomOAuthTokenSet CreateTokenSet(
        DateTimeOffset issuedAtUtc,
        DateTimeOffset accessTokenExpiresAtUtc,
        DateTimeOffset? refreshTokenExpiresAtUtc)
    {
        return new DexcomOAuthTokenSet(
            "access-token",
            "refresh-token",
            "Bearer",
            issuedAtUtc,
            accessTokenExpiresAtUtc,
            refreshTokenExpiresAtUtc);
    }

    private sealed class FakeTokenStore : IDexcomOAuthTokenStore
    {
        private readonly Result<DexcomOAuthTokenSet> _tokenResult;

        /// <summary>
        /// Initializes a new instance of the <see cref="FakeTokenStore"/> class.
        /// </summary>
        /// <param name="tokenResult">The token result.</param>
        public FakeTokenStore(Result<DexcomOAuthTokenSet> tokenResult)
        {
            _tokenResult = tokenResult;
        }

        /// <inheritdoc />
        public Task<Result> SaveTokenSetAsync(
            DexcomOAuthTokenSet tokenSet,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }

        /// <inheritdoc />
        public Task<Result<DexcomOAuthTokenSet>> GetTokenSetAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(_tokenResult);
        }

        /// <inheritdoc />
        public Task<Result<bool>> HasTokenSetAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<bool>.Success(_tokenResult.IsSuccess));
        }

        /// <inheritdoc />
        public Task<Result> ClearTokenSetAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
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