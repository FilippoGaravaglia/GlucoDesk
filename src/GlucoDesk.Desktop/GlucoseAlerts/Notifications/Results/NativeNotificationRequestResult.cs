namespace GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;

/// <summary>
/// Describes the result of a native operating-system notification request.
/// </summary>
/// <param name="Status">The request status.</param>
/// <param name="UserMessage">A short user-facing status message.</param>
/// <param name="DiagnosticMessage">An optional technical diagnostic message for logs or tests.</param>
public sealed record NativeNotificationRequestResult(
    NativeNotificationRequestStatus Status,
    string UserMessage,
    string? DiagnosticMessage = null)
{
    /// <summary>
    /// Gets whether the native notification request was accepted by the local backend.
    /// </summary>
    public bool WasRequested =>
        Status is NativeNotificationRequestStatus.Requested
            or NativeNotificationRequestStatus.UnknownDelivery;

    /// <summary>
    /// Creates a result for a native notification request accepted by the local backend.
    /// </summary>
    /// <returns>The request result.</returns>
    public static NativeNotificationRequestResult Requested()
    {
        return new NativeNotificationRequestResult(
            NativeNotificationRequestStatus.Requested,
            "Native notification requested.");
    }

    /// <summary>
    /// Creates a result for a native notification request whose visible delivery cannot be confirmed.
    /// </summary>
    /// <returns>The request result.</returns>
    public static NativeNotificationRequestResult UnknownDelivery()
    {
        return new NativeNotificationRequestResult(
            NativeNotificationRequestStatus.UnknownDelivery,
            "Native notification requested. Check your OS notification settings.");
    }

    /// <summary>
    /// Creates a result for an unsupported native notification environment.
    /// </summary>
    /// <param name="diagnosticMessage">The diagnostic message.</param>
    /// <returns>The request result.</returns>
    public static NativeNotificationRequestResult NotSupported(string diagnosticMessage)
    {
        return new NativeNotificationRequestResult(
            NativeNotificationRequestStatus.NotSupported,
            "Native notifications are not available on this platform.",
            diagnosticMessage);
    }

    /// <summary>
    /// Creates a result for a failed native notification request.
    /// </summary>
    /// <param name="diagnosticMessage">The diagnostic message.</param>
    /// <returns>The request result.</returns>
    public static NativeNotificationRequestResult Failed(string diagnosticMessage)
    {
        return new NativeNotificationRequestResult(
            NativeNotificationRequestStatus.Failed,
            "Native notification request failed.",
            diagnosticMessage);
    }
}
