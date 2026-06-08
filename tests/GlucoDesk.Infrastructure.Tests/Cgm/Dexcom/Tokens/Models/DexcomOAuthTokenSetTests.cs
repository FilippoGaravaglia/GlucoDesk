using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Tokens.Models;

public sealed class DexcomOAuthTokenSetTests
{
    [Fact]
    public void Constructor_ShouldCreateTokenSet_WhenValuesAreValid()
    {
        var issuedAtUtc = new DateTimeOffset(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);

        var tokenSet = new DexcomOAuthTokenSet(
            " access-token ",
            " refresh-token ",
            " Bearer ",
            issuedAtUtc,
            issuedAtUtc.AddHours(2),
            issuedAtUtc.AddDays(1));

        Assert.Equal("access-token", tokenSet.AccessToken);
        Assert.Equal("refresh-token", tokenSet.RefreshToken);
        Assert.Equal("Bearer", tokenSet.TokenType);
        Assert.True(tokenSet.IsBearerToken);
        Assert.Equal(issuedAtUtc.AddHours(2), tokenSet.AccessTokenExpiresAtUtc);
        Assert.Equal(issuedAtUtc.AddDays(1), tokenSet.RefreshTokenExpiresAtUtc);
    }

    [Fact]
    public void Constructor_ShouldRejectExpiredAccessToken()
    {
        var issuedAtUtc = new DateTimeOffset(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomOAuthTokenSet(
                "access-token",
                "refresh-token",
                "Bearer",
                issuedAtUtc,
                issuedAtUtc,
                null));

        Assert.Equal("accessTokenExpiresAtUtc", exception.ParamName);
    }
}