using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization.Listeners;

public sealed class DexcomLocalOAuthCallbackListenRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest_WhenValuesAreValid()
    {
        var redirectUri = new Uri("http://127.0.0.1:51234/callback");

        var request = new DexcomLocalOAuthCallbackListenRequest(
            redirectUri,
            " state-value ",
            TimeSpan.FromSeconds(30));

        Assert.Equal(redirectUri, request.RedirectUri);
        Assert.Equal("state-value", request.ExpectedState);
        Assert.Equal(TimeSpan.FromSeconds(30), request.Timeout);
    }

    [Fact]
    public void Constructor_ShouldRejectNullRedirectUri()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new DexcomLocalOAuthCallbackListenRequest(
                null!,
                "state-value"));

        Assert.Equal("redirectUri", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectRelativeRedirectUri()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomLocalOAuthCallbackListenRequest(
                new Uri("/callback", UriKind.Relative),
                "state-value"));

        Assert.Equal("redirectUri", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectHttpsRedirectUri()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomLocalOAuthCallbackListenRequest(
                new Uri("https://127.0.0.1:51234/callback"),
                "state-value"));

        Assert.Equal("redirectUri", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectNonLoopbackRedirectUri()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomLocalOAuthCallbackListenRequest(
                new Uri("http://example.com/callback"),
                "state-value"));

        Assert.Equal("redirectUri", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidExpectedState(string expectedState)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomLocalOAuthCallbackListenRequest(
                new Uri("http://127.0.0.1:51234/callback"),
                expectedState));

        Assert.Equal("expectedState", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectNonPositiveTimeout()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomLocalOAuthCallbackListenRequest(
                new Uri("http://127.0.0.1:51234/callback"),
                "state-value",
                TimeSpan.Zero));

        Assert.Equal("timeout", exception.ParamName);
    }
}