using GlucoDesk.Desktop.ViewModels.Dashboard.Errors;

namespace GlucoDesk.Desktop.Tests.ViewModels.Dashboard.Errors;

public sealed class DashboardRefreshErrorPresentationTests
{
    [Fact]
    public void Constructor_ShouldCreatePresentation_WhenValuesAreValid()
    {
        var presentation = new DashboardRefreshErrorPresentation(
            " Dexcom authorization required ",
            " Reconnect Dexcom. ",
            " Dexcom.EgvUnauthorized ");

        Assert.Equal("Dexcom authorization required", presentation.StatusText);
        Assert.Equal("Reconnect Dexcom.", presentation.Message);
        Assert.Equal("Dexcom.EgvUnauthorized", presentation.TechnicalCode);
        Assert.Equal("Reconnect Dexcom. (Dexcom.EgvUnauthorized)", presentation.FullMessage);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidStatusText(string statusText)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DashboardRefreshErrorPresentation(
                statusText,
                "Message",
                "Code"));

        Assert.Equal("statusText", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidMessage(string message)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DashboardRefreshErrorPresentation(
                "Status",
                message,
                "Code"));

        Assert.Equal("message", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidTechnicalCode(string technicalCode)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DashboardRefreshErrorPresentation(
                "Status",
                "Message",
                technicalCode));

        Assert.Equal("technicalCode", exception.ParamName);
    }
}