using GlucoDesk.Application.Settings.Models;

namespace GlucoDesk.Application.Tests.Settings.Models;

public sealed class ApplicationSettingsGlucoseAlertStabilityTests
{
    [Fact]
    public void Default_ShouldUseBalancedGlucoseAlertStability()
    {
        var settings = ApplicationSettings.Default;

        Assert.Equal(
            ApplicationSettings.DefaultGlucoseAlertRequiredConsecutiveReadings,
            settings.GlucoseAlertRequiredConsecutiveReadings);
    }

    [Theory]
    [InlineData(1)]
    [InlineData(2)]
    [InlineData(5)]
    public void Constructor_ShouldAcceptValidGlucoseAlertStabilityValues(int requiredConsecutiveReadings)
    {
        var settings = new ApplicationSettings(
            glucoseAlertRequiredConsecutiveReadings: requiredConsecutiveReadings);

        Assert.Equal(
            requiredConsecutiveReadings,
            settings.GlucoseAlertRequiredConsecutiveReadings);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(6)]
    public void Constructor_ShouldRejectInvalidGlucoseAlertStabilityValues(int requiredConsecutiveReadings)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new ApplicationSettings(
            glucoseAlertRequiredConsecutiveReadings: requiredConsecutiveReadings));
    }
}
