using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Media;
using Avalonia.Threading;
using GlucoDesk.Desktop.DesktopPresence.Models;

namespace GlucoDesk.Desktop.DesktopPresence.Windows;

/// <summary>
/// Represents the persistent desktop presence popover window.
/// </summary>
public sealed class DesktopPresencePopoverWindow : Window
{
    private static readonly IBrush PopoverBackgroundBrush = new SolidColorBrush(Color.FromRgb(18, 24, 32));
    private static readonly IBrush PopoverBorderBrush = new SolidColorBrush(Color.FromRgb(58, 70, 88));
    private static readonly IBrush CaptionBrush = new SolidColorBrush(Color.FromRgb(142, 158, 178));
    private static readonly IBrush PrimaryTextBrush = Brushes.White;
    private static readonly IBrush SecondaryTextBrush = new SolidColorBrush(Color.FromRgb(198, 207, 220));

    private static readonly IBrush PrimaryButtonBrush = new SolidColorBrush(Color.FromRgb(18, 145, 224));
    private static readonly IBrush PrimaryButtonHoverBrush = new SolidColorBrush(Color.FromRgb(66, 184, 255));
    private static readonly IBrush PrimaryButtonPressedBrush = new SolidColorBrush(Color.FromRgb(10, 104, 170));
    private static readonly IBrush PrimaryButtonDisabledBrush = new SolidColorBrush(Color.FromRgb(16, 72, 108));

    private static readonly IBrush SecondaryButtonBrush = new SolidColorBrush(Color.FromRgb(36, 50, 68));
    private static readonly IBrush SecondaryButtonHoverBrush = new SolidColorBrush(Color.FromRgb(64, 86, 116));
    private static readonly IBrush SecondaryButtonPressedBrush = new SolidColorBrush(Color.FromRgb(22, 34, 50));

    private readonly TextBlock _headerTextBlock;
    private readonly TextBlock _detailsTextBlock;

    private readonly Border _refreshActionBorder;
    private readonly TextBlock _refreshActionTextBlock;

    private readonly Border _privacyActionBorder;
    private readonly TextBlock _privacyActionTextBlock;

    private readonly Func<Task> _refreshAsync;
    private readonly Action _togglePrivacyMode;
    private readonly Action _openDashboard;
    private readonly Action _quitApplication;

    private bool _isClosingFromCode;
    private bool _isRefreshInProgress;

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopPresencePopoverWindow"/> class.
    /// </summary>
    /// <param name="refreshAsync">The refresh callback.</param>
    /// <param name="togglePrivacyMode">The privacy mode callback.</param>
    /// <param name="openDashboard">The open dashboard callback.</param>
    /// <param name="quitApplication">The quit application callback.</param>
    public DesktopPresencePopoverWindow(
        Func<Task> refreshAsync,
        Action togglePrivacyMode,
        Action openDashboard,
        Action quitApplication)
    {
        _refreshAsync = refreshAsync;
        _togglePrivacyMode = togglePrivacyMode;
        _openDashboard = openDashboard;
        _quitApplication = quitApplication;

        Title = "GlucoDesk";
        Width = 340;
        SizeToContent = SizeToContent.Height;
        CanResize = false;
        ShowInTaskbar = false;
        Topmost = true;
        WindowStartupLocation = WindowStartupLocation.Manual;
        WindowDecorations = WindowDecorations.None;
        Background = Brushes.Transparent;
        TransparencyBackgroundFallback = Brushes.Transparent;
        TransparencyLevelHint =
        [
            WindowTransparencyLevel.Transparent
        ];

        _headerTextBlock = new TextBlock
        {
            FontSize = 26,
            FontWeight = FontWeight.SemiBold,
            Foreground = PrimaryTextBrush,
            Text = "Waiting for glucose data"
        };

        _detailsTextBlock = new TextBlock
        {
            FontSize = 13,
            Foreground = SecondaryTextBrush,
            TextWrapping = TextWrapping.Wrap,
            Text = "GlucoDesk is waiting for fresh desktop presence data."
        };

        var refreshAction = CreateActionControl(
            "Refresh now",
            PrimaryButtonBrush,
            PrimaryButtonHoverBrush,
            PrimaryButtonPressedBrush);

        _refreshActionBorder = refreshAction.Container;
        _refreshActionTextBlock = refreshAction.Label;

        var privacyAction = CreateActionControl(
            "Privacy mode: Off",
            PrimaryButtonBrush,
            PrimaryButtonHoverBrush,
            PrimaryButtonPressedBrush);

        _privacyActionBorder = privacyAction.Container;
        _privacyActionTextBlock = privacyAction.Label;

        _refreshActionBorder.PointerReleased += async (_, _) => await RefreshSafelyAsync();
        _privacyActionBorder.PointerReleased += (_, _) => _togglePrivacyMode();

        Content = CreateContent();

        Deactivated += (_, _) => CloseAfterFocusLossSafely();
    }

    /// <summary>
    /// Updates the popover presentation state.
    /// </summary>
    /// <param name="text">The formatted desktop presence text.</param>
    /// <param name="isPrivacyModeEnabled">Whether privacy mode is enabled.</param>
    /// <param name="isRefreshing">Whether a refresh operation is in progress.</param>
    public void Update(
        DesktopPresenceText text,
        bool isPrivacyModeEnabled,
        bool isRefreshing)
    {
        ArgumentNullException.ThrowIfNull(text);

        _headerTextBlock.Text = text.MenuHeader;
        _detailsTextBlock.Text = text.Tooltip;

        SetRefreshVisualState(isRefreshing);

        _privacyActionTextBlock.Text = isPrivacyModeEnabled
            ? "Privacy mode: On"
            : "Privacy mode: Off";

        ApplyActionVisualState(
            _privacyActionBorder,
            _privacyActionTextBlock,
            PrimaryButtonBrush,
            opacity: 1d);
    }

    /// <summary>
    /// Closes the popover from application code.
    /// </summary>
    public void CloseFromCode()
    {
        _isClosingFromCode = true;
        Close();
    }

    #region Helpers

    /// <summary>
    /// Creates the popover content.
    /// </summary>
    /// <returns>The popover content control.</returns>
    private Control CreateContent()
    {
        var openAction = CreateActionControl(
            "Open GlucoDesk",
            SecondaryButtonBrush,
            SecondaryButtonHoverBrush,
            SecondaryButtonPressedBrush);

        openAction.Container.PointerReleased += (_, _) =>
        {
            _openDashboard();
            CloseFromCode();
        };

        var quitAction = CreateActionControl(
            "Quit GlucoDesk",
            SecondaryButtonBrush,
            SecondaryButtonHoverBrush,
            SecondaryButtonPressedBrush);

        quitAction.Container.PointerReleased += (_, _) =>
        {
            _quitApplication();
            CloseFromCode();
        };

        var footerGrid = new Grid
        {
            ColumnDefinitions =
            {
                new ColumnDefinition(GridLength.Star),
                new ColumnDefinition(new GridLength(10)),
                new ColumnDefinition(GridLength.Star)
            },
            Children =
            {
                openAction.Container,
                quitAction.Container
            }
        };

        Grid.SetColumn(openAction.Container, 0);
        Grid.SetColumn(quitAction.Container, 2);

        var contentPanel = new StackPanel
        {
            Spacing = 12,
            Children =
            {
                new TextBlock
                {
                    Text = "GlucoDesk",
                    FontSize = 13,
                    FontWeight = FontWeight.SemiBold,
                    Foreground = CaptionBrush
                },
                _headerTextBlock,
                _detailsTextBlock,
                _refreshActionBorder,
                _privacyActionBorder,
                footerGrid
            }
        };

        return new Border
        {
            Padding = new Thickness(16),
            CornerRadius = new CornerRadius(18),
            ClipToBounds = true,
            Background = PopoverBackgroundBrush,
            BorderBrush = PopoverBorderBrush,
            BorderThickness = new Thickness(1),
            Child = contentPanel
        };
    }

    /// <summary>
    /// Creates a styled clickable action control.
    /// </summary>
    /// <param name="content">The control text.</param>
    /// <param name="normalBrush">The normal background brush.</param>
    /// <param name="hoverBrush">The hover background brush.</param>
    /// <param name="pressedBrush">The pressed background brush.</param>
    /// <returns>The created action control.</returns>
    private static (Border Container, TextBlock Label) CreateActionControl(
        string content,
        IBrush normalBrush,
        IBrush hoverBrush,
        IBrush pressedBrush)
    {
        var label = new TextBlock
        {
            Text = content,
            Foreground = PrimaryTextBrush,
            FontWeight = FontWeight.SemiBold,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center
        };

        var container = new Border
        {
            Height = 38,
            HorizontalAlignment = HorizontalAlignment.Stretch,
            Background = normalBrush,
            CornerRadius = new CornerRadius(12),
            Padding = new Thickness(12, 8),
            Cursor = new Cursor(StandardCursorType.Hand),
            Child = label
        };

        container.PointerEntered += (_, _) =>
        {
            ApplyActionVisualState(
                container,
                label,
                hoverBrush,
                opacity: 1d);
        };

        container.PointerExited += (_, _) =>
        {
            ApplyActionVisualState(
                container,
                label,
                normalBrush,
                opacity: 1d);
        };

        container.PointerPressed += (_, _) =>
        {
            ApplyActionVisualState(
                container,
                label,
                pressedBrush,
                opacity: 1d);
        };

        container.PointerReleased += (_, _) =>
        {
            ApplyActionVisualState(
                container,
                label,
                hoverBrush,
                opacity: 1d);
        };

        return (container, label);
    }

    /// <summary>
    /// Applies the visual state of an action control.
    /// </summary>
    /// <param name="container">The action container.</param>
    /// <param name="label">The action label.</param>
    /// <param name="background">The background brush.</param>
    /// <param name="opacity">The opacity.</param>
    private static void ApplyActionVisualState(
        Border container,
        TextBlock label,
        IBrush background,
        double opacity)
    {
        container.Background = background;
        container.Opacity = opacity;
        label.Foreground = PrimaryTextBrush;
    }

    /// <summary>
    /// Sets the refresh action visual state.
    /// </summary>
    /// <param name="isRefreshing">Whether refresh is currently in progress.</param>
    private void SetRefreshVisualState(bool isRefreshing)
    {
        _isRefreshInProgress = isRefreshing;

        _refreshActionTextBlock.Text = isRefreshing
            ? "Refreshing..."
            : "Refresh now";

        ApplyActionVisualState(
            _refreshActionBorder,
            _refreshActionTextBlock,
            isRefreshing ? PrimaryButtonDisabledBrush : PrimaryButtonBrush,
            opacity: 1d);
    }

    /// <summary>
    /// Runs the refresh callback without closing the popover.
    /// </summary>
    /// <returns>A task representing the asynchronous operation.</returns>
    private async Task RefreshSafelyAsync()
    {
        if (_isRefreshInProgress)
        {
            return;
        }

        SetRefreshVisualState(isRefreshing: true);

        try
        {
            await _refreshAsync();
        }
        finally
        {
            SetRefreshVisualState(isRefreshing: false);
        }
    }

    /// <summary>
    /// Closes the popover after focus is lost.
    /// </summary>
    private void CloseAfterFocusLossSafely()
    {
        if (_isClosingFromCode)
        {
            return;
        }

        Dispatcher.UIThread.Post(
            () =>
            {
                if (!IsActive)
                {
                    CloseFromCode();
                }
            },
            DispatcherPriority.Background);
    }

    #endregion
}
