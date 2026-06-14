using GlucoDesk.Application.Cgm.Statistics.Requests;

namespace GlucoDesk.Application.Tests.Cgm.Statistics.Requests;

public sealed class GlucoseStatisticsRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest_WhenValuesAreValid()
    {
        var from = new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero);
        var to = from.AddHours(12);
        var targetRange = GlucoseStatisticsTargetRange.DefaultMgDl();

        var request = new GlucoseStatisticsRequest(
            from,
            to,
            targetRange,
            includeMockData: true);

        Assert.Equal(from, request.From);
        Assert.Equal(to, request.To);
        Assert.Equal(targetRange, request.TargetRange);
        Assert.True(request.IncludeMockData);
    }

    [Fact]
    public void Constructor_ShouldRejectNullTargetRange()
    {
        var from = new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero);

        Assert.Throws<ArgumentNullException>(
            () => new GlucoseStatisticsRequest(
                from,
                from.AddHours(1),
                null!));
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidRange()
    {
        var from = new DateTimeOffset(2026, 6, 14, 8, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GlucoseStatisticsRequest(
                from,
                from,
                GlucoseStatisticsTargetRange.DefaultMgDl()));

        Assert.Equal("to", exception.ParamName);
    }
}