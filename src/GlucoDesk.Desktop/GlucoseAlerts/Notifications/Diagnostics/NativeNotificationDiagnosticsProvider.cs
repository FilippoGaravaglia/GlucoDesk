namespace GlucoDesk.Desktop.GlucoseAlerts.Notifications.Diagnostics;

/// <summary>
/// Provides privacy-safe diagnostics for native notification delivery.
/// </summary>
public sealed class NativeNotificationDiagnosticsProvider
{
    private readonly Func<NativeNotificationPlatform> _platformAccessor;
    private readonly Func<string?> _processPathAccessor;
    private readonly Func<string> _baseDirectoryAccessor;

    /// <summary>
    /// Initializes a new instance of the <see cref="NativeNotificationDiagnosticsProvider"/> class.
    /// </summary>
    /// <param name="platformAccessor">The optional platform accessor.</param>
    /// <param name="processPathAccessor">The optional process path accessor.</param>
    /// <param name="baseDirectoryAccessor">The optional base directory accessor.</param>
    public NativeNotificationDiagnosticsProvider(
        Func<NativeNotificationPlatform>? platformAccessor = null,
        Func<string?>? processPathAccessor = null,
        Func<string>? baseDirectoryAccessor = null)
    {
        _platformAccessor = platformAccessor ?? ResolveCurrentPlatform;
        _processPathAccessor = processPathAccessor ?? (() => Environment.ProcessPath);
        _baseDirectoryAccessor = baseDirectoryAccessor ?? (() => AppContext.BaseDirectory);
    }

    /// <summary>
    /// Creates the default native notification diagnostics provider.
    /// </summary>
    /// <returns>The native notification diagnostics provider.</returns>
    public static NativeNotificationDiagnosticsProvider CreateDefault()
    {
        return new NativeNotificationDiagnosticsProvider();
    }

    /// <summary>
    /// Gets native notification diagnostics for the current process.
    /// </summary>
    /// <returns>The native notification diagnostics.</returns>
    public NativeNotificationDiagnostics GetDiagnostics()
    {
        var platform = _platformAccessor();
        var processName = ResolveProcessName(_processPathAccessor());
        var baseDirectory = _baseDirectoryAccessor();
        var isPackagedAppLikely = IsPackagedAppLikely(
            platform,
            processName,
            baseDirectory);

        return platform switch
        {
            NativeNotificationPlatform.MacOS => new NativeNotificationDiagnostics(
                platform,
                "macOS",
                "AppleScript display notification",
                IsSupportedPlatform: true,
                IsPackagedAppLikely: isPackagedAppLikely,
                DeliveryConfirmationAvailable: false,
                PermissionHint: "If no notification appears, check macOS System Settings → Notifications for Terminal, Visual Studio Code, osascript, Script Editor, Avalonia Application, or GlucoDesk. Also check Focus / Do Not Disturb.",
                PackagingHint: isPackagedAppLikely
                    ? "The app appears to be running with an application identity."
                    : "Development mode detected: notifications may be associated with the host process instead of GlucoDesk."),

            NativeNotificationPlatform.Windows => new NativeNotificationDiagnostics(
                platform,
                "Windows",
                "Windows toast notifications",
                IsSupportedPlatform: true,
                IsPackagedAppLikely: isPackagedAppLikely,
                DeliveryConfirmationAvailable: false,
                PermissionHint: "If no toast appears, check Windows Settings → System → Notifications and Focus assist / Do not disturb.",
                PackagingHint: isPackagedAppLikely
                    ? "The app appears to be running with an application identity."
                    : "Development mode detected: toast delivery may be more reliable once the app is packaged."),

            NativeNotificationPlatform.Linux => new NativeNotificationDiagnostics(
                platform,
                "Linux",
                "not configured",
                IsSupportedPlatform: false,
                IsPackagedAppLikely: isPackagedAppLikely,
                DeliveryConfirmationAvailable: false,
                PermissionHint: "Native Linux desktop notifications are not enabled in this build.",
                PackagingHint: "Use the in-app banner as the reliable alert surface."),

            _ => new NativeNotificationDiagnostics(
                NativeNotificationPlatform.Unsupported,
                "this platform",
                "not configured",
                IsSupportedPlatform: false,
                IsPackagedAppLikely: isPackagedAppLikely,
                DeliveryConfirmationAvailable: false,
                PermissionHint: "Native notifications are not available on this platform.",
                PackagingHint: "Use the in-app banner as the reliable alert surface.")
        };
    }

    /// <summary>
    /// Gets a compact native notification diagnostics message for Settings.
    /// </summary>
    /// <returns>The diagnostics message.</returns>
    public string GetSettingsText()
    {
        return GetDiagnostics().ToSettingsText();
    }

    #region Helpers

    /// <summary>
    /// Resolves the current platform.
    /// </summary>
    /// <returns>The current native notification platform.</returns>
    private static NativeNotificationPlatform ResolveCurrentPlatform()
    {
        if (OperatingSystem.IsMacOS())
        {
            return NativeNotificationPlatform.MacOS;
        }

        if (OperatingSystem.IsWindows())
        {
            return NativeNotificationPlatform.Windows;
        }

        if (OperatingSystem.IsLinux())
        {
            return NativeNotificationPlatform.Linux;
        }

        return NativeNotificationPlatform.Unsupported;
    }

    /// <summary>
    /// Resolves the current process name.
    /// </summary>
    /// <param name="processPath">The process path.</param>
    /// <returns>The process name.</returns>
    private static string ResolveProcessName(string? processPath)
    {
        if (string.IsNullOrWhiteSpace(processPath))
        {
            return string.Empty;
        }

        var normalizedPath = NormalizePathForDetection(processPath);
        var fileName = normalizedPath
            .Split('/', StringSplitOptions.RemoveEmptyEntries)
            .LastOrDefault();

        return string.IsNullOrWhiteSpace(fileName)
            ? string.Empty
            : Path.GetFileNameWithoutExtension(fileName);
    }

    /// <summary>
    /// Detects whether the app likely runs as a packaged application.
    /// </summary>
    /// <param name="platform">The platform.</param>
    /// <param name="processName">The process name.</param>
    /// <param name="baseDirectory">The base directory.</param>
    /// <returns>True when the process appears to be packaged; otherwise false.</returns>
    private static bool IsPackagedAppLikely(
        NativeNotificationPlatform platform,
        string processName,
        string baseDirectory)
    {
        if (platform == NativeNotificationPlatform.MacOS)
        {
            return baseDirectory.Contains(".app/", StringComparison.OrdinalIgnoreCase)
                   || baseDirectory.Contains(".app", StringComparison.OrdinalIgnoreCase);
        }

        if (platform == NativeNotificationPlatform.Windows)
        {
            var normalizedBaseDirectory = NormalizePathForDetection(baseDirectory);

            return !IsDevelopmentHostProcess(processName)
                   && !normalizedBaseDirectory.Contains("/bin/", StringComparison.OrdinalIgnoreCase)
                   && !normalizedBaseDirectory.Contains("/obj/", StringComparison.OrdinalIgnoreCase);
        }

        return false;
    }

    /// <summary>
    /// Normalizes a file-system path for platform-independent diagnostics detection.
    /// </summary>
    /// <param name="path">The file-system path.</param>
    /// <returns>The normalized path.</returns>
    private static string NormalizePathForDetection(string path)
    {
        return path.Replace('\\', '/');
    }

    /// <summary>
    /// Returns whether the process is a common development host.
    /// </summary>
    /// <param name="processName">The process name.</param>
    /// <returns>True when the process is a development host; otherwise false.</returns>
    private static bool IsDevelopmentHostProcess(string processName)
    {
        return string.Equals(processName, "dotnet", StringComparison.OrdinalIgnoreCase)
               || string.Equals(processName, "Code", StringComparison.OrdinalIgnoreCase)
               || string.Equals(processName, "Code Helper", StringComparison.OrdinalIgnoreCase)
               || string.Equals(processName, "VisualStudio", StringComparison.OrdinalIgnoreCase)
               || string.Equals(processName, "devenv", StringComparison.OrdinalIgnoreCase);
    }

    #endregion
}
