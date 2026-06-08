using GlucoDesk.Application.Cgm.History.Requests;

namespace GlucoDesk.Application.Tests.Cgm.History.Requests;

public sealed class GlucoseHistoryRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest_WhenRangeIsValid()
    {
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);
        var to = from.AddHours(1);

        var request = new GlucoseHistoryRequest(from, to);

        Assert.Equal(from, request.From);
        Assert.Equal(to, request.To);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidRange()
    {
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GlucoseHistoryRequest(from, from));

        Assert.Equal("to", exception.ParamName);
    }
}