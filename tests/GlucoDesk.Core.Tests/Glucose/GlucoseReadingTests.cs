using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Core.Tests.Glucose;

public sealed class GlucoseReadingTests
{
    [Fact]
    public void Constructor_ShouldRejectDefaultTimestamp()
    {
        var value = new GlucoseValue(120, GlucoseUnit.MgDl);

        var exception = Assert.Throws<ArgumentException>(
            () => new GlucoseReading(
                default,
                value,
                TrendDirection.Flat,
                CgmProviderKind.Mock,
                GlucoseDataFreshness.Live));

        Assert.Equal("timestamp", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldNormalizeBlankDeviceToNull()
    {
        var reading = new GlucoseReading(
            DateTimeOffset.UtcNow,
            new GlucoseValue(120, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.Live,
            "   ");

        Assert.Null(reading.Device);
    }

    [Fact]
    public void Constructor_ShouldTrimDeviceName()
    {
        var reading = new GlucoseReading(
            DateTimeOffset.UtcNow,
            new GlucoseValue(120, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.Live,
            "  Dexcom G7  ");

        Assert.Equal("Dexcom G7", reading.Device);
    }

    [Theory]
    [InlineData(65, GlucoseStatus.Low)]
    [InlineData(120, GlucoseStatus.InRange)]
    [InlineData(230, GlucoseStatus.High)]
    public void GetStatus_ShouldClassifyReading(decimal amount, GlucoseStatus expectedStatus)
    {
        var reading = new GlucoseReading(
            DateTimeOffset.UtcNow,
            new GlucoseValue(amount, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.Live);

        var status = reading.GetStatus(GlucoseRange.StandardMgDl);

        Assert.Equal(expectedStatus, status);
    }

    [Fact]
    public void IsStale_ShouldReturnTrue_WhenReadingIsOlderThanMaximumAge()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var reading = new GlucoseReading(
            now.AddMinutes(-20),
            new GlucoseValue(120, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.Live);

        var result = reading.IsStale(now, TimeSpan.FromMinutes(15));

        Assert.True(result);
    }

    [Fact]
    public void IsStale_ShouldReturnFalse_WhenReadingIsWithinMaximumAge()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var reading = new GlucoseReading(
            now.AddMinutes(-10),
            new GlucoseValue(120, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.Live);

        var result = reading.IsStale(now, TimeSpan.FromMinutes(15));

        Assert.False(result);
    }

    [Fact]
    public void IsStale_ShouldRejectInvalidMaximumAge()
    {
        var reading = new GlucoseReading(
            DateTimeOffset.UtcNow,
            new GlucoseValue(120, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.Live);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => reading.IsStale(DateTimeOffset.UtcNow, TimeSpan.Zero));

        Assert.Equal("maxAge", exception.ParamName);
    }
}