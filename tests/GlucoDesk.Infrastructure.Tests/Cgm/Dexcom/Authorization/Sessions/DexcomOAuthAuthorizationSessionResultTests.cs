using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Sessions;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Models;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization.Sessions;

public sealed class DexcomOAuthAuthorizationSessionResultTests
{
    [Fact]
    public void Constructor_ShouldCreateResult_WhenValuesAreValid()
    {
        var authorizationUri = new Uri("https://sandbox-api.dexcom.com/v3/oauth2/login");
        var callbackUri = new Uri("http://127.0.0.1:51234/callback?code=authorization-code&state=state-value");
        var tokenSet = CreateTokenSet();

        var result = new DexcomOAuthAuthorizationSessionResult(
            authorizationUri,
            " state-value ",
            callbackUri,
            tokenSet);

        Assert.Equal(authorizationUri, result.AuthorizationUri);
        Assert.Equal("state-value", result.State);
        Assert.Equal(callbackUri, result.CallbackUri);
        Assert.Same(tokenSet, result.TokenSet);
    }

    [Fact]
    public void Constructor_ShouldRejectNullAuthorizationUri()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new DexcomOAuthAuthorizationSessionResult(
                null!,
                "state-value",
                new Uri("http://127.0.0.1:51234/callback"),
                CreateTokenSet()));

        Assert.Equal("authorizationUri", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidState()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomOAuthAuthorizationSessionResult(
                new Uri("https://sandbox-api.dexcom.com/v3/oauth2/login"),
                "   ",
                new Uri("http://127.0.0.1:51234/callback"),
                CreateTokenSet()));

        Assert.Equal("state", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectNullCallbackUri()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new DexcomOAuthAuthorizationSessionResult(
                new Uri("https://sandbox-api.dexcom.com/v3/oauth2/login"),
                "state-value",
                null!,
                CreateTokenSet()));

        Assert.Equal("callbackUri", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectNullTokenSet()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new DexcomOAuthAuthorizationSessionResult(
                new Uri("https://sandbox-api.dexcom.com/v3/oauth2/login"),
                "state-value",
                new Uri("http://127.0.0.1:51234/callback"),
                null!));

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