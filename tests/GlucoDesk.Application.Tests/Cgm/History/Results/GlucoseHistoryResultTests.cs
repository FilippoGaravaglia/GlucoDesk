using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.History.Results;

public sealed class GlucoseHistoryResultTests
{
    [Fact]
    public void Constructor_ShouldSortReadingsByTimestamp()
    {
        var firstTimestamp = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);
        var secondTimestamp = firstTimestamp.AddMinutes(5);

        var result = new GlucoseHistoryResult(
            [
                CreateReading(secondTimestamp),
                CreateReading(firstTimestamp)
            ]);

        Assert.Equal(2, result.Count);
        Assert.Equal(firstTimestamp, result.Readings.First().Timestamp);
        Assert.Equal(secondTimestamp, result.Readings.Last().Timestamp);
    }

    #region Helpers

    /// <summary>
    /// Creates a glucose reading for the supplied timestamp.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(DateTimeOffset timestamp)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(110, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.NearRealTime);
    }

    #endregion
}