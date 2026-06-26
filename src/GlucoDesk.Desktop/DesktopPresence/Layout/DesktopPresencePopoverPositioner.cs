using Avalonia;

namespace GlucoDesk.Desktop.DesktopPresence.Layout;

/// <summary>
/// Calculates screen-aware desktop presence popover positions.
/// </summary>
public static class DesktopPresencePopoverPositioner
{
    /// <summary>
    /// The default edge margin, in physical screen pixels.
    /// </summary>
    public const int EdgeMarginPixels = 24;

    /// <summary>
    /// The fallback popover width, in Avalonia device-independent pixels.
    /// </summary>
    public const int DefaultPopoverWidthDip = 360;

    /// <summary>
    /// The fallback popover height, in Avalonia device-independent pixels.
    /// </summary>
    public const int DefaultPopoverHeightDip = 520;

    /// <summary>
    /// Calculates a popover position that keeps the window inside the provided screen working area whenever possible.
    /// </summary>
    /// <param name="workingArea">The current screen working area, in physical pixels.</param>
    /// <param name="popoverSize">The popover size, in physical pixels.</param>
    /// <returns>The calculated popover top-left position.</returns>
    public static PixelPoint Calculate(PixelRect workingArea, PixelSize popoverSize)
    {
        if (workingArea.Width <= 0 || workingArea.Height <= 0)
        {
            return new PixelPoint(0, 0);
        }

        var popoverWidth = Math.Max(1, popoverSize.Width);
        var popoverHeight = Math.Max(1, popoverSize.Height);

        var x = CalculateAxisPosition(
            workingArea.X,
            workingArea.Width,
            popoverWidth,
            preferEnd: true);

        var y = CalculateAxisPosition(
            workingArea.Y,
            workingArea.Height,
            popoverHeight,
            preferEnd: false);

        return new PixelPoint(x, y);
    }

    /// <summary>
    /// Calculates the popover position on a single screen axis.
    /// </summary>
    /// <param name="workingAreaStart">The start coordinate of the working area.</param>
    /// <param name="workingAreaLength">The length of the working area.</param>
    /// <param name="popoverLength">The length of the popover.</param>
    /// <param name="preferEnd">A value indicating whether the popover should prefer the end side of the axis.</param>
    /// <returns>The calculated axis coordinate.</returns>
    private static int CalculateAxisPosition(
        int workingAreaStart,
        int workingAreaLength,
        int popoverLength,
        bool preferEnd)
    {
        var min = workingAreaStart + EdgeMarginPixels;
        var max = workingAreaStart + workingAreaLength - popoverLength - EdgeMarginPixels;

        if (max < min)
        {
            return workingAreaStart + Math.Max(0, (workingAreaLength - popoverLength) / 2);
        }

        var preferred = preferEnd
            ? max
            : min;

        return Math.Clamp(preferred, min, max);
    }
}
