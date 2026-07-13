using System.ComponentModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Desktop.Cgm.History.Continuity.ViewModels;
using GlucoDesk.Desktop.ViewModels.Account;
using GlucoDesk.Desktop.ViewModels.BackgroundSync;
using GlucoDesk.Desktop.ViewModels.Common;
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Diary;
using GlucoDesk.Desktop.ViewModels.Settings;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.ViewModels.Main;

/// <summary>
/// Represents the main window view model.
/// </summary>
public sealed partial class MainWindowViewModel : ViewModelBase, IDisposable
{
    private bool _isDisposed;

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
    /// <param name="historyContinuitySyncStatus">The history continuity synchronization status ViewModel.</param>
    public MainWindowViewModel(
        DashboardViewModel dashboard,
        AccountViewModel account,
        SettingsViewModel settings,
        BackgroundSyncStatusViewModel backgroundSyncStatus,
        DiaryViewModel diary,
        DesktopHistoryContinuitySyncStatusViewModel historyContinuitySyncStatus)
    {
        ArgumentNullException.ThrowIfNull(dashboard);
        ArgumentNullException.ThrowIfNull(account);
        ArgumentNullException.ThrowIfNull(settings);
        ArgumentNullException.ThrowIfNull(backgroundSyncStatus);
        ArgumentNullException.ThrowIfNull(diary);
        ArgumentNullException.ThrowIfNull(historyContinuitySyncStatus);

        Dashboard = dashboard;
        Account = account;
        Settings = settings;
        BackgroundSyncStatus = backgroundSyncStatus;
        Diary = diary;
        HistoryContinuitySyncStatus = historyContinuitySyncStatus;

        BackgroundSyncStatus.PropertyChanged += OnBackgroundSyncStatusPropertyChanged;
        LocalizationManager.LanguageChanged += OnLanguageChanged;

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
    /// Gets the history continuity synchronization status ViewModel.
    /// </summary>
    public DesktopHistoryContinuitySyncStatusViewModel HistoryContinuitySyncStatus { get; }

    /// <summary>
    /// Gets the consumer-facing local history status text.
    /// </summary>
    public string LocalHistoryStatusText =>
        BackgroundSyncStatus.HasSuccessfulSync
            ? T("SidebarLocalHistoryUpToDate")
            : BackgroundSyncStatus.StatusText;

    /// <summary>
    /// Gets the consumer-facing local history badge text.
    /// </summary>
    public string LocalHistoryBadgeText =>
        BackgroundSyncStatus.HasSuccessfulSync
            ? T("SidebarLocalHistorySynced")
            : T("SidebarLocalHistoryUpdating");

    /// <summary>
    /// Gets the consumer-facing local history description text.
    /// </summary>
    public string LocalHistoryDescriptionText =>
        T("SidebarLocalHistoryDescription");

    /// <inheritdoc />
    public void Dispose()
    {
        if (_isDisposed)
        {
            return;
        }

        BackgroundSyncStatus.PropertyChanged -=
            OnBackgroundSyncStatusPropertyChanged;

        LocalizationManager.LanguageChanged -= OnLanguageChanged;

        _isDisposed = true;
    }

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
    /// Handles background sync status changes and refreshes consumer-facing sidebar properties.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="eventArgs">The property changed event arguments.</param>
    private void OnBackgroundSyncStatusPropertyChanged(
        object? sender,
        PropertyChangedEventArgs eventArgs)
    {
        if (eventArgs.PropertyName is null ||
            eventArgs.PropertyName == nameof(BackgroundSyncStatusViewModel.HasSuccessfulSync) ||
            eventArgs.PropertyName == nameof(BackgroundSyncStatusViewModel.StatusText))
        {
            OnPropertyChanged(nameof(LocalHistoryStatusText));
            OnPropertyChanged(nameof(LocalHistoryBadgeText));
        }
    }

    /// <summary>
    /// Refreshes localized sidebar values when the application language changes.
    /// </summary>
    private void OnLanguageChanged(object? sender, EventArgs eventArgs)
    {
        _ = sender;
        _ = eventArgs;

        OnPropertyChanged(nameof(LocalHistoryStatusText));
        OnPropertyChanged(nameof(LocalHistoryBadgeText));
        OnPropertyChanged(nameof(LocalHistoryDescriptionText));
    }

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

    private static string T(string key)
    {
        return LocalizationManager.GetString(key);
    }

    #endregion
}