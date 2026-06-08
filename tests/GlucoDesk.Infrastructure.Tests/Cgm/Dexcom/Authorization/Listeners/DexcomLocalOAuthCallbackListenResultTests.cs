using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization.Listeners;

public sealed class DexcomLocalOAuthCallbackListenResultTests
{
    [Fact]
    public void Constructor_ShouldCreateResult_WhenValuesAreValid()
    {
        var callbackUri = new Uri("http://127.0.0.1:51234/callback?code=authorization-code&state=state-value");
        var callbackResult = new DexcomOAuthCallbackResult("authorization-code", "state-value");

        var result = new DexcomLocalOAuthCallbackListenResult(callbackUri, callbackResult);

        Assert.Equal(callbackUri, result.CallbackUri);
        Assert.Same(callbackResult, result.CallbackResult);
        Assert.Equal("authorization-code", result.AuthorizationCode);
        Assert.Equal("state-value", result.State);
    }

    [Fact]
    public void Constructor_ShouldRejectNullCallbackUri()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new DexcomLocalOAuthCallbackListenResult(
                null!,
                new DexcomOAuthCallbackResult("authorization-code", "state-value")));

        Assert.Equal("callbackUri", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectNullCallbackResult()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new DexcomLocalOAuthCallbackListenResult(
                new Uri("http://127.0.0.1:51234/callback"),
                null!));

        Assert.Equal("callbackResult", exception.ParamName);
    }
}