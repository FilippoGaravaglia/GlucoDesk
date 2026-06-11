using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers.Options;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Providers.Options;

public sealed class DexcomCgmProviderOptionsTests
{
    [Fact]
    public void Default_ShouldUseExpectedValues()
    {
        var options = DexcomCgmProviderOptions.Default;

        Assert.Null(options.ClientSecret);
        Assert.False(options.HasClientSecret);
        Assert.Equal(TimeSpan.FromHours(24), options.LatestReadingLookback);
        Assert.Equal("Dexcom Official API", options.DisplayName);
    }

    [Fact]
    public void Constructor_ShouldCreateOptions_WhenValuesAreValid()
    {
        var options = new DexcomCgmProviderOptions(
            " client-secret ",
            TimeSpan.FromHours(12),
            " Dexcom Provider ");

        Assert.Equal("client-secret", options.ClientSecret);
        Assert.True(options.HasClientSecret);
        Assert.Equal(TimeSpan.FromHours(12), options.LatestReadingLookback);
        Assert.Equal("Dexcom Provider", options.DisplayName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldTreatBlankClientSecretAsMissing(string? clientSecret)
    {
        var options = new DexcomCgmProviderOptions(clientSecret);

        Assert.Null(options.ClientSecret);
        Assert.False(options.HasClientSecret);
    }

    [Fact]
    public void Constructor_ShouldRejectNonPositiveLatestReadingLookback()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomCgmProviderOptions(
                "client-secret",
                TimeSpan.Zero));

        Assert.Equal("latestReadingLookback", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectLatestReadingLookbackGreaterThanThirtyDays()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomCgmProviderOptions(
                "client-secret",
                TimeSpan.FromDays(30).Add(TimeSpan.FromTicks(1))));

        Assert.Equal("latestReadingLookback", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidDisplayName(string displayName)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomCgmProviderOptions(
                "client-secret",
                TimeSpan.FromHours(24),
                displayName));

        Assert.Equal("displayName", exception.ParamName);
    }
}