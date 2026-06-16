using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Credentials;

/// <summary>
/// Represents persisted Dexcom Share credentials.
/// </summary>
public sealed record DexcomShareCredentials
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomShareCredentials"/> class.
    /// </summary>
    /// <param name="username">The Dexcom account username or email.</param>
    /// <param name="password">The Dexcom account password.</param>
    /// <param name="region">The Dexcom Share region.</param>
    public DexcomShareCredentials(
        string username,
        string password,
        DexcomShareRegion region)
    {
        Username = username.Trim();
        Password = password;
        Region = region;
    }

    /// <summary>
    /// Gets the Dexcom account username or email.
    /// </summary>
    public string Username { get; }

    /// <summary>
    /// Gets the Dexcom account password.
    /// </summary>
    public string Password { get; }

    /// <summary>
    /// Gets the Dexcom Share region.
    /// </summary>
    public DexcomShareRegion Region { get; }

    /// <summary>
    /// Gets a value indicating whether the credentials are complete.
    /// </summary>
    public bool IsConfigured =>
        !string.IsNullOrWhiteSpace(Username)
        && !string.IsNullOrWhiteSpace(Password)
        && Region is not DexcomShareRegion.Unknown;
}