using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.Readings.Results;

public sealed class LatestGlucoseReadingResultTests
{
    [Fact]
    public void Constructor_ShouldRejectDefaultRetrievedAt()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new LatestGlucoseReadingResult(null, default));

        Assert.Equal("retrievedAt", exception.ParamName);
    }

    [Fact]
    public void HasReading_ShouldReturnFalse_WhenReadingIsMissing()
    {
        var result = new LatestGlucoseReadingResult(
            null,
            DateTimeOffset.UtcNow);

        Assert.False(result.HasReading);
    }

    [Fact]
    public void HasReading_ShouldReturnTrue_WhenReadingIsAvailable()
    {
        var result = new LatestGlucoseReadingResult(
            CreateReading(),
            DateTimeOffset.UtcNow);

        Assert.True(result.HasReading);
    }

    #region Helpers

    /// <summary>
    /// Creates a valid glucose reading used by the tests.
    /// </summary>
    /// <returns>A valid glucose reading.</returns>
    private static GlucoseReading CreateReading()
    {
        return new GlucoseReading(
            DateTimeOffset.UtcNow,
            new GlucoseValue(120, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.Live);
    }

    #endregion
}