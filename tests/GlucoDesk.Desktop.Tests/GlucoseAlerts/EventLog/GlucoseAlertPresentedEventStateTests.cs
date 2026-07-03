using GlucoDesk.Desktop.GlucoseAlerts.EventLog;
using GlucoDesk.Desktop.GlucoseAlerts.Models;

namespace GlucoDesk.Desktop.Tests.GlucoseAlerts.EventLog;

public sealed class GlucoseAlertPresentedEventStateTests
{
    [Fact]
    public void ShouldLogPresented_ShouldReturnTrue_ForFirstAlertKind()
    {
        var state = new GlucoseAlertPresentedEventState();

        var shouldLog = state.ShouldLogPresented(GlucoseAlertKind.High);

        Assert.True(shouldLog);
        Assert.Equal(GlucoseAlertKind.High, state.LastLoggedPresentedKind);
    }

    [Fact]
    public void ShouldLogPresented_ShouldReturnFalse_ForRepeatedSameAlertKind()
    {
        var state = new GlucoseAlertPresentedEventState();

        _ = state.ShouldLogPresented(GlucoseAlertKind.High);
        var shouldLog = state.ShouldLogPresented(GlucoseAlertKind.High);

        Assert.False(shouldLog);
        Assert.Equal(GlucoseAlertKind.High, state.LastLoggedPresentedKind);
    }

    [Fact]
    public void ShouldLogPresented_ShouldReturnTrue_WhenAlertKindChanges()
    {
        var state = new GlucoseAlertPresentedEventState();

        _ = state.ShouldLogPresented(GlucoseAlertKind.High);
        var shouldLog = state.ShouldLogPresented(GlucoseAlertKind.Low);

        Assert.True(shouldLog);
        Assert.Equal(GlucoseAlertKind.Low, state.LastLoggedPresentedKind);
    }

    [Fact]
    public void ShouldLogPresented_ShouldReset_WhenAlertKindIsNone()
    {
        var state = new GlucoseAlertPresentedEventState();

        _ = state.ShouldLogPresented(GlucoseAlertKind.High);
        var shouldLog = state.ShouldLogPresented(GlucoseAlertKind.None);

        Assert.False(shouldLog);
        Assert.Equal(GlucoseAlertKind.None, state.LastLoggedPresentedKind);
    }

    [Fact]
    public void Reset_ShouldAllowSameAlertKindToBeLoggedAgain()
    {
        var state = new GlucoseAlertPresentedEventState();

        _ = state.ShouldLogPresented(GlucoseAlertKind.High);
        state.Reset();

        var shouldLog = state.ShouldLogPresented(GlucoseAlertKind.High);

        Assert.True(shouldLog);
        Assert.Equal(GlucoseAlertKind.High, state.LastLoggedPresentedKind);
    }
}
