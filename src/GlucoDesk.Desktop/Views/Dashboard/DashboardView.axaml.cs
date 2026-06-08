using System.ComponentModel;
using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GlucoDesk.Desktop.ViewModels.Dashboard;

namespace GlucoDesk.Desktop.Views.Dashboard;

public partial class DashboardView : UserControl
{
    private DispatcherTimer? _autoRefreshTimer;
    private DashboardViewModel? _subscribedViewModel;
    private bool _hasLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardView"/> class.
    /// </summary>
    public DashboardView()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (_hasLoaded)
        {
            return;
        }

        _hasLoaded = true;

        SubscribeToViewModel();
        await InitializeDashboardAsync();
        StartAutoRefreshTimer();
        await RefreshDashboardAsync();
    }

    /// <inheritdoc />
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        StopAutoRefreshTimer();
        UnsubscribeFromViewModel();

        base.OnUnloaded(e);
    }

    #region Helpers

    /// <summary>
    /// Subscribes to view model property changes.
    /// </summary>
    private void SubscribeToViewModel()
    {
        if (DataContext is not DashboardViewModel viewModel)
        {
            return;
        }

        if (ReferenceEquals(_subscribedViewModel, viewModel))
        {
            return;
        }

        UnsubscribeFromViewModel();

        _subscribedViewModel = viewModel;
        _subscribedViewModel.PropertyChanged += OnViewModelPropertyChanged;
    }

    /// <summary>
    /// Unsubscribes from view model property changes.
    /// </summary>
    private void UnsubscribeFromViewModel()
    {
        if (_subscribedViewModel is null)
        {
            return;
        }

        _subscribedViewModel.PropertyChanged -= OnViewModelPropertyChanged;
        _subscribedViewModel = null;
    }

    /// <summary>
    /// Handles view model property changes.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="eventArgs">The property changed event arguments.</param>
    private void OnViewModelPropertyChanged(
        object? sender,
        PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName == nameof(DashboardViewModel.AutoRefreshInterval))
        {
            UpdateAutoRefreshTimerInterval();
        }
    }

    /// <summary>
    /// Initializes dashboard settings using the bound view model.
    /// </summary>
    /// <returns>A task representing the asynchronous initialization operation.</returns>
    private async Task InitializeDashboardAsync()
    {
        if (DataContext is not DashboardViewModel viewModel)
        {
            return;
        }

        await viewModel.InitializeCommand.ExecuteAsync(null);
    }

    /// <summary>
    /// Starts the dashboard automatic refresh timer.
    /// </summary>
    private void StartAutoRefreshTimer()
    {
        if (DataContext is not DashboardViewModel viewModel)
        {
            return;
        }

        _autoRefreshTimer = new DispatcherTimer
        {
            Interval = viewModel.AutoRefreshInterval
        };

        _autoRefreshTimer.Tick += async (_, _) => await RefreshDashboardAsync();
        _autoRefreshTimer.Start();
    }

    /// <summary>
    /// Updates the automatic refresh timer interval using the current view model value.
    /// </summary>
    private void UpdateAutoRefreshTimerInterval()
    {
        if (_autoRefreshTimer is null ||
            DataContext is not DashboardViewModel viewModel)
        {
            return;
        }

        _autoRefreshTimer.Interval = viewModel.AutoRefreshInterval;
    }

    /// <summary>
    /// Stops the dashboard automatic refresh timer.
    /// </summary>
    private void StopAutoRefreshTimer()
    {
        if (_autoRefreshTimer is null)
        {
            return;
        }

        _autoRefreshTimer.Stop();
        _autoRefreshTimer = null;
    }

    /// <summary>
    /// Refreshes the dashboard using the bound view model.
    /// </summary>
    /// <returns>A task representing the asynchronous refresh operation.</returns>
    private async Task RefreshDashboardAsync()
    {
        if (DataContext is not DashboardViewModel viewModel)
        {
            return;
        }

        await viewModel.RefreshCommand.ExecuteAsync(null);
    }

    #endregion
}