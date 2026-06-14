using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Tests.Cgm.History.Results;

public sealed class GlucoseHistorySaveResultTests
{
    [Fact]
    public void Constructor_ShouldCreateResult_WhenCountsAreValid()
    {
        var result = new GlucoseHistorySaveResult(
            CgmProviderKind.Nightscout,
            10,
            4,
            6,
            20);

        Assert.Equal(CgmProviderKind.Nightscout, result.ProviderKind);
        Assert.Equal(10, result.IncomingReadingsCount);
        Assert.Equal(4, result.AddedReadingsCount);
        Assert.Equal(6, result.DuplicateReadingsCount);
        Assert.Equal(20, result.StoredReadingsCount);
        Assert.True(result.HasNewReadings);
    }

    [Fact]
    public void Constructor_ShouldExposeNoNewReadings_WhenAddedCountIsZero()
    {
        var result = new GlucoseHistorySaveResult(
            CgmProviderKind.Nightscout,
            10,
            0,
            10,
            20);

        Assert.False(result.HasNewReadings);
    }

    [Theory]
    [InlineData(-1, 0, 0, 0, "incomingReadingsCount")]
    [InlineData(0, -1, 0, 0, "addedReadingsCount")]
    [InlineData(0, 0, -1, 0, "duplicateReadingsCount")]
    [InlineData(0, 0, 0, -1, "storedReadingsCount")]
    public void Constructor_ShouldRejectNegativeCounts(
        int incomingReadingsCount,
        int addedReadingsCount,
        int duplicateReadingsCount,
        int storedReadingsCount,
        string expectedParameterName)
    {
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GlucoseHistorySaveResult(
                CgmProviderKind.Nightscout,
                incomingReadingsCount,
                addedReadingsCount,
                duplicateReadingsCount,
                storedReadingsCount));

        Assert.Equal(expectedParameterName, exception.ParamName);
    }
}