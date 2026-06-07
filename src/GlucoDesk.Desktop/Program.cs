using Avalonia;

namespace GlucoDesk.Desktop;

internal static class Program
{
    [STAThread]
    public static void Main(string[] args)
    {
        BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);
    }

    #region Helpers

    /// <summary>
    /// Builds the Avalonia application instance.
    /// </summary>
    /// <returns>The configured Avalonia application builder.</returns>
    private static AppBuilder BuildAvaloniaApp()
    {
        return AppBuilder
            .Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace();
    }

    #endregion
}