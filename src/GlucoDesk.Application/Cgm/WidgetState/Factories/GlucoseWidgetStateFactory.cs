using System.Globalization;
using GlucoDesk.Application.Cgm.WidgetState.Enums;
using GlucoDesk.Application.Cgm.WidgetState.Snapshots;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;

namespace GlucoDesk.Application.Cgm.WidgetState.Factories;

/// <summary>
/// Creates widget state snapshots from glucose readings.
/// </summary>
public static class GlucoseWidgetStateFactory
{
    private const int CurrentSchemaVersion = 1;
    private const decimal LowThresholdMgDl = 70m;
    private const decimal HighThresholdMgDl = 180m;

    /// <summary>
    /// Creates a widget state from the latest glucose reading.
    /// </summary>
    /// <param name="reading">The latest glucose reading.</param>
    /// <param name="generatedAt">The timestamp at which the state is generated.</param>
    /// <param name="staleAfter">The duration after which the reading should be considered stale.</param>
    /// <returns>The glucose widget state.</returns>
    public static GlucoseWidgetState FromReading(
        GlucoseReading reading,
        DateTimeOffset generatedAt,
        TimeSpan staleAfter)
    {
        ArgumentNullException.ThrowIfNull(reading);

        if (staleAfter <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(staleAfter),
                staleAfter,
                "Stale duration must be greater than zero.");
        }

        var expiresAt = reading.Timestamp.Add(staleAfter);
        var statusLevel = ResolveStatusLevel(reading, generatedAt, expiresAt);

        return new GlucoseWidgetState(
            CurrentSchemaVersion,
            generatedAt,
            reading.Timestamp,
            expiresAt,
            reading.Value.Amount,
            reading.Value.Unit,
            reading.Trend,
            reading.Provider,
            reading.Freshness,
            statusLevel,
            FormatDisplayValue(reading),
            FormatUnitLabel(reading.Value.Unit),
            FormatTrendLabel(reading.Trend),
            FormatStatusMessage(statusLevel));
    }

    /// <summary>
    /// Creates an unavailable widget state.
    /// </summary>
    /// <param name="generatedAt">The timestamp at which the state is generated.</param>
    /// <param name="providerKind">The provider kind.</param>
    /// <param name="statusMessage">The status message.</param>
    /// <returns>The unavailable widget state.</returns>
    public static GlucoseWidgetState Unavailable(
        DateTimeOffset generatedAt,
        CgmProviderKind providerKind,
        string statusMessage)
    {
        if (string.IsNullOrWhiteSpace(statusMessage))
        {
            throw new ArgumentException(
                "Status message cannot be empty.",
                nameof(statusMessage));
        }

        return new GlucoseWidgetState(
            CurrentSchemaVersion,
            generatedAt,
            null,
            null,
            null,
            null,
            null,
            providerKind,
            GlucoseDataFreshness.Unknown,
            WidgetGlucoseStatusLevel.Unavailable,
            "--",
            "mg/dL",
            "Unknown",
            statusMessage);
    }

    #region Helpers

    /// <summary>
    /// Resolves the widget glucose status level.
    /// </summary>
    /// <param name="reading">The glucose reading.</param>
    /// <param name="generatedAt">The timestamp at which the state is generated.</param>
    /// <param name="expiresAt">The state expiration timestamp.</param>
    /// <returns>The widget glucose status level.</returns>
    private static WidgetGlucoseStatusLevel ResolveStatusLevel(
        GlucoseReading reading,
        DateTimeOffset generatedAt,
        DateTimeOffset expiresAt)
    {
        if (generatedAt > expiresAt)
        {
            return WidgetGlucoseStatusLevel.Stale;
        }

        if (reading.Value.Unit != GlucoseUnit.MgDl)
        {
            return WidgetGlucoseStatusLevel.Unknown;
        }

        if (reading.Value.Amount < LowThresholdMgDl)
        {
            return WidgetGlucoseStatusLevel.Low;
        }

        if (reading.Value.Amount > HighThresholdMgDl)
        {
            return WidgetGlucoseStatusLevel.High;
        }

        return WidgetGlucoseStatusLevel.InRange;
    }

    /// <summary>
    /// Formats the glucose value for widget display.
    /// </summary>
    /// <param name="reading">The glucose reading.</param>
    /// <returns>The display-ready glucose value.</returns>
    private static string FormatDisplayValue(GlucoseReading reading)
    {
        return reading.Value.Amount.ToString("0", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats the glucose unit for widget display.
    /// </summary>
    /// <param name="unit">The glucose unit.</param>
    /// <returns>The display-ready unit label.</returns>
    private static string FormatUnitLabel(GlucoseUnit unit)
    {
        return unit switch
        {
            GlucoseUnit.MgDl => "mg/dL",
            _ => unit.ToString()
        };
    }

    /// <summary>
    /// Formats the trend direction for widget display.
    /// </summary>
    /// <param name="trend">The trend direction.</param>
    /// <returns>The display-ready trend label.</returns>
    private static string FormatTrendLabel(TrendDirection trend)
    {
        return trend.ToString();
    }

    /// <summary>
    /// Formats the status message for widget display.
    /// </summary>
    /// <param name="statusLevel">The widget glucose status level.</param>
    /// <returns>The display-ready status message.</returns>
    private static string FormatStatusMessage(WidgetGlucoseStatusLevel statusLevel)
    {
        return statusLevel switch
        {
            WidgetGlucoseStatusLevel.InRange => "Glucose in range",
            WidgetGlucoseStatusLevel.Low => "Glucose below range",
            WidgetGlucoseStatusLevel.High => "Glucose above range",
            WidgetGlucoseStatusLevel.Stale => "Glucose data is stale",
            WidgetGlucoseStatusLevel.Unavailable => "Glucose unavailable",
            _ => "Glucose status unknown"
        };
    }

    #endregion
}