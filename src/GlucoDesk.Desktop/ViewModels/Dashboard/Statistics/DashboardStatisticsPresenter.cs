using System.Globalization;
using GlucoDesk.Application.Cgm.Statistics.Requests;
using GlucoDesk.Application.Cgm.Statistics.Results;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Desktop.ViewModels.Dashboard.Statistics;

/// <summary>
/// Builds user-facing dashboard statistics presentation values.
/// </summary>
public static class DashboardStatisticsPresenter
{
    /// <summary>
    /// Creates a dashboard statistics presentation for a successful statistics result.
    /// </summary>
    /// <param name="result">The glucose statistics result.</param>
    /// <param name="targetRange">The target range used by statistics.</param>
    /// <returns>The dashboard statistics presentation.</returns>
    public static DashboardStatisticsPresentation Present(
        GlucoseStatisticsResult result,
        GlucoseStatisticsTargetRange targetRange)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(targetRange);

        var targetRangeText = BuildTargetRangeText(targetRange);

        if (!result.HasData)
        {
            return new DashboardStatisticsPresentation(
                "No statistics available for the selected dashboard period.",
                "—",
                "—",
                "—",
                "—",
                BuildReadingsText(result),
                targetRangeText,
                false);
        }

        return new DashboardStatisticsPresentation(
            $"Statistics calculated from {result.AnalyzedReadingsCount} reading(s).",
            $"{FormatDecimal(result.AverageGlucose)} {FormatUnit(result.Unit)}",
            $"{FormatDecimal(result.InRangePercentage)}%",
            $"{FormatDecimal(result.BelowRangePercentage)}%",
            $"{FormatDecimal(result.AboveRangePercentage)}%",
            BuildReadingsText(result),
            targetRangeText,
            true);
    }

    /// <summary>
    /// Creates a dashboard statistics presentation for disabled statistics.
    /// </summary>
    /// <returns>The dashboard statistics presentation.</returns>
    public static DashboardStatisticsPresentation Disabled()
    {
        return new DashboardStatisticsPresentation(
            "Statistics are not available in the current desktop runtime.",
            "—",
            "—",
            "—",
            "—",
            "—",
            "Target range: —",
            false);
    }

    /// <summary>
    /// Creates a dashboard statistics presentation for failed statistics calculation.
    /// </summary>
    /// <param name="errorCode">The statistics error code.</param>
    /// <returns>The dashboard statistics presentation.</returns>
    public static DashboardStatisticsPresentation Failed(string errorCode)
    {
        return new DashboardStatisticsPresentation(
            $"Statistics update failed · {errorCode}",
            "—",
            "—",
            "—",
            "—",
            "—",
            "Target range: —",
            false);
    }

    #region Helpers

    /// <summary>
    /// Builds the analyzed readings text.
    /// </summary>
    /// <param name="result">The glucose statistics result.</param>
    /// <returns>The analyzed readings text.</returns>
    private static string BuildReadingsText(GlucoseStatisticsResult result)
    {
        if (result.LoadedReadingsCount == result.AnalyzedReadingsCount)
        {
            return $"{result.AnalyzedReadingsCount} analyzed";
        }

        return $"{result.AnalyzedReadingsCount} analyzed / {result.LoadedReadingsCount} loaded";
    }

    /// <summary>
    /// Builds the target range text.
    /// </summary>
    /// <param name="targetRange">The target range.</param>
    /// <returns>The target range text.</returns>
    private static string BuildTargetRangeText(GlucoseStatisticsTargetRange targetRange)
    {
        return $"Target range: {FormatDecimal(targetRange.Low)}–{FormatDecimal(targetRange.High)} {FormatUnit(targetRange.Unit)}";
    }

    /// <summary>
    /// Formats a nullable decimal value for dashboard statistics.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The formatted value.</returns>
    private static string FormatDecimal(decimal? value)
    {
        return value is null
            ? "—"
            : FormatDecimal(value.Value);
    }

    /// <summary>
    /// Formats a decimal value for dashboard statistics.
    /// </summary>
    /// <param name="value">The value.</param>
    /// <returns>The formatted value.</returns>
    private static string FormatDecimal(decimal value)
    {
        return value % 1 == 0
            ? value.ToString("0", CultureInfo.InvariantCulture)
            : value.ToString("0.0", CultureInfo.InvariantCulture);
    }

    /// <summary>
    /// Formats a glucose unit for dashboard statistics.
    /// </summary>
    /// <param name="unit">The glucose unit.</param>
    /// <returns>The formatted unit.</returns>
    private static string FormatUnit(GlucoseUnit unit)
    {
        return unit switch
        {
            GlucoseUnit.MgDl => "mg/dL",
            GlucoseUnit.MmolL => "mmol/L",
            _ => unit.ToString()
        };
    }

    #endregion
}