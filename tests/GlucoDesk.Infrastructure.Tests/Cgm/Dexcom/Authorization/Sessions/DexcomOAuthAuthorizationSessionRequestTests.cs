using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Sessions;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization.Sessions;

public sealed class DexcomOAuthAuthorizationSessionRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest_WhenValuesAreValid()
    {
        var request = new DexcomOAuthAuthorizationSessionRequest(
            " client-secret ",
            TimeSpan.FromMinutes(2));

        Assert.Equal("client-secret", request.ClientSecret);
        Assert.Equal(TimeSpan.FromMinutes(2), request.CallbackTimeout);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidClientSecret(string clientSecret)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomOAuthAuthorizationSessionRequest(clientSecret));

        Assert.Equal("clientSecret", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectNonPositiveCallbackTimeout()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomOAuthAuthorizationSessionRequest(
                "client-secret",
                TimeSpan.Zero));

        Assert.Equal("callbackTimeout", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectTooLongCallbackTimeout()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomOAuthAuthorizationSessionRequest(
                "client-secret",
                TimeSpan.FromMinutes(11)));

        Assert.Equal("callbackTimeout", exception.ParamName);
    }
}