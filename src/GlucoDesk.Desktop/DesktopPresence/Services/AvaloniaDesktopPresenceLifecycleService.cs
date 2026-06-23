using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using GlucoDesk.Desktop.DesktopPresence.Enums;
using GlucoDesk.Desktop.DesktopPresence.Formatters;
using GlucoDesk.Desktop.DesktopPresence.Models;
using GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Main;
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
    private readonly IDesktopPresenceDashboardTextFormatter _dashboardTextFormatter;
    private readonly ILogger<AvaloniaDesktopPresenceLifecycleService> _logger;

    private TrayIcon? _trayIcon;
    private NativeMenuItem? _statusMenuItem;
    private NativeMenuItem? _refreshNowMenuItem;
    private DashboardViewModel? _dashboardViewModel;
    private bool _isStarted;

    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaDesktopPresenceLifecycleService"/> class.
    /// </summary>
    /// <param name="textFormatter">The desktop presence text formatter.</param>
    /// <param name="dashboardTextFormatter">The dashboard desktop presence text formatter.</param>
    /// <param name="logger">The logger.</param>
    public AvaloniaDesktopPresenceLifecycleService(
        IDesktopPresenceTextFormatter textFormatter,
        IDesktopPresenceDashboardTextFormatter dashboardTextFormatter,
        ILogger<AvaloniaDesktopPresenceLifecycleService> logger)
    {
        _textFormatter = textFormatter;
        _dashboardTextFormatter = dashboardTextFormatter;
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
                var trayIcon = CreateTrayIcon(
                    desktopLifetime,
                    initialText,
                    out var statusMenuItem,
                    out var refreshNowMenuItem);

                var trayIcons = new TrayIcons
                {
                    trayIcon
                };

                TrayIcon.SetIcons(application, trayIcons);

                _trayIcon = trayIcon;
                _statusMenuItem = statusMenuItem;
                _refreshNowMenuItem = refreshNowMenuItem;
                _isStarted = true;

                AttachDashboardState(desktopLifetime);
                RefreshFromDashboardState();

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
                DetachDashboardState();

                var application = AvaloniaApplication.Current;

                if (application is not null)
                {
                    TrayIcon.SetIcons(application, null);
                }

                _trayIcon?.Dispose();
                _trayIcon = null;
                _statusMenuItem = null;
                _refreshNowMenuItem = null;
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
    /// <param name="statusMenuItem">The created status menu item.</param>
    /// <param name="refreshNowMenuItem">The created refresh menu item.</param>
    /// <returns>The created tray icon.</returns>
    private TrayIcon CreateTrayIcon(
        IClassicDesktopStyleApplicationLifetime desktopLifetime,
        DesktopPresenceText initialText,
        out NativeMenuItem statusMenuItem,
        out NativeMenuItem refreshNowMenuItem)
    {
        statusMenuItem = new NativeMenuItem(initialText.MenuHeader)
        {
            IsEnabled = false
        };

        refreshNowMenuItem = new NativeMenuItem("Refresh now")
        {
            IsEnabled = false
        };

        refreshNowMenuItem.Click += (_, _) => _ = RefreshDashboardFromTraySafelyAsync();

        return new TrayIcon
        {
            Icon = LoadTrayIcon(),
            ToolTipText = initialText.Tooltip,
            Menu = CreateTrayMenu(
                desktopLifetime,
                statusMenuItem,
                refreshNowMenuItem),
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
    /// <param name="statusMenuItem">The status menu item.</param>
    /// <param name="refreshNowMenuItem">The refresh menu item.</param>
    /// <returns>The native tray menu.</returns>
    private static NativeMenu CreateTrayMenu(
        IClassicDesktopStyleApplicationLifetime desktopLifetime,
        NativeMenuItem statusMenuItem,
        NativeMenuItem refreshNowMenuItem)
    {
        var openItem = new NativeMenuItem("Open GlucoDesk");
        openItem.Click += (_, _) => ShowMainWindow(desktopLifetime);

        var quitItem = new NativeMenuItem("Quit GlucoDesk");
        quitItem.Click += (_, _) => desktopLifetime.Shutdown();

        return new NativeMenu
        {
            Items =
            {
                statusMenuItem,
                new NativeMenuItemSeparator(),
                openItem,
                refreshNowMenuItem,
                new NativeMenuItemSeparator(),
                quitItem
            }
        };
    }

    /// <summary>
    /// Attaches the desktop presence indicator to dashboard state changes.
    /// </summary>
    /// <param name="desktopLifetime">The desktop application lifetime.</param>
    private void AttachDashboardState(IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        if (desktopLifetime.MainWindow?.DataContext is not MainWindowViewModel mainWindowViewModel)
        {
            _logger.LogWarning("Desktop presence indicator could not find the main window view model.");
            return;
        }

        _dashboardViewModel = mainWindowViewModel.Dashboard;
        _dashboardViewModel.PropertyChanged += OnDashboardPropertyChanged;
    }

    /// <summary>
    /// Detaches the desktop presence indicator from dashboard state changes.
    /// </summary>
    private void DetachDashboardState()
    {
        if (_dashboardViewModel is not null)
        {
            _dashboardViewModel.PropertyChanged -= OnDashboardPropertyChanged;
            _dashboardViewModel = null;
        }
    }

    /// <summary>
    /// Handles dashboard property changes that affect desktop presence text.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The property changed event arguments.</param>
    private void OnDashboardPropertyChanged(
        object? sender,
        PropertyChangedEventArgs e)
    {
        if (!ShouldRefreshFromDashboardProperty(e.PropertyName))
        {
            return;
        }

        RefreshFromDashboardState();
    }

    /// <summary>
    /// Determines whether the specified dashboard property should refresh desktop presence text.
    /// </summary>
    /// <param name="propertyName">The changed property name.</param>
    /// <returns><c>true</c> when desktop presence text should refresh; otherwise, <c>false</c>.</returns>
    private static bool ShouldRefreshFromDashboardProperty(string? propertyName)
    {
        return string.IsNullOrWhiteSpace(propertyName)
               || string.Equals(propertyName, nameof(DashboardViewModel.ProviderDisplayName), StringComparison.Ordinal)
               || string.Equals(propertyName, nameof(DashboardViewModel.LatestValueText), StringComparison.Ordinal)
               || string.Equals(propertyName, nameof(DashboardViewModel.TrendText), StringComparison.Ordinal)
               || string.Equals(propertyName, nameof(DashboardViewModel.FreshnessText), StringComparison.Ordinal)
               || string.Equals(propertyName, nameof(DashboardViewModel.LastUpdatedText), StringComparison.Ordinal)
               || string.Equals(propertyName, nameof(DashboardViewModel.StatusText), StringComparison.Ordinal)
               || string.Equals(propertyName, nameof(DashboardViewModel.IsBusy), StringComparison.Ordinal);
    }

    /// <summary>
    /// Refreshes the dashboard from the tray menu without breaking the desktop presence indicator.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RefreshDashboardFromTraySafelyAsync()
    {
        var dashboardViewModel = _dashboardViewModel;

        if (dashboardViewModel is null)
        {
            UpdateRefreshMenuStateSafely();
            return;
        }

        if (dashboardViewModel.IsBusy)
        {
            UpdateRefreshMenuStateSafely();
            return;
        }

        try
        {
            UpdateRefreshMenuStateSafely();

            await dashboardViewModel
                .RefreshCommand
                .ExecuteAsync(null);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to refresh dashboard data from the desktop presence indicator.");
        }
        finally
        {
            RefreshFromDashboardState();
        }
    }

    /// <summary>
    /// Refreshes the tray icon text from the current dashboard state.
    /// </summary>
    private void RefreshFromDashboardState()
    {
        RunOnUiThread(() =>
        {
            if (_dashboardViewModel is null)
            {
                UpdateRefreshMenuState();
                return;
            }

            var text = _dashboardTextFormatter.Format(
                CreateDashboardState(_dashboardViewModel));

            ApplyDesktopPresenceText(text);
            UpdateRefreshMenuState();
        });
    }

    /// <summary>
    /// Updates the refresh menu item on the UI thread.
    /// </summary>
    private void UpdateRefreshMenuStateSafely()
    {
        RunOnUiThread(UpdateRefreshMenuState);
    }

    /// <summary>
    /// Updates the refresh menu item state.
    /// </summary>
    private void UpdateRefreshMenuState()
    {
        if (_refreshNowMenuItem is null)
        {
            return;
        }

        if (_dashboardViewModel is null)
        {
            _refreshNowMenuItem.Header = "Refresh now";
            _refreshNowMenuItem.IsEnabled = false;
            return;
        }

        if (_dashboardViewModel.IsBusy)
        {
            _refreshNowMenuItem.Header = "Refreshing...";
            _refreshNowMenuItem.IsEnabled = false;
            return;
        }

        _refreshNowMenuItem.Header = "Refresh now";
        _refreshNowMenuItem.IsEnabled = true;
    }

    /// <summary>
    /// Creates desktop presence dashboard state from the dashboard view model.
    /// </summary>
    /// <param name="dashboardViewModel">The dashboard view model.</param>
    /// <returns>The desktop presence dashboard state.</returns>
    private static DesktopPresenceDashboardState CreateDashboardState(DashboardViewModel dashboardViewModel)
    {
        return new DesktopPresenceDashboardState(
            dashboardViewModel.ProviderDisplayName,
            dashboardViewModel.LatestValueText,
            dashboardViewModel.TrendText,
            dashboardViewModel.FreshnessText,
            dashboardViewModel.LastUpdatedText,
            dashboardViewModel.StatusText);
    }

    /// <summary>
    /// Applies formatted desktop presence text to the tray icon and menu.
    /// </summary>
    /// <param name="text">The formatted desktop presence text.</param>
    private void ApplyDesktopPresenceText(DesktopPresenceText text)
    {
        if (_trayIcon is not null)
        {
            _trayIcon.ToolTipText = text.Tooltip;
        }

        if (_statusMenuItem is not null)
        {
            _statusMenuItem.Header = text.MenuHeader;
        }
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
