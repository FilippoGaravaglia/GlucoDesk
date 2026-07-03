namespace GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;

/// <summary>
/// Represents the settings UI presentation for a native notification request result.
/// </summary>
/// <param name="IsFailure">Whether the result should be presented as a failure.</param>
/// <param name="StatusMessage">The main settings status message.</param>
/// <param name="NotificationStatusText">The native notification status text.</param>
/// <param name="ErrorMessage">The optional settings error message.</param>
public sealed record NativeNotificationRequestResultPresentation(
    bool IsFailure,
    string StatusMessage,
    string NotificationStatusText,
    string? ErrorMessage)
{
    private const string RequestedStatusMessage = "Native test notification requested.";

    private const string RequestedNotificationStatusText =
        "Test notification requested. Check your OS notification center.";

    private const string FailureStatusMessage = "Unable to send native test notification";

    private const string FailureNotificationStatusText =
        "Unable to send the test notification. Check OS notification permissions.";

    /// <summary>
    /// Creates the settings UI presentation for a native notification request result.
    /// </summary>
    /// <param name="result">The native notification request result.</param>
    /// <returns>The settings UI presentation.</returns>
    public static NativeNotificationRequestResultPresentation FromResult(
        NativeNotificationRequestResult result)
    {
        ArgumentNullException.ThrowIfNull(result);

        return result.Status switch
        {
            NativeNotificationRequestStatus.Failed
                or NativeNotificationRequestStatus.NotSupported
                => new NativeNotificationRequestResultPresentation(
                    true,
                    FailureStatusMessage,
                    FailureNotificationStatusText,
                    result.UserMessage),
            _ => new NativeNotificationRequestResultPresentation(
                false,
                RequestedStatusMessage,
                RequestedNotificationStatusText,
                null),
        };
    }
}
