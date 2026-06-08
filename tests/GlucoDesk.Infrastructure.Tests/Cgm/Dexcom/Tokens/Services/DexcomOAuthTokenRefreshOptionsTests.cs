using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Tokens.Services;

public sealed class DexcomOAuthTokenRefreshOptionsTests
{
    [Fact]
    public void Default_ShouldUseExpectedRefreshSafetyWindow()
    {
        Assert.Equal(TimeSpan.FromMinutes(5), DexcomOAuthTokenRefreshOptions.Default.RefreshSafetyWindow);
    }

    [Fact]
    public void Constructor_ShouldCreateOptions_WhenValueIsValid()
    {
        var options = new DexcomOAuthTokenRefreshOptions(TimeSpan.FromMinutes(10));

        Assert.Equal(TimeSpan.FromMinutes(10), options.RefreshSafetyWindow);
    }

    [Fact]
    public void Constructor_ShouldAllowZeroRefreshSafetyWindow()
    {
        var options = new DexcomOAuthTokenRefreshOptions(TimeSpan.Zero);

        Assert.Equal(TimeSpan.Zero, options.RefreshSafetyWindow);
    }

    [Fact]
    public void Constructor_ShouldRejectNegativeRefreshSafetyWindow()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomOAuthTokenRefreshOptions(TimeSpan.FromSeconds(-1)));

        Assert.Equal("refreshSafetyWindow", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectTooLargeRefreshSafetyWindow()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomOAuthTokenRefreshOptions(TimeSpan.FromMinutes(61)));

        Assert.Equal("refreshSafetyWindow", exception.ParamName);
    }
}