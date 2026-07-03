using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Results;

namespace GlucoDesk.Desktop.Tests.GlucoseAlerts.Notifications.Results;

public sealed class NativeNotificationRequestResultTests
{
    [Theory]
    [InlineData(NativeNotificationRequestStatus.Requested, true)]
    [InlineData(NativeNotificationRequestStatus.UnknownDelivery, true)]
    [InlineData(NativeNotificationRequestStatus.NotSupported, false)]
    [InlineData(NativeNotificationRequestStatus.Failed, false)]
    public void WasRequested_ShouldReflectWhetherBackendAcceptedRequest(
        NativeNotificationRequestStatus status,
        bool expected)
    {
        var result = new NativeNotificationRequestResult(
            status,
            "Test message.");

        Assert.Equal(expected, result.WasRequested);
    }

    [Fact]
    public void Requested_ShouldCreateRequestedResult()
    {
        var result = NativeNotificationRequestResult.Requested();

        Assert.Equal(NativeNotificationRequestStatus.Requested, result.Status);
        Assert.True(result.WasRequested);
        Assert.Equal("Native notification requested.", result.UserMessage);
        Assert.Null(result.DiagnosticMessage);
    }

    [Fact]
    public void UnknownDelivery_ShouldCreateUnknownDeliveryResult()
    {
        var result = NativeNotificationRequestResult.UnknownDelivery();

        Assert.Equal(NativeNotificationRequestStatus.UnknownDelivery, result.Status);
        Assert.True(result.WasRequested);
        Assert.Contains("Check your OS notification settings", result.UserMessage);
        Assert.Null(result.DiagnosticMessage);
    }

    [Fact]
    public void NotSupported_ShouldCreateNotSupportedResult()
    {
        var result = NativeNotificationRequestResult.NotSupported("Linux backend is not configured.");

        Assert.Equal(NativeNotificationRequestStatus.NotSupported, result.Status);
        Assert.False(result.WasRequested);
        Assert.Equal("Native notifications are not available on this platform.", result.UserMessage);
        Assert.Equal("Linux backend is not configured.", result.DiagnosticMessage);
    }

    [Fact]
    public void Failed_ShouldCreateFailedResult()
    {
        var result = NativeNotificationRequestResult.Failed("Process exit code 1.");

        Assert.Equal(NativeNotificationRequestStatus.Failed, result.Status);
        Assert.False(result.WasRequested);
        Assert.Equal("Native notification request failed.", result.UserMessage);
        Assert.Equal("Process exit code 1.", result.DiagnosticMessage);
    }
}
