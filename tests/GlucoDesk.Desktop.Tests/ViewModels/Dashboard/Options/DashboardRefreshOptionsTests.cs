using GlucoDesk.Desktop.ViewModels.Dashboard.Options;

namespace GlucoDesk.Desktop.Tests.ViewModels.Dashboard.Options;

public sealed class DashboardRefreshOptionsTests
{
    [Fact]
    public void Default_ShouldUseExpectedAutoRefreshInterval()
    {
        var options = DashboardRefreshOptions.Default;

        Assert.Equal(TimeSpan.FromSeconds(30), options.AutoRefreshInterval);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidAutoRefreshInterval()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DashboardRefreshOptions(TimeSpan.Zero));

        Assert.Equal("autoRefreshInterval", exception.ParamName);
    }
}