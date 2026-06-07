using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Infrastructure.Cgm.Mock.Options;

namespace GlucoDesk.Infrastructure.Cgm.Mock.Generators;

internal sealed class MockGlucoseReadingGenerator
{
    private readonly MockCgmProviderOptions _options;

    public MockGlucoseReadingGenerator(MockCgmProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    public GlucoseReading Generate(DateTimeOffset timestamp, GlucoseDataFreshness freshness)
    {
        if (timestamp == default)
        {
            throw new ArgumentException("Timestamp must be specified.", nameof(timestamp));
        }

        var currentValue = CalculateValue(timestamp);
        var previousValue = CalculateValue(timestamp.Subtract(_options.ReadingInterval));
        var trend = CalculateTrend(currentValue, previousValue);

        return new GlucoseReading(
            timestamp,
            new GlucoseValue(currentValue, GlucoseUnit.MgDl),
            trend,
            CgmProviderKind.Mock,
            freshness,
            _options.DeviceName);
    }

    #region Helpers

    /// <summary>
    /// Calculates a deterministic glucose value for the supplied timestamp.
    /// </summary>
    /// <param name="timestamp">The reading timestamp.</param>
    /// <returns>The generated glucose value expressed in mg/dL.</returns>
    private int CalculateValue(DateTimeOffset timestamp)
    {
        var slot = timestamp.ToUnixTimeSeconds() / Math.Max(1, (long)_options.ReadingInterval.TotalSeconds);

        var slowWave = Math.Sin(slot / 8.0);
        var fastWave = Math.Sin(slot / 19.0);
        var combinedVariation = (slowWave * _options.Variation) + (fastWave * (_options.Variation / 2.0));

        var generatedValue = _options.BaseValue + combinedVariation;
        var roundedValue = (int)Math.Round(generatedValue, MidpointRounding.AwayFromZero);

        return Math.Clamp(roundedValue, _options.MinimumValue, _options.MaximumValue);
    }

    /// <summary>
    /// Calculates the trend direction by comparing the current and previous glucose values.
    /// </summary>
    /// <param name="currentValue">The current glucose value.</param>
    /// <param name="previousValue">The previous glucose value.</param>
    /// <returns>The calculated trend direction.</returns>
    private static TrendDirection CalculateTrend(int currentValue, int previousValue)
    {
        var delta = currentValue - previousValue;

        return delta switch
        {
            >= 6 => TrendDirection.DoubleUp,
            >= 3 => TrendDirection.SingleUp,
            >= 1 => TrendDirection.FortyFiveUp,
            <= -6 => TrendDirection.DoubleDown,
            <= -3 => TrendDirection.SingleDown,
            <= -1 => TrendDirection.FortyFiveDown,
            _ => TrendDirection.Flat
        };
    }

    #endregion
}