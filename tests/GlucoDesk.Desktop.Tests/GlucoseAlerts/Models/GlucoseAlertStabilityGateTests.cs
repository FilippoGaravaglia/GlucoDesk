using GlucoDesk.Desktop.GlucoseAlerts.Models;

namespace GlucoDesk.Desktop.Tests.GlucoseAlerts.Models;

public sealed class GlucoseAlertStabilityGateTests
{
    [Fact]
    public void Constructor_ShouldRejectInvalidRequiredObservationCount()
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new GlucoseAlertStabilityGate(0));
    }

    [Fact]
    public void ShouldPresent_ShouldReturnFalse_ForFirstOutOfRangeObservation()
    {
        var gate = new GlucoseAlertStabilityGate();

        var shouldPresent = gate.ShouldPresent(GlucoseAlertKind.High);

        Assert.False(shouldPresent);
        Assert.Equal(GlucoseAlertKind.High, gate.CurrentKind);
        Assert.Equal(1, gate.ConsecutiveObservations);
    }

    [Fact]
    public void ShouldPresent_ShouldReturnTrue_ForSecondConsecutiveSameCondition()
    {
        var gate = new GlucoseAlertStabilityGate();

        _ = gate.ShouldPresent(GlucoseAlertKind.High);
        var shouldPresent = gate.ShouldPresent(GlucoseAlertKind.High);

        Assert.True(shouldPresent);
        Assert.Equal(GlucoseAlertKind.High, gate.CurrentKind);
        Assert.Equal(2, gate.ConsecutiveObservations);
    }

    [Fact]
    public void ShouldPresent_ShouldRestartCounting_WhenConditionChanges()
    {
        var gate = new GlucoseAlertStabilityGate();

        _ = gate.ShouldPresent(GlucoseAlertKind.High);
        var shouldPresent = gate.ShouldPresent(GlucoseAlertKind.Low);

        Assert.False(shouldPresent);
        Assert.Equal(GlucoseAlertKind.Low, gate.CurrentKind);
        Assert.Equal(1, gate.ConsecutiveObservations);
    }

    [Fact]
    public void ShouldPresent_ShouldReset_WhenReadingReturnsInRange()
    {
        var gate = new GlucoseAlertStabilityGate();

        _ = gate.ShouldPresent(GlucoseAlertKind.High);
        var shouldPresent = gate.ShouldPresent(GlucoseAlertKind.None);

        Assert.False(shouldPresent);
        Assert.Equal(GlucoseAlertKind.None, gate.CurrentKind);
        Assert.Equal(0, gate.ConsecutiveObservations);
    }

    [Fact]
    public void Reset_ShouldClearTrackedState()
    {
        var gate = new GlucoseAlertStabilityGate();

        _ = gate.ShouldPresent(GlucoseAlertKind.High);
        gate.Reset();

        Assert.Equal(GlucoseAlertKind.None, gate.CurrentKind);
        Assert.Equal(0, gate.ConsecutiveObservations);
    }
}
