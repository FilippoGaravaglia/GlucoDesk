using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Settings;

namespace GlucoDesk.Desktop.ViewModels.Main;

/// <summary>
/// Represents the main window view model.
/// </summary>
public sealed partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private bool _isDashboardSelected = true;

    [ObservableProperty]
    private bool _isSettingsSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="dashboard">The dashboard view model.</param>
    /// <param name="settings">The settings view model.</param>
    public MainWindowViewModel(
        DashboardViewModel dashboard,
        SettingsViewModel settings)
    {
        ArgumentNullException.ThrowIfNull(dashboard);
        ArgumentNullException.ThrowIfNull(settings);

        Dashboard = dashboard;
        Settings = settings;
        CurrentContent = dashboard;
    }

    /// <summary>
    /// Gets the dashboard view model.
    /// </summary>
    public DashboardViewModel Dashboard { get; }

    /// <summary>
    /// Gets the settings view model.
    /// </summary>
    public SettingsViewModel Settings { get; }

    /// <summary>
    /// Gets the currently selected content view model.
    /// </summary>
    public ViewModelBase CurrentContent { get; private set; }

    /// <summary>
    /// Selects the dashboard section.
    /// </summary>
    [RelayCommand]
    private void ShowDashboard()
    {
        SelectSection(Dashboard);
    }

    /// <summary>
    /// Selects the settings section.
    /// </summary>
    [RelayCommand]
    private void ShowSettings()
    {
        SelectSection(Settings);
    }

    #region Helpers

    /// <summary>
    /// Selects the active main window section.
    /// </summary>
    /// <param name="selectedContent">The selected content view model.</param>
    private void SelectSection(ViewModelBase selectedContent)
    {
        CurrentContent = selectedContent;
        IsDashboardSelected = ReferenceEquals(selectedContent, Dashboard);
        IsSettingsSelected = ReferenceEquals(selectedContent, Settings);

        OnPropertyChanged(nameof(CurrentContent));
    }

    #endregion
}