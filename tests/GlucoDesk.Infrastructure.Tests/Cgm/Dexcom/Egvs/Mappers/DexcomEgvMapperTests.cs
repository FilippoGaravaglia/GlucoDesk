using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Dtos;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Mappers;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Egvs.Mappers;

public sealed class DexcomEgvMapperTests
{
    [Fact]
    public void MapRecord_ShouldMapValidSandboxRecord()
    {
        var mapper = new DexcomEgvMapper();

        var result = mapper.MapRecord(
            CreateRecord(),
            DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsSuccess);
        Assert.Equal(new DateTimeOffset(2025, 1, 30, 23, 49, 55, TimeSpan.Zero), result.Value.Timestamp);
        Assert.Equal(101, result.Value.Value.Amount);
        Assert.Equal(GlucoseUnit.MgDl, result.Value.Value.Unit);
        Assert.Equal(TrendDirection.Flat, result.Value.Trend);
        Assert.Equal(CgmProviderKind.DexcomSandbox, result.Value.Provider);
        Assert.Equal(GlucoseDataFreshness.Delayed, result.Value.Freshness);
        Assert.Equal("iOS / G7", result.Value.Device);
    }

    [Theory]
    [InlineData(DexcomApiEnvironment.ProductionUs)]
    [InlineData(DexcomApiEnvironment.ProductionEu)]
    [InlineData(DexcomApiEnvironment.ProductionJapan)]
    public void MapRecord_ShouldMapProductionEnvironmentsToDexcomOfficial(
        DexcomApiEnvironment environment)
    {
        var mapper = new DexcomEgvMapper();

        var result = mapper.MapRecord(CreateRecord(), environment);

        Assert.True(result.IsSuccess);
        Assert.Equal(CgmProviderKind.DexcomOfficial, result.Value.Provider);
    }

    [Theory]
    [InlineData("doubleUp", TrendDirection.DoubleUp)]
    [InlineData("singleUp", TrendDirection.SingleUp)]
    [InlineData("fortyFiveUp", TrendDirection.FortyFiveUp)]
    [InlineData("flat", TrendDirection.Flat)]
    [InlineData("fortyFiveDown", TrendDirection.FortyFiveDown)]
    [InlineData("singleDown", TrendDirection.SingleDown)]
    [InlineData("doubleDown", TrendDirection.DoubleDown)]
    [InlineData("notComputable", TrendDirection.NotComputable)]
    [InlineData("rateOutOfRange", TrendDirection.RateOutOfRange)]
    [InlineData("unknown-trend", TrendDirection.Unknown)]
    [InlineData("", TrendDirection.Unknown)]
    [InlineData(null, TrendDirection.Unknown)]
    public void MapRecord_ShouldMapTrendValues(
        string? trend,
        TrendDirection expectedTrend)
    {
        var mapper = new DexcomEgvMapper();
        var record = CreateRecord() with { Trend = trend };

        var result = mapper.MapRecord(record, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedTrend, result.Value.Trend);
    }

    [Theory]
    [InlineData("mg/dL", GlucoseUnit.MgDl)]
    [InlineData("mgdl", GlucoseUnit.MgDl)]
    [InlineData("mmol/L", GlucoseUnit.MmolL)]
    [InlineData("mmoll", GlucoseUnit.MmolL)]
    public void MapRecord_ShouldMapSupportedUnits(
        string unit,
        GlucoseUnit expectedUnit)
    {
        var mapper = new DexcomEgvMapper();
        var record = CreateRecord() with { Unit = unit };

        var result = mapper.MapRecord(record, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsSuccess);
        Assert.Equal(expectedUnit, result.Value.Value.Unit);
    }

    [Fact]
    public void MapRecord_ShouldUseTransmitterGenerationAsFallbackDevice()
    {
        var mapper = new DexcomEgvMapper();

        var record = CreateRecord() with
        {
            DisplayDevice = null,
            DisplayApp = null,
            TransmitterGeneration = "g7"
        };

        var result = mapper.MapRecord(record, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsSuccess);
        Assert.Equal("g7", result.Value.Device);
    }

    [Fact]
    public void MapRecord_ShouldReturnNullDevice_WhenDeviceFieldsAreMissing()
    {
        var mapper = new DexcomEgvMapper();

        var record = CreateRecord() with
        {
            DisplayDevice = null,
            DisplayApp = null,
            TransmitterGeneration = null
        };

        var result = mapper.MapRecord(record, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsSuccess);
        Assert.Null(result.Value.Device);
    }

    [Fact]
    public void MapRecord_ShouldReturnFailure_WhenRecordIsNull()
    {
        var mapper = new DexcomEgvMapper();

        var result = mapper.MapRecord(null!, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvRecordNull", result.Error.Code);
    }

    [Fact]
    public void MapRecord_ShouldReturnFailure_WhenSystemTimeIsMissing()
    {
        var mapper = new DexcomEgvMapper();
        var record = CreateRecord() with { SystemTime = null };

        var result = mapper.MapRecord(record, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvMissingSystemTime", result.Error.Code);
    }

    [Fact]
    public void MapRecord_ShouldReturnFailure_WhenSystemTimeIsInvalid()
    {
        var mapper = new DexcomEgvMapper();
        var record = CreateRecord() with { SystemTime = "invalid-date" };

        var result = mapper.MapRecord(record, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvInvalidSystemTime", result.Error.Code);
    }

    [Fact]
    public void MapRecord_ShouldReturnFailure_WhenValueIsMissing()
    {
        var mapper = new DexcomEgvMapper();
        var record = CreateRecord() with { Value = null };

        var result = mapper.MapRecord(record, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvMissingValue", result.Error.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void MapRecord_ShouldReturnFailure_WhenValueIsInvalid(int value)
    {
        var mapper = new DexcomEgvMapper();
        var record = CreateRecord() with { Value = value };

        var result = mapper.MapRecord(record, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvInvalidValue", result.Error.Code);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("unsupported")]
    public void MapRecord_ShouldReturnFailure_WhenUnitIsUnsupported(string? unit)
    {
        var mapper = new DexcomEgvMapper();
        var record = CreateRecord() with { Unit = unit };

        var result = mapper.MapRecord(record, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvUnsupportedUnit", result.Error.Code);
    }

    [Fact]
    public void MapResponse_ShouldMapAllRecords()
    {
        var mapper = new DexcomEgvMapper();

        var response = new DexcomEgvResponseDto
        {
            RecordType = "egv",
            RecordVersion = "3.0",
            UserId = "user-id",
            Records =
            [
                CreateRecord() with { RecordId = "record-1", Value = 101 },
                CreateRecord() with { RecordId = "record-2", Value = 110 }
            ]
        };

        var result = mapper.MapResponse(response, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsSuccess);
        Assert.Equal(2, result.Value.Count);
        Assert.Equal(101, result.Value[0].Value.Amount);
        Assert.Equal(110, result.Value[1].Value.Amount);
    }

    [Fact]
    public void MapResponse_ShouldReturnFailure_WhenResponseIsNull()
    {
        var mapper = new DexcomEgvMapper();

        var result = mapper.MapResponse(null!, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvResponseNull", result.Error.Code);
    }

    [Fact]
    public void MapResponse_ShouldReturnFailure_WhenRecordsAreMissing()
    {
        var mapper = new DexcomEgvMapper();

        var response = new DexcomEgvResponseDto
        {
            RecordType = "egv",
            RecordVersion = "3.0",
            UserId = "user-id",
            Records = null
        };

        var result = mapper.MapResponse(response, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvRecordsMissing", result.Error.Code);
    }

    [Fact]
    public void MapResponse_ShouldReturnFailure_WhenAnyRecordIsInvalid()
    {
        var mapper = new DexcomEgvMapper();

        var response = new DexcomEgvResponseDto
        {
            RecordType = "egv",
            RecordVersion = "3.0",
            UserId = "user-id",
            Records =
            [
                CreateRecord() with { RecordId = "valid-record", Value = 101 },
                CreateRecord() with { RecordId = "invalid-record", Value = null }
            ]
        };

        var result = mapper.MapResponse(response, DexcomApiEnvironment.Sandbox);

        Assert.True(result.IsFailure);
        Assert.Equal("Dexcom.EgvMissingValue", result.Error.Code);
    }

    #region Helpers

    /// <summary>
    /// Creates a valid Dexcom EGV record DTO for tests.
    /// </summary>
    /// <returns>The Dexcom EGV record DTO.</returns>
    private static DexcomEgvRecordDto CreateRecord()
    {
        return new DexcomEgvRecordDto
        {
            RecordId = "record-id",
            SystemTime = "2025-01-30T23:49:55Z",
            DisplayTime = "2025-01-30T15:49:55-08:00",
            TransmitterId = "transmitter-id",
            TransmitterTicks = 85273,
            Value = 101,
            Status = null,
            Trend = "flat",
            TrendRate = 0,
            Unit = "mg/dL",
            RateUnit = "mg/dL/min",
            DisplayDevice = "iOS",
            TransmitterGeneration = "g7",
            DisplayApp = "G7"
        };
    }

    #endregion
}