using System.Text.Json.Serialization;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Dtos;

/// <summary>
/// Represents the Dexcom EGV API response payload.
/// </summary>
public sealed record DexcomEgvResponseDto
{
    /// <summary>
    /// Gets the returned record type.
    /// </summary>
    [JsonPropertyName("recordType")]
    public string? RecordType { get; init; }

    /// <summary>
    /// Gets the returned record version.
    /// </summary>
    [JsonPropertyName("recordVersion")]
    public string? RecordVersion { get; init; }

    /// <summary>
    /// Gets the Dexcom user id for this client.
    /// </summary>
    [JsonPropertyName("userId")]
    public string? UserId { get; init; }

    /// <summary>
    /// Gets the returned EGV records.
    /// </summary>
    [JsonPropertyName("records")]
    public IReadOnlyList<DexcomEgvRecordDto>? Records { get; init; }
}