using GlucoDesk.Application.Cgm.BackgroundSync.DependencyInjection;
using GlucoDesk.Application.Common.DependencyInjection;
using GlucoDesk.Desktop.BackgroundSync.Options;
using GlucoDesk.Desktop.BackgroundSync.Services;
using GlucoDesk.Desktop.BackgroundSync.Services.Abstractions;
using GlucoDesk.Desktop.Bootstrap.Providers.DependencyInjection;
using GlucoDesk.Desktop.ViewModels.Account;
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Dashboard.Options;
using GlucoDesk.Desktop.ViewModels.Main;
using GlucoDesk.Desktop.ViewModels.Settings;
using GlucoDesk.Desktop.Views.Main;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.History.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.WidgetState.DependencyInjection;
using GlucoDesk.Infrastructure.Settings.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using GlucoDesk.Desktop.BackgroundSync.Dispatching;
using GlucoDesk.Desktop.BackgroundSync.Dispatching.Abstractions;
using GlucoDesk.Desktop.ViewModels.BackgroundSync;
using GlucoDesk.Infrastructure.Cgm.Diary.Excel.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.Diary.Pdf.DependencyInjection;
using GlucoDesk.Desktop.Diary.Services;
using GlucoDesk.Desktop.Diary.Services.Abstractions;
using GlucoDesk.Desktop.ViewModels.Diary;

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
        services.AddDexcomShareCgmProvider();
        services.AddJsonApplicationSettingsStore();
        services.AddJsonGlucoseHistoryStore();
        services.AddGlycemicDiaryExcelExport();
        services.AddGlycemicDiaryPdfExport();
        services.AddJsonWidgetStateStore();
        services.AddCgmBackgroundSync();
        services.AddDesktopBackgroundSyncLifecycle();
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
    /// Registers desktop background sync lifecycle services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void AddDesktopBackgroundSyncLifecycle(this IServiceCollection services)
    {
        services.AddSingleton(DesktopBackgroundSyncLifecycleOptions.Default);
        services.AddSingleton<IBackgroundSyncUiDispatcher, AvaloniaBackgroundSyncUiDispatcher>();
        services.AddSingleton<IDesktopBackgroundSyncLifecycleService, DesktopBackgroundSyncLifecycleService>();
        services.AddSingleton<BackgroundSyncStatusViewModel>();
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
        services.AddTransient<DiaryViewModel>();
        services.AddTransient<AccountViewModel>();
        services.AddTransient<SettingsViewModel>();
        services.AddSingleton<IDiaryExportFileSaveService, AvaloniaDiaryExportFileSaveService>();
    }

    #endregion
}