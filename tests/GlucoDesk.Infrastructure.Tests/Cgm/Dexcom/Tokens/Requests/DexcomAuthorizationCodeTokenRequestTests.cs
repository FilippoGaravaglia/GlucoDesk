using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Requests;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Tokens.Requests;

public sealed class DexcomAuthorizationCodeTokenRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest_WhenValuesAreValid()
    {
        var request = new DexcomAuthorizationCodeTokenRequest(
            " authorization-code ",
            " client-secret ");

        Assert.Equal("authorization-code", request.AuthorizationCode);
        Assert.Equal("client-secret", request.ClientSecret);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidAuthorizationCode(string authorizationCode)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomAuthorizationCodeTokenRequest(
                authorizationCode,
                "client-secret"));

        Assert.Equal("authorizationCode", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidClientSecret(string clientSecret)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomAuthorizationCodeTokenRequest(
                "authorization-code",
                clientSecret));

        Assert.Equal("clientSecret", exception.ParamName);
    }
}