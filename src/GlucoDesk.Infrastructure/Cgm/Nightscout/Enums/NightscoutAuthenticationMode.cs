namespace GlucoDesk.Infrastructure.Cgm.Nightscout.Enums;

/// <summary>
/// Defines the supported Nightscout authentication strategies.
/// </summary>
public enum NightscoutAuthenticationMode
{
    /// <summary>
    /// No authentication is applied.
    /// </summary>
    None = 0,

    /// <summary>
    /// Uses a SHA1-hashed Nightscout API secret in the api-secret header.
    /// </summary>
    ApiSecretSha1Header = 1,

    /// <summary>
    /// Uses a Nightscout access token in the query string.
    /// </summary>
    AccessTokenQueryString = 2
}