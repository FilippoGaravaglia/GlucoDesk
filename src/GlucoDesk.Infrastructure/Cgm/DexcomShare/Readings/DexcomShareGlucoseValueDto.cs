using System.Text.Json;
using System.Text.Json.Serialization;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Readings;

/// <summary>
/// Represents a Dexcom Share glucose value response item.
/// </summary>
public sealed record DexcomShareGlucoseValueDto
{
    /// <summary>
    /// Gets the glucose value expressed in mg/dL.
    /// </summary>
    [JsonPropertyName("Value")]
    public int Value { get; init; }

    /// <summary>
    /// Gets the Dexcom trend payload. Dexcom Share may return this as either a number or a string.
    /// </summary>
    [JsonPropertyName("Trend")]
    public JsonElement Trend { get; init; }

    /// <summary>
    /// Gets the system timestamp encoded as a Dexcom date string.
    /// </summary>
    [JsonPropertyName("ST")]
    public string? SystemTime { get; init; }

    /// <summary>
    /// Gets the display timestamp encoded as a Dexcom date string.
    /// </summary>
    [JsonPropertyName("DT")]
    public string? DisplayTime { get; init; }

    /// <summary>
    /// Gets the wall-clock timestamp encoded as a Dexcom date string.
    /// </summary>
    [JsonPropertyName("WT")]
    public string? WallTime { get; init; }
}