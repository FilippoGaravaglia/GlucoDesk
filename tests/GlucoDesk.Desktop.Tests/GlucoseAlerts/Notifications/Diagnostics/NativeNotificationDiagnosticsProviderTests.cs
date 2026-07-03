using GlucoDesk.Desktop.GlucoseAlerts.Notifications.Diagnostics;

namespace GlucoDesk.Desktop.Tests.GlucoseAlerts.Notifications.Diagnostics;

public sealed class NativeNotificationDiagnosticsProviderTests
{
    [Fact]
    public void GetDiagnostics_ShouldReturnMacOsGuidance_ForMacOsDevelopmentMode()
    {
        var provider = new NativeNotificationDiagnosticsProvider(
            () => NativeNotificationPlatform.MacOS,
            () => "/usr/local/share/dotnet/dotnet",
            () => "/Users/test/GlucoDesk/bin/Release/net10.0/");

        var diagnostics = provider.GetDiagnostics();

        Assert.Equal(NativeNotificationPlatform.MacOS, diagnostics.Platform);
        Assert.True(diagnostics.IsSupportedPlatform);
        Assert.False(diagnostics.IsPackagedAppLikely);
        Assert.False(diagnostics.DeliveryConfirmationAvailable);
        Assert.Contains("macOS System Settings", diagnostics.PermissionHint);
        Assert.Contains("Development mode detected", diagnostics.PackagingHint);
    }

    [Fact]
    public void GetDiagnostics_ShouldDetectMacOsPackagedApp()
    {
        var provider = new NativeNotificationDiagnosticsProvider(
            () => NativeNotificationPlatform.MacOS,
            () => "/Applications/GlucoDesk.app/Contents/MacOS/GlucoDesk",
            () => "/Applications/GlucoDesk.app/Contents/MacOS/");

        var diagnostics = provider.GetDiagnostics();

        Assert.True(diagnostics.IsPackagedAppLikely);
        Assert.Contains("application identity", diagnostics.PackagingHint);
    }

    [Fact]
    public void GetDiagnostics_ShouldReturnWindowsGuidance_ForWindowsDevelopmentMode()
    {
        var provider = new NativeNotificationDiagnosticsProvider(
            () => NativeNotificationPlatform.Windows,
            () => @"C:\Program Files\dotnet\dotnet.exe",
            () => @"C:\Projects\GlucoDesk\src\GlucoDesk.Desktop\bin\Release\net10.0\");

        var diagnostics = provider.GetDiagnostics();

        Assert.Equal(NativeNotificationPlatform.Windows, diagnostics.Platform);
        Assert.True(diagnostics.IsSupportedPlatform);
        Assert.False(diagnostics.IsPackagedAppLikely);
        Assert.False(diagnostics.DeliveryConfirmationAvailable);
        Assert.Contains("Windows Settings", diagnostics.PermissionHint);
        Assert.Contains("Development mode detected", diagnostics.PackagingHint);
    }

    [Fact]
    public void GetDiagnostics_ShouldReturnUnsupportedGuidance_ForLinux()
    {
        var provider = new NativeNotificationDiagnosticsProvider(
            () => NativeNotificationPlatform.Linux,
            () => "/usr/bin/dotnet",
            () => "/home/test/GlucoDesk/bin/Release/net10.0/");

        var diagnostics = provider.GetDiagnostics();

        Assert.Equal(NativeNotificationPlatform.Linux, diagnostics.Platform);
        Assert.False(diagnostics.IsSupportedPlatform);
        Assert.False(diagnostics.IsPackagedAppLikely);
        Assert.Contains("not enabled", diagnostics.PermissionHint);
    }

    [Fact]
    public void GetSettingsText_ShouldReturnCompactUserFacingMessage()
    {
        var provider = new NativeNotificationDiagnosticsProvider(
            () => NativeNotificationPlatform.MacOS,
            () => "/usr/local/share/dotnet/dotnet",
            () => "/Users/test/GlucoDesk/bin/Release/net10.0/");

        var text = provider.GetSettingsText();

        Assert.Contains("Native notifications are optional", text);
        Assert.Contains("macOS notification permissions", text);
    }
}
