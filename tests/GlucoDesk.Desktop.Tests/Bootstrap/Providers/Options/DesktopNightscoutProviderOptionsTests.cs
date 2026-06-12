using GlucoDesk.Desktop.Bootstrap.Providers.Options;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Enums;

namespace GlucoDesk.Desktop.Tests.Bootstrap.Providers.Options;

public sealed class DesktopNightscoutProviderOptionsTests
{
    [Fact]
    public void Constructor_ShouldCreateOptions_WhenValuesAreValid()
    {
        var options = new DesktopNightscoutProviderOptions(
            true,
            new Uri("https://example-nightscout.test"),
            " Nightscout Demo ",
            NightscoutAuthenticationMode.None,
            latestReadingLookback: TimeSpan.FromMinutes(30),
            requestTimeout: TimeSpan.FromSeconds(10),
            maxReadingsPerRequest: 100);

        Assert.True(options.IsEnabled);
        Assert.Equal(new Uri("https://example-nightscout.test"), options.BaseUri);
        Assert.Equal("Nightscout Demo", options.DisplayName);
        Assert.Equal(NightscoutAuthenticationMode.None, options.AuthenticationMode);
        Assert.Equal(TimeSpan.FromMinutes(30), options.LatestReadingLookback);
        Assert.Equal(TimeSpan.FromSeconds(10), options.RequestTimeout);
        Assert.Equal(100, options.MaxReadingsPerRequest);
    }

    [Fact]
    public void ToNightscoutOptions_ShouldCreateInfrastructureOptions_WhenEnabled()
    {
        var desktopOptions = new DesktopNightscoutProviderOptions(
            true,
            new Uri("https://example-nightscout.test"),
            "Nightscout Demo",
            NightscoutAuthenticationMode.AccessTokenQueryString,
            accessToken: "token-value");

        var options = desktopOptions.ToNightscoutOptions();

        Assert.Equal(new Uri("https://example-nightscout.test"), options.BaseUri);
        Assert.Equal("Nightscout Demo", options.DisplayName);
        Assert.Equal(NightscoutAuthenticationMode.AccessTokenQueryString, options.AuthenticationMode);
        Assert.Equal("token-value", options.AccessToken);
    }

    [Fact]
    public void ToNightscoutOptions_ShouldRejectDisabledProvider()
    {
        var desktopOptions = new DesktopNightscoutProviderOptions(
            false,
            new Uri("https://example-nightscout.test"));

        var exception = Assert.Throws<InvalidOperationException>(
            desktopOptions.ToNightscoutOptions);

        Assert.Equal("Nightscout provider is not enabled.", exception.Message);
    }

    [Fact]
    public void ToNightscoutOptions_ShouldRejectMissingBaseUri_WhenEnabled()
    {
        var desktopOptions = new DesktopNightscoutProviderOptions(
            true,
            null);

        var exception = Assert.Throws<InvalidOperationException>(
            desktopOptions.ToNightscoutOptions);

        Assert.Equal(
            "Nightscout provider is enabled but the base URI is not configured.",
            exception.Message);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidDisplayName()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DesktopNightscoutProviderOptions(
                true,
                new Uri("https://example-nightscout.test"),
                " "));

        Assert.Equal("displayName", exception.ParamName);
    }
}