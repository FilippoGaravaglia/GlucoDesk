using Avalonia.Controls;
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
using GlucoDesk.Desktop.Localization;
using GlucoDesk.Desktop.ViewModels.Onboarding;
using GlucoDesk.Desktop.Views.Onboarding;
using GlucoDesk.Desktop.Onboarding;
namespace GlucoDesk.Desktop;

public partial class App : Avalonia.Application
{
    private ServiceProvider? _serviceProvider;
    private IServiceScope? _applicationScope;
    private bool _isExplicitShutdownRequested;
    private bool _isMainWindowHideInProgress;
    private bool _hasStartedDesktopRuntime;

    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        global::GlucoDesk.Desktop.Localization.LocalizationManager.InitializeIfNeeded();
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        _serviceProvider =
            DesktopServiceProviderBuilder.BuildServiceProvider();

        _applicationScope =
            _serviceProvider.CreateScope();

        if (ApplicationLifetime
            is IClassicDesktopStyleApplicationLifetime desktop)
        {
            ConfigureDesktopLifetime(desktop);
        }

        base.OnFrameworkInitializationCompleted();
    }

    #region Helpers

    /// <summary>
    /// Configures desktop lifetime events and selects the initial window.
    /// </summary>
    private void ConfigureDesktopLifetime(
        IClassicDesktopStyleApplicationLifetime desktop)
    {
        ArgumentNullException.ThrowIfNull(desktop);

        var serviceProvider =
            _applicationScope?.ServiceProvider
            ?? throw new InvalidOperationException(
                "The application service scope is not available.");

        desktop.ShutdownMode =
            ShutdownMode.OnExplicitShutdown;

        RegisterDesktopActivationHandler(desktop);

        desktop.ShutdownRequested += (_, _) =>
        {
            _isExplicitShutdownRequested = true;
        };

        desktop.Exit += async (_, _) =>
        {
            _isExplicitShutdownRequested = true;

            StopDesktopPresenceSafely(serviceProvider);

            await StopBackgroundSyncSafelyAsync(serviceProvider)
                .ConfigureAwait(false);

            DisposeServices();
        };

        if (ShouldShowLanguageOnboarding())
        {
            ConfigureLanguageOnboarding(
                desktop,
                serviceProvider);

            return;
        }

        if (ShouldShowFeatureTour(serviceProvider))
        {
            ConfigureFeatureTour(
                desktop,
                serviceProvider);

            return;
        }

        ConfigureMainApplicationWindow(
            desktop,
            serviceProvider,
            showImmediately: false);
    }

    /// <summary>
    /// Determines whether the first-launch language experience is required.
    /// </summary>
    private static bool ShouldShowLanguageOnboarding()
    {
        var preference =
            LanguagePreferenceStore.ReadPreference();

        var forceValue =
            Environment.GetEnvironmentVariable(
                "GLUCODESK_FORCE_LANGUAGE_ONBOARDING");

        return LanguageOnboardingLaunchPolicy.ShouldShow(
            preference.HasExplicitPreference,
            forceValue);
    }

    /// <summary>
    /// Configures the first-launch language window.
    /// </summary>
    private void ConfigureLanguageOnboarding(
        IClassicDesktopStyleApplicationLifetime desktop,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(desktop);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var onboardingWindow = serviceProvider
            .GetRequiredService<LanguageOnboardingWindow>();

        if (onboardingWindow.DataContext
            is not LanguageOnboardingViewModel viewModel)
        {
            throw new InvalidOperationException(
                "The language onboarding window does not expose "
                + "the expected view model.");
        }

        var onboardingCompleted = false;

        desktop.MainWindow = onboardingWindow;

        viewModel.Completed += OnOnboardingCompleted;
        onboardingWindow.Closed += OnOnboardingClosed;

        void OnOnboardingCompleted(
            object? sender,
            LanguageOnboardingCompletedEventArgs eventArgs)
        {
            _ = sender;
            _ = eventArgs;

            if (onboardingCompleted)
            {
                return;
            }

            onboardingCompleted = true;

            viewModel.Completed -= OnOnboardingCompleted;
            onboardingWindow.Closed -= OnOnboardingClosed;

            onboardingWindow.Close();

            ConfigureFeatureTour(
                desktop,
                serviceProvider);
        }

        void OnOnboardingClosed(
            object? sender,
            EventArgs eventArgs)
        {
            _ = sender;
            _ = eventArgs;

            if (onboardingCompleted
                || _isExplicitShutdownRequested)
            {
                return;
            }

            _isExplicitShutdownRequested = true;
            desktop.Shutdown();
        }
    }

    /// <summary>
    /// Determines whether the localized feature tour is required.
    /// </summary>
    private static bool ShouldShowFeatureTour(
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var preferenceStore = serviceProvider
            .GetRequiredService<FeatureTourPreferenceStore>();

        var forceValue =
            Environment.GetEnvironmentVariable(
                "GLUCODESK_FORCE_FEATURE_TOUR");

        return FeatureTourLaunchPolicy.ShouldShow(
            preferenceStore.HasCompletedCurrentTour(),
            forceValue);
    }

    /// <summary>
    /// Configures the first-run localized product tour.
    /// </summary>
    private void ConfigureFeatureTour(
        IClassicDesktopStyleApplicationLifetime desktop,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(desktop);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        if (!ShouldShowFeatureTour(serviceProvider))
        {
            ConfigureMainApplicationWindow(
                desktop,
                serviceProvider,
                showImmediately: true);

            return;
        }

        var tourWindow = serviceProvider
            .GetRequiredService<FeatureTourWindow>();

        if (tourWindow.DataContext
            is not FeatureTourViewModel viewModel)
        {
            throw new InvalidOperationException(
                "The feature tour window does not expose "
                + "the expected view model.");
        }

        var tourCompleted = false;

        desktop.MainWindow = tourWindow;

        viewModel.Completed += OnTourCompleted;
        tourWindow.Show();
        tourWindow.Activate();
        tourWindow.Closed += OnTourClosed;

        void OnTourCompleted(
            object? sender,
            FeatureTourCompletedEventArgs eventArgs)
        {
            _ = sender;
            _ = eventArgs;

            if (tourCompleted)
            {
                return;
            }

            tourCompleted = true;

            viewModel.Completed -= OnTourCompleted;
            tourWindow.Closed -= OnTourClosed;
            viewModel.Dispose();

            ConfigureMainApplicationWindow(
                desktop,
                serviceProvider,
                showImmediately: true);

            tourWindow.Close();
        }

        void OnTourClosed(
            object? sender,
            EventArgs eventArgs)
        {
            _ = sender;
            _ = eventArgs;

            if (tourCompleted
                || _isExplicitShutdownRequested)
            {
                return;
            }

            viewModel.Completed -= OnTourCompleted;
            tourWindow.Closed -= OnTourClosed;
            viewModel.Dispose();

            _isExplicitShutdownRequested = true;
            desktop.Shutdown();
        }
    }

    /// <summary>
    /// Creates and configures the normal GlucoDesk main window.
    /// </summary>
    private void ConfigureMainApplicationWindow(
        IClassicDesktopStyleApplicationLifetime desktop,
        IServiceProvider serviceProvider,
        bool showImmediately)
    {
        ArgumentNullException.ThrowIfNull(desktop);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        var mainWindow = serviceProvider
            .GetRequiredService<MainWindow>();

        desktop.MainWindow = mainWindow;

        mainWindow.Closing += (_, eventArgs) =>
        {
            HandleMainWindowClosing(
                desktop,
                eventArgs);
        };

        mainWindow.Opened += (_, _) =>
        {
            StartDesktopRuntime(
                desktop,
                serviceProvider);
        };

        if (!showImmediately)
        {
            return;
        }

        mainWindow.Show();
        mainWindow.Activate();
    }

    /// <summary>
    /// Starts tray, background synchronization and history continuity once.
    /// </summary>
    private void StartDesktopRuntime(
        IClassicDesktopStyleApplicationLifetime desktop,
        IServiceProvider serviceProvider)
    {
        ArgumentNullException.ThrowIfNull(desktop);
        ArgumentNullException.ThrowIfNull(serviceProvider);

        if (_hasStartedDesktopRuntime)
        {
            return;
        }

        _hasStartedDesktopRuntime = true;

        StartDesktopPresenceSafely(
            desktop,
            serviceProvider);

        _ = StartBackgroundSyncSafelyAsync(
            serviceProvider);

        _ = RunStartupHistoryContinuitySyncSafelyAsync(
            serviceProvider);
    }


    /// <summary>
    /// Handles main window close requests without terminating the background desktop companion.
    /// </summary>
    /// <param name="desktop">The desktop application lifetime.</param>
    /// <param name="eventArgs">The window closing event arguments.</param>
    private void HandleMainWindowClosing(
        IClassicDesktopStyleApplicationLifetime desktop,
        WindowClosingEventArgs eventArgs)
    {
        ArgumentNullException.ThrowIfNull(desktop);
        ArgumentNullException.ThrowIfNull(eventArgs);

        if (_isExplicitShutdownRequested)
        {
            return;
        }

        eventArgs.Cancel = true;

        var mainWindow = desktop.MainWindow;

        if (mainWindow is null)
        {
            return;
        }

        if (_isMainWindowHideInProgress)
        {
            return;
        }

        if (mainWindow.WindowState == WindowState.FullScreen)
        {
            _isMainWindowHideInProgress = true;
            _ = HideMainWindowAfterLeavingFullscreenAsync(mainWindow);
            return;
        }

        mainWindow.Hide();
    }

    /// <summary>
    /// Leaves fullscreen first and then hides the main window after the macOS fullscreen transition.
    /// </summary>
    /// <param name="mainWindow">The main application window.</param>
    /// <returns>A task representing the asynchronous hide operation.</returns>
    private async Task HideMainWindowAfterLeavingFullscreenAsync(Window mainWindow)
    {
        ArgumentNullException.ThrowIfNull(mainWindow);

        try
        {
            mainWindow.WindowState = WindowState.Normal;

            await Task.Delay(TimeSpan.FromMilliseconds(650))
                .ConfigureAwait(false);

            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (!_isExplicitShutdownRequested)
                {
                    mainWindow.Hide();
                }
            });
        }
        catch
        {
            // Closing from fullscreen is best-effort on macOS/Avalonia and must not break the app lifecycle.
        }
        finally
        {
            Dispatcher.UIThread.Post(() =>
            {
                _isMainWindowHideInProgress = false;
            });
        }
    }

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

        if (mainWindow.WindowState == WindowState.Minimized)
        {
            mainWindow.WindowState = WindowState.Normal;
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
