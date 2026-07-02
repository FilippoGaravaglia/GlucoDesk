using GlucoDesk.Desktop.GlucoseAlerts.Models;
using GlucoDesk.Desktop.GlucoseAlerts.Services;

namespace GlucoDesk.Desktop.Tests.GlucoseAlerts.Services;

public sealed class GlucoseAlertNotificationTestServiceTests
{
    [Fact]
    public async Task SendTestNotificationAsync_ShouldSendPrivacySafeNotification()
    {
        var notificationService = new RecordingGlucoseAlertNotificationService();
        var service = new GlucoseAlertNotificationTestService(notificationService);

        var result = await service.SendTestNotificationAsync(CancellationToken.None);

        Assert.False(result.IsFailure);
        Assert.Single(notificationService.Notifications);
        Assert.Equal("GlucoDesk notification test", notificationService.Notifications[0].Title);
        Assert.DoesNotContain("mg/dL", notificationService.Notifications[0].Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("mmol", notificationService.Notifications[0].Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task SendTestNotificationAsync_ShouldReturnFailure_WhenNativeNotificationFails()
    {
        var notificationService = new FailingGlucoseAlertNotificationService();
        var service = new GlucoseAlertNotificationTestService(notificationService);

        var result = await service.SendTestNotificationAsync(CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("GlucoseAlerts.TestNotificationFailed", result.Error.Code);
    }

    private sealed class RecordingGlucoseAlertNotificationService : IGlucoseAlertNotificationService
    {
        public List<GlucoseAlertNativeNotification> Notifications { get; } = [];

        public Task ShowAsync(
            GlucoseAlertNativeNotification notification,
            CancellationToken cancellationToken)
        {
            Notifications.Add(notification);
            return Task.CompletedTask;
        }
    }

    private sealed class FailingGlucoseAlertNotificationService : IGlucoseAlertNotificationService
    {
        public Task ShowAsync(
            GlucoseAlertNativeNotification notification,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Native notification failure.");
        }
    }
}
