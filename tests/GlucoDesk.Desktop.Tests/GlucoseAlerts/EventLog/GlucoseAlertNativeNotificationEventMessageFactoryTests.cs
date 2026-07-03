using GlucoDesk.Desktop.GlucoseAlerts.EventLog;
using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;

namespace GlucoDesk.Desktop.Tests.GlucoseAlerts.EventLog;

public sealed class GlucoseAlertNativeNotificationEventMessageFactoryTests
{
    [Fact]
    public void Create_WhenResultIsRequested_ShouldReturnRequestedMessage()
    {
        var result = NativeNotificationRequestResult.Requested();

        var message = GlucoseAlertNativeNotificationEventMessageFactory.Create(result);

        Assert.Equal("Native notification requested.", message);
    }

    [Fact]
    public void Create_WhenResultHasUnknownDelivery_ShouldReturnOsSettingsMessage()
    {
        var result = NativeNotificationRequestResult.UnknownDelivery();

        var message = GlucoseAlertNativeNotificationEventMessageFactory.Create(result);

        Assert.Equal(
            "Native notification requested. Delivery depends on OS notification settings.",
            message);
    }

    [Fact]
    public void Create_WhenResultFailedWithDiagnostic_ShouldReturnDiagnosticMessage()
    {
        var result = NativeNotificationRequestResult.Failed("Unable to request native notification.");

        var message = GlucoseAlertNativeNotificationEventMessageFactory.Create(result);

        Assert.Equal("Unable to request native notification.", message);
    }

    [Fact]
    public void Create_WhenResultFailedWithoutDiagnostic_ShouldReturnUserMessage()
    {
        var result = new NativeNotificationRequestResult(
            NativeNotificationRequestStatus.Failed,
            "Native notification request failed.");

        var message = GlucoseAlertNativeNotificationEventMessageFactory.Create(result);

        Assert.Equal("Native notification request failed.", message);
    }

    [Fact]
    public void Create_WhenResultIsNotSupported_ShouldReturnDiagnosticMessage()
    {
        var result = NativeNotificationRequestResult.NotSupported(
            "Native notifications are disabled.");

        var message = GlucoseAlertNativeNotificationEventMessageFactory.Create(result);

        Assert.Equal("Native notifications are disabled.", message);
    }

    [Fact]
    public void Create_WhenResultIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(
            () => GlucoseAlertNativeNotificationEventMessageFactory.Create(null!));
    }
}
