using Avalonia;
using GlucoDesk.Desktop.DesktopPresence.Layout;

namespace GlucoDesk.Desktop.Tests.DesktopPresence.Layout;

public sealed class DesktopPresencePopoverPositionerTests
{
    [Fact]
    public void Calculate_KeepsPopoverInsideRightScreenEdge()
    {
        var workingArea = new PixelRect(0, 0, 1920, 1080);
        var popoverSize = new PixelSize(420, 560);

        var position = DesktopPresencePopoverPositioner.Calculate(
            workingArea,
            popoverSize);

        Assert.True(position.X >= workingArea.X);
        Assert.True(position.X + popoverSize.Width <= workingArea.X + workingArea.Width);
        Assert.Equal(1920 - 420 - DesktopPresencePopoverPositioner.EdgeMarginPixels, position.X);
    }

    [Fact]
    public void Calculate_KeepsPopoverInsideOffsetScreenWorkingArea()
    {
        var workingArea = new PixelRect(1920, 0, 1920, 1040);
        var popoverSize = new PixelSize(420, 560);

        var position = DesktopPresencePopoverPositioner.Calculate(
            workingArea,
            popoverSize);

        Assert.True(position.X >= workingArea.X);
        Assert.True(position.X + popoverSize.Width <= workingArea.X + workingArea.Width);
        Assert.Equal(1920 + 1920 - 420 - DesktopPresencePopoverPositioner.EdgeMarginPixels, position.X);
    }

    [Fact]
    public void Calculate_KeepsPopoverBelowTopWorkingAreaEdge()
    {
        var workingArea = new PixelRect(0, 40, 1920, 1040);
        var popoverSize = new PixelSize(420, 560);

        var position = DesktopPresencePopoverPositioner.Calculate(
            workingArea,
            popoverSize);

        Assert.Equal(40 + DesktopPresencePopoverPositioner.EdgeMarginPixels, position.Y);
    }

    [Fact]
    public void Calculate_UsesBestEffortPositionWhenScreenIsNarrowerThanPopover()
    {
        var workingArea = new PixelRect(0, 0, 300, 600);
        var popoverSize = new PixelSize(420, 560);

        var position = DesktopPresencePopoverPositioner.Calculate(
            workingArea,
            popoverSize);

        Assert.Equal(0, position.X);
        Assert.True(position.Y >= workingArea.Y);
    }
}
