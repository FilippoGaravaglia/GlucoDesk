using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.GlucoseAlerts.Models;
using GlucoDesk.Desktop.GlucoseAlerts.Services;

namespace GlucoDesk.Desktop.Tests.GlucoseAlerts.Services;

public sealed class GlucoseAlertCoordinatorTests
{
    [Fact]
    public void Evaluate_ShouldReturnNone_WhenAlertsAreDisabled()
    {
        var coordinator = CreateCoordinator(out _);
        var settings = new ApplicationSettings(glucoseAlertsEnabled: false);

        var presentation = coordinator.Evaluate(60m, settings, GlucoseUnit.MgDl);

        Assert.Equal(GlucoseAlertKind.None, presentation.Kind);
        Assert.False(presentation.ShouldSendNativeNotification);
    }

    [Fact]
    public void Evaluate_ShouldUsePrivacyMessage_WhenPrivacyModeIsEnabled()
    {
        var coordinator = CreateCoordinator(out _);
        var settings = new ApplicationSettings(
            nativeGlucoseNotificationsEnabled: true,
            glucoseAlertPrivacyModeEnabled: true);

        var presentation = coordinator.Evaluate(60m, settings, GlucoseUnit.MgDl);

        Assert.Equal(GlucoseAlertKind.Low, presentation.Kind);
        Assert.Contains("below your configured target range", presentation.Message, StringComparison.OrdinalIgnoreCase);
        Assert.DoesNotContain("60", presentation.Message, StringComparison.OrdinalIgnoreCase);
        Assert.True(presentation.ShouldSendNativeNotification);
    }

    [Fact]
    public void Evaluate_ShouldIncludeValue_WhenPrivacyModeIsDisabled()
    {
        var coordinator = CreateCoordinator(out _);
        var settings = new ApplicationSettings(
            nativeGlucoseNotificationsEnabled: true,
            glucoseAlertPrivacyModeEnabled: false);

        var presentation = coordinator.Evaluate(60m, settings, GlucoseUnit.MgDl);

        Assert.Equal(GlucoseAlertKind.Low, presentation.Kind);
        Assert.Contains("60 mg/dL", presentation.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("70-180 mg/dL", presentation.Message, StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void Evaluate_ShouldSuppressRepeatedNativeNotificationInsideCooldown()
    {
        var coordinator = CreateCoordinator(out var clock);
        var settings = new ApplicationSettings(
            nativeGlucoseNotificationsEnabled: true,
            glucoseAlertRepeatInterval: TimeSpan.FromMinutes(30));

        var first = coordinator.Evaluate(60m, settings, GlucoseUnit.MgDl);

        clock.Now = clock.Now.AddMinutes(10);
        var second = coordinator.Evaluate(61m, settings, GlucoseUnit.MgDl);

        Assert.True(first.ShouldSendNativeNotification);
        Assert.False(second.ShouldSendNativeNotification);
    }

    [Fact]
    public void Evaluate_ShouldAllowRepeatedNativeNotificationAfterCooldown()
    {
        var coordinator = CreateCoordinator(out var clock);
        var settings = new ApplicationSettings(
            nativeGlucoseNotificationsEnabled: true,
            glucoseAlertRepeatInterval: TimeSpan.FromMinutes(30));

        var first = coordinator.Evaluate(60m, settings, GlucoseUnit.MgDl);

        clock.Now = clock.Now.AddMinutes(31);
        var second = coordinator.Evaluate(61m, settings, GlucoseUnit.MgDl);

        Assert.True(first.ShouldSendNativeNotification);
        Assert.True(second.ShouldSendNativeNotification);
    }

    [Fact]
    public void Evaluate_ShouldNotifyImmediately_WhenAlertKindChanges()
    {
        var coordinator = CreateCoordinator(out var clock);
        var settings = new ApplicationSettings(
            nativeGlucoseNotificationsEnabled: true,
            glucoseAlertRepeatInterval: TimeSpan.FromMinutes(30));

        var low = coordinator.Evaluate(60m, settings, GlucoseUnit.MgDl);

        clock.Now = clock.Now.AddMinutes(5);
        var high = coordinator.Evaluate(220m, settings, GlucoseUnit.MgDl);

        Assert.True(low.ShouldSendNativeNotification);
        Assert.True(high.ShouldSendNativeNotification);
        Assert.Equal(GlucoseAlertKind.High, high.Kind);
    }

    [Fact]
    public async Task SendNativeNotificationAsync_ShouldDelegateToNativeService()
    {
        var service = new RecordingGlucoseAlertNotificationService();
        var clock = new FakeGlucoseAlertClock();
        var coordinator = new GlucoseAlertCoordinator(service, clock);
        var settings = new ApplicationSettings(nativeGlucoseNotificationsEnabled: true);
        var presentation = coordinator.Evaluate(60m, settings, GlucoseUnit.MgDl);

        await coordinator.SendNativeNotificationAsync(presentation, CancellationToken.None);

        Assert.Single(service.Notifications);
        Assert.Equal("Glucose below target", service.Notifications[0].Title);
    }

    private static GlucoseAlertCoordinator CreateCoordinator(out FakeGlucoseAlertClock clock)
    {
        clock = new FakeGlucoseAlertClock();

        return new GlucoseAlertCoordinator(
            new RecordingGlucoseAlertNotificationService(),
            clock);
    }

    private sealed class FakeGlucoseAlertClock : IGlucoseAlertClock
    {
        public DateTimeOffset Now { get; set; } = new(2026, 07, 02, 12, 0, 0, TimeSpan.Zero);
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
}
