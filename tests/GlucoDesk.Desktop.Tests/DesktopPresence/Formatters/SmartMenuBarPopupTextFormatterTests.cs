using GlucoDesk.Desktop.DesktopPresence.Formatters;
using GlucoDesk.Desktop.DesktopPresence.Models;

namespace GlucoDesk.Desktop.Tests.DesktopPresence.Formatters;

public sealed class SmartMenuBarPopupTextFormatterTests
{
    [Fact]
    public void Format_ShouldExposeAmbientSummaryInPopupDetails()
    {
        var formatter = new DesktopPresenceDashboardTextFormatter();

        var state = new DesktopPresenceDashboardState(
            ProviderDisplayName: "Dexcom Share",
            LatestValueText: "123 mg/dL",
            TrendText: "→ Stable",
            FreshnessText: "Near real-time",
            LastUpdatedText: "09:48",
            StatusText: "Stable and in range.",
            IsPrivacyModeEnabled: false);

        var text = formatter.Format(state);

        Assert.Contains("123 mg/dL", text.MenuHeader, StringComparison.Ordinal);
        Assert.Contains("Stable and in range.", text.Tooltip, StringComparison.Ordinal);
    }

    [Fact]
    public void Format_ShouldKeepPopupPrivacySafe_WhenPrivacyModeIsEnabled()
    {
        var formatter = new DesktopPresenceDashboardTextFormatter();

        var state = new DesktopPresenceDashboardState(
            ProviderDisplayName: "Dexcom Share",
            LatestValueText: "123 mg/dL",
            TrendText: "→ Stable",
            FreshnessText: "Near real-time",
            LastUpdatedText: "09:48",
            StatusText: "In range",
            IsPrivacyModeEnabled: true);

        var text = formatter.Format(state);

        Assert.Equal("Glucose hidden", text.MenuHeader);
        Assert.DoesNotContain("123 mg/dL", text.Tooltip, StringComparison.Ordinal);
    }
}
