namespace GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;

/// <summary>
/// Represents the outcome of a native operating-system notification request.
/// </summary>
public enum NativeNotificationRequestStatus
{
    /// <summary>
    /// The notification request was accepted by the local notification backend.
    /// </summary>
    Requested,

    /// <summary>
    /// Native notifications are not supported by the current platform or runtime mode.
    /// </summary>
    NotSupported,

    /// <summary>
    /// The notification request failed before it could be handed to the operating system.
    /// </summary>
    Failed,

    /// <summary>
    /// The notification request was made, but the operating system cannot confirm visible delivery.
    /// </summary>
    UnknownDelivery
}
