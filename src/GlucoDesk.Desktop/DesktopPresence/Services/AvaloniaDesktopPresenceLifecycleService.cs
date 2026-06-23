using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using GlucoDesk.Desktop.DesktopPresence.Enums;
using GlucoDesk.Desktop.DesktopPresence.Formatters;
using GlucoDesk.Desktop.DesktopPresence.Models;
using GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;
using Microsoft.Extensions.Logging;
using AvaloniaApplication = Avalonia.Application;

namespace GlucoDesk.Desktop.DesktopPresence.Services;

/// <summary>
/// Manages the Avalonia tray icon used as GlucoDesk desktop presence indicator.
/// </summary>
public sealed class AvaloniaDesktopPresenceLifecycleService : IDesktopPresenceLifecycleService
{
    private static readonly Uri IconUri = new("avares://GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.png");

    private readonly IDesktopPresenceTextFormatter _textFormatter;
    private readonly ILogger<AvaloniaDesktopPresenceLifecycleService> _logger;

    private TrayIcon? _trayIcon;
    private bool _isStarted;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaDesktopPresenceLifecycleService"/> class.
    /// </summary>
    /// <param name="textFormatter">The desktop presence text formatter.</param>
    /// <param name="logger">The logger.</param>
    public AvaloniaDesktopPresenceLifecycleService(
        IDesktopPresenceTextFormatter textFormatter,
        ILogger<AvaloniaDesktopPresenceLifecycleService> logger)
    {
        _textFormatter = textFormatter;
        _logger = logger;
    }

    /// <inheritdoc />
    public void Start(IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        ArgumentNullException.ThrowIfNull(desktopLifetime);

        RunOnUiThread(() =>
        {
            if (_isStarted)
            {
                return;
            }

            try
            {
                var application = AvaloniaApplication.Current;

                if (application is null)
                {
                    _logger.LogWarning("Desktop presence indicator cannot start because the Avalonia application is not available.");
                    return;
                }

                var initialText = _textFormatter.Format(CreateInitialSnapshot());
                var trayIcon = CreateTrayIcon(desktopLifetime, initialText);

                var trayIcons = new TrayIcons
                {
                    trayIcon
                };

                TrayIcon.SetIcons(application, trayIcons);

                _trayIcon = trayIcon;
                _isStarted = true;

                _logger.LogInformation("Desktop presence indicator started.");
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to start the desktop presence indicator.");
            }
        });
    }

    /// <inheritdoc />
    public void Stop()
    {
        RunOnUiThread(() =>
        {
            if (!_isStarted)
            {
                return;
            }

            try
            {
                var application = AvaloniaApplication.Current;

                if (application is not null)
                {
                    TrayIcon.SetIcons(application, null);
                }

                _trayIcon?.Dispose();
                _trayIcon = null;
                _isStarted = false;

                _logger.LogInformation("Desktop presence indicator stopped.");
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to stop the desktop presence indicator.");
            }
        });
    }

    #region Helpers

    /// <summary>
    /// Creates the initial desktop presence snapshot.
    /// </summary>
    /// <returns>The initial desktop presence snapshot.</returns>
    private static DesktopPresenceSnapshot CreateInitialSnapshot()
    {
        return new DesktopPresenceSnapshot(
            DesktopPresenceDataState.ProviderNotConfigured,
            DisplayValue: null,
            UnitSymbol: "mg/dL",
            TrendText: null,
            ReadingTimestamp: null,
            Now: DateTimeOffset.Now,
            IsPrivacyModeEnabled: false);
    }

    /// <summary>
    /// Creates the Avalonia tray icon.
    /// </summary>
    /// <param name="desktopLifetime">The desktop application lifetime.</param>
    /// <param name="initialText">The initial formatted desktop presence text.</param>
    /// <returns>The created tray icon.</returns>
    private static TrayIcon CreateTrayIcon(
        IClassicDesktopStyleApplicationLifetime desktopLifetime,
        DesktopPresenceText initialText)
    {
        return new TrayIcon
        {
            Icon = LoadTrayIcon(),
            ToolTipText = initialText.Tooltip,
            Menu = CreateTrayMenu(desktopLifetime),
            IsVisible = true
        };
    }

    /// <summary>
    /// Loads the tray icon from application assets.
    /// </summary>
    /// <returns>The loaded window icon.</returns>
    private static WindowIcon LoadTrayIcon()
    {
        using var stream = AssetLoader.Open(IconUri);

        return new WindowIcon(stream);
    }

    /// <summary>
    /// Creates the native tray menu.
    /// </summary>
    /// <param name="desktopLifetime">The desktop application lifetime.</param>
    /// <returns>The native tray menu.</returns>
    private static NativeMenu CreateTrayMenu(IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        var openItem = new NativeMenuItem("Open GlucoDesk");
        openItem.Click += (_, _) => ShowMainWindow(desktopLifetime);

        var quitItem = new NativeMenuItem("Quit GlucoDesk");
        quitItem.Click += (_, _) => desktopLifetime.Shutdown();

        return new NativeMenu
        {
            Items =
            {
                openItem,
                new NativeMenuItemSeparator(),
                quitItem
            }
        };
    }

    /// <summary>
    /// Shows and activates the main application window.
    /// </summary>
    /// <param name="desktopLifetime">The desktop application lifetime.</param>
    private static void ShowMainWindow(IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        var mainWindow = desktopLifetime.MainWindow;

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

    #endregion
}
