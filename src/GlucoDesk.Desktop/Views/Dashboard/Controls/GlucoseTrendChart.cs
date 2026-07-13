using System.Globalization;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using GlucoDesk.Desktop.ViewModels.Dashboard.Chart;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.Views.Dashboard.Controls;

/// <summary>
/// Displays an enterprise-grade glucose trend chart for dashboard readings.
/// </summary>
public sealed class GlucoseTrendChart : Control
{
    private const double PlotPaddingLeft = 64;
    private const double PlotPaddingTop = 42;
    private const double PlotPaddingRight = 22;
    private const double PlotPaddingBottom = 38;
    private const double AxisLabelFontSize = 11;
    private const double TargetLabelFontSize = 10;
    private const double YAxisLabelGap = 10;
    private const double YAxisUnitTopOffset = 28;
    private const double YAxisTopTickExtraOffset = 14;

    private static readonly IBrush PlotBackgroundBrush = new SolidColorBrush(
        Color.FromArgb(150, 248, 251, 255));

    private static readonly Pen PlotBorderPen = new(
        new SolidColorBrush(Color.FromArgb(150, 199, 221, 245)),
        1);

    private static readonly Pen GridPen = new(
        new SolidColorBrush(Color.FromArgb(95, 148, 163, 184)),
        1);

    private static readonly Pen AxisPen = new(
        new SolidColorBrush(Color.FromArgb(130, 100, 116, 139)),
        1);

    private static readonly Pen TargetLinePen = new(
        new SolidColorBrush(Color.FromArgb(145, 13, 148, 136)),
        1.2);

    private static readonly Pen TrendShadowPen = new(
        new SolidColorBrush(Color.FromArgb(65, 2, 132, 199)),
        5);

    private static readonly Pen TrendLinePen = new(
        new SolidColorBrush(Color.FromRgb(2, 132, 199)),
        3);

    private static readonly Pen HighTrendShadowPen = new(
        new SolidColorBrush(Color.FromArgb(70, 245, 158, 11)),
        5);

    private static readonly Pen HighTrendLinePen = new(
        new SolidColorBrush(Color.FromRgb(245, 158, 11)),
        3);

    private static readonly Pen LowTrendShadowPen = new(
        new SolidColorBrush(Color.FromArgb(70, 220, 38, 38)),
        5);

    private static readonly Pen LowTrendLinePen = new(
        new SolidColorBrush(Color.FromRgb(220, 38, 38)),
        3);

    private static readonly Pen PointBorderPen = new(
        new SolidColorBrush(Color.FromRgb(2, 132, 199)),
        1.4);

    private static readonly Pen HighPointBorderPen = new(
        new SolidColorBrush(Color.FromRgb(245, 158, 11)),
        1.6);

    private static readonly Pen LowPointBorderPen = new(
        new SolidColorBrush(Color.FromRgb(220, 38, 38)),
        1.6);

    private static readonly IBrush TargetRangeBrush = new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(1, 1, RelativeUnit.Relative),
        GradientStops =
        {
            new GradientStop(Color.FromArgb(80, 204, 251, 241), 0),
            new GradientStop(Color.FromArgb(55, 220, 252, 231), 1)
        }
    };

    private static readonly IBrush TrendAreaBrush = new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        GradientStops =
        {
            new GradientStop(Color.FromArgb(90, 56, 189, 248), 0),
            new GradientStop(Color.FromArgb(20, 56, 189, 248), 1)
        }
    };

    private static readonly IBrush HighTrendAreaBrush = new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        GradientStops =
        {
            new GradientStop(Color.FromArgb(95, 245, 158, 11), 0),
            new GradientStop(Color.FromArgb(24, 245, 158, 11), 1)
        }
    };

    private static readonly IBrush LowTrendAreaBrush = new LinearGradientBrush
    {
        StartPoint = new RelativePoint(0, 0, RelativeUnit.Relative),
        EndPoint = new RelativePoint(0, 1, RelativeUnit.Relative),
        GradientStops =
        {
            new GradientStop(Color.FromArgb(90, 220, 38, 38), 0),
            new GradientStop(Color.FromArgb(22, 220, 38, 38), 1)
        }
    };

    private static readonly IBrush PointBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));

    private static readonly IBrush CurrentPointBrush = new SolidColorBrush(Color.FromRgb(255, 255, 255));

    private static readonly IBrush TooltipBackgroundBrush = new SolidColorBrush(Color.FromArgb(248, 255, 255, 255));

    private static readonly IBrush TooltipTextBrush = new SolidColorBrush(Color.FromRgb(15, 23, 42));

    private static readonly Pen TooltipBorderPen = new(new SolidColorBrush(Color.FromRgb(125, 211, 252)), 1);

    private const double TooltipPaddingX = 10d;

    private const double TooltipPaddingY = 8d;

    private Point? hoverPosition;

    private static readonly IBrush AxisTextBrush = new SolidColorBrush(Color.FromRgb(100, 116, 139));

    private static readonly IBrush TargetTextBrush = new SolidColorBrush(Color.FromRgb(13, 148, 136));

    private static readonly IBrush EmptyTextBrush = new SolidColorBrush(Color.FromRgb(100, 116, 139));

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

    /// <summary>
    /// Defines the visible chart time window expressed in hours.
    /// </summary>
    public static readonly StyledProperty<int> WindowHoursProperty =
        AvaloniaProperty.Register<GlucoseTrendChart, int>(nameof(WindowHours), 3);

    /// <summary>
    /// Defines the maximum visible chart value expressed in mg/dL.
    /// </summary>
    public static readonly StyledProperty<int> MaxVisibleMgDlProperty =
        AvaloniaProperty.Register<GlucoseTrendChart, int>(
            nameof(MaxVisibleMgDl),
            defaultValue: 300);

    /// <summary>
    /// Defines the glucose display unit used for chart labels.
    /// </summary>
    public static readonly StyledProperty<GlucoseUnit> DisplayUnitProperty =
        AvaloniaProperty.Register<GlucoseTrendChart, GlucoseUnit>(
            nameof(DisplayUnit),
            defaultValue: GlucoseUnit.MgDl);
        
    /// <summary>
    /// Gets or sets the maximum visible chart value expressed in mg/dL.
    /// </summary>
    public int MaxVisibleMgDl
    {
        get => GetValue(MaxVisibleMgDlProperty);
        set => SetValue(MaxVisibleMgDlProperty, value);
    }

    /// <summary>
    /// Gets or sets the glucose display unit used for chart labels.
    /// </summary>
    public GlucoseUnit DisplayUnit
    {
        get => GetValue(DisplayUnitProperty);
        set => SetValue(DisplayUnitProperty, value);
    }

    static GlucoseTrendChart()
    {
        AffectsRender<GlucoseTrendChart>(
            PointsProperty,
            TargetLowMgDlProperty,
            TargetHighMgDlProperty,
            WindowHoursProperty,
            MaxVisibleMgDlProperty,
            DisplayUnitProperty);
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

    /// <summary>
    /// Gets or sets the visible chart time window expressed in hours.
    /// </summary>
    public int WindowHours
    {
        get => GetValue(WindowHoursProperty);
        set => SetValue(WindowHoursProperty, value);
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
        var scale = CalculateScale(MaxVisibleMgDl);
        var displayUnit = NormalizeDisplayUnit(DisplayUnit);
        var yTicks = CreateYAxisTicks(scale);
        var timeScale = CalculateTimeScale(WindowHours);
        var xTicks = CreateXAxisTicks(timeScale, points.Length);

        DrawPlotBackground(context, plotArea);
        DrawTargetRange(context, plotArea, scale, targetRange);
        DrawGrid(context, plotArea, scale, yTicks, timeScale, xTicks);
        DrawAxes(context, plotArea);
        DrawYAxisLabels(context, plotArea, scale, yTicks, displayUnit);
        DrawXAxisLabels(context, plotArea, timeScale, xTicks);
        DrawTargetLabels(context, plotArea, scale, targetRange, displayUnit);

        if (points.Length == 0)
        {
            DrawEmptyState(context, plotArea);
            return;
        }

        var mappedPoints = MapPoints(points, plotArea, scale, timeScale);

        DrawTrendArea(context, plotArea, mappedPoints, targetRange);
        DrawTrendLine(context, mappedPoints, targetRange);
        DrawPoints(context, mappedPoints, targetRange);
        DrawPointTooltip(context, plotArea, mappedPoints, targetRange);
    }


    /// <inheritdoc />
    protected override void OnPointerMoved(PointerEventArgs e)
    {
        base.OnPointerMoved(e);

        hoverPosition = e.GetPosition(this);
        InvalidateVisual();
    }

    /// <inheritdoc />
    protected override void OnPointerExited(PointerEventArgs e)
    {
        base.OnPointerExited(e);

        hoverPosition = null;
        InvalidateVisual();
    }


    #region Helpers

    /// <summary>
    /// Normalizes unsupported glucose display units to the default display unit.
    /// </summary>
    /// <param name="displayUnit">The requested display unit.</param>
    /// <returns>The normalized display unit.</returns>
    private static GlucoseUnit NormalizeDisplayUnit(GlucoseUnit displayUnit)
    {
        return Enum.IsDefined(displayUnit)
            ? displayUnit
            : GlucoseUnit.MgDl;
    }
    
    /// <summary>
    /// Formats a glucose value stored in mg/dL for the selected display unit.
    /// </summary>
    /// <param name="valueMgDl">The glucose value expressed in mg/dL.</param>
    /// <param name="displayUnit">The glucose display unit.</param>
    /// <returns>The formatted glucose value without unit suffix.</returns>
    private static string FormatGlucoseValueLabel(
        decimal valueMgDl,
        GlucoseUnit displayUnit)
    {
        var convertedValue = new GlucoseValue(valueMgDl, GlucoseUnit.MgDl)
            .ConvertTo(displayUnit);
    
        return displayUnit switch
        {
            GlucoseUnit.MgDl => convertedValue.Amount.ToString("0", CultureInfo.InvariantCulture),
            GlucoseUnit.MmolL => convertedValue.Amount.ToString("0.0", CultureInfo.InvariantCulture),
            _ => valueMgDl.ToString("0", CultureInfo.InvariantCulture)
        };
    }
    
    /// <summary>
    /// Formats glucose unit labels for chart rendering.
    /// </summary>
    /// <param name="displayUnit">The glucose display unit.</param>
    /// <returns>The formatted unit label.</returns>
    private static string FormatGlucoseUnitLabel(GlucoseUnit displayUnit)
    {
        return displayUnit switch
        {
            GlucoseUnit.MgDl => "mg/dL",
            GlucoseUnit.MmolL => "mmol/L",
            _ => "mg/dL"
        };
    }

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
    /// Calculates the chart scale from the configured chart maximum.
    /// </summary>
    /// <param name="maxVisibleMgDl">The configured maximum visible chart value.</param>
    /// <returns>The calculated chart scale.</returns>
    private static ChartScale CalculateScale(int maxVisibleMgDl)
    {
        const decimal minimumValue = 40m;

        var maximumValue = NormalizeMaxVisibleMgDl(maxVisibleMgDl);

        return new ChartScale(minimumValue, maximumValue);
    }

    /// <summary>
    /// Normalizes maximum visible chart values to supported options.
    /// </summary>
    /// <param name="maxVisibleMgDl">The requested maximum visible value.</param>
    /// <returns>The normalized maximum visible value.</returns>
    private static decimal NormalizeMaxVisibleMgDl(int maxVisibleMgDl)
    {
        return maxVisibleMgDl is 400
            ? 400m
            : 300m;
    }

    /// <summary>
    /// Calculates the chart time scale from the selected chart window.
    /// </summary>
    /// <param name="windowHours">The selected chart window in hours.</param>
    /// <returns>The calculated time scale.</returns>
    private static ChartTimeScale CalculateTimeScale(int windowHours)
    {
        var normalizedWindowHours = NormalizeChartWindowHours(windowHours);
        var windowEnd = DateTimeOffset.Now;
        var windowStart = windowEnd.AddHours(-normalizedWindowHours);

        return new ChartTimeScale(windowStart, windowEnd);
    }

    /// <summary>
    /// Normalizes chart window values to supported options.
    /// </summary>
    /// <param name="windowHours">The requested chart window in hours.</param>
    /// <returns>The normalized chart window in hours.</returns>
    private static int NormalizeChartWindowHours(int windowHours)
    {
        return windowHours switch
        {
            6 => 6,
            12 => 12,
            24 => 24,
            _ => 3
        };
    }

    /// <summary>
    /// Creates Y axis tick values for the chart.
    /// </summary>
    /// <param name="scale">The chart scale.</param>
    /// <returns>The Y axis tick values.</returns>
    private static IReadOnlyList<decimal> CreateYAxisTicks(ChartScale scale)
    {
        if (scale.MaximumValue == 400m)
        {
            return [40m, 80m, 120m, 160m, 200m, 240m, 280m, 320m, 360m, 400m];
        }

        return [40m, 80m, 120m, 160m, 200m, 240m, 300m];
    }

    /// <summary>
    /// Creates X axis tick values for the chart.
    /// </summary>
    /// <param name="timeScale">The chart time scale.</param>
    /// <param name="pointsCount">The number of chart points.</param>
    /// <returns>The X axis tick values.</returns>
    private static IReadOnlyList<DateTimeOffset> CreateXAxisTicks(
        ChartTimeScale timeScale,
        int pointsCount)
    {
        var tickCount = pointsCount <= 2 ? 2 : 4;
        var totalTicksSpan = timeScale.End - timeScale.Start;

        if (totalTicksSpan <= TimeSpan.Zero)
        {
            return [timeScale.Start, timeScale.End];
        }

        var ticks = new List<DateTimeOffset>(tickCount);

        for (var index = 0; index < tickCount; index++)
        {
            var ratio = tickCount == 1 ? 0 : (double)index / (tickCount - 1);
            ticks.Add(timeScale.Start + TimeSpan.FromTicks((long)(totalTicksSpan.Ticks * ratio)));
        }

        return ticks;
    }

    /// <summary>
    /// Draws the chart plot background.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    private static void DrawPlotBackground(DrawingContext context, Rect plotArea)
    {
        context.DrawRectangle(PlotBackgroundBrush, PlotBorderPen, plotArea, 14, 14);
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
    /// Draws chart grid lines.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="scale">The chart scale.</param>
    /// <param name="yTicks">The Y axis tick values.</param>
    /// <param name="timeScale">The chart time scale.</param>
    /// <param name="xTicks">The X axis tick values.</param>
    private static void DrawGrid(
        DrawingContext context,
        Rect plotArea,
        ChartScale scale,
        IReadOnlyList<decimal> yTicks,
        ChartTimeScale timeScale,
        IReadOnlyList<DateTimeOffset> xTicks)
    {
        foreach (var tick in yTicks)
        {
            var y = MapValueToY(tick, plotArea, scale);
            context.DrawLine(GridPen, new Point(plotArea.Left, y), new Point(plotArea.Right, y));
        }

        foreach (var tick in xTicks)
        {
            var x = MapTimeToX(tick, plotArea, timeScale);
            context.DrawLine(GridPen, new Point(x, plotArea.Top), new Point(x, plotArea.Bottom));
        }
    }

    /// <summary>
    /// Draws chart axes.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    private static void DrawAxes(DrawingContext context, Rect plotArea)
    {
        context.DrawLine(AxisPen, new Point(plotArea.Left, plotArea.Top), new Point(plotArea.Left, plotArea.Bottom));
        context.DrawLine(AxisPen, new Point(plotArea.Left, plotArea.Bottom), new Point(plotArea.Right, plotArea.Bottom));
    }

   /// <summary>
    /// Draws Y axis labels.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="scale">The chart scale.</param>
    /// <param name="yTicks">The Y axis tick values.</param>
    /// <param name="displayUnit">The glucose display unit.</param>
    private static void DrawYAxisLabels(
        DrawingContext context,
        Rect plotArea,
        ChartScale scale,
        IReadOnlyList<decimal> yTicks,
        GlucoseUnit displayUnit)
    {
        var unitText = CreateText(
            FormatGlucoseUnitLabel(displayUnit),
            AxisTextBrush,
            TargetLabelFontSize);

        context.DrawText(
            unitText,
            new Point(
                plotArea.Left - unitText.Width - YAxisLabelGap,
                Math.Max(0, plotArea.Top - 26)));

        foreach (var tick in yTicks)
        {
            var label = FormatGlucoseValueLabel(tick, displayUnit);
            var text = CreateText(label, AxisTextBrush, AxisLabelFontSize);

            var y = ClampAxisLabelY(
                MapValueToY(tick, plotArea, scale) - (text.Height / 2),
                text.Height,
                plotArea);

            context.DrawText(
                text,
                new Point(
                    plotArea.Left - text.Width - YAxisLabelGap,
                    y));
        }
    }

    /// <summary>
    /// Draws X axis labels.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="timeScale">The chart time scale.</param>
    /// <param name="xTicks">The X axis tick values.</param>
    private static void DrawXAxisLabels(
        DrawingContext context,
        Rect plotArea,
        ChartTimeScale timeScale,
        IReadOnlyList<DateTimeOffset> xTicks)
    {
        foreach (var tick in xTicks)
        {
            var label = tick.ToLocalTime().ToString("HH:mm", CultureInfo.InvariantCulture);
            var text = CreateText(label, AxisTextBrush, AxisLabelFontSize);
            var x = MapTimeToX(tick, plotArea, timeScale) - (text.Width / 2);

            context.DrawText(text, new Point(x, plotArea.Bottom + 10));
        }
    }

    /// <summary>
    /// Keeps Y axis labels inside the visible plot area.
    /// </summary>
    /// <param name="labelY">The desired label Y coordinate.</param>
    /// <param name="labelHeight">The label height.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <returns>The clamped label Y coordinate.</returns>
    private static double ClampAxisLabelY(
        double labelY,
        double labelHeight,
        Rect plotArea)
    {
        return Math.Clamp(
            labelY,
            plotArea.Top,
            plotArea.Bottom - labelHeight);
    }

    /// <summary>
    /// Draws target range labels near target lines.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="scale">The chart scale.</param>
    /// <param name="targetRange">The chart target range.</param>
    /// <param name="displayUnit">The glucose display unit.</param>
    private static void DrawTargetLabels(
        DrawingContext context,
        Rect plotArea,
        ChartScale scale,
        ChartTargetRange targetRange,
        GlucoseUnit displayUnit)
    {
        var highLabel = CreateText(
            $"{LocalizationManager.GetString("DashboardChartHigh")} {FormatGlucoseValueLabel(targetRange.HighMgDl, displayUnit)}",
            TargetTextBrush,
            TargetLabelFontSize);

        var lowLabel = CreateText(
            $"{LocalizationManager.GetString("DashboardChartLow")} {FormatGlucoseValueLabel(targetRange.LowMgDl, displayUnit)}",
            TargetTextBrush,
            TargetLabelFontSize);

        var highY = MapValueToY(targetRange.HighMgDl, plotArea, scale) - highLabel.Height - 3;
        var lowY = MapValueToY(targetRange.LowMgDl, plotArea, scale) + 3;

        context.DrawText(highLabel, new Point(plotArea.Right - highLabel.Width - 8, highY));
        context.DrawText(lowLabel, new Point(plotArea.Right - lowLabel.Width - 8, lowY));
    }

    /// <summary>
    /// Draws the empty chart state.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    private static void DrawEmptyState(DrawingContext context, Rect plotArea)
    {
        var text = CreateText(
            LocalizationManager.GetString("DashboardChartNoReadings"),
            EmptyTextBrush,
            13);
        var origin = new Point(
            plotArea.Left + ((plotArea.Width - text.Width) / 2),
            plotArea.Top + ((plotArea.Height - text.Height) / 2));

        context.DrawText(text, origin);
    }

    /// <summary>
    /// Maps glucose points to chart drawing points.
    /// </summary>
    /// <param name="points">The glucose chart points.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="scale">The chart scale.</param>
    /// <param name="timeScale">The chart time scale.</param>
    /// <returns>The mapped chart points.</returns>
    private static IReadOnlyList<MappedChartPoint> MapPoints(
        IReadOnlyList<GlucoseChartPoint> points,
        Rect plotArea,
        ChartScale scale,
        ChartTimeScale timeScale)
    {
        return points
            .Select(point => new MappedChartPoint(
                MapTimeToX(point.Timestamp, plotArea, timeScale),
                MapValueToY(point.ValueMgDl, plotArea, scale),
                point))
            .ToArray();
    }

    /// <summary>
    /// Draws a soft range-aware area below the glucose trend line.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="points">The mapped chart points.</param>
    /// <param name="targetRange">The chart target range.</param>
    private static void DrawTrendArea(
        DrawingContext context,
        Rect plotArea,
        IReadOnlyList<MappedChartPoint> points,
        ChartTargetRange targetRange)
    {
        if (points.Count < 2)
        {
            return;
        }

        for (var index = 1; index < points.Count; index++)
        {
            DrawRangeAwareTrendAreaSegment(
                context,
                plotArea,
                points[index - 1],
                points[index],
                targetRange);
        }
    }

    /// <summary>
    /// Draws a single range-aware area segment below the trend line.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="start">The mapped start point.</param>
    /// <param name="end">The mapped end point.</param>
    /// <param name="targetRange">The chart target range.</param>
    private static void DrawRangeAwareTrendAreaSegment(
        DrawingContext context,
        Rect plotArea,
        MappedChartPoint start,
        MappedChartPoint end,
        ChartTargetRange targetRange)
    {
        var breakpoints = CreateSegmentBreakpoints(
            start.Source.ValueMgDl,
            end.Source.ValueMgDl,
            targetRange);

        for (var index = 1; index < breakpoints.Count; index++)
        {
            var fromRatio = breakpoints[index - 1];
            var toRatio = breakpoints[index];

            var fromPoint = InterpolatePoint(start, end, fromRatio);
            var toPoint = InterpolatePoint(start, end, toRatio);
            var midpointValue = InterpolateValue(
                start.Source.ValueMgDl,
                end.Source.ValueMgDl,
                (fromRatio + toRatio) / 2);

            var areaBrush = GetTrendAreaBrush(midpointValue, targetRange);
            var geometry = new StreamGeometry();

            using (var geometryContext = geometry.Open())
            {
                geometryContext.BeginFigure(
                    new Point(fromPoint.X, plotArea.Bottom),
                    isFilled: true);

                geometryContext.LineTo(fromPoint);
                geometryContext.LineTo(toPoint);
                geometryContext.LineTo(new Point(toPoint.X, plotArea.Bottom));
                geometryContext.EndFigure(isClosed: true);
            }

            context.DrawGeometry(areaBrush, null, geometry);
        }
    }

    /// <summary>
    /// Gets the trend area brush for the supplied glucose value.
    /// </summary>
    /// <param name="valueMgDl">The glucose value expressed in mg/dL.</param>
    /// <param name="targetRange">The chart target range.</param>
    /// <returns>The range-aware trend area brush.</returns>
    private static IBrush GetTrendAreaBrush(
        decimal valueMgDl,
        ChartTargetRange targetRange)
    {
        if (valueMgDl < targetRange.LowMgDl)
        {
            return LowTrendAreaBrush;
        }

        if (valueMgDl > targetRange.HighMgDl)
        {
            return HighTrendAreaBrush;
        }

        return TrendAreaBrush;
    }

    /// <summary>
    /// Draws the glucose trend line with range-aware segment colors.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="points">The mapped chart points.</param>
    /// <param name="targetRange">The chart target range.</param>
    private static void DrawTrendLine(
        DrawingContext context,
        IReadOnlyList<MappedChartPoint> points,
        ChartTargetRange targetRange)
    {
        if (points.Count < 2)
        {
            return;
        }

        for (var index = 1; index < points.Count; index++)
        {
            DrawRangeAwareTrendSegment(
                context,
                points[index - 1],
                points[index],
                targetRange);
        }
    }

    /// <summary>
    /// Draws a single trend segment, splitting it when it crosses glucose target thresholds.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="start">The mapped start point.</param>
    /// <param name="end">The mapped end point.</param>
    /// <param name="targetRange">The chart target range.</param>
    private static void DrawRangeAwareTrendSegment(
        DrawingContext context,
        MappedChartPoint start,
        MappedChartPoint end,
        ChartTargetRange targetRange)
    {
        var breakpoints = CreateSegmentBreakpoints(
            start.Source.ValueMgDl,
            end.Source.ValueMgDl,
            targetRange);

        for (var index = 1; index < breakpoints.Count; index++)
        {
            var fromRatio = breakpoints[index - 1];
            var toRatio = breakpoints[index];

            var fromPoint = InterpolatePoint(start, end, fromRatio);
            var toPoint = InterpolatePoint(start, end, toRatio);
            var midpointValue = InterpolateValue(
                start.Source.ValueMgDl,
                end.Source.ValueMgDl,
                (fromRatio + toRatio) / 2);

            var pens = GetTrendPens(midpointValue, targetRange);

            context.DrawLine(pens.ShadowPen, fromPoint, toPoint);
            context.DrawLine(pens.LinePen, fromPoint, toPoint);
        }
    }

    /// <summary>
    /// Creates normalized segment breakpoints when a segment crosses target thresholds.
    /// </summary>
    /// <param name="startValue">The start glucose value.</param>
    /// <param name="endValue">The end glucose value.</param>
    /// <param name="targetRange">The chart target range.</param>
    /// <returns>The normalized segment breakpoints.</returns>
    private static IReadOnlyList<double> CreateSegmentBreakpoints(
        decimal startValue,
        decimal endValue,
        ChartTargetRange targetRange)
    {
        var breakpoints = new List<double> { 0, 1 };

        if (TryCalculateThresholdRatio(startValue, endValue, targetRange.LowMgDl, out var lowRatio))
        {
            breakpoints.Add(lowRatio);
        }

        if (TryCalculateThresholdRatio(startValue, endValue, targetRange.HighMgDl, out var highRatio))
        {
            breakpoints.Add(highRatio);
        }

        breakpoints.Sort();

        return breakpoints;
    }

    /// <summary>
    /// Calculates where a segment crosses a glucose threshold.
    /// </summary>
    /// <param name="startValue">The start glucose value.</param>
    /// <param name="endValue">The end glucose value.</param>
    /// <param name="threshold">The glucose threshold.</param>
    /// <param name="ratio">The calculated crossing ratio.</param>
    /// <returns>True when the segment crosses the threshold; otherwise, false.</returns>
    private static bool TryCalculateThresholdRatio(
        decimal startValue,
        decimal endValue,
        decimal threshold,
        out double ratio)
    {
        ratio = 0;

        if (startValue == endValue)
        {
            return false;
        }

        var crossesThreshold =
            startValue < threshold && endValue > threshold ||
            startValue > threshold && endValue < threshold;

        if (!crossesThreshold)
        {
            return false;
        }

        ratio = (double)((threshold - startValue) / (endValue - startValue));

        return ratio > 0 && ratio < 1;
    }

    /// <summary>
    /// Interpolates a point between two mapped chart points.
    /// </summary>
    /// <param name="start">The start point.</param>
    /// <param name="end">The end point.</param>
    /// <param name="ratio">The interpolation ratio.</param>
    /// <returns>The interpolated drawing point.</returns>
    private static Point InterpolatePoint(
        MappedChartPoint start,
        MappedChartPoint end,
        double ratio)
    {
        var x = start.X + ((end.X - start.X) * ratio);
        var y = start.Y + ((end.Y - start.Y) * ratio);

        return new Point(x, y);
    }

    /// <summary>
    /// Interpolates a glucose value between two values.
    /// </summary>
    /// <param name="startValue">The start glucose value.</param>
    /// <param name="endValue">The end glucose value.</param>
    /// <param name="ratio">The interpolation ratio.</param>
    /// <returns>The interpolated glucose value.</returns>
    private static decimal InterpolateValue(
        decimal startValue,
        decimal endValue,
        double ratio)
    {
        return startValue + ((endValue - startValue) * (decimal)ratio);
    }

    /// <summary>
    /// Gets the trend pens for the supplied glucose value.
    /// </summary>
    /// <param name="valueMgDl">The glucose value expressed in mg/dL.</param>
    /// <param name="targetRange">The chart target range.</param>
    /// <returns>The shadow and line pens.</returns>
    private static (Pen ShadowPen, Pen LinePen) GetTrendPens(
        decimal valueMgDl,
        ChartTargetRange targetRange)
    {
        if (valueMgDl < targetRange.LowMgDl)
        {
            return (LowTrendShadowPen, LowTrendLinePen);
        }

        if (valueMgDl > targetRange.HighMgDl)
        {
            return (HighTrendShadowPen, HighTrendLinePen);
        }

        return (TrendShadowPen, TrendLinePen);
    }

    /// <summary>
    /// Draws a compact tooltip for the glucose chart point nearest to the pointer.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="points">The mapped chart points.</param>
    /// <param name="targetRange">The chart target range.</param>
    private void DrawPointTooltip(
        DrawingContext context,
        Rect plotArea,
        IReadOnlyList<MappedChartPoint> points,
        ChartTargetRange targetRange)
    {
        if (hoverPosition is not { } pointerPosition)
        {
            return;
        }

        if (!plotArea.Contains(pointerPosition) || points.Count == 0)
        {
            return;
        }

        var nearestPoint = FindNearestPoint(points, pointerPosition);

        if (nearestPoint is null)
        {
            return;
        }

        var valueText = FormatTooltipGlucoseValue(nearestPoint.Source.ValueMgDl, DisplayUnit);
        var timeText = nearestPoint.Source.Timestamp.ToLocalTime().ToString("HH:mm", CultureInfo.CurrentCulture);
        var statusText = GetTooltipStatusText(nearestPoint.Source.ValueMgDl, targetRange);
        var tooltipText = string.Join(Environment.NewLine, valueText, timeText, statusText);

        var formattedText = new FormattedText(
            tooltipText,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily.Default),
            12,
            TooltipTextBrush);

        var tooltipWidth = formattedText.Width + (TooltipPaddingX * 2);
        var tooltipHeight = formattedText.Height + (TooltipPaddingY * 2);

        var x = nearestPoint.X + 12;
        var y = nearestPoint.Y - tooltipHeight - 12;

        if (x + tooltipWidth > plotArea.Right)
        {
            x = nearestPoint.X - tooltipWidth - 12;
        }

        if (x < plotArea.Left)
        {
            x = plotArea.Left;
        }

        if (y < plotArea.Top)
        {
            y = nearestPoint.Y + 12;
        }

        if (y + tooltipHeight > plotArea.Bottom)
        {
            y = plotArea.Bottom - tooltipHeight;
        }

        var tooltipRect = new Rect(x, y, tooltipWidth, tooltipHeight);

        context.DrawRectangle(TooltipBackgroundBrush, TooltipBorderPen, tooltipRect, 8, 8);
        context.DrawText(formattedText, new Point(x + TooltipPaddingX, y + TooltipPaddingY));
    }

    /// <summary>
    /// Finds the chart point nearest to the pointer position.
    /// </summary>
    /// <param name="points">The mapped chart points.</param>
    /// <param name="pointerPosition">The pointer position.</param>
    /// <returns>The nearest mapped chart point.</returns>
    private static MappedChartPoint? FindNearestPoint(
        IReadOnlyList<MappedChartPoint> points,
        Point pointerPosition)
    {
        MappedChartPoint? nearestPoint = null;
        var nearestDistance = double.MaxValue;

        foreach (var point in points)
        {
            var deltaX = point.X - pointerPosition.X;
            var deltaY = point.Y - pointerPosition.Y;
            var distance = Math.Sqrt((deltaX * deltaX) + (deltaY * deltaY));

            if (distance < nearestDistance)
            {
                nearestDistance = distance;
                nearestPoint = point;
            }
        }

        return nearestPoint;
    }

    /// <summary>
    /// Formats a glucose value for the chart tooltip.
    /// </summary>
    /// <param name="valueMgDl">The glucose value expressed in mg/dL.</param>
    /// <param name="displayUnit">The selected display unit.</param>
    /// <returns>The formatted tooltip glucose value.</returns>
    private static string FormatTooltipGlucoseValue(
        decimal valueMgDl,
        GlucoseUnit displayUnit)
    {
        if (displayUnit.ToString().Contains("Mmol", StringComparison.OrdinalIgnoreCase))
        {
            var valueMmolL = valueMgDl / 18m;

            return string.Create(
                CultureInfo.CurrentCulture,
                $"{valueMmolL:0.0} mmol/L");
        }

        return string.Create(
            CultureInfo.CurrentCulture,
            $"{valueMgDl:0} mg/dL");
    }

    /// <summary>
    /// Gets the tooltip status text for the supplied glucose value.
    /// </summary>
    /// <param name="valueMgDl">The glucose value expressed in mg/dL.</param>
    /// <param name="targetRange">The chart target range.</param>
    /// <returns>The tooltip status text.</returns>
    private static string GetTooltipStatusText(
        decimal valueMgDl,
        ChartTargetRange targetRange)
    {
        if (valueMgDl < targetRange.LowMgDl)
        {
            return "Below target";
        }

        if (valueMgDl > targetRange.HighMgDl)
        {
            return "Above target";
        }

        return LocalizationManager.GetString("DashboardChartInRange");
    }

    /// <summary>
    /// Draws individual glucose chart points.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    /// <param name="points">The mapped chart points.</param>
    /// <param name="targetRange">The chart target range.</param>
    private static void DrawPoints(
        DrawingContext context,
        IReadOnlyList<MappedChartPoint> points,
        ChartTargetRange targetRange)
    {
        for (var index = 0; index < points.Count; index++)
        {
            var point = points[index];
            var center = new Point(point.X, point.Y);
            var pointBorderPen = GetPointBorderPen(point.Source.ValueMgDl, targetRange);
            var isCurrentPoint = index == points.Count - 1;

            if (isCurrentPoint)
            {
                context.DrawEllipse(pointBorderPen.Brush, new Pen(CurrentPointBrush, 1.25), center, 5.6, 5.6);
                continue;
            }

            context.DrawEllipse(PointBrush, pointBorderPen, center, 3.5, 3.5);
        }
    }

    /// <summary>
    /// Gets the point border pen for the supplied glucose value.
    /// </summary>
    /// <param name="valueMgDl">The glucose value expressed in mg/dL.</param>
    /// <param name="targetRange">The chart target range.</param>
    /// <returns>The point border pen.</returns>
    private static Pen GetPointBorderPen(
        decimal valueMgDl,
        ChartTargetRange targetRange)
    {
        if (valueMgDl < targetRange.LowMgDl)
        {
            return LowPointBorderPen;
        }

        if (valueMgDl > targetRange.HighMgDl)
        {
            return HighPointBorderPen;
        }

        return PointBorderPen;
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

    /// <summary>
    /// Maps a timestamp to an X coordinate inside the plot area.
    /// </summary>
    /// <param name="timestamp">The timestamp.</param>
    /// <param name="plotArea">The chart plot area.</param>
    /// <param name="timeScale">The chart time scale.</param>
    /// <returns>The mapped X coordinate.</returns>
    private static double MapTimeToX(
        DateTimeOffset timestamp,
        Rect plotArea,
        ChartTimeScale timeScale)
    {
        var totalSeconds = Math.Max(1, (timeScale.End - timeScale.Start).TotalSeconds);
        var elapsedSeconds = Math.Clamp((timestamp - timeScale.Start).TotalSeconds, 0, totalSeconds);

        return plotArea.Left + (plotArea.Width * elapsedSeconds / totalSeconds);
    }

    /// <summary>
    /// Creates formatted text for chart labels.
    /// </summary>
    /// <param name="text">The text value.</param>
    /// <param name="brush">The text brush.</param>
    /// <param name="fontSize">The font size.</param>
    /// <returns>The formatted text.</returns>
    private static FormattedText CreateText(
        string text,
        IBrush brush,
        double fontSize)
    {
        return new FormattedText(
            text,
            CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            Typeface.Default,
            fontSize,
            brush);
    }

    /// <summary>
    /// Rounds a decimal value down to the nearest step.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="step">The rounding step.</param>
    /// <returns>The rounded value.</returns>
    private static decimal RoundDown(decimal value, decimal step)
    {
        return Math.Floor(value / step) * step;
    }

    /// <summary>
    /// Rounds a decimal value up to the nearest step.
    /// </summary>
    /// <param name="value">The value to round.</param>
    /// <param name="step">The rounding step.</param>
    /// <returns>The rounded value.</returns>
    private static decimal RoundUp(decimal value, decimal step)
    {
        return Math.Ceiling(value / step) * step;
    }

    #endregion

    private sealed record ChartScale(decimal MinimumValue, decimal MaximumValue);

    private sealed record ChartTargetRange(decimal LowMgDl, decimal HighMgDl);

    private sealed record ChartTimeScale(DateTimeOffset Start, DateTimeOffset End);

    private sealed record MappedChartPoint(double X, double Y, GlucoseChartPoint Source);
}