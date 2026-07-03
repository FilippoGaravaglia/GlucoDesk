namespace GlucoDesk.Desktop.Controls.Statistics;

/// <summary>
/// Calculates a deterministic layout for the glucose exposure summary bars.
/// </summary>
public static class GlucoseRangeBarsLayoutCalculator
{
    private const double DefaultGapWidth = 12d;
    private const double MinimumVisibleFillWidth = 8d;

    /// <summary>
    /// Calculates the layout widths for the combined below-range and above-range bars.
    /// </summary>
    /// <param name="totalWidth">The total available width.</param>
    /// <param name="belowPercent">The below-range percentage.</param>
    /// <param name="abovePercent">The above-range percentage.</param>
    /// <returns>The calculated layout.</returns>
    public static GlucoseRangeBarsLayout Calculate(
        double totalWidth,
        double belowPercent,
        double abovePercent)
    {
        if (totalWidth <= 0d)
        {
            return GlucoseRangeBarsLayout.Empty;
        }

        var gapWidth = totalWidth > DefaultGapWidth ? DefaultGapWidth : 0d;
        var laneWidth = Math.Max(0d, (totalWidth - gapWidth) / 2d);

        var belowFillWidth = CalculateFillWidth(belowPercent, laneWidth);
        var aboveFillWidth = CalculateFillWidth(abovePercent, laneWidth);

        var belowRemainingWidth = Math.Max(0d, laneWidth - belowFillWidth);
        var aboveRemainingWidth = Math.Max(0d, laneWidth - aboveFillWidth);

        return new GlucoseRangeBarsLayout(
            belowFillWidth,
            belowRemainingWidth,
            gapWidth,
            aboveFillWidth,
            aboveRemainingWidth);
    }

    #region Helpers

    /// <summary>
    /// Calculates the visible fill width for a lane.
    /// </summary>
    /// <param name="percent">The percentage value.</param>
    /// <param name="laneWidth">The total width of the lane.</param>
    /// <returns>The clamped fill width.</returns>
    private static double CalculateFillWidth(double percent, double laneWidth)
    {
        if (laneWidth <= 0d || percent <= 0d)
        {
            return 0d;
        }

        var clampedPercent = Math.Clamp(percent, 0d, 100d);
        var rawWidth = laneWidth * clampedPercent / 100d;
        var minimumVisibleWidth = Math.Min(MinimumVisibleFillWidth, laneWidth);

        return Math.Min(
            laneWidth,
            Math.Max(rawWidth, minimumVisibleWidth));
    }

    #endregion
}

/// <summary>
/// Represents the deterministic layout of the glucose exposure bars.
/// </summary>
/// <param name="BelowFillWidth">The below-range fill width.</param>
/// <param name="BelowRemainingWidth">The below-range remaining track width.</param>
/// <param name="GapWidth">The fixed gap width.</param>
/// <param name="AboveFillWidth">The above-range fill width.</param>
/// <param name="AboveRemainingWidth">The above-range remaining track width.</param>
public readonly record struct GlucoseRangeBarsLayout(
    double BelowFillWidth,
    double BelowRemainingWidth,
    double GapWidth,
    double AboveFillWidth,
    double AboveRemainingWidth)
{
    /// <summary>
    /// Gets an empty layout.
    /// </summary>
    public static GlucoseRangeBarsLayout Empty => new(
        0d,
        0d,
        0d,
        0d,
        0d);
}
