using System.Text.Json.Serialization;

namespace GlucoDesk.Infrastructure.Cgm.Nightscout.Dtos;

/// <summary>
/// Represents a Nightscout glucose entry DTO.
/// </summary>
public sealed record NightscoutEntryDto
{
    /// <summary>
    /// Gets the Nightscout document identifier.
    /// </summary>
    [JsonPropertyName("_id")]
    public string? Id { get; init; }

    /// <summary>
    /// Gets the sensor glucose value in mg/dL.
    /// </summary>
    [JsonPropertyName("sgv")]
    public int? Sgv { get; init; }

    /// <summary>
    /// Gets the epoch timestamp in milliseconds.
    /// </summary>
    [JsonPropertyName("date")]
    public long? Date { get; init; }

    /// <summary>
    /// Gets the ISO timestamp.
    /// </summary>
    [JsonPropertyName("dateString")]
    public string? DateString { get; init; }

    /// <summary>
    /// Gets the Nightscout trend direction.
    /// </summary>
    [JsonPropertyName("direction")]
    public string? Direction { get; init; }

    /// <summary>
    /// Gets the Nightscout device name.
    /// </summary>
    [JsonPropertyName("device")]
    public string? Device { get; init; }

    /// <summary>
    /// Gets the Nightscout entry type.
    /// </summary>
    [JsonPropertyName("type")]
    public string? Type { get; init; }
}