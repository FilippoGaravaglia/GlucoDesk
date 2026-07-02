using GlucoDesk.Desktop.GlucoseAlerts.Models;

namespace GlucoDesk.Desktop.Tests.GlucoseAlerts.Models;

public sealed class GlucoseAlertSnoozeStateTests
{
    private static readonly DateTimeOffset Now = new(2026, 7, 2, 12, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Snooze_ShouldTrackSelectedAlertKind()
    {
        var state = new GlucoseAlertSnoozeState();

        var snoozedUntil = state.Snooze(
            GlucoseAlertKind.High,
            TimeSpan.FromMinutes(30),
            Now);

        Assert.Equal(GlucoseAlertKind.High, state.SnoozedKind);
        Assert.Equal(Now.AddMinutes(30), snoozedUntil);
        Assert.Equal(Now.AddMinutes(30), state.SnoozedUntil);
    }

    [Fact]
    public void IsSnoozed_ShouldReturnTrue_ForSameKindBeforeExpiration()
    {
        var state = new GlucoseAlertSnoozeState();

        state.Snooze(
            GlucoseAlertKind.Low,
            TimeSpan.FromMinutes(30),
            Now);

        Assert.True(state.IsSnoozed(GlucoseAlertKind.Low, Now.AddMinutes(10)));
    }

    [Fact]
    public void IsSnoozed_ShouldReturnFalse_ForDifferentAlertKind()
    {
        var state = new GlucoseAlertSnoozeState();

        state.Snooze(
            GlucoseAlertKind.High,
            TimeSpan.FromMinutes(30),
            Now);

        Assert.False(state.IsSnoozed(GlucoseAlertKind.Low, Now.AddMinutes(10)));
    }

    [Fact]
    public void IsSnoozed_ShouldExpireAutomatically()
    {
        var state = new GlucoseAlertSnoozeState();

        state.Snooze(
            GlucoseAlertKind.High,
            TimeSpan.FromMinutes(30),
            Now);

        Assert.False(state.IsSnoozed(GlucoseAlertKind.High, Now.AddMinutes(30)));

        Assert.Equal(GlucoseAlertKind.None, state.SnoozedKind);
        Assert.Null(state.SnoozedUntil);
    }

    [Fact]
    public void GetRemaining_ShouldReturnRemainingDuration()
    {
        var state = new GlucoseAlertSnoozeState();

        state.Snooze(
            GlucoseAlertKind.High,
            TimeSpan.FromMinutes(30),
            Now);

        var remaining = state.GetRemaining(
            GlucoseAlertKind.High,
            Now.AddMinutes(12));

        Assert.Equal(TimeSpan.FromMinutes(18), remaining);
    }

    [Fact]
    public void Snooze_ShouldClearState_WhenKindIsNone()
    {
        var state = new GlucoseAlertSnoozeState();

        state.Snooze(
            GlucoseAlertKind.High,
            TimeSpan.FromMinutes(30),
            Now);

        state.Snooze(
            GlucoseAlertKind.None,
            TimeSpan.FromMinutes(30),
            Now.AddMinutes(5));

        Assert.Equal(GlucoseAlertKind.None, state.SnoozedKind);
        Assert.Null(state.SnoozedUntil);
    }

    [Fact]
    public void Snooze_ShouldRejectNonPositiveDuration()
    {
        var state = new GlucoseAlertSnoozeState();

        Assert.Throws<ArgumentOutOfRangeException>(() => state.Snooze(
            GlucoseAlertKind.High,
            TimeSpan.Zero,
            Now));
    }
}
