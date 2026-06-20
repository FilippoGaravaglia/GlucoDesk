namespace GlucoDesk.Desktop.Common.Dispatching.Abstractions;

/// <summary>
/// Dispatches work to the desktop UI thread.
/// </summary>
public interface IDesktopUiDispatcher
{
    /// <summary>
    /// Posts an action to the desktop UI thread.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    void Post(Action action);
}