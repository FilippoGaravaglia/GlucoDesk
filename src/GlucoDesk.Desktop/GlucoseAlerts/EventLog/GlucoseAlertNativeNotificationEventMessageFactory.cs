using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;

namespace GlucoDesk.Desktop.GlucoseAlerts.EventLog;

/// <summary>
/// Creates privacy-safe event log messages for native glucose notification request results.
/// </summary>
public static class GlucoseAlertNativeNotificationEventMessageFactory
{
    private const string RequestedMessage = "Native notification requested.";

    private const string UnknownDeliveryMessage =
        "Native notification requested. Delivery depends on OS notification settings.";

    /// <summary>
    /// Creates a privacy-safe event log message for a native notification request result.
    /// </summary>
    /// <param name="result">The native notification request result.</param>
    /// <returns>The privacy-safe event log message.</returns>
    public static string Create(NativeNotificationRequestResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Status switch
        {
            NativeNotificationRequestStatus.Requested => RequestedMessage,
            NativeNotificationRequestStatus.UnknownDelivery => UnknownDeliveryMessage,
            NativeNotificationRequestStatus.Failed => result.DiagnosticMessage ?? result.UserMessage,
            NativeNotificationRequestStatus.NotSupported => result.DiagnosticMessage ?? result.UserMessage,
            _ => result.UserMessage,
        };
    }
}
