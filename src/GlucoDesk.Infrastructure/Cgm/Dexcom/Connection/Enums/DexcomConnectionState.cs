namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Enums;

/// <summary>
/// Represents the current Dexcom connection state.
/// </summary>
public enum DexcomConnectionState
{
    /// <summary>
    /// The connection state is unknown.
    /// </summary>
    Unknown = 0,

    /// <summary>
    /// The Dexcom provider is not registered in the current runtime.
    /// </summary>
    ProviderNotRegistered = 1,

    /// <summary>
    /// The Dexcom provider is registered, but no OAuth token set is available.
    /// </summary>
    TokenMissing = 2,

    /// <summary>
    /// A valid Dexcom access token is currently available.
    /// </summary>
    Connected = 3,

    /// <summary>
    /// The access token is expired or close to expiration, but a refresh can be attempted.
    /// </summary>
    AccessTokenRefreshRequired = 4,

    /// <summary>
    /// The refresh token is expired and the user must authorize Dexcom again.
    /// </summary>
    RefreshTokenExpired = 5,

    /// <summary>
    /// The token store could not be inspected.
    /// </summary>
    TokenStoreUnavailable = 6
}