using GlucoDesk.Infrastructure.Cgm.Nightscout.Enums;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Options;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Nightscout.Options;

public sealed class NightscoutOptionsTests
{
    [Fact]
    public void Constructor_ShouldCreateOptions_WhenValuesAreValid()
    {
        var options = new NightscoutOptions(
            new Uri("https://example-nightscout.test"),
            " Nightscout Demo ",
            NightscoutAuthenticationMode.None,
            latestReadingLookback: TimeSpan.FromMinutes(30),
            requestTimeout: TimeSpan.FromSeconds(10),
            maxReadingsPerRequest: 100);

        Assert.Equal(new Uri("https://example-nightscout.test"), options.BaseUri);
        Assert.Equal("Nightscout Demo", options.DisplayName);
        Assert.Equal(NightscoutAuthenticationMode.None, options.AuthenticationMode);
        Assert.Equal(TimeSpan.FromMinutes(30), options.LatestReadingLookback);
        Assert.Equal(TimeSpan.FromSeconds(10), options.RequestTimeout);
        Assert.Equal(100, options.MaxReadingsPerRequest);
    }

    [Fact]
    public void Constructor_ShouldRejectRelativeBaseUri()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new NightscoutOptions(new Uri("/api/v1", UriKind.Relative)));

        Assert.Equal("baseUri", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectMissingApiSecretSha1_WhenHeaderAuthenticationIsEnabled()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new NightscoutOptions(
                new Uri("https://example-nightscout.test"),
                authenticationMode: NightscoutAuthenticationMode.ApiSecretSha1Header));

        Assert.Equal("apiSecretSha1", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectMissingAccessToken_WhenQueryStringAuthenticationIsEnabled()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new NightscoutOptions(
                new Uri("https://example-nightscout.test"),
                authenticationMode: NightscoutAuthenticationMode.AccessTokenQueryString));

        Assert.Equal("accessToken", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldAcceptApiSecretSha1HeaderAuthentication()
    {
        var options = new NightscoutOptions(
            new Uri("https://example-nightscout.test"),
            authenticationMode: NightscoutAuthenticationMode.ApiSecretSha1Header,
            apiSecretSha1: " abc123 ");

        Assert.Equal(NightscoutAuthenticationMode.ApiSecretSha1Header, options.AuthenticationMode);
        Assert.Equal("abc123", options.ApiSecretSha1);
    }

    [Fact]
    public void Constructor_ShouldAcceptAccessTokenQueryStringAuthentication()
    {
        var options = new NightscoutOptions(
            new Uri("https://example-nightscout.test"),
            authenticationMode: NightscoutAuthenticationMode.AccessTokenQueryString,
            accessToken: " token-value ");

        Assert.Equal(NightscoutAuthenticationMode.AccessTokenQueryString, options.AuthenticationMode);
        Assert.Equal("token-value", options.AccessToken);
    }
}