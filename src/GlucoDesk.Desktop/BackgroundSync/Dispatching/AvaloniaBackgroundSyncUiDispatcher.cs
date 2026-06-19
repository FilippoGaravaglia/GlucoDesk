using Avalonia.Threading;
using GlucoDesk.Desktop.BackgroundSync.Dispatching.Abstractions;

namespace GlucoDesk.Desktop.BackgroundSync.Dispatching;

/// <summary>
/// Dispatches background sync desktop updates to the Avalonia UI thread.
/// </summary>
public sealed class AvaloniaBackgroundSyncUiDispatcher : IBackgroundSyncUiDispatcher
{
    /// <inheritdoc />
    public void Post(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.UIThread.Post(action);
    }
}