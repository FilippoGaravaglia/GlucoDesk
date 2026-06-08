using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Stores;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Tokens.Stores;

public sealed class InMemoryDexcomOAuthTokenStoreTests
{
    [Fact]
    public async Task SaveTokenSetAsync_ShouldStoreTokenSet()
    {
        var store = new InMemoryDexcomOAuthTokenStore();
        var tokenSet = CreateTokenSet();

        var saveResult = await store.SaveTokenSetAsync(tokenSet, CancellationToken.None);
        var getResult = await store.GetTokenSetAsync(CancellationToken.None);

        Assert.True(saveResult.IsSuccess);
        Assert.True(getResult.IsSuccess);
        Assert.Same(tokenSet, getResult.Value);
    }

    [Fact]
    public async Task SaveTokenSetAsync_ShouldRejectNullTokenSet()
    {
        var store = new InMemoryDexcomOAuthTokenStore();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => store.SaveTokenSetAsync(null!, CancellationToken.None));

        Assert.Equal("tokenSet", exception.ParamName);
    }

    [Fact]
    public async Task GetTokenSetAsync_ShouldReturnFailure_WhenNoTokenSetIsStored()
    {
        var store = new InMemoryDexcomOAuthTokenStore();

        var result = await store.GetTokenSetAsync(CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.TokenStoreEmpty", result.Error.Code);
    }

    [Fact]
    public async Task HasTokenSetAsync_ShouldReturnFalse_WhenNoTokenSetIsStored()
    {
        var store = new InMemoryDexcomOAuthTokenStore();

        var result = await store.HasTokenSetAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.False(result.Value);
    }

    [Fact]
    public async Task HasTokenSetAsync_ShouldReturnTrue_WhenTokenSetIsStored()
    {
        var store = new InMemoryDexcomOAuthTokenStore();

        await store.SaveTokenSetAsync(CreateTokenSet(), CancellationToken.None);

        var result = await store.HasTokenSetAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(result.Value);
    }

    [Fact]
    public async Task ClearTokenSetAsync_ShouldRemoveStoredTokenSet()
    {
        var store = new InMemoryDexcomOAuthTokenStore();

        await store.SaveTokenSetAsync(CreateTokenSet(), CancellationToken.None);

        var clearResult = await store.ClearTokenSetAsync(CancellationToken.None);
        var hasTokenSetResult = await store.HasTokenSetAsync(CancellationToken.None);

        Assert.True(clearResult.IsSuccess);
        Assert.True(hasTokenSetResult.IsSuccess);
        Assert.False(hasTokenSetResult.Value);
    }

    #region Helpers

    /// <summary>
    /// Creates a valid Dexcom OAuth token set for tests.
    /// </summary>
    /// <returns>The Dexcom OAuth token set.</returns>
    private static DexcomOAuthTokenSet CreateTokenSet()
    {
        var issuedAtUtc = new DateTimeOffset(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);

        return new DexcomOAuthTokenSet(
            "access-token",
            "refresh-token",
            "Bearer",
            issuedAtUtc,
            issuedAtUtc.AddHours(2),
            null);
    }

    #endregion
}