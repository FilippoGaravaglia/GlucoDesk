using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.Readings.Results;

public sealed class GlucoseReadingsResultTests
{
    [Fact]
    public void Constructor_ShouldRejectNullReadings()
    {
        var exception = Assert.Throws<ArgumentNullException>(
            () => new GlucoseReadingsResult(null!, DateTimeOffset.UtcNow));

        Assert.Equal("readings", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectDefaultRetrievedAt()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new GlucoseReadingsResult([], default));

        Assert.Equal("retrievedAt", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldOrderReadingsByTimestamp()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var result = new GlucoseReadingsResult(
            [
                CreateReading(now.AddMinutes(-5)),
                CreateReading(now.AddMinutes(-15)),
                CreateReading(now.AddMinutes(-10))
            ],
            now);

        var orderedTimestamps = result.Readings
            .Select(reading => reading.Timestamp)
            .ToArray();

        Assert.Equal(
            [
                now.AddMinutes(-15),
                now.AddMinutes(-10),
                now.AddMinutes(-5)
            ],
            orderedTimestamps);
    }

    [Fact]
    public void HasReadings_ShouldReturnFalse_WhenCollectionIsEmpty()
    {
        var result = new GlucoseReadingsResult([], DateTimeOffset.UtcNow);

        Assert.False(result.HasReadings);
    }

    [Fact]
    public void HasReadings_ShouldReturnTrue_WhenCollectionContainsReadings()
    {
        var result = new GlucoseReadingsResult(
            [CreateReading(DateTimeOffset.UtcNow)],
            DateTimeOffset.UtcNow);

        Assert.True(result.HasReadings);
    }

    #region Helpers

    /// <summary>
    /// Creates a glucose reading for the supplied timestamp.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <returns>A glucose reading.</returns>
    private static GlucoseReading CreateReading(DateTimeOffset timestamp)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(120, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.Live);
    }

    #endregion
}