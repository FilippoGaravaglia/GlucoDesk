using GlucoDesk.Application.Cgm.Dashboard.Results;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.Dashboard.Results;

public sealed class GlucoseDashboardSnapshotTests
{
    [Fact]
    public void Constructor_ShouldOrderRecentReadings()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var snapshot = new GlucoseDashboardSnapshot(
            CreateMetadata(),
            CreateReading(now.AddMinutes(-5)),
            [
                CreateReading(now.AddMinutes(-5)),
                CreateReading(now.AddMinutes(-15)),
                CreateReading(now.AddMinutes(-10))
            ],
            now,
            now,
            now,
            TimeSpan.FromMinutes(15));

        var timestamps = snapshot.RecentReadings
            .Select(reading => reading.Timestamp)
            .ToArray();

        Assert.Equal(
            [
                now.AddMinutes(-15),
                now.AddMinutes(-10),
                now.AddMinutes(-5)
            ],
            timestamps);
    }

    [Fact]
    public void HasLatestReading_ShouldReturnFalse_WhenLatestReadingIsMissing()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var snapshot = new GlucoseDashboardSnapshot(
            CreateMetadata(),
            null,
            [],
            now,
            now,
            now,
            TimeSpan.FromMinutes(15));

        Assert.False(snapshot.HasLatestReading);
    }

    [Fact]
    public void IsLatestReadingStale_ShouldReturnTrue_WhenLatestReadingIsMissing()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var snapshot = new GlucoseDashboardSnapshot(
            CreateMetadata(),
            null,
            [],
            now,
            now,
            now,
            TimeSpan.FromMinutes(15));

        Assert.True(snapshot.IsLatestReadingStale);
    }

    [Fact]
    public void IsLatestReadingStale_ShouldReturnTrue_WhenLatestReadingIsOlderThanThreshold()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var snapshot = new GlucoseDashboardSnapshot(
            CreateMetadata(),
            CreateReading(now.AddMinutes(-20)),
            [],
            now,
            now,
            now,
            TimeSpan.FromMinutes(15));

        Assert.True(snapshot.IsLatestReadingStale);
    }

    [Fact]
    public void IsLatestReadingStale_ShouldReturnFalse_WhenLatestReadingIsInsideThreshold()
    {
        var now = new DateTimeOffset(2026, 6, 7, 10, 0, 0, TimeSpan.Zero);

        var snapshot = new GlucoseDashboardSnapshot(
            CreateMetadata(),
            CreateReading(now.AddMinutes(-10)),
            [],
            now,
            now,
            now,
            TimeSpan.FromMinutes(15));

        Assert.False(snapshot.IsLatestReadingStale);
    }

    #region Helpers

    /// <summary>
    /// Creates provider metadata used by snapshot tests.
    /// </summary>
    /// <returns>The provider metadata.</returns>
    private static CgmProviderMetadata CreateMetadata()
    {
        return new CgmProviderMetadata(
            CgmProviderKind.Mock,
            "Mock",
            GlucoseDataFreshness.NearRealTime,
            supportsLiveReadings: true,
            supportsHistoricalReadings: true);
    }

    /// <summary>
    /// Creates a glucose reading for the supplied timestamp.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(DateTimeOffset timestamp)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(120, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.Mock,
            GlucoseDataFreshness.NearRealTime);
    }

    #endregion
}