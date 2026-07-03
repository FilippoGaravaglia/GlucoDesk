using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;

namespace GlucoDesk.Desktop.Tests.GlucoseAlerts.Notifications.Results;

public sealed class NativeNotificationRequestResultPresentationTests
{
    [Theory]
    [InlineData(NativeNotificationRequestStatus.Requested)]
    [InlineData(NativeNotificationRequestStatus.UnknownDelivery)]
    public void FromResult_WhenRequestWasAccepted_ShouldCreateSuccessPresentation(
        NativeNotificationRequestStatus status)
    {
        var result = new NativeNotificationRequestResult(
            status,
            "Native notification requested.");

        var presentation = NativeNotificationRequestResultPresentation.FromResult(result);

        Assert.False(presentation.IsFailure);
        Assert.Equal("Native test notification requested.", presentation.StatusMessage);
        Assert.Equal(
            "Test notification requested. Check your OS notification center.",
            presentation.NotificationStatusText);
        Assert.Null(presentation.ErrorMessage);
    }

    [Theory]
    [InlineData(NativeNotificationRequestStatus.Failed)]
    [InlineData(NativeNotificationRequestStatus.NotSupported)]
    public void FromResult_WhenRequestFailed_ShouldCreateFailurePresentation(
        NativeNotificationRequestStatus status)
    {
        var result = new NativeNotificationRequestResult(
            status,
            "Native notification request failed.",
            "Backend diagnostic.");

        var presentation = NativeNotificationRequestResultPresentation.FromResult(result);

        Assert.True(presentation.IsFailure);
        Assert.Equal("Unable to send native test notification", presentation.StatusMessage);
        Assert.Equal(
            "Unable to send the test notification. Check OS notification permissions.",
            presentation.NotificationStatusText);
        Assert.Equal("Native notification request failed.", presentation.ErrorMessage);
    }

    [Fact]
    public void FromResult_WhenResultIsNull_ShouldThrow()
    {
        Assert.Throws<ArgumentNullException>(
            () => NativeNotificationRequestResultPresentation.FromResult(null!));
    }
}
