using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization;

public sealed class DexcomAuthorizationRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest_WhenValuesAreValid()
    {
        var request = new DexcomAuthorizationRequest(
            " client-id ",
            new Uri("http://127.0.0.1:51234/callback"),
            ["egv", " offline_access ", "egv"],
            " state-value ");

        Assert.Equal("client-id", request.ClientId);
        Assert.Equal(new Uri("http://127.0.0.1:51234/callback"), request.RedirectUri);
        Assert.Equal(["egv", "offline_access"], request.Scopes);
        Assert.Equal("state-value", request.State);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidClientId(string clientId)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomAuthorizationRequest(
                clientId,
                new Uri("http://127.0.0.1:51234/callback")));

        Assert.Equal("clientId", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectNullRedirectUri()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new DexcomAuthorizationRequest(
                "client-id",
                null!));

        Assert.Equal("redirectUri", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectRelativeRedirectUri()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomAuthorizationRequest(
                "client-id",
                new Uri("/callback", UriKind.Relative)));

        Assert.Equal("redirectUri", exception.ParamName);
    }
}