using GlucoDesk.Infrastructure.Cgm.Nightscout.Requests;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Nightscout.Requests;

public sealed class NightscoutEntriesRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest_WhenValuesAreValid()
    {
        var from = DateTimeOffset.Parse("2026-06-12T08:00:00Z");
        var to = DateTimeOffset.Parse("2026-06-12T09:00:00Z");

        var request = new NightscoutEntriesRequest(from, to, 12);

        Assert.Equal(from, request.From);
        Assert.Equal(to, request.To);
        Assert.Equal(12, request.Count);
    }

    [Fact]
    public void Constructor_ShouldRejectFromAfterTo()
    {
        var from = DateTimeOffset.Parse("2026-06-12T09:00:00Z");
        var to = DateTimeOffset.Parse("2026-06-12T08:00:00Z");

        var exception = Assert.Throws<ArgumentException>(
            () => new NightscoutEntriesRequest(from, to, 12));

        Assert.Equal("from", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidCount()
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new NightscoutEntriesRequest(
                DateTimeOffset.Parse("2026-06-12T08:00:00Z"),
                DateTimeOffset.Parse("2026-06-12T09:00:00Z"),
                0));

        Assert.Equal("count", exception.ParamName);
    }
}