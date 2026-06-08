using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization.Listeners;

public sealed class DexcomLocalOAuthCallbackOptionsTests
{
    [Fact]
    public void Default_ShouldUseExpectedTimeout()
    {
        Assert.Equal(TimeSpan.FromMinutes(2), DexcomLocalOAuthCallbackOptions.Default.DefaultTimeout);
    }

    [Fact]
    public void Constructor_ShouldCreateOptions_WhenTimeoutIsValid()
    {
        var options = new DexcomLocalOAuthCallbackOptions(TimeSpan.FromSeconds(30));

        Assert.Equal(TimeSpan.FromSeconds(30), options.DefaultTimeout);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectNonPositiveTimeout(int seconds)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomLocalOAuthCallbackOptions(TimeSpan.FromSeconds(seconds)));

        Assert.Equal("defaultTimeout", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectTooLongTimeout()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomLocalOAuthCallbackOptions(TimeSpan.FromMinutes(11)));

        Assert.Equal("defaultTimeout", exception.ParamName);
    }
}