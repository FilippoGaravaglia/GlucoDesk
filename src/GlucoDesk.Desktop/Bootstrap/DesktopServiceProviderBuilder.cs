using GlucoDesk.Application.Common.DependencyInjection;
using GlucoDesk.Desktop.Bootstrap.Providers.DependencyInjection;
using GlucoDesk.Desktop.Bootstrap.Providers.DexcomShare;
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Dashboard.Options;
using GlucoDesk.Desktop.ViewModels.Main;
using GlucoDesk.Desktop.ViewModels.Settings;
using GlucoDesk.Desktop.Views.Main;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.History.DependencyInjection;
using GlucoDesk.Infrastructure.Settings.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Desktop.Bootstrap;

/// <summary>
/// Builds the dependency injection container used by the desktop application.
/// </summary>
internal static class DesktopServiceProviderBuilder
{
    /// <summary>
    /// Builds the desktop service provider.
    /// </summary>
    /// <returns>The configured service provider.</returns>
    public static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddGlucoDeskApplication();
        services.AddDesktopCgmProviders();
        services.AddDesktopDexcomShareProviderIfConfigured();
        services.AddJsonApplicationSettingsStore();
        services.AddJsonGlucoseHistoryStore();
        services.AddDesktopShell();

        return services.BuildServiceProvider(
            new ServiceProviderOptions
            {
                ValidateOnBuild = true,
                ValidateScopes = true
            });
    }

    #region Helpers

    /// <summary>
    /// Registers Dexcom Share as a desktop CGM provider when environment configuration is available.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void AddDesktopDexcomShareProviderIfConfigured(this IServiceCollection services)
    {
        var dexcomShareOptions = DesktopDexcomShareProviderOptions.FromEnvironment();

        if (!dexcomShareOptions.IsConfigured)
        {
            return;
        }

        services.AddDexcomShareCgmProvider(dexcomShareOptions);
    }

    /// <summary>
    /// Registers desktop windows, view models and desktop-specific options.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void AddDesktopShell(this IServiceCollection services)
    {
        services.AddSingleton(DashboardRefreshOptions.Default);

        services.AddTransient<MainWindow>();
        services.AddTransient<MainWindowViewModel>();
        services.AddTransient<DashboardViewModel>();
        services.AddTransient<SettingsViewModel>();
    }

    #endregion
}