namespace GlucoDesk.Desktop.BackgroundSync.Dispatching.Abstractions;

/// <summary>
/// Defines UI dispatching operations for background sync desktop updates.
/// </summary>
public interface IBackgroundSyncUiDispatcher
{
    /// <summary>
    /// Posts an action to the UI thread.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void Post(Action action);
}