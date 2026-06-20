using Avalonia.Threading;
using GlucoDesk.Desktop.Common.Dispatching.Abstractions;

namespace GlucoDesk.Desktop.Common.Dispatching;

/// <summary>
/// Avalonia implementation of the desktop UI dispatcher.
/// </summary>
public sealed class AvaloniaDesktopUiDispatcher : IDesktopUiDispatcher
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