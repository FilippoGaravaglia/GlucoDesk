using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GlucoDesk.Desktop.BackgroundSync.Services.Abstractions;
using GlucoDesk.Desktop.Bootstrap;
using GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;
using GlucoDesk.Desktop.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Desktop.Views.Main;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

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

            desktop.MainWindow.Opened += (_, _) =>
            {
                StartDesktopPresenceSafely(
                    desktop,
                    _applicationScope.ServiceProvider);

                _ = StartBackgroundSyncSafelyAsync(_applicationScope.ServiceProvider);
                _ = RunStartupHistoryContinuitySyncSafelyAsync(_applicationScope.ServiceProvider);
            };

            desktop.Exit += async (_, _) =>
            {
                StopDesktopPresenceSafely(_applicationScope.ServiceProvider);

                await StopBackgroundSyncSafelyAsync(_applicationScope.ServiceProvider)
                    .ConfigureAwait(false);

                DisposeServices();
            };
        }

        base.OnFrameworkInitializationCompleted();
    }

    #region Helpers

    /// <summary>
    /// Starts the desktop presence indicator without breaking application startup.
    /// </summary>
    /// <param name="desktop">The desktop application lifetime.</param>
    /// <param name="serviceProvider">The service provider.</param>
    private static void StartDesktopPresenceSafely(
        IClassicDesktopStyleApplicationLifetime desktop,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(desktop);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        try
        {
            var lifecycleService = serviceProvider
                .GetService<IDesktopPresenceLifecycleService>();

            lifecycleService?.Start(desktop);
        }
        catch (Exception exception)
        {
            LogSafely(
                serviceProvider,
                exception,
                "Unexpected error while starting the desktop presence indicator.");
        }
    }

    /// <summary>
    /// Stops the desktop presence indicator without breaking application shutdown.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    private static void StopDesktopPresenceSafely(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        try
        {
            var lifecycleService = serviceProvider
                .GetService<IDesktopPresenceLifecycleService>();

            lifecycleService?.Stop();
        }
        catch (Exception exception)
        {
            LogSafely(
                serviceProvider,
                exception,
                "Unexpected error while stopping the desktop presence indicator.");
        }
    }

    /// <summary>
    /// Starts the desktop background sync lifecycle without breaking application startup.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task StartBackgroundSyncSafelyAsync(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

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
        catch (Exception exception)
        {
            LogSafely(
                serviceProvider,
                exception,
                "Unexpected error while starting the desktop background sync lifecycle.");
        }
    }

    /// <summary>
    /// Runs startup history continuity synchronization without blocking the Avalonia UI startup flow.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task RunStartupHistoryContinuitySyncSafelyAsync(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        try
        {
            var coordinator = serviceProvider
                .GetService<IDesktopHistoryContinuitySyncCoordinator>();

            if (coordinator is null)
            {
                return;
            }

            _ = await coordinator
                .RunStartupSyncAsync(CancellationToken.None)
                .ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            LogSafely(
                serviceProvider,
                exception,
                "Unexpected error while running startup history continuity synchronization.");
        }
    }

    /// <summary>
    /// Stops the desktop background sync lifecycle without breaking application shutdown.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    private static async Task StopBackgroundSyncSafelyAsync(IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

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
        catch (Exception exception)
        {
            LogSafely(
                serviceProvider,
                exception,
                "Unexpected error while stopping the desktop background sync lifecycle.");
        }
    }

    /// <summary>
    /// Logs an application startup or shutdown error when logging services are available.
    /// </summary>
    /// <param name="serviceProvider">The service provider.</param>
    /// <param name="exception">The exception to log.</param>
    /// <param name="message">The log message.</param>
    private static void LogSafely(
        IServiceProvider serviceProvider,
        Exception exception,
        string message)
    {
        try
        {
            var logger = serviceProvider.GetService<ILogger<App>>();

            logger?.LogError(
                exception,
                "{Message}",
                message);
        }
        catch
        {
            // Logging must never break application startup or shutdown.
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