using System.Text.Json.Serialization;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Authentication;

/// <summary>
/// Represents a Dexcom Share authentication request.
/// </summary>
public sealed record DexcomShareAuthenticationRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomShareAuthenticationRequest"/> class.
    /// </summary>
    /// <param name="accountName">The Dexcom account name.</param>
    /// <param name="password">The Dexcom account password.</param>
    /// <param name="applicationId">The Dexcom Share application identifier.</param>
    public DexcomShareAuthenticationRequest(
        string accountName,
        string password,
        string applicationId)
    {
        AccountName = accountName;
        Password = password;
        ApplicationId = applicationId;
    }

    /// <summary>
    /// Gets the Dexcom account name.
    /// </summary>
    [JsonPropertyName("accountName")]
    public string AccountName { get; }

    /// <summary>
    /// Gets the Dexcom account password.
    /// </summary>
    [JsonPropertyName("password")]
    public string Password { get; }

    /// <summary>
    /// Gets the Dexcom Share application identifier.
    /// </summary>
    [JsonPropertyName("applicationId")]
    public string ApplicationId { get; }
}