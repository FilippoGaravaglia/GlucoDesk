using System.Text.Json;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Dtos;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Nightscout.Dtos;

public sealed class NightscoutEntryDtoTests
{
    [Fact]
    public void Deserialize_ShouldReadNightscoutEntry()
    {
        const string json = """
        {
          "_id": "entry-id",
          "sgv": 123,
          "date": 1781251200000,
          "dateString": "2026-06-12T08:00:00.000Z",
          "direction": "Flat",
          "device": "xDrip",
          "type": "sgv"
        }
        """;

        var entry = JsonSerializer.Deserialize<NightscoutEntryDto>(
            json,
            new JsonSerializerOptions(JsonSerializerDefaults.Web));

        Assert.NotNull(entry);
        Assert.Equal("entry-id", entry.Id);
        Assert.Equal(123, entry.Sgv);
        Assert.Equal(1781251200000, entry.Date);
        Assert.Equal("2026-06-12T08:00:00.000Z", entry.DateString);
        Assert.Equal("Flat", entry.Direction);
        Assert.Equal("xDrip", entry.Device);
        Assert.Equal("sgv", entry.Type);
    }
}