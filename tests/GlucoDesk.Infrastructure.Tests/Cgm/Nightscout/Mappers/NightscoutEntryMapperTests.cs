using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Dtos;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Mappers;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Nightscout.Mappers;

public sealed class NightscoutEntryMapperTests
{
    [Fact]
    public void MapEntry_ShouldMapValidEntry()
    {
        var mapper = new NightscoutEntryMapper();

        var result = mapper.MapEntry(new NightscoutEntryDto
        {
            Sgv = 123,
            DateString = "2026-06-12T08:00:00.000Z",
            Direction = "Flat",
            Device = "xDrip"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(123m, result.Value.Value.Amount);
        Assert.Equal(GlucoseUnit.MgDl, result.Value.Value.Unit);
        Assert.Equal(DateTimeOffset.Parse("2026-06-12T08:00:00Z"), result.Value.Timestamp);
        Assert.Equal(TrendDirection.Flat, result.Value.Trend);
        Assert.Equal(CgmProviderKind.Nightscout, result.Value.Provider);
        Assert.Equal(GlucoseDataFreshness.NearRealTime, result.Value.Freshness);
    }

    [Fact]
    public void MapEntry_ShouldUseEpochMilliseconds_WhenDateStringIsMissing()
    {
        var mapper = new NightscoutEntryMapper();

        var result = mapper.MapEntry(new NightscoutEntryDto
        {
            Sgv = 123,
            Date = DateTimeOffset.Parse("2026-06-12T08:00:00Z").ToUnixTimeMilliseconds(),
            Direction = "SingleUp"
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(DateTimeOffset.Parse("2026-06-12T08:00:00Z"), result.Value.Timestamp);
        Assert.Equal(TrendDirection.SingleUp, result.Value.Trend);
    }

    [Theory]
    [InlineData("DoubleUp", TrendDirection.DoubleUp)]
    [InlineData("SingleUp", TrendDirection.SingleUp)]
    [InlineData("FortyFiveUp", TrendDirection.FortyFiveUp)]
    [InlineData("Flat", TrendDirection.Flat)]
    [InlineData("FortyFiveDown", TrendDirection.FortyFiveDown)]
    [InlineData("SingleDown", TrendDirection.SingleDown)]
    [InlineData("DoubleDown", TrendDirection.DoubleDown)]
    [InlineData("NOT COMPUTABLE", TrendDirection.NotComputable)]
    [InlineData("RATE OUT OF RANGE", TrendDirection.RateOutOfRange)]
    [InlineData("SomethingElse", TrendDirection.Unknown)]
    public void MapEntry_ShouldMapTrendDirections(string direction, TrendDirection expectedTrend)
    {
        var mapper = new NightscoutEntryMapper();

        var result = mapper.MapEntry(new NightscoutEntryDto
        {
            Sgv = 123,
            DateString = "2026-06-12T08:00:00.000Z",
            Direction = direction
        });

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedTrend, result.Value.Trend);
    }

    [Fact]
    public void MapEntry_ShouldReturnFailure_WhenSgvIsMissing()
    {
        var mapper = new NightscoutEntryMapper();

        var result = mapper.MapEntry(new NightscoutEntryDto
        {
            DateString = "2026-06-12T08:00:00.000Z"
        });

        Assert.True(result.IsFailure);
        Assert.Equal("Nightscout.EntryMissingSgv", result.Error.Code);
    }

    [Fact]
    public void MapEntry_ShouldReturnFailure_WhenTimestampIsMissing()
    {
        var mapper = new NightscoutEntryMapper();

        var result = mapper.MapEntry(new NightscoutEntryDto
        {
            Sgv = 123
        });

        Assert.True(result.IsFailure);
        Assert.Equal("Nightscout.EntryMissingTimestamp", result.Error.Code);
    }

    [Fact]
    public void MapEntries_ShouldReturnOrderedReadings()
    {
        var mapper = new NightscoutEntryMapper();

        var result = mapper.MapEntries(
        [
            new NightscoutEntryDto
            {
                Sgv = 125,
                DateString = "2026-06-12T08:05:00.000Z"
            },
            new NightscoutEntryDto
            {
                Sgv = 120,
                DateString = "2026-06-12T08:00:00.000Z"
            }
        ]);

        Assert.True(result.IsSuccess);
        Assert.Collection(
            result.Value,
            first => Assert.Equal(120m, first.Value.Amount),
            second => Assert.Equal(125m, second.Value.Amount));
    }
}