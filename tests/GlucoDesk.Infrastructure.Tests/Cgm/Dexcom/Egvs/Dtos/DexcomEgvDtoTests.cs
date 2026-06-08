using System.Text.Json;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Dtos;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Egvs.Dtos;

public sealed class DexcomEgvDtoTests
{
    [Fact]
    public void DexcomEgvResponseDto_ShouldDeserializeDexcomPayload()
    {
        const string json = """
        {
          "recordType": "egv",
          "recordVersion": "3.0",
          "userId": "user-id",
          "records": [
            {
              "recordId": "record-id",
              "systemTime": "2025-01-30T23:49:55Z",
              "displayTime": "2025-01-30T15:49:55-08:00",
              "transmitterId": "transmitter-id",
              "transmitterTicks": 85273,
              "value": 101,
              "status": null,
              "trend": "flat",
              "trendRate": 0,
              "unit": "mg/dL",
              "rateUnit": "mg/dL/min",
              "displayDevice": "iOS",
              "transmitterGeneration": "g7",
              "displayApp": "G7"
            }
          ]
        }
        """;

        var response = JsonSerializer.Deserialize<DexcomEgvResponseDto>(
            json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(response);
        Assert.Equal("egv", response.RecordType);
        Assert.Equal("3.0", response.RecordVersion);
        Assert.Equal("user-id", response.UserId);

        var record = Assert.Single(response.Records!);
        Assert.Equal("record-id", record.RecordId);
        Assert.Equal("2025-01-30T23:49:55Z", record.SystemTime);
        Assert.Equal("2025-01-30T15:49:55-08:00", record.DisplayTime);
        Assert.Equal("transmitter-id", record.TransmitterId);
        Assert.Equal(85273, record.TransmitterTicks);
        Assert.Equal(101, record.Value);
        Assert.Null(record.Status);
        Assert.Equal("flat", record.Trend);
        Assert.Equal(0, record.TrendRate);
        Assert.Equal("mg/dL", record.Unit);
        Assert.Equal("mg/dL/min", record.RateUnit);
        Assert.Equal("iOS", record.DisplayDevice);
        Assert.Equal("g7", record.TransmitterGeneration);
        Assert.Equal("G7", record.DisplayApp);
    }
}