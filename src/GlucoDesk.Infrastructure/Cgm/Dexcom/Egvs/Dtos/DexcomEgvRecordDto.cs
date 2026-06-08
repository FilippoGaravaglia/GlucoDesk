using System.Text.Json.Serialization;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Dtos;

/// <summary>
/// Represents a single Dexcom EGV API record.
/// </summary>
public sealed record DexcomEgvRecordDto
{
    /// <summary>
    /// Gets the unique EGV record id for the given client.
    /// </summary>
    [JsonPropertyName("recordId")]
    public string? RecordId { get; init; }

    /// <summary>
    /// Gets the recorded system time as returned by Dexcom.
    /// </summary>
    [JsonPropertyName("systemTime")]
    public string? SystemTime { get; init; }

    /// <summary>
    /// Gets the recorded display time as returned by Dexcom.
    /// </summary>
    [JsonPropertyName("displayTime")]
    public string? DisplayTime { get; init; }

    /// <summary>
    /// Gets the hashed and encrypted transmitter id.
    /// </summary>
    [JsonPropertyName("transmitterId")]
    public string? TransmitterId { get; init; }

    /// <summary>
    /// Gets the transmitter tick count.
    /// </summary>
    [JsonPropertyName("transmitterTicks")]
    public long? TransmitterTicks { get; init; }

    /// <summary>
    /// Gets the estimated glucose value.
    /// </summary>
    [JsonPropertyName("value")]
    public int? Value { get; init; }

    /// <summary>
    /// Gets the EGV status when the value is outside the measuring range.
    /// </summary>
    [JsonPropertyName("status")]
    public string? Status { get; init; }

    /// <summary>
    /// Gets the EGV trend.
    /// </summary>
    [JsonPropertyName("trend")]
    public string? Trend { get; init; }

    /// <summary>
    /// Gets the EGV trend rate.
    /// </summary>
    [JsonPropertyName("trendRate")]
    public double? TrendRate { get; init; }

    /// <summary>
    /// Gets the glucose value unit.
    /// </summary>
    [JsonPropertyName("unit")]
    public string? Unit { get; init; }

    /// <summary>
    /// Gets the trend rate unit.
    /// </summary>
    [JsonPropertyName("rateUnit")]
    public string? RateUnit { get; init; }

    /// <summary>
    /// Gets the display device.
    /// </summary>
    [JsonPropertyName("displayDevice")]
    public string? DisplayDevice { get; init; }

    /// <summary>
    /// Gets the transmitter generation.
    /// </summary>
    [JsonPropertyName("transmitterGeneration")]
    public string? TransmitterGeneration { get; init; }

    /// <summary>
    /// Gets the display application.
    /// </summary>
    [JsonPropertyName("displayApp")]
    public string? DisplayApp { get; init; }
}