using CommunityToolkit.Mvvm.ComponentModel;
using GlucoDesk.Application.Updates;
using GlucoDesk.Core.Updates;

namespace GlucoDesk.Desktop.ViewModels.Updates;

/// <summary>
/// Holds the shared update-center state consumed by the desktop UI.
/// </summary>
public sealed partial class UpdateCenterStore : ObservableObject
{
    [ObservableProperty]
    private UpdateCenterSnapshot _snapshot = new()
    {
        State = UpdateCenterState.Idle,
        CurrentVersion = string.Empty
    };

    /// <summary>
    /// Gets a value indicating whether an update is currently available.
    /// </summary>
    public bool HasUpdate =>
        Snapshot.State == UpdateCenterState.UpdateAvailable;

    /// <summary>
    /// Gets a value indicating whether an update check is running.
    /// </summary>
    public bool IsChecking =>
        Snapshot.State == UpdateCenterState.Checking;

    /// <summary>
    /// Gets a value indicating whether the installed version is current.
    /// </summary>
    public bool IsUpToDate =>
        Snapshot.State == UpdateCenterState.UpToDate;

    /// <summary>
    /// Gets a value indicating whether the latest update check failed.
    /// </summary>
    public bool HasError =>
        Snapshot.State == UpdateCenterState.Error;

    /// <summary>
    /// Gets the badge text displayed in the sidebar when an update exists.
    /// </summary>
    public string? BadgeText =>
        HasUpdate ? "1" : null;

    partial void OnSnapshotChanged(
        UpdateCenterSnapshot value)
    {
        _ = value;

        OnPropertyChanged(nameof(HasUpdate));
        OnPropertyChanged(nameof(IsChecking));
        OnPropertyChanged(nameof(IsUpToDate));
        OnPropertyChanged(nameof(HasError));
        OnPropertyChanged(nameof(BadgeText));
    }
}
