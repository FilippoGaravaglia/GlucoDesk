using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Threading;
using GlucoDesk.Desktop.DesktopPresence.Enums;
using GlucoDesk.Desktop.DesktopPresence.Formatters;
using GlucoDesk.Desktop.DesktopPresence.Models;
using GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;
using GlucoDesk.Desktop.DesktopPresence.Windows;
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
    private const int PopoverWidth = 340;
    private const int PopoverMargin = 14;

    private static readonly Uri DefaultTrayIconUri = new("avares://GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.png");
    private static readonly Uri MacOsMenuBarIconUri = new("avares://GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon.png");

    private readonly IDesktopPresenceTextFormatter _textFormatter;
    private readonly IDesktopPresenceDashboardTextFormatter _dashboardTextFormatter;
    private readonly ILogger<AvaloniaDesktopPresenceLifecycleService> _logger;

    private TrayIcon? _trayIcon;
    private NativeMenuItem? _statusMenuItem;
    private NativeMenuItem? _presencePanelMenuItem;
    private DesktopPresencePopoverWindow? _popoverWindow;
    private DashboardViewModel? _dashboardViewModel;
    private bool _isStarted;
    private bool _isPrivacyModeEnabled;

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
                    out var presencePanelMenuItem);

                var trayIcons = new TrayIcons
                {
                    trayIcon
                };

                TrayIcon.SetIcons(application, trayIcons);

                _trayIcon = trayIcon;
                _statusMenuItem = statusMenuItem;
                _presencePanelMenuItem = presencePanelMenuItem;
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
                ClosePopover();

                DetachDashboardState();

                var application = AvaloniaApplication.Current;

                if (application is not null)
                {
                    TrayIcon.SetIcons(application, null);
                }

                _trayIcon?.Dispose();
                _trayIcon = null;
                _statusMenuItem = null;
                _presencePanelMenuItem = null;
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
    /// <param name="presencePanelMenuItem">The created presence panel menu item.</param>
    /// <returns>The created tray icon.</returns>
    private TrayIcon CreateTrayIcon(
        IClassicDesktopStyleApplicationLifetime desktopLifetime,
        DesktopPresenceText initialText,
        out NativeMenuItem statusMenuItem,
        out NativeMenuItem presencePanelMenuItem)
    {
        statusMenuItem = new NativeMenuItem(initialText.MenuHeader)
        {
            IsEnabled = false
        };

        presencePanelMenuItem = new NativeMenuItem("Show presence panel");
        presencePanelMenuItem.Click += (_, _) => TogglePopover(desktopLifetime);

        return new TrayIcon
        {
            Icon = LoadTrayIcon(),
            ToolTipText = initialText.Tooltip,
            Menu = CreateTrayMenu(
                desktopLifetime,
                statusMenuItem,
                presencePanelMenuItem),
            IsVisible = true
        };
    }

    /// <summary>
    /// Gets the platform-specific tray icon asset URI.
    /// </summary>
    /// <returns>The tray icon asset URI.</returns>
    private static Uri GetTrayIconUri()
    {
        return OperatingSystem.IsMacOS()
            ? MacOsMenuBarIconUri
            : DefaultTrayIconUri;
    }

    /// <summary>
    /// Loads the tray icon from application assets.
    /// </summary>
    /// <returns>The loaded window icon.</returns>
    private static WindowIcon LoadTrayIcon()
    {
        using var stream = AssetLoader.Open(GetTrayIconUri());

        return new WindowIcon(stream);
    }

    /// <summary>
    /// Creates the native tray menu.
    /// </summary>
    /// <param name="desktopLifetime">The desktop application lifetime.</param>
    /// <param name="statusMenuItem">The status menu item.</param>
    /// <param name="presencePanelMenuItem">The presence panel menu item.</param>
    /// <returns>The native tray menu.</returns>
    private static NativeMenu CreateTrayMenu(
        IClassicDesktopStyleApplicationLifetime desktopLifetime,
        NativeMenuItem statusMenuItem,
        NativeMenuItem presencePanelMenuItem)
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
                presencePanelMenuItem,
                openItem,
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
    /// Toggles the persistent desktop presence popover.
    /// </summary>
    /// <param name="desktopLifetime">The desktop application lifetime.</param>
    private void TogglePopover(IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        RunOnUiThread(() =>
        {
            if (_popoverWindow is not null)
            {
                ClosePopover();
                return;
            }

            ShowPopover(desktopLifetime);
        });
    }

    /// <summary>
    /// Shows the persistent desktop presence popover.
    /// </summary>
    /// <param name="desktopLifetime">The desktop application lifetime.</param>
    private void ShowPopover(IClassicDesktopStyleApplicationLifetime desktopLifetime)
    {
        var popoverWindow = new DesktopPresencePopoverWindow(
            RefreshDashboardFromTraySafelyAsync,
            TogglePrivacyMode,
            () => ShowMainWindow(desktopLifetime),
            () => desktopLifetime.Shutdown());

        popoverWindow.Position = CalculatePopoverPosition(desktopLifetime.MainWindow);
        popoverWindow.Closed += (_, _) =>
        {
            if (ReferenceEquals(_popoverWindow, popoverWindow))
            {
                _popoverWindow = null;
                UpdatePresencePanelMenuState();
            }
        };

        _popoverWindow = popoverWindow;

        UpdatePresencePanelMenuState();
        RefreshPopoverState();

        popoverWindow.Show();
        popoverWindow.Activate();
    }

    /// <summary>
    /// Calculates the popover position near the top-right area of the primary screen.
    /// </summary>
    /// <param name="mainWindow">The main window.</param>
    /// <returns>The calculated popover position.</returns>
    private static PixelPoint CalculatePopoverPosition(Window? mainWindow)
    {
        var screen = mainWindow?.Screens.Primary;

        if (screen is null)
        {
            return new PixelPoint(PopoverMargin, PopoverMargin);
        }

        var workingArea = screen.WorkingArea;

        return new PixelPoint(
            workingArea.X + workingArea.Width - PopoverWidth - PopoverMargin,
            workingArea.Y + PopoverMargin);
    }

    /// <summary>
    /// Closes the persistent desktop presence popover.
    /// </summary>
    private void ClosePopover()
    {
        var popoverWindow = _popoverWindow;

        if (popoverWindow is null)
        {
            return;
        }

        _popoverWindow = null;
        popoverWindow.CloseFromCode();
        UpdatePresencePanelMenuState();
    }

    /// <summary>
    /// Toggles desktop presence privacy mode.
    /// </summary>
    private void TogglePrivacyMode()
    {
        RunOnUiThread(() =>
        {
            _isPrivacyModeEnabled = !_isPrivacyModeEnabled;

            RefreshFromDashboardState();
        });
    }

    /// <summary>
    /// Refreshes the dashboard from the popover without closing it.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RefreshDashboardFromTraySafelyAsync()
    {
        var dashboardViewModel = _dashboardViewModel;

        if (dashboardViewModel is null)
        {
            RefreshFromDashboardState();
            return;
        }

        if (dashboardViewModel.IsBusy)
        {
            RefreshFromDashboardState();
            return;
        }

        try
        {
            RefreshFromDashboardState();

            await dashboardViewModel
                .RefreshCommand
                .ExecuteAsync(null);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Unable to refresh dashboard data from the desktop presence popover.");
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
                RefreshPopoverState();
                return;
            }

            var text = _dashboardTextFormatter.Format(
                CreateDashboardState(_dashboardViewModel));

            ApplyDesktopPresenceText(text);
            RefreshPopoverState(text);
        });
    }

    /// <summary>
    /// Updates the presence panel menu item state.
    /// </summary>
    private void UpdatePresencePanelMenuState()
    {
        if (_presencePanelMenuItem is null)
        {
            return;
        }

        _presencePanelMenuItem.Header = _popoverWindow is null
            ? "Show presence panel"
            : "Hide presence panel";
    }

    /// <summary>
    /// Refreshes the popover state using current dashboard data.
    /// </summary>
    private void RefreshPopoverState()
    {
        if (_dashboardViewModel is null)
        {
            return;
        }

        RefreshPopoverState(
            _dashboardTextFormatter.Format(
                CreateDashboardState(_dashboardViewModel)));
    }

    /// <summary>
    /// Refreshes the popover state using formatted desktop presence text.
    /// </summary>
    /// <param name="text">The formatted desktop presence text.</param>
    private void RefreshPopoverState(DesktopPresenceText text)
    {
        if (_popoverWindow is null)
        {
            return;
        }

        _popoverWindow.Update(
            text,
            _isPrivacyModeEnabled,
            _dashboardViewModel?.IsBusy ?? false);
    }

    /// <summary>
    /// Creates desktop presence dashboard state from the dashboard view model.
    /// </summary>
    /// <param name="dashboardViewModel">The dashboard view model.</param>
    /// <returns>The desktop presence dashboard state.</returns>
    private DesktopPresenceDashboardState CreateDashboardState(DashboardViewModel dashboardViewModel)
    {
        return new DesktopPresenceDashboardState(
            dashboardViewModel.ProviderDisplayName,
            dashboardViewModel.LatestValueText,
            dashboardViewModel.TrendText,
            dashboardViewModel.FreshnessText,
            dashboardViewModel.LastUpdatedText,
            dashboardViewModel.StatusText,
            _isPrivacyModeEnabled);
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
