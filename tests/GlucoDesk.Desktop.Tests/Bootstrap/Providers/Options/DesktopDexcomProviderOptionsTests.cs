using GlucoDesk.Desktop.Bootstrap.Providers.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

namespace GlucoDesk.Desktop.Tests.Bootstrap.Providers.Options;

public sealed class DesktopDexcomProviderOptionsTests
{
    [Fact]
    public void Disabled_ShouldCreateDisabledOptions()
    {
        var options = DesktopDexcomProviderOptions.Disabled;

        Assert.False(options.IsEnabled);
        Assert.Equal(DexcomApiEnvironment.Sandbox, options.Environment);
        Assert.Null(options.ClientId);
        Assert.Null(options.ClientSecret);
        Assert.Equal(new Uri("http://127.0.0.1:51234/callback"), options.RedirectUri);
        Assert.Contains("egv", options.Scopes);
        Assert.Contains("offline_access", options.Scopes);
        Assert.Equal(TimeSpan.FromHours(24), options.LatestReadingLookback);
        Assert.Equal("Dexcom Official API", options.DisplayName);
    }

    [Fact]
    public void Constructor_ShouldCreateEnabledOptions_WhenValuesAreValid()
    {
        var options = new DesktopDexcomProviderOptions(
            isEnabled: true,
            environment: DexcomApiEnvironment.ProductionEu,
            clientId: " client-id ",
            clientSecret: " client-secret ",
            redirectUri: new Uri("http://127.0.0.1:51234/callback"),
            scopes: ["egv", "offline_access"],
            latestReadingLookback: TimeSpan.FromHours(12),
            displayName: " Dexcom EU ");

        Assert.True(options.IsEnabled);
        Assert.Equal(DexcomApiEnvironment.ProductionEu, options.Environment);
        Assert.Equal("client-id", options.ClientId);
        Assert.Equal("client-secret", options.ClientSecret);
        Assert.Equal(TimeSpan.FromHours(12), options.LatestReadingLookback);
        Assert.Equal("Dexcom EU", options.DisplayName);
    }

    [Fact]
    public void ToApiOptions_ShouldCreateDexcomApiOptions_WhenEnabled()
    {
        var options = CreateEnabledOptions();

        var apiOptions = options.ToApiOptions();

        Assert.Equal(DexcomApiEnvironment.Sandbox, apiOptions.Environment);
        Assert.Equal("client-id", apiOptions.ClientId);
        Assert.Equal(new Uri("http://127.0.0.1:51234/callback"), apiOptions.RedirectUri);
        Assert.Contains("egv", apiOptions.Scopes);
    }

    [Fact]
    public void ToProviderOptions_ShouldCreateDexcomProviderOptions_WhenEnabled()
    {
        var options = CreateEnabledOptions();

        var providerOptions = options.ToProviderOptions();

        Assert.Equal("client-secret", providerOptions.ClientSecret);
        Assert.Equal(TimeSpan.FromHours(24), providerOptions.LatestReadingLookback);
        Assert.Equal("Dexcom Official API", providerOptions.DisplayName);
    }

    [Fact]
    public void ToApiOptions_ShouldThrow_WhenDisabled()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            DesktopDexcomProviderOptions.Disabled.ToApiOptions);

        Assert.Equal("Dexcom provider options are disabled.", exception.Message);
    }

    [Fact]
    public void ToProviderOptions_ShouldThrow_WhenDisabled()
    {
        var exception = Assert.Throws<InvalidOperationException>(
            DesktopDexcomProviderOptions.Disabled.ToProviderOptions);

        Assert.Equal("Dexcom provider options are disabled.", exception.Message);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectMissingClientId_WhenEnabled(string? clientId)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DesktopDexcomProviderOptions(
                isEnabled: true,
                clientId: clientId,
                clientSecret: "client-secret"));

        Assert.Equal("clientId", exception.ParamName);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectMissingClientSecret_WhenEnabled(string? clientSecret)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DesktopDexcomProviderOptions(
                isEnabled: true,
                clientId: "client-id",
                clientSecret: clientSecret));

        Assert.Equal("clientSecret", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectLookbackGreaterThanThirtyDays()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DesktopDexcomProviderOptions(
                latestReadingLookback: TimeSpan.FromDays(31)));

        Assert.Equal("latestReadingLookback", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates enabled Dexcom desktop provider options for tests.
    /// </summary>
    /// <returns>The enabled Dexcom desktop provider options.</returns>
    private static DesktopDexcomProviderOptions CreateEnabledOptions()
    {
        return new DesktopDexcomProviderOptions(
            isEnabled: true,
            clientId: "client-id",
            clientSecret: "client-secret");
    }

    #endregion
}