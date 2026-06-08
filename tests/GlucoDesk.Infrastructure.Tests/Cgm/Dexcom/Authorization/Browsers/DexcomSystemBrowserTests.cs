using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Browsers;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization.Browsers;

public sealed class DexcomSystemBrowserTests
{
    [Fact]
    public async Task OpenAsync_ShouldRejectRelativeUri()
    {
        var browser = new DexcomSystemBrowser();

        var result = await browser.OpenAsync(
            new Uri("/relative", UriKind.Relative),
            CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.BrowserInvalidUri", result.Error.Code);
    }

    [Fact]
    public async Task OpenAsync_ShouldRejectNullUri()
    {
        var browser = new DexcomSystemBrowser();

        var exception = await Assert.ThrowsAsync<ArgumentNullException>(
            () => browser.OpenAsync(null!, CancellationToken.None));

        Assert.Equal("authorizationUri", exception.ParamName);
    }

    [Fact]
    public async Task OpenAsync_ShouldReturnFailure_WhenCancellationIsAlreadyRequested()
    {
        var browser = new DexcomSystemBrowser();

        using var cancellationTokenSource = new CancellationTokenSource();
        await cancellationTokenSource.CancelAsync();

        var result = await browser.OpenAsync(
            new Uri("https://sandbox-api.dexcom.com/v3/oauth2/login"),
            cancellationTokenSource.Token);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.BrowserOpenCancelled", result.Error.Code);
    }
}