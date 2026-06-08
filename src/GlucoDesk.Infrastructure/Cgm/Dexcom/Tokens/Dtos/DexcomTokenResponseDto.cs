using System.Text.Json.Serialization;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Dtos;

/// <summary>
/// Represents the Dexcom OAuth token response payload.
/// </summary>
internal sealed record DexcomTokenResponseDto
{
    /// <summary>
    /// Gets the OAuth access token.
    /// </summary>
    [JsonPropertyName("access_token")]
    public string? AccessToken { get; init; }

    /// <summary>
    /// Gets the access token lifetime in seconds.
    /// </summary>
    [JsonPropertyName("expires_in")]
    public int ExpiresIn { get; init; }

    /// <summary>
    /// Gets the OAuth token type.
    /// </summary>
    [JsonPropertyName("token_type")]
    public string? TokenType { get; init; }

    /// <summary>
    /// Gets the OAuth refresh token.
    /// </summary>
    [JsonPropertyName("refresh_token")]
    public string? RefreshToken { get; init; }

    /// <summary>
    /// Gets the refresh token lifetime in seconds, when provided.
    /// </summary>
    [JsonPropertyName("refresh_expires_in")]
    public int RefreshExpiresIn { get; init; }
}