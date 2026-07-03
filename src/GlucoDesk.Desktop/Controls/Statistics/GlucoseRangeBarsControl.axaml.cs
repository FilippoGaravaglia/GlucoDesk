using Avalonia;
using Avalonia.Controls;

namespace GlucoDesk.Desktop.Controls.Statistics;

/// <summary>
/// Renders deterministic below-range and above-range summary bars.
/// </summary>
public partial class GlucoseRangeBarsControl : UserControl
{
    /// <summary>
    /// Identifies the <see cref="BelowPercent"/> styled property.
    /// </summary>
    public static readonly StyledProperty<double> BelowPercentProperty =
        AvaloniaProperty.Register<GlucoseRangeBarsControl, double>(nameof(BelowPercent));

    /// <summary>
    /// Identifies the <see cref="AbovePercent"/> styled property.
    /// </summary>
    public static readonly StyledProperty<double> AbovePercentProperty =
        AvaloniaProperty.Register<GlucoseRangeBarsControl, double>(nameof(AbovePercent));

    static GlucoseRangeBarsControl()
    {
        BelowPercentProperty.Changed.AddClassHandler<GlucoseRangeBarsControl>(
            static (control, _) => control.UpdateBars());

        AbovePercentProperty.Changed.AddClassHandler<GlucoseRangeBarsControl>(
            static (control, _) => control.UpdateBars());
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseRangeBarsControl"/> class.
    /// </summary>
    public GlucoseRangeBarsControl()
    {
        InitializeComponent();

        BarsGrid.SizeChanged += HandleBarsGridSizeChanged;
        AttachedToVisualTree += HandleAttachedToVisualTree;
    }

    /// <summary>
    /// Gets or sets the below-range percentage.
    /// </summary>
    public double BelowPercent
    {
        get => GetValue(BelowPercentProperty);
        set => SetValue(BelowPercentProperty, value);
    }

    /// <summary>
    /// Gets or sets the above-range percentage.
    /// </summary>
    public double AbovePercent
    {
        get => GetValue(AbovePercentProperty);
        set => SetValue(AbovePercentProperty, value);
    }

    #region Helpers

    /// <summary>
    /// Applies the calculated bar widths to the visual elements.
    /// </summary>
    private void UpdateBars()
    {
        if (BarsGrid is null)
        {
            return;
        }

        var layout = GlucoseRangeBarsLayoutCalculator.Calculate(
            BarsGrid.Bounds.Width,
            BelowPercent,
            AbovePercent);

        BelowFillBar.Width = layout.BelowFillWidth;
        BelowRemainingBar.Width = layout.BelowRemainingWidth;
        GapBar.Width = layout.GapWidth;
        AboveFillBar.Width = layout.AboveFillWidth;
        AboveRemainingBar.Width = layout.AboveRemainingWidth;
    }

    /// <summary>
    /// Handles the visual-tree attachment.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event arguments.</param>
    private void HandleAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UpdateBars();
    }

    /// <summary>
    /// Handles a size change of the bars grid.
    /// </summary>
    /// <param name="sender">The sender.</param>
    /// <param name="e">The event arguments.</param>
    private void HandleBarsGridSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateBars();
    }

    #endregion
}
