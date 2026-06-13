namespace GlucoDesk.Desktop.Bootstrap.Providers.Connection.Nightscout.Enums;

/// <summary>
/// Represents the Nightscout desktop connection diagnostic state.
/// </summary>
public enum NightscoutConnectionState
{
    Unknown = 0,
    NotConfigured = 1,
    Configured = 2,
    Connected = 3,
    Unauthorized = 4,
    Forbidden = 5,
    NotFound = 6,
    EmptyResponse = 7,
    InvalidResponse = 8,
    RequestTimeout = 9,
    NetworkError = 10,
    ServerUnavailable = 11,
    UnexpectedError = 12
}