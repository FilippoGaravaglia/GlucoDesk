using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Options;

public sealed class DexcomApiOptionsTests
{
    [Fact]
    public void Constructor_ShouldCreateOptions_WhenValuesAreValid()
    {
        var options = new DexcomApiOptions(
            DexcomApiEnvironment.Sandbox,
            " client-id ",
            new Uri("http://127.0.0.1:51234/callback"),
            ["egv", " offline_access ", "egv"]);

        Assert.Equal(DexcomApiEnvironment.Sandbox, options.Environment);
        Assert.Equal("client-id", options.ClientId);
        Assert.Equal(new Uri("http://127.0.0.1:51234/callback"), options.RedirectUri);
        Assert.Equal(["egv", "offline_access"], options.Scopes);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidClientId(string clientId)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomApiOptions(
                DexcomApiEnvironment.Sandbox,
                clientId,
                new Uri("http://127.0.0.1:51234/callback")));

        Assert.Equal("clientId", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectNullRedirectUri()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new DexcomApiOptions(
                DexcomApiEnvironment.Sandbox,
                "client-id",
                null!));

        Assert.Equal("redirectUri", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectRelativeRedirectUri()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomApiOptions(
                DexcomApiEnvironment.Sandbox,
                "client-id",
                new Uri("/callback", UriKind.Relative)));

        Assert.Equal("redirectUri", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectUnsupportedEnvironment()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomApiOptions(
                (DexcomApiEnvironment)999,
                "client-id",
                new Uri("http://127.0.0.1:51234/callback")));

        Assert.Equal("environment", exception.ParamName);
    }
}