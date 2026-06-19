using GlucoDesk.Application.Cgm.WidgetState.Enums;
using GlucoDesk.Application.Cgm.WidgetState.Factories;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.WidgetState.Factories;

public sealed class GlucoseWidgetStateFactoryTests
{
    [Fact]
    public void FromReading_ShouldCreateInRangeState_WhenReadingIsInRange()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var reading = CreateReading(timestamp, 120m);

        // Act
        var state = GlucoseWidgetStateFactory.FromReading(
            reading,
            timestamp.AddMinutes(1),
            TimeSpan.FromMinutes(15));

        // Assert
        Assert.Equal(1, state.SchemaVersion);
        Assert.True(state.HasReading);
        Assert.Equal(timestamp, state.ReadingTimestamp);
        Assert.Equal(timestamp.AddMinutes(15), state.ExpiresAt);
        Assert.Equal(120m, state.GlucoseAmount);
        Assert.Equal(GlucoseUnit.MgDl, state.GlucoseUnit);
        Assert.Equal(TrendDirection.Flat, state.Trend);
        Assert.Equal(CgmProviderKind.DexcomShare, state.ProviderKind);
        Assert.Equal(GlucoseDataFreshness.NearRealTime, state.Freshness);
        Assert.Equal(WidgetGlucoseStatusLevel.InRange, state.StatusLevel);
        Assert.Equal("120", state.DisplayValue);
        Assert.Equal("mg/dL", state.UnitLabel);
        Assert.Equal("Flat", state.TrendLabel);
        Assert.Equal("Glucose in range", state.StatusMessage);
    }

    [Fact]
    public void FromReading_ShouldCreateLowState_WhenReadingIsBelowRange()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var reading = CreateReading(timestamp, 65m);

        // Act
        var state = GlucoseWidgetStateFactory.FromReading(
            reading,
            timestamp.AddMinutes(1),
            TimeSpan.FromMinutes(15));

        // Assert
        Assert.Equal(WidgetGlucoseStatusLevel.Low, state.StatusLevel);
        Assert.Equal("Glucose below range", state.StatusMessage);
    }

    [Fact]
    public void FromReading_ShouldCreateHighState_WhenReadingIsAboveRange()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var reading = CreateReading(timestamp, 190m);

        // Act
        var state = GlucoseWidgetStateFactory.FromReading(
            reading,
            timestamp.AddMinutes(1),
            TimeSpan.FromMinutes(15));

        // Assert
        Assert.Equal(WidgetGlucoseStatusLevel.High, state.StatusLevel);
        Assert.Equal("Glucose above range", state.StatusMessage);
    }

    [Fact]
    public void FromReading_ShouldCreateStaleState_WhenReadingExpired()
    {
        // Arrange
        var timestamp = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);
        var reading = CreateReading(timestamp, 120m);

        // Act
        var state = GlucoseWidgetStateFactory.FromReading(
            reading,
            timestamp.AddMinutes(20),
            TimeSpan.FromMinutes(15));

        // Assert
        Assert.Equal(WidgetGlucoseStatusLevel.Stale, state.StatusLevel);
        Assert.Equal("Glucose data is stale", state.StatusMessage);
    }

    [Fact]
    public void Unavailable_ShouldCreateUnavailableState()
    {
        // Arrange
        var generatedAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);

        // Act
        var state = GlucoseWidgetStateFactory.Unavailable(
            generatedAt,
            CgmProviderKind.DexcomShare,
            "Provider not connected");

        // Assert
        Assert.False(state.HasReading);
        Assert.Equal(WidgetGlucoseStatusLevel.Unavailable, state.StatusLevel);
        Assert.Equal("--", state.DisplayValue);
        Assert.Equal("Provider not connected", state.StatusMessage);
    }

    #region Helpers

    /// <summary>
    /// Creates a glucose reading for widget state tests.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <param name="valueMgDl">The glucose value in mg/dL.</param>
    /// <returns>The glucose reading.</returns>
    private static GlucoseReading CreateReading(
        DateTimeOffset timestamp,
        decimal valueMgDl)
    {
        return new GlucoseReading(
            timestamp,
            new GlucoseValue(valueMgDl, GlucoseUnit.MgDl),
            TrendDirection.Flat,
            CgmProviderKind.DexcomShare,
            GlucoseDataFreshness.NearRealTime);
    }

    #endregion
}