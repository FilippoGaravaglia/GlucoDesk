using GlucoDesk.Desktop.GlucoseAlerts.Models;
using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;
using GlucoDesk.Desktop.GlucoseAlerts.Services;

namespace GlucoDesk.Desktop.Tests.GlucoseAlerts.Services;

public sealed class GlucoseAlertNotificationTestServiceTests
{
    [Xunit.Fact]
    public async Task SendTestNotificationAsync_WhenNativeNotificationSucceeds_ShouldReturnUnknownDeliveryResult()
    {
        var notificationService = new CapturingGlucoseAlertNotificationService();
        var service = new GlucoseAlertNotificationTestService(notificationService);

        var result = await service
            .SendTestNotificationAsync(CancellationToken.None);

        Xunit.Assert.Equal(NativeNotificationRequestStatus.UnknownDelivery, result.Status);
        Xunit.Assert.True(result.WasRequested);
        Xunit.Assert.Equal(
            "Native notification requested. Check your OS notification settings.",
            result.UserMessage);
        Xunit.Assert.Null(result.DiagnosticMessage);

        Xunit.Assert.NotNull(notificationService.LastNotification);
        Xunit.Assert.Equal("GlucoDesk notification test", notificationService.LastNotification.Title);
        Xunit.Assert.Equal(
            "Native glucose awareness notifications are enabled on this desktop.",
            notificationService.LastNotification.Message);
    }

    [Xunit.Fact]
    public async Task SendTestNotificationAsync_WhenNativeNotificationFails_ShouldReturnFailedResult()
    {
        var notificationService = new ThrowingGlucoseAlertNotificationService();
        var service = new GlucoseAlertNotificationTestService(notificationService);

        var result = await service
            .SendTestNotificationAsync(CancellationToken.None);

        Xunit.Assert.Equal(NativeNotificationRequestStatus.Failed, result.Status);
        Xunit.Assert.False(result.WasRequested);
        Xunit.Assert.Equal("Native notification request failed.", result.UserMessage);
        Xunit.Assert.Contains(
            "Unable to send the native test notification.",
            result.DiagnosticMessage);
        Xunit.Assert.Contains(
            nameof(InvalidOperationException),
            result.DiagnosticMessage);
    }

    [Xunit.Fact]
    public async Task SendTestNotificationAsync_WhenCancelled_ShouldRethrowCancellation()
    {
        using var cancellationTokenSource = new CancellationTokenSource();

        await cancellationTokenSource.CancelAsync();

        var notificationService = new CancellableGlucoseAlertNotificationService();
        var service = new GlucoseAlertNotificationTestService(notificationService);

        await Xunit.Assert.ThrowsAsync<OperationCanceledException>(
            () => service.SendTestNotificationAsync(cancellationTokenSource.Token));
    }

    private sealed class CapturingGlucoseAlertNotificationService : IGlucoseAlertNotificationService
    {
        public GlucoseAlertNativeNotification? LastNotification { get; private set; }

        public Task ShowAsync(
            GlucoseAlertNativeNotification notification,
            CancellationToken cancellationToken)
        {
            LastNotification = notification;

            return Task.CompletedTask;
        }
    }

    private sealed class ThrowingGlucoseAlertNotificationService : IGlucoseAlertNotificationService
    {
        public Task ShowAsync(
            GlucoseAlertNativeNotification notification,
            CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Native notification backend failed.");
        }
    }

    private sealed class CancellableGlucoseAlertNotificationService : IGlucoseAlertNotificationService
    {
        public Task ShowAsync(
            GlucoseAlertNativeNotification notification,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }
}
