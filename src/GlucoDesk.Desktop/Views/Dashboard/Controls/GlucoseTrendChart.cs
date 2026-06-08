using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using GlucoDesk.Desktop.ViewModels.Dashboard.Chart;

namespace GlucoDesk.Desktop.Views.Dashboard.Controls;

/// <summary>
/// Displays a lightweight glucose trend chart for dashboard readings.
/// </summary>
public sealed class GlucoseTrendChart : Control
{
    private const double PlotPaddingLeft = 16;
    private const double PlotPaddingTop = 12;
    private const double PlotPaddingRight = 16;
    private const double PlotPaddingBottom = 12;

    private static readonly Pen GridPen = new(
        new SolidColorBrush(Color.FromArgb(45, 255, 255, 255)),
        1);

    private static readonly Pen TargetLinePen = new(
        new SolidColorBrush(Color.FromArgb(90, 255, 255, 255)),
        1);

    private static readonly Pen TrendLinePen = new(
        new SolidColorBrush(Color.FromRgb(99, 179, 237)),
        2);

    private static readonly IBrush TargetRangeBrush = new SolidColorBrush(
        Color.FromArgb(35, 72, 187, 120));

    private static readonly IBrush PointBrush = new SolidColorBrush(
        Color.FromRgb(235, 245, 255));

    /// <summary>
    /// Defines the chart points displayed by the control.
    /// </summary>
    public static readonly StyledProperty<IReadOnlyList<GlucoseChartPoint>?> PointsProperty =
        AvaloniaProperty.Register<GlucoseTrendChart, IReadOnlyList<GlucoseChartPoint>?>(nameof(Points));

    /// <summary>
    /// Defines the lower glucose target value expressed in mg/dL.
    /// </summary>
    public static readonly StyledProperty<decimal> TargetLowMgDlProperty =
        AvaloniaProperty.Register<GlucoseTrendChart, decimal>(nameof(TargetLowMgDl), 70m);

    /// <summary>
    /// Defines the upper glucose target value expressed in mg/dL.
    /// </summary>
    public static readonly StyledProperty<decimal> TargetHighMgDlProperty =
        AvaloniaProperty.Register<GlucoseTrendChart, decimal>(nameof(TargetHighMgDl), 180m);

    static GlucoseTrendChart()
    {
        AffectsRender<GlucoseTrendChart>(
            PointsProperty,
            TargetLowMgDlProperty,
            TargetHighMgDlProperty);
    }

    /// <summary>
    /// Gets or sets the chart points displayed by the control.
    /// </summary>
    public IReadOnlyList<GlucoseChartPoint>? Points
    {
        get => GetValue(PointsProperty);
        set => SetValue(PointsProperty, value);
    }

    /// <summary>
    /// Gets or sets the lower glucose target value expressed in mg/dL.
    /// </summary>
    public decimal TargetLowMgDl
    {
        get => GetValue(TargetLowMgDlProperty);
        set => SetValue(TargetLowMgDlProperty, value);
    }

    /// <summary>
    /// Gets or sets the upper glucose target value expressed in mg/dL.
    /// </summary>
    public decimal TargetHighMgDl
    {
        get => GetValue(TargetHighMgDlProperty);
        set => SetValue(TargetHighMgDlProperty, value);
    }

    /// <inheritdoc />
    public override void Render(DrawingContext context)
    {
        base.Render(context);

        var plotArea = CreatePlotArea(Bounds);

        if (plotArea.Width <= 0 || plotArea.Height <= 0)
        {
            return;
        }

        var points = Points?
            .OrderBy(point => point.Timestamp)
            .ToArray() ?? [];

        var targetRange = NormalizeTargetRange(TargetLowMgDl, TargetHighMgDl);
        var scale = CalculateScale(points, targetRange);

        DrawGrid(context, plotArea);
        DrawTargetRange(context, plotArea, scale, targetRange);

        if (points.Length == 0)
        {
            return;
        }

        DrawTrendLine(context, plotArea, scale, points);
        DrawPoints(context, plotArea, scale, points);
    }

    #region Helpers

    /// <summary>
    /// Creates the drawable plot area using the control bounds.
    /// </summary>
    /// <param name="bounds">The control bounds.</param>
    /// <returns>The drawable plot area.</returns>
    private static Rect CreatePlotArea(Rect bounds)
    {
        var width = Math.Max(0, bounds.Width - PlotPaddingLeft - PlotPaddingRight);
        var height = Math.Max(0, bounds.Height - PlotPaddingTop - PlotPaddingBottom);

        return new Rect(
            PlotPaddingLeft,
            PlotPaddingTop,
            width,
            height);
    }

    /// <summary>
    /// Normalizes target range values to keep chart rendering safe.
    /// </summary>
    /// <param name="targetLowMgDl">The lower target value.</param>
    /// <param name="targetHighMgDl">The upper target value.</param>
    /// <returns>The normalized target range.</returns>
    private static ChartTargetRange NormalizeTargetRange(
        decimal targetLowMgDl,
        decimal targetHighMgDl)
    {
        var normalizedLow = targetLowMgDl <= 0 ? 70m : targetLowMgDl;
        var normalizedHigh = targetHighMgDl <= normalizedLow
            ? normalizedLow + 1
            : targetHighMgDl;

        return new ChartTargetRange(normalizedLow, normalizedHigh);
    }

    /// <summary>
    /// Calculates the chart scale including the target range.
    /// </summary>
    /// <param name="points">The chart points.</param>
    /// <param name="targetRange">The chart target range.</param>
    /// <returns>The calculated chart scale.</returns>
    private static ChartScale CalculateScale(
        IReadOnlyList<GlucoseChartPoint> points,
        ChartTargetRange targetRange)
    {
        var minimumValue = points.Count == 0
            ? targetRange.LowMgDl
            : Math.Min(points.Min(point => point.ValueMgDl), targetRange.LowMgDl);

        var maximumValue = points.Count == 0
            ? targetRange.HighMgDl
            : Math.Max(points.Max(point => point.ValueMgDl), targetRange.HighMgDl);

        var paddedMinimum = Math.Max(0, minimumValue - 20);
        var paddedMaximum = maximumValue + 20;

        if (paddedMaximum <= paddedMinimum)
        {
            paddedMaximum = paddedMinimum + 1;
        }

        return new ChartScale(paddedMinimum, paddedMaximum);
    }

    /// <summary>
    /// Draws chart grid lines.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    private static void DrawGrid(DrawingContext context, Rect plotArea)
    {
        var firstY = plotArea.Top + (plotArea.Height / 3);
        var secondY = plotArea.Top + ((plotArea.Height / 3) * 2);

        context.DrawLine(GridPen, new Point(plotArea.Left, firstY), new Point(plotArea.Right, firstY));
        context.DrawLine(GridPen, new Point(plotArea.Left, secondY), new Point(plotArea.Right, secondY));
    }

    /// <summary>
    /// Draws the configured glucose target range.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="scale">The chart scale.</param>
    /// <param name="targetRange">The chart target range.</param>
    private static void DrawTargetRange(
        DrawingContext context,
        Rect plotArea,
        ChartScale scale,
        ChartTargetRange targetRange)
    {
        var targetTop = MapValueToY(targetRange.HighMgDl, plotArea, scale);
        var targetBottom = MapValueToY(targetRange.LowMgDl, plotArea, scale);

        var targetRect = new Rect(
            plotArea.Left,
            targetTop,
            plotArea.Width,
            Math.Max(1, targetBottom - targetTop));

        context.DrawRectangle(TargetRangeBrush, null, targetRect);
        context.DrawLine(TargetLinePen, new Point(plotArea.Left, targetTop), new Point(plotArea.Right, targetTop));
        context.DrawLine(TargetLinePen, new Point(plotArea.Left, targetBottom), new Point(plotArea.Right, targetBottom));
    }

    /// <summary>
    /// Draws the glucose trend line.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="scale">The chart scale.</param>
    /// <param name="points">The chart points.</param>
    private static void DrawTrendLine(
        DrawingContext context,
        Rect plotArea,
        ChartScale scale,
        IReadOnlyList<GlucoseChartPoint> points)
    {
        if (points.Count < 2)
        {
            return;
        }

        for (var index = 1; index < points.Count; index++)
        {
            var previousPoint = MapPoint(points[index - 1], index - 1, points.Count, plotArea, scale);
            var currentPoint = MapPoint(points[index], index, points.Count, plotArea, scale);

            context.DrawLine(TrendLinePen, previousPoint, currentPoint);
        }
    }

    /// <summary>
    /// Draws individual glucose chart points.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="scale">The chart scale.</param>
    /// <param name="points">The chart points.</param>
    private static void DrawPoints(
        DrawingContext context,
        Rect plotArea,
        ChartScale scale,
        IReadOnlyList<GlucoseChartPoint> points)
    {
        for (var index = 0; index < points.Count; index++)
        {
            var mappedPoint = MapPoint(points[index], index, points.Count, plotArea, scale);

            context.DrawEllipse(PointBrush, null, mappedPoint, 3.5, 3.5);
        }
    }

    /// <summary>
    /// Maps a chart point to the plot area.
    /// </summary>
    /// <param name="point">The chart point.</param>
    /// <param name="index">The point index.</param>
    /// <param name="totalCount">The total point count.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="scale">The chart scale.</param>
    /// <returns>The mapped drawing point.</returns>
    private static Point MapPoint(
        GlucoseChartPoint point,
        int index,
        int totalCount,
        Rect plotArea,
        ChartScale scale)
    {
        var x = totalCount <= 1
            ? plotArea.Left + (plotArea.Width / 2)
            : plotArea.Left + (plotArea.Width * index / (totalCount - 1));

        var y = MapValueToY(point.ValueMgDl, plotArea, scale);

        return new Point(x, y);
    }

    /// <summary>
    /// Maps a glucose value to a Y coordinate inside the plot area.
    /// </summary>
    /// <param name="value">The glucose value.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="scale">The chart scale.</param>
    /// <returns>The mapped Y coordinate.</returns>
    private static double MapValueToY(decimal value, Rect plotArea, ChartScale scale)
    {
        var normalizedValue = (double)((value - scale.MinimumValue) / (scale.MaximumValue - scale.MinimumValue));

        return plotArea.Bottom - (plotArea.Height * normalizedValue);
    }

    #endregion

    private sealed record ChartScale(decimal MinimumValue, decimal MaximumValue);

    private sealed record ChartTargetRange(decimal LowMgDl, decimal HighMgDl);
}