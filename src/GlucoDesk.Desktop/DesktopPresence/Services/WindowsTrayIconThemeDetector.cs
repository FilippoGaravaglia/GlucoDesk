using System.Globalization;
using System.Runtime.Versioning;
using Microsoft.Win32;

namespace GlucoDesk.Desktop.DesktopPresence.Services;

/// <summary>
/// Detects the Windows system theme used by the taskbar and notification area.
/// </summary>
public static class WindowsTrayIconThemeDetector
{
    private const string PersonalizeRegistryPath =
        @"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize";

    private const string SystemUsesLightThemeValueName = "SystemUsesLightTheme";

    /// <summary>
    /// Determines whether the Windows tray icon should use the light icon variant.
    /// </summary>
    /// <returns>
    /// true when the Windows system tray is expected to use a dark surface; otherwise, false.
    /// </returns>
    public static bool ShouldUseLightIconForCurrentSystemTheme()
    {
        if (!OperatingSystem.IsWindows())
        {
            return false;
        }

        return IsWindowsSystemThemeDark();
    }

    /// <summary>
    /// Determines whether the registry value represents a dark Windows system theme.
    /// </summary>
    /// <param name="registryValue">The raw registry value.</param>
    /// <returns>true when the value represents dark mode; otherwise, false.</returns>
    public static bool IsDarkSystemThemeRegistryValue(object? registryValue)
    {
        return registryValue switch
        {
            int intValue => intValue == 0,
            long longValue => longValue == 0,
            string stringValue when int.TryParse(
                stringValue,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var parsedValue) => parsedValue == 0,
            _ => false,
        };
    }

    #region Helpers

    /// <summary>
    /// Reads the Windows system theme from the registry.
    /// </summary>
    /// <returns>true when Windows system theme is dark; otherwise, false.</returns>
    [SupportedOSPlatform("windows")]
    private static bool IsWindowsSystemThemeDark()
    {
        using var personalizeKey = Registry.CurrentUser.OpenSubKey(PersonalizeRegistryPath);

        var systemUsesLightTheme = personalizeKey?.GetValue(SystemUsesLightThemeValueName);

        return IsDarkSystemThemeRegistryValue(systemUsesLightTheme);
    }

    #endregion
}
