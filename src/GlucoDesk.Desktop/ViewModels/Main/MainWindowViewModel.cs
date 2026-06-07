using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Dashboard;

namespace GlucoDesk.Desktop.ViewModels.Main;

/// <summary>
/// Represents the main window view model.
/// </summary>
public sealed partial class MainWindowViewModel : ViewModelBase
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="dashboard">The dashboard view model.</param>
    public MainWindowViewModel(DashboardViewModel dashboard)
    {
        ArgumentNullException.ThrowIfNull(dashboard);

        Dashboard = dashboard;
    }

    /// <summary>
    /// Gets the dashboard view model.
    /// </summary>
    public DashboardViewModel Dashboard { get; }
}