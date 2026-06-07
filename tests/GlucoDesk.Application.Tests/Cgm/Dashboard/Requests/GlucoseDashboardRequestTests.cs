using GlucoDesk.Application.Cgm.Dashboard.Requests;

namespace GlucoDesk.Application.Tests.Cgm.Dashboard.Requests;

public sealed class GlucoseDashboardRequestTests
{
    [Fact]
    public void Default_ShouldUseExpectedValues()
    {
        var request = GlucoseDashboardRequest.Default;

        Assert.Equal(TimeSpan.FromHours(3), request.HistoryDuration);
        Assert.Equal(TimeSpan.FromMinutes(15), request.StaleThreshold);
        Assert.Equal(36, request.MaxReadings);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidHistoryDuration()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GlucoseDashboardRequest(
                TimeSpan.Zero,
                TimeSpan.FromMinutes(15)));

        Assert.Equal("historyDuration", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidStaleThreshold()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GlucoseDashboardRequest(
                TimeSpan.FromHours(3),
                TimeSpan.Zero));

        Assert.Equal("staleThreshold", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidMaxReadings(int maxReadings)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GlucoseDashboardRequest(
                TimeSpan.FromHours(3),
                TimeSpan.FromMinutes(15),
                maxReadings));

        Assert.Equal("maxReadings", exception.ParamName);
    }
}