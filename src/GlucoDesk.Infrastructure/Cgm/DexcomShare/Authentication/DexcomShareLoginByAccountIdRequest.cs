using System.Text.Json.Serialization;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Authentication;

/// <summary>
/// Represents a Dexcom Share login request based on a publisher account identifier.
/// </summary>
public sealed record DexcomShareLoginByAccountIdRequest
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomShareLoginByAccountIdRequest"/> class.
    /// </summary>
    /// <param name="accountId">The Dexcom Share account identifier.</param>
    /// <param name="password">The Dexcom account password.</param>
    /// <param name="applicationId">The Dexcom Share application identifier.</param>
    public DexcomShareLoginByAccountIdRequest(
        string accountId,
        string password,
        string applicationId)
    {
        AccountId = accountId;
        Password = password;
        ApplicationId = applicationId;
    }

    /// <summary>
    /// Gets the Dexcom Share account identifier.
    /// </summary>
    [JsonPropertyName("accountId")]
    public string AccountId { get; }

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