using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Requests;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Tokens.Requests;

public sealed class DexcomRefreshTokenRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest_WhenValuesAreValid()
    {
        var request = new DexcomRefreshTokenRequest(
            " refresh-token ",
            " client-secret ");

        Assert.Equal("refresh-token", request.RefreshToken);
        Assert.Equal("client-secret", request.ClientSecret);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidRefreshToken(string refreshToken)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomRefreshTokenRequest(
                refreshToken,
                "client-secret"));

        Assert.Equal("refreshToken", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidClientSecret(string clientSecret)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomRefreshTokenRequest(
                "refresh-token",
                clientSecret));

        Assert.Equal("clientSecret", exception.ParamName);
    }
}