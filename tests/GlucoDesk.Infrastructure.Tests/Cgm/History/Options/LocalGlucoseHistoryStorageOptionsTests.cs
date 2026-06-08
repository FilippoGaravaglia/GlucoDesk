using GlucoDesk.Infrastructure.Cgm.History.Options;

namespace GlucoDesk.Infrastructure.Tests.Cgm.History.Options;

public sealed class LocalGlucoseHistoryStorageOptionsTests
{
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidHistoryFilePath(string historyFilePath)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new LocalGlucoseHistoryStorageOptions(historyFilePath));

        Assert.Equal("historyFilePath", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldTrimHistoryFilePath()
    {
        var options = new LocalGlucoseHistoryStorageOptions("  /tmp/glucodesk/history.json  ");

        Assert.Equal("/tmp/glucodesk/history.json", options.HistoryFilePath);
    }
}