using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Desktop.ViewModels.Account;
using GlucoDesk.Desktop.ViewModels.BackgroundSync;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Diary;
using GlucoDesk.Desktop.ViewModels.Settings;

namespace GlucoDesk.Desktop.ViewModels.Main;

/// <summary>
/// Represents the main window view model.
/// </summary>
public sealed partial class MainWindowViewModel : ViewModelBase
{
    [ObservableProperty]
    private ViewModelBase _currentContent = null!;

    [ObservableProperty]
    private bool _isDashboardSelected;

    [ObservableProperty]
    private bool _isDiarySelected;

    [ObservableProperty]
    private bool _isAccountSelected;

    [ObservableProperty]
    private bool _isSettingsSelected;

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindowViewModel"/> class.
    /// </summary>
    /// <param name="dashboard">The dashboard view model.</param>
    /// <param name="account">The account view model.</param>
    /// <param name="settings">The settings view model.</param>
    /// <param name="backgroundSyncStatus">The background sync status view model.</param>
    /// <param name="diary">The diary view model.</param>
    public MainWindowViewModel(
        DashboardViewModel dashboard,
        AccountViewModel account,
        SettingsViewModel settings,
        BackgroundSyncStatusViewModel backgroundSyncStatus,
        DiaryViewModel diary)
    {
        ArgumentNullException.ThrowIfNull(dashboard);
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(backgroundSyncStatus);
        ArgumentNullException.ThrowIfNull(diary);

        Dashboard = dashboard;
        Account = account;
        Settings = settings;
        BackgroundSyncStatus = backgroundSyncStatus;
        Diary = diary;

        SelectSection(Dashboard);
    }

    /// <summary>
    /// Gets the dashboard view model.
    /// </summary>
    public DashboardViewModel Dashboard { get; }

    /// <summary>
    /// Gets the account view model.
    /// </summary>
    public AccountViewModel Account { get; }

    /// <summary>
    /// Gets the settings view model.
    /// </summary>
    public SettingsViewModel Settings { get; }

    /// <summary>
    /// Gets the background sync status view model.
    /// </summary>
    public BackgroundSyncStatusViewModel BackgroundSyncStatus { get; }

    /// <summary>
    /// Gets the diary view model.
    /// </summary>
    public DiaryViewModel Diary { get; }

    /// <summary>
    /// Selects the dashboard section.
    /// </summary>
    [RelayCommand]
    private void ShowDashboard()
    {
        SelectSection(Dashboard);
    }

    /// <summary>
    /// Selects the diary section.
    /// </summary>
    [RelayCommand]
    private void ShowDiary()
    {
        SelectSection(Diary);
    }

    /// <summary>
    /// Selects the account section.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    [RelayCommand]
    private async Task ShowAccountAsync(CancellationToken cancellationToken)
    {
        await Account.LoadAsync(cancellationToken)
            .ConfigureAwait(false);

        SelectSection(Account);
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
    /// Selects the active main window section and updates navigation state.
    /// </summary>
    /// <param name="selectedContent">The selected content view model.</param>
    private void SelectSection(ViewModelBase selectedContent)
    {
        ArgumentNullException.ThrowIfNull(selectedContent);

        CurrentContent = selectedContent;
        IsDashboardSelected = ReferenceEquals(selectedContent, Dashboard);
        IsDiarySelected = ReferenceEquals(selectedContent, Diary);
        IsAccountSelected = ReferenceEquals(selectedContent, Account);
        IsSettingsSelected = ReferenceEquals(selectedContent, Settings);
    }

    #endregion
}