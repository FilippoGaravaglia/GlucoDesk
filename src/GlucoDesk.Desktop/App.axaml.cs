using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GlucoDesk.Desktop.BackgroundSync.Services.Abstractions;
using GlucoDesk.Desktop.Bootstrap;
using GlucoDesk.Desktop.Views.Main;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Desktop;

public partial class App : Avalonia.Application
{
    private ServiceProvider? _serviceProvider;
    private IServiceScope? _applicationScope;

    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        _serviceProvider = DesktopServiceProviderBuilder.BuildServiceProvider();
        _applicationScope = _serviceProvider.CreateScope();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = _applicationScope
                .ServiceProvider
                .GetRequiredService<MainWindow>();

            desktop.MainWindow.Opened += async (_, _) =>
            {
                await StartBackgroundSyncSafelyAsync(_applicationScope.ServiceProvider)
                    .ConfigureAwait(false);
            };

            desktop.Exit += async (_, _) =>
            {
                await StopBackgroundSyncSafelyAsync(_applicationScope.ServiceProvider)
                    .ConfigureAwait(false);

                DisposeServices();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    #region Helpers

    /// <summary>
    /// Starts the desktop background sync lifecycle without breaking application startup.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    private static async Task StartBackgroundSyncSafelyAsync(IServiceProvider serviceProvider)
    {
        try
        {
            var lifecycleService = serviceProvider
                .GetService<IDesktopBackgroundSyncLifecycleService>();

            if (lifecycleService is null)
            {
                return;
            }

            _ = await lifecycleService
                .StartAsync(CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch
        {
            // Background sync startup must never break the desktop app startup.
        }
    }

    /// <summary>
    /// Stops the desktop background sync lifecycle without breaking application shutdown.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    private static async Task StopBackgroundSyncSafelyAsync(IServiceProvider serviceProvider)
    {
        try
        {
            var lifecycleService = serviceProvider
                .GetService<IDesktopBackgroundSyncLifecycleService>();

            if (lifecycleService is null)
            {
                return;
            }

            _ = await lifecycleService
                .StopAsync(CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch
        {
            // Background sync shutdown must never block or break the desktop app shutdown.
        }
    }

    /// <summary>
    /// Disposes application-level dependency injection services.
    /// </summary>
    private void DisposeServices()
    {
        _applicationScope?.Dispose();
        _serviceProvider?.Dispose();

        _applicationScope = null;
        _serviceProvider = null;
    }

    #endregion
}