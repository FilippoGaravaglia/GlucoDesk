using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization.Callbacks;

public sealed class DexcomOAuthCallbackResultTests
{
    [Fact]
    public void Constructor_ShouldCreateResult_WhenValuesAreValid()
    {
        var result = new DexcomOAuthCallbackResult(
            " authorization-code ",
            " state-value ");

        Assert.Equal("authorization-code", result.AuthorizationCode);
        Assert.Equal("state-value", result.State);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidAuthorizationCode(string authorizationCode)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomOAuthCallbackResult(
                authorizationCode,
                "state-value"));

        Assert.Equal("authorizationCode", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidState(string state)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomOAuthCallbackResult(
                "authorization-code",
                state));

        Assert.Equal("state", exception.ParamName);
    }
}