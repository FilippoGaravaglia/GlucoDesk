using System.ComponentModel;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Platform;
using Avalonia.Styling;
using Avalonia.Threading;
using GlucoDesk.Desktop.DesktopPresence.Enums;
using GlucoDesk.Desktop.DesktopPresence.Formatters;
using GlucoDesk.Desktop.DesktopPresence.Layout;
using GlucoDesk.Desktop.DesktopPresence.Models;
using GlucoDesk.Desktop.DesktopPresence.Services.Abstractions;
using GlucoDesk.Desktop.DesktopPresence.Windows;
using GlucoDesk.Desktop.Localization;
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
    private static readonly Uri DefaultTrayIconUri = new("avares://GlucoDesk.Desktop/Assets/AppIcon/glucodesk-app-icon.png");

    private static readonly Uri WindowsTrayIconWhiteUri = new("avares://GlucoDesk.Desktop/Assets/AppIcon/glucodesk-windows-tray-icon-white.png");
    private static readonly Uri WindowsTrayIconBlackUri = new("avares://GlucoDesk.Desktop/Assets/AppIcon/glucodesk-windows-tray-icon-black.png");

    private static readonly Uri MacOsMenuBarIconWhiteUri = new("avares://GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon-white.png");
    private static readonly Uri MacOsMenuBarIconBlackUri = new("avares://GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon-black.png");

    private readonly IDesktopPresenceTextFormatter _textFormatter;
    private readonly IDesktopPresenceDashboardTextFormatter _dashboardTextFormatter;
    private readonly IDesktopPresencePrivacyModeService _privacyModeService;
    private readonly ILogger<AvaloniaDesktopPresenceLifecycleService> _logger;

    private AvaloniaApplication? _application;
    private TrayIcon? _trayIcon;
    private NativeMenuItem? _statusMenuItem;
    private NativeMenuItem? _presencePanelMenuItem;
    private DesktopPresencePopoverWindow? _popoverWindow;
    private DashboardViewModel? _dashboardViewModel;
    private IClassicDesktopStyleApplicationLifetime? _desktopLifetime;
    private bool _isStarted;
    private bool _isPrivacyModeEnabled;
    private const string MenuBarIconInRangeAssetUri = "avares://GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon-in-range.png";
    private const string MenuBarIconPrivacyAssetUri = "avares://GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon-privacy.png";
    private const string MenuBarIconHighAssetUri = "avares://GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon-high.png";
    private const string MenuBarIconLowAssetUri = "avares://GlucoDesk.Desktop/Assets/MenuBar/glucodesk-menubar-icon-low.png";

    private string? _lastAppliedMenuBarIconAssetUri;


    /// <summary>
    /// Initializes a new instance of the <see cref="AvaloniaDesktopPresenceLifecycleService"/> class.
    /// </summary>
    /// <param name="textFormatter">The desktop presence text formatter.</param>
    /// <param name="dashboardTextFormatter">The dashboard desktop presence text formatter.</param>
    /// <param name="privacyModeService">The desktop presence privacy mode service.</param>
    /// <param name="logger">The logger.</param>
    public AvaloniaDesktopPresenceLifecycleService(
        IDesktopPresenceTextFormatter textFormatter,
        IDesktopPresenceDashboardTextFormatter dashboardTextFormatter,
        IDesktopPresencePrivacyModeService privacyModeService,
        ILogger<AvaloniaDesktopPresenceLifecycleService> logger)
    {
        ArgumentNullException.ThrowIfNull(textFormatter);
        ArgumentNullException.ThrowIfNull(dashboardTextFormatter);
        ArgumentNullException.ThrowIfNull(privacyModeService);
        ArgumentNullException.ThrowIfNull(logger);

        _textFormatter = textFormatter;
        _dashboardTextFormatter = dashboardTextFormatter;
        _privacyModeService = privacyModeService;
        _privacyModeService.StateChanged += OnPrivacyModeStateChanged;
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

                _privacyModeService.Reload();
                _isPrivacyModeEnabled = _privacyModeService.IsEnabled;

                var initialText = _textFormatter.Format(CreateInitialSnapshot());

                var trayIcon = CreateTrayIcon(
                    desktopLifetime,
                    initialText,
                    out var statusMenuItem,
                    out var presencePanelMenuItem);

                var trayIcons = new TrayIcons { trayIcon };

                TrayIcon.SetIcons(application, trayIcons);

                application.PropertyChanged += OnApplicationPropertyChanged;

                _application = application;
                _desktopLifetime = desktopLifetime;
                _trayIcon = trayIcon;
                _statusMenuItem = statusMenuItem;
                _presencePanelMenuItem = presencePanelMenuItem;
                _isStarted = true;

                LocalizationManager.LanguageChanged += OnLanguageChanged;

                AttachDashboardState(desktopLifetime);
                RefreshFromDashboardState();
                RefreshMenuBarIconFromDashboardState();

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
                LocalizationManager.LanguageChanged -= OnLanguageChanged;

                ClosePopover();
                DetachDashboardState();

                if (_application is not null)
                {
                    _application.PropertyChanged -= OnApplicationPropertyChanged;
                    TrayIcon.SetIcons(_application, null);
                }

                _trayIcon?.Dispose();

                _application = null;
                _desktopLifetime = null;
                _trayIcon = null;
                _statusMenuItem = null;
                _lastAppliedMenuBarIconAssetUri = null;
                _lastAppliedMenuBarIconAssetUri = null;
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
    /// Refreshes the menu bar icon using only the current glycemic alert state.
    /// </summary>
    private void RefreshMenuBarIconFromDashboardState()
    {
        var alertKindName = _dashboardViewModel?.CurrentGlucoseAlertKind.ToString();
        var assetUri = SelectMenuBarIconAssetUri(alertKindName, _isPrivacyModeEnabled);

        ApplyMenuBarIcon(assetUri);
    }

    /// <summary>
    /// Selects the menu bar icon asset from the current glycemic alert state.
    /// </summary>
    /// <param name="alertKindName">The glucose alert kind name.</param>
    /// <param name="isPrivacyModeEnabled">Whether privacy mode is enabled.</param>
    /// <returns>The menu bar icon asset URI.</returns>
    private static string SelectMenuBarIconAssetUri(
        string? alertKindName,
        bool isPrivacyModeEnabled)
    {
        if (isPrivacyModeEnabled)
        {
            return MenuBarIconPrivacyAssetUri;
        }

        return alertKindName switch
        {
            "AboveTarget" or "High" => MenuBarIconHighAssetUri,
            "BelowTarget" or "Low" => MenuBarIconLowAssetUri,
            _ => MenuBarIconInRangeAssetUri,
        };
    }

    /// <summary>
    /// Applies the specified menu bar icon if it is different from the currently applied icon.
    /// </summary>
    /// <param name="assetUri">The Avalonia asset URI.</param>
    private void ApplyMenuBarIcon(string assetUri)
    {
        if (_trayIcon is null)
        {
            return;
        }

        if (string.Equals(_lastAppliedMenuBarIconAssetUri, assetUri, StringComparison.Ordinal))
        {
            return;
        }

        try
        {
            using var stream = AssetLoader.Open(new Uri(assetUri, UriKind.Absolute));
            _trayIcon.Icon = new WindowIcon(stream);
            _lastAppliedMenuBarIconAssetUri = assetUri;
        }
        catch (Exception exception)
        {
            _logger.LogWarning(
                exception,
                "Unable to apply glycemic menu bar icon asset {AssetUri}. Keeping the previous tray icon.",
                assetUri);
        }
    }

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
            IsEnabled = false,
        };

        presencePanelMenuItem = new NativeMenuItem(
            Text("DesktopPresenceShowPanel"));
        presencePanelMenuItem.Click += (_, _) => TogglePopover(desktopLifetime);

        return new TrayIcon
        {
            Icon = LoadTrayIcon(),
            ToolTipText = initialText.Tooltip,
            Menu = CreateTrayMenu(
                desktopLifetime,
                statusMenuItem,
                presencePanelMenuItem),
            IsVisible = true,
        };
    }

    /// <summary>
    /// Handles application property changes that may require refreshing the tray icon.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The property changed event arguments.</param>
    private void OnApplicationPropertyChanged(object? sender, AvaloniaPropertyChangedEventArgs e)
    {
        if (e.Property != AvaloniaApplication.ActualThemeVariantProperty)
        {
            return;
        }

        RefreshTrayIcon();
    }

    /// <summary>
    /// Refreshes the tray icon after an application theme change.
    /// </summary>
    private void RefreshTrayIcon()
    {
        RunOnUiThread(() =>
        {
            if (_trayIcon is null)
            {
                return;
            }

            try
            {
                _trayIcon.Icon = LoadTrayIcon();
            }
            catch (Exception exception)
            {
                _logger.LogError(
                    exception,
                    "Unable to refresh the desktop presence icon after a theme change.");
            }
        });
    }

    /// <summary>
    /// Gets the platform-specific tray icon asset URI.
    /// </summary>
    /// <returns>The tray icon asset URI.</returns>
    private static Uri GetTrayIconUri()
    {
        if (OperatingSystem.IsMacOS())
        {
            return ShouldUseWhiteIconOnMacOs()
                ? MacOsMenuBarIconWhiteUri
                : MacOsMenuBarIconBlackUri;
        }

        if (OperatingSystem.IsWindows())
        {
            return WindowsTrayIconThemeDetector.ShouldUseLightIconForCurrentSystemTheme()
                ? WindowsTrayIconWhiteUri
                : WindowsTrayIconBlackUri;
        }

        return DefaultTrayIconUri;
    }

    /// <summary>
    /// Determines whether the desktop presence icon should use the white variant.
    /// </summary>
    /// <returns>true when the white icon should be used; otherwise, false.</returns>
    private static bool ShouldUseWhiteIcon()
    {
        if (OperatingSystem.IsMacOS())
        {
            return ShouldUseWhiteIconOnMacOs();
        }

        return IsApplicationAppearanceDark();
    }

    /// <summary>
    /// Determines whether the macOS menu bar icon should use the white variant.
    /// </summary>
    /// <returns>true when the white macOS menu bar icon should be used; otherwise, false.</returns>
    private static bool ShouldUseWhiteIconOnMacOs()
    {
        if (!OperatingSystem.IsMacOSVersionAtLeast(27))
        {
            return true;
        }

        return IsMacOsSystemAppearanceDark()
            ?? IsApplicationAppearanceDark();
    }

    /// <summary>
    /// Determines whether the current Avalonia application appearance is dark.
    /// </summary>
    /// <returns>true when the application appearance is dark; otherwise, false.</returns>
    private static bool IsApplicationAppearanceDark()
    {
        var currentTheme = AvaloniaApplication.Current?.ActualThemeVariant;

        return currentTheme is null || currentTheme == ThemeVariant.Dark;
    }

    /// <summary>
    /// Reads the macOS system appearance from user defaults.
    /// </summary>
    /// <returns>true for dark mode, false for light mode, or null when the value cannot be read.</returns>
    private static bool? IsMacOsSystemAppearanceDark()
    {
        try
        {
            using var process = Process.Start(new ProcessStartInfo
            {
                FileName = "defaults",
                ArgumentList = { "read", "-g", "AppleInterfaceStyle" },
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true,
            });

            if (process is null)
            {
                return null;
            }

            var output = process.StandardOutput.ReadToEnd();

            if (!process.WaitForExit(1000))
            {
                try
                {
                    process.Kill(entireProcessTree: true);
                }
                catch
                {
                    // Best-effort cleanup only.
                }

                return null;
            }

            if (process.ExitCode != 0)
            {
                return false;
            }

            return string.Equals(
                output.Trim(),
                "Dark",
                StringComparison.OrdinalIgnoreCase);
        }
        catch
        {
            return null;
        }
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
        var openItem = new NativeMenuItem(
            Text("DesktopPresenceOpen"));
        openItem.Click += (_, _) => ShowMainWindow(desktopLifetime);

        var quitItem = new NativeMenuItem(
            Text("DesktopPresenceQuit"));
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
                quitItem,
            },
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
    /// <returns>true when desktop presence text should refresh; otherwise, false.</returns>
    private static bool ShouldRefreshFromDashboardProperty(string? propertyName)
    {
        return string.IsNullOrWhiteSpace(propertyName)
            || string.Equals(propertyName, nameof(DashboardViewModel.ProviderDisplayName), StringComparison.Ordinal)
            || string.Equals(propertyName, nameof(DashboardViewModel.LatestValueText), StringComparison.Ordinal)
            || string.Equals(propertyName, nameof(DashboardViewModel.TrendText), StringComparison.Ordinal)
            || string.Equals(propertyName, nameof(DashboardViewModel.FreshnessText), StringComparison.Ordinal)
            || string.Equals(propertyName, nameof(DashboardViewModel.LastUpdatedText), StringComparison.Ordinal)
            || string.Equals(propertyName, nameof(DashboardViewModel.StatusText), StringComparison.Ordinal)
            || string.Equals(propertyName, nameof(DashboardViewModel.CurrentGlucoseAlertKind), StringComparison.Ordinal)
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
        _privacyModeService.Reload();
        _isPrivacyModeEnabled = _privacyModeService.IsEnabled;
        RefreshMenuBarIconFromDashboardState();

        var popoverWindow = new DesktopPresencePopoverWindow(
            RefreshDashboardFromTraySafelyAsync,
            TogglePrivacyMode,
            () => ShowMainWindow(desktopLifetime),
            () => desktopLifetime.Shutdown());

        popoverWindow.Position = CalculatePopoverPosition(desktopLifetime.MainWindow, popoverWindow);
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
    /// Calculates a screen-aware popover position inside the current display working area.
    /// </summary>
    /// <param name="mainWindow">The main window used to resolve the current screen.</param>
    /// <param name="popoverWindow">The popover window being positioned.</param>
    /// <returns>The calculated popover position.</returns>
    private static PixelPoint CalculatePopoverPosition(Window? mainWindow, Window popoverWindow)
    {
        var screen = mainWindow?.Screens.ScreenFromWindow(mainWindow)
            ?? mainWindow?.Screens.Primary;

        if (screen is null)
        {
            return new PixelPoint(
                DesktopPresencePopoverPositioner.EdgeMarginPixels,
                DesktopPresencePopoverPositioner.EdgeMarginPixels);
        }

        var popoverSize = CalculatePopoverPixelSize(
            popoverWindow,
            screen.Scaling);

        return DesktopPresencePopoverPositioner.Calculate(
            screen.WorkingArea,
            popoverSize);
    }

    /// <summary>
    /// Calculates the popover size in physical screen pixels.
    /// </summary>
    /// <param name="popoverWindow">The popover window.</param>
    /// <param name="screenScaling">The scaling of the screen that will contain the popover.</param>
    /// <returns>The popover size in physical pixels.</returns>
    private static PixelSize CalculatePopoverPixelSize(Window popoverWindow, double screenScaling)
    {
        var scaling = screenScaling > 0 ? screenScaling : 1;

        var width = ResolveWindowDimension(
            popoverWindow.Width,
            popoverWindow.MinWidth,
            DesktopPresencePopoverPositioner.DefaultPopoverWidthDip);

        var height = ResolveWindowDimension(
            popoverWindow.Height,
            popoverWindow.MinHeight,
            DesktopPresencePopoverPositioner.DefaultPopoverHeightDip);

        return new PixelSize(
            Math.Max(1, (int)Math.Ceiling(width * scaling)),
            Math.Max(1, (int)Math.Ceiling(height * scaling)));
    }

    /// <summary>
    /// Resolves an Avalonia window dimension, falling back when the value has not been measured yet.
    /// </summary>
    /// <param name="value">The preferred window dimension.</param>
    /// <param name="minimumValue">The minimum window dimension.</param>
    /// <param name="fallbackValue">The fallback dimension.</param>
    /// <returns>The resolved dimension.</returns>
    private static double ResolveWindowDimension(
        double value,
        double minimumValue,
        double fallbackValue)
    {
        if (!double.IsNaN(value) && value > 0)
        {
            return value;
        }

        if (!double.IsNaN(minimumValue) && minimumValue > 0)
        {
            return minimumValue;
        }

        return fallbackValue;
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
    /// Refreshes native menu, tooltip and popover text after a language change.
    /// </summary>
    private void OnLanguageChanged(
        object? sender,
        EventArgs eventArgs)
    {
        _ = sender;
        _ = eventArgs;

        RunOnUiThread(() =>
        {
            if (!_isStarted)
            {
                return;
            }

            /*
             * Do not replace TrayIcon.Menu after Avalonia has exported it to
             * the native macOS menu system.
             *
             * Avalonia Native requires the original NativeMenu instance to
             * remain associated with the tray icon. Replacing it at runtime
             * throws:
             *
             * "The menu being updated does not match."
             *
             * Refresh the existing menu item instances instead.
             */
            RefreshFromDashboardState();
            UpdatePresencePanelMenuState();

            /*
             * A visible popover owns localized controls created with the
             * previous language. Closing it avoids displaying mixed-language
             * content; reopening it creates the controls with the new
             * language.
             */
            if (_popoverWindow is not null)
            {
                ClosePopover();
            }
        });
    }

    /// <summary>
    /// Handles desktop presence privacy mode state changes.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The event arguments.</param>
    private void OnPrivacyModeStateChanged(object? sender, EventArgs e)
    {
        RunOnUiThread(() =>
        {
            _isPrivacyModeEnabled = _privacyModeService.IsEnabled;
            RefreshFromDashboardState();
            RefreshMenuBarIconFromDashboardState();
        });
    }

    /// <summary>
    /// Toggles desktop presence privacy mode.
    /// </summary>
    private void TogglePrivacyMode()
    {
        RunOnUiThread(() =>
        {
            _privacyModeService.Toggle();
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
        
        RefreshMenuBarIconFromDashboardState();
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
            ? Text("DesktopPresenceShowPanel")
            : Text("DesktopPresenceHidePanel");
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
            _isPrivacyModeEnabled
                ? dashboardViewModel.StatusText
                : dashboardViewModel.AmbientGlucoseSummaryText,
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

    private static string Text(string key)
    {
        return LocalizationManager.GetString(key);
    }

    #endregion
}
