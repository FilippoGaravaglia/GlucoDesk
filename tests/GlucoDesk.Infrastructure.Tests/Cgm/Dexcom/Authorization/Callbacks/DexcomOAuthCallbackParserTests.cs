using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization.Callbacks;

public sealed class DexcomOAuthCallbackParserTests
{
    [Fact]
    public void ParseCallback_ShouldReturnSuccess_WhenCallbackIsValid()
    {
        var parser = new DexcomOAuthCallbackParser();

        var result = parser.ParseCallback(
            new Uri("http://127.0.0.1:51234/callback?code=authorization-code&state=state-value"),
            "state-value");

        Assert.True(result.IsSuccess);
        Assert.Equal("authorization-code", result.Value.AuthorizationCode);
        Assert.Equal("state-value", result.Value.State);
    }

    [Fact]
    public void ParseCallback_ShouldDecodeQueryValues()
    {
        var parser = new DexcomOAuthCallbackParser();

        var result = parser.ParseCallback(
            new Uri("http://127.0.0.1:51234/callback?code=authorization%20code&state=state%20value"),
            "state value");

        Assert.True(result.IsSuccess);
        Assert.Equal("authorization code", result.Value.AuthorizationCode);
        Assert.Equal("state value", result.Value.State);
    }

    [Fact]
    public void ParseCallback_ShouldReturnFailure_WhenDexcomReturnsOAuthError()
    {
        var parser = new DexcomOAuthCallbackParser();

        var result = parser.ParseCallback(
            new Uri("http://127.0.0.1:51234/callback?error=access_denied&error_description=User+denied+access&state=state-value"),
            "state-value");

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.OAuthRejected", result.Error.Code);
        Assert.Contains("access_denied", result.Error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void ParseCallback_ShouldReturnFailure_WhenAuthorizationCodeIsMissing()
    {
        var parser = new DexcomOAuthCallbackParser();

        var result = parser.ParseCallback(
            new Uri("http://127.0.0.1:51234/callback?state=state-value"),
            "state-value");

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.OAuthMissingCode", result.Error.Code);
    }

    [Fact]
    public void ParseCallback_ShouldReturnFailure_WhenStateIsMissing()
    {
        var parser = new DexcomOAuthCallbackParser();

        var result = parser.ParseCallback(
            new Uri("http://127.0.0.1:51234/callback?code=authorization-code"),
            "state-value");

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.OAuthMissingState", result.Error.Code);
    }

    [Fact]
    public void ParseCallback_ShouldReturnFailure_WhenStateDoesNotMatch()
    {
        var parser = new DexcomOAuthCallbackParser();

        var result = parser.ParseCallback(
            new Uri("http://127.0.0.1:51234/callback?code=authorization-code&state=returned-state"),
            "expected-state");

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.OAuthStateMismatch", result.Error.Code);
    }

    [Fact]
    public void ParseCallback_ShouldReturnFailure_WhenCallbackUriIsRelative()
    {
        var parser = new DexcomOAuthCallbackParser();

        var result = parser.ParseCallback(
            new Uri("/callback?code=authorization-code&state=state-value", UriKind.Relative),
            "state-value");

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.OAuthInvalidCallback", result.Error.Code);
    }

    [Fact]
    public void ParseCallback_ShouldRejectNullCallbackUri()
    {
        var parser = new DexcomOAuthCallbackParser();

        var exception = Assert.Throws<ArgumentNullException>(
            () => parser.ParseCallback(null!, "state-value"));

        Assert.Equal("callbackUri", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void ParseCallback_ShouldRejectInvalidExpectedState(string expectedState)
    {
        var parser = new DexcomOAuthCallbackParser();

        var exception = Assert.Throws<ArgumentException>(
            () => parser.ParseCallback(
                new Uri("http://127.0.0.1:51234/callback?code=authorization-code&state=state-value"),
                expectedState));

        Assert.Equal("expectedState", exception.ParamName);
    }
}