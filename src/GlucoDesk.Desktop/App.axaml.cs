using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using GlucoDesk.Desktop.BackgroundSync.Services.Abstractions;
using GlucoDesk.Desktop.Bootstrap;
using GlucoDesk.Desktop.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;
using GlucoDesk.Desktop.Views.Main;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace GlucoDesk.Desktop;

public partial class App : Avalonia.Application
{
    private ServiceProvider? _serviceProvider;
    private IServiceScope? _applicationScope;
    private bool _isExplicitShutdownRequested;

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
            desktop.ShutdownMode = Avalonia.Controls.ShutdownMode.OnExplicitShutdown;

            desktop.MainWindow = _applicationScope
                .ServiceProvider
                .GetRequiredService<MainWindow>();

            RegisterDesktopActivationHandler(desktop);

            desktop.ShutdownRequested += (_, _) =>
            {
                _isExplicitShutdownRequested = true;
            };

            desktop.MainWindow.Closing += (_, eventArgs) =>
            {
                if (_isExplicitShutdownRequested)
                {
                    return;
                }

                eventArgs.Cancel = true;
                desktop.MainWindow.Hide();
            };

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
                _isExplicitShutdownRequested = true;

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
    /// Registers desktop activation handling so the main window can be reopened from the macOS Dock.
    /// </summary>
    /// <param name="desktop">The desktop application lifetime.</param>
    private void RegisterDesktopActivationHandler(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var activatableLifetime = TryGetFeature(typeof(IActivatableLifetime)) as IActivatableLifetime;

        if (activatableLifetime is null)
        {
            return;
        }

        activatableLifetime.Activated += (_, eventArgs) =>
        {
            if (eventArgs.Kind != ActivationKind.Reopen)
            {
                return;
            }

            ShowMainWindowSafely(
                desktop,
                _applicationScope?.ServiceProvider);
        };
    }

    /// <summary>
    /// Shows and activates the main application window without breaking application activation.
    /// </summary>
    /// <param name="desktop">The desktop application lifetime.</param>
    /// <param name="serviceProvider">The service provider, when available.</param>
    private static void ShowMainWindowSafely(
        IClassicDesktopStyleApplicationLifetime desktop,
        IServiceProvider? serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(desktop);

        try
        {
            RunOnUiThread(() => ShowMainWindow(desktop));
        }
        catch (Exception exception)
        {
            if (serviceProvider is not null)
            {
                LogSafely(
                    serviceProvider,
                    exception,
                    "Unexpected error while reopening the main window from desktop activation.");
            }
        }
    }

    /// <summary>
    /// Shows and activates the main application window.
    /// </summary>
    /// <param name="desktop">The desktop application lifetime.</param>
    private static void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktop)
    {
        var mainWindow = desktop.MainWindow;

        if (mainWindow is null)
        {
            return;
        }

        if (mainWindow.WindowState == Avalonia.Controls.WindowState.Minimized)
        {
            mainWindow.WindowState = Avalonia.Controls.WindowState.Normal;
        }

        mainWindow.Show();
        mainWindow.Activate();
    }

    /// <summary>
    /// Runs the specified action on the Avalonia UI thread.
    /// </summary>
    /// <param name="action">The action to run.</param>
    private static void RunOnUiThread(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.UIThread.Post(action);
    }

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
