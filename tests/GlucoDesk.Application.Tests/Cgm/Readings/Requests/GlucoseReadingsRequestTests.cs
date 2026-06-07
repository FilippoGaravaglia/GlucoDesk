using GlucoDesk.Application.Cgm.Readings.Requests;

namespace GlucoDesk.Application.Tests.Cgm.Readings.Requests;

public sealed class GlucoseReadingsRequestTests
{
    [Fact]
    public void Constructor_ShouldRejectDefaultFrom()
    {
        var to = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentException>(
            () => new GlucoseReadingsRequest(default, to));

        Assert.Equal("from", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectDefaultTo()
    {
        var from = new DateTimeOffset(2026, 6, 7, 9, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentException>(
            () => new GlucoseReadingsRequest(from, default));

        Assert.Equal("to", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidRange()
    {
        var timestamp = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentException>(
            () => new GlucoseReadingsRequest(timestamp, timestamp));

        Assert.Equal("to", exception.ParamName);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_ShouldRejectInvalidLimit(int limit)
    {
        var from = new DateTimeOffset(2026, 6, 7, 9, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GlucoseReadingsRequest(from, to, limit));

        Assert.Equal("limit", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldCreateRequest()
    {
        var from = new DateTimeOffset(2026, 6, 7, 9, 0, 0, TimeSpan.Zero);
        var to = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var request = new GlucoseReadingsRequest(from, to, 12);

        Assert.Equal(from, request.From);
        Assert.Equal(to, request.To);
        Assert.Equal(12, request.Limit);
    }

    [Fact]
    public void ForLast_ShouldCreateRequestUsingDuration()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var request = GlucoseReadingsRequest.ForLast(TimeSpan.FromHours(3), now, 36);

        Assert.Equal(now.AddHours(-3), request.From);
        Assert.Equal(now, request.To);
        Assert.Equal(36, request.Limit);
    }

    [Fact]
    public void ForLast_ShouldRejectInvalidDuration()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => GlucoseReadingsRequest.ForLast(TimeSpan.Zero, now));

        Assert.Equal("duration", exception.ParamName);
    }
}