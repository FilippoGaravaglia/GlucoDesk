using Avalonia.Controls;
using Avalonia.Interactivity;
using Avalonia.Threading;
using GlucoDesk.Desktop.ViewModels.Dashboard;

namespace GlucoDesk.Desktop.Views.Dashboard;

public partial class DashboardView : UserControl
{
    private DispatcherTimer? _autoRefreshTimer;
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

        StartAutoRefreshTimer();
        await RefreshDashboardAsync();
    }

    /// <inheritdoc />
    protected override void OnUnloaded(RoutedEventArgs e)
    {
        StopAutoRefreshTimer();

        base.OnUnloaded(e);
    }

    #region Helpers

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