using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Tokens.Services;

public sealed class DexcomAccessTokenResultTests
{
    [Fact]
    public void Constructor_ShouldCreateResult_WhenValuesAreValid()
    {
        var tokenSet = CreateTokenSet();

        var result = new DexcomAccessTokenResult(
            tokenSet,
            wasRefreshed: true);

        Assert.Same(tokenSet, result.TokenSet);
        Assert.True(result.WasRefreshed);
        Assert.Equal("access-token", result.AccessToken);
        Assert.Equal("Bearer", result.TokenType);
        Assert.Equal(tokenSet.AccessTokenExpiresAtUtc, result.AccessTokenExpiresAtUtc);
    }

    [Fact]
    public void Constructor_ShouldRejectNullTokenSet()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new DexcomAccessTokenResult(null!, wasRefreshed: false));

        Assert.Equal("tokenSet", exception.ParamName);
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