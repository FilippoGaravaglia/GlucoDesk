using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Tokens.Services;

public sealed class DexcomOAuthTokenRefreshRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest_WhenValuesAreValid()
    {
        var request = new DexcomOAuthTokenRefreshRequest(
            " client-secret ",
            forceRefresh: true);

        Assert.Equal("client-secret", request.ClientSecret);
        Assert.True(request.ForceRefresh);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidClientSecret(string clientSecret)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomOAuthTokenRefreshRequest(clientSecret));

        Assert.Equal("clientSecret", exception.ParamName);
    }
}