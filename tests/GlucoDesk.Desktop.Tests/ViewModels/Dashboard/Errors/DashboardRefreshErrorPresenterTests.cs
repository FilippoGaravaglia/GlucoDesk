using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Desktop.ViewModels.Dashboard.Errors;

namespace GlucoDesk.Desktop.Tests.ViewModels.Dashboard.Errors;

public sealed class DashboardRefreshErrorPresenterTests
{
    [Theory]
    [InlineData(
        "Dexcom.TokenStoreEmpty",
        "Dexcom is not connected",
        "Dexcom is selected as provider, but no active Dexcom connection is available. Open Settings and connect Dexcom again.")]

    [InlineData(
        "Dexcom.TokenRefreshFailed",
        "Dexcom token refresh failed",
        "GlucoDesk could not refresh the Dexcom session. Reconnect Dexcom from Settings and try again.")]

    [InlineData(
        "Dexcom.ProviderClientSecretMissing",
        "Dexcom configuration incomplete",
        "Dexcom is selected, but the desktop runtime is missing the client secret. Check the Dexcom environment variables and restart GlucoDesk.")]

    [InlineData(
        "Dexcom.EgvUnauthorized",
        "Dexcom authorization required",
        "Dexcom rejected the current authorization. Reconnect Dexcom from Settings and try again.")]

    [InlineData(
        "Dexcom.EgvForbidden",
        "Dexcom access denied",
        "Dexcom denied access to glucose data for the current authorization. Reconnect Dexcom and verify the selected account.")]

    [InlineData(
        "Dexcom.EgvRateLimited",
        "Dexcom rate limit reached",
        "Dexcom is temporarily rate limiting requests. Wait a few minutes before refreshing again.")]

    [InlineData(
        "Dexcom.EgvServerUnavailable",
        "Dexcom temporarily unavailable",
        "Dexcom is currently unavailable or returned a server error. Try again later.")]

    [InlineData(
        "Dexcom.EgvNetworkError",
        "Dexcom network error",
        "GlucoDesk could not complete the Dexcom glucose request due to a network problem.")]

    [InlineData(
        "Dexcom.EgvRequestTimeout",
        "Dexcom request timed out",
        "The Dexcom glucose request took too long. Try refreshing again.")]

    [InlineData(
        "Cgm.LiveProviderUnavailable",
        "Selected provider unavailable",
        "The selected CGM provider is not available in the current desktop runtime. Open Settings and select an available provider.")]
    
    [InlineData(
        "Nightscout.EntriesUnauthorized",
        "Nightscout authorization required",
        "Nightscout rejected the current authorization. Check the configured Nightscout secret or access token.")]

    [InlineData(
        "Nightscout.EntriesForbidden",
        "Nightscout access denied",
        "Nightscout denied access to glucose entries. Verify the configured Nightscout permissions.")]  

    [InlineData(
        "Nightscout.EntriesRateLimited",
        "Nightscout rate limit reached",
        "Nightscout is temporarily rate limiting requests. Wait a few minutes before refreshing again.")]   

    [InlineData(
        "Nightscout.EntriesServerUnavailable",
        "Nightscout temporarily unavailable",
        "Nightscout is currently unavailable or returned a server error. Try again later.")]    

    [InlineData(
        "Nightscout.EntriesNetworkError",
        "Nightscout network error",
        "GlucoDesk could not complete the Nightscout entries request due to a network problem.")]   

    [InlineData(
        "Nightscout.EntriesRequestTimeout",
        "Nightscout request timed out",
        "The Nightscout entries request took too long. Try refreshing again.")]

    public void Present_ShouldMapKnownErrors(
        string errorCode,
        string expectedStatusText,
        string expectedMessage)
    {
        var presentation = DashboardRefreshErrorPresenter.Present(
            new Error(errorCode, "Technical message."));

        Assert.Equal(expectedStatusText, presentation.StatusText);
        Assert.Equal(expectedMessage, presentation.Message);
        Assert.Equal(errorCode, presentation.TechnicalCode);
    }

    [Fact]
    public void Present_ShouldMapUnknownDexcomErrorsToGenericDexcomMessage()
    {
        var presentation = DashboardRefreshErrorPresenter.Present(
            new Error("Dexcom.SomeNewError", "Technical Dexcom message."));

        Assert.Equal("Dexcom refresh failed", presentation.StatusText);
        Assert.Equal(
            "GlucoDesk could not refresh Dexcom glucose data. Check the Dexcom connection in Settings and try again.",
            presentation.Message);
        Assert.Equal("Dexcom.SomeNewError", presentation.TechnicalCode);
    }

    [Fact]
    public void Present_ShouldMapUnknownErrorsToOriginalMessage()
    {
        var presentation = DashboardRefreshErrorPresenter.Present(
            new Error("Some.UnknownError", "Original message."));

        Assert.Equal("Unable to refresh glucose data", presentation.StatusText);
        Assert.Equal("Original message.", presentation.Message);
        Assert.Equal("Some.UnknownError", presentation.TechnicalCode);
    }

    [Fact]
    public void Present_ShouldRejectNullError()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => DashboardRefreshErrorPresenter.Present(null!));

        Assert.Equal("error", exception.ParamName);
    }

    [Fact]
    public void Present_ShouldMapUnknownNightscoutErrorsToGenericNightscoutMessage()
    {
        var presentation = DashboardRefreshErrorPresenter.Present(
            new Error("Nightscout.SomeNewError", "Technical Nightscout message."));
    
        Assert.Equal("Nightscout refresh failed", presentation.StatusText);
        Assert.Equal(
            "GlucoDesk could not refresh Nightscout glucose data. Check the Nightscout configuration and try again.",
            presentation.Message);
        Assert.Equal("Nightscout.SomeNewError", presentation.TechnicalCode);
    }
}