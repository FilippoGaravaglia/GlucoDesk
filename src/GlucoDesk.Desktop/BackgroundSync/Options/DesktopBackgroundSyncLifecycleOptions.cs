namespace GlucoDesk.Desktop.BackgroundSync.Options;

/// <summary>
/// Provides desktop lifecycle options for the in-app background sync loop.
/// </summary>
public sealed record DesktopBackgroundSyncLifecycleOptions
{
    /// <summary>
    /// Gets the default stop timeout.
    /// </summary>
    public static readonly TimeSpan DefaultStopTimeout = TimeSpan.FromSeconds(5);

    /// <summary>
    /// Gets the default desktop background sync lifecycle options.
    /// </summary>
    public static DesktopBackgroundSyncLifecycleOptions Default => new(
        startOnApplicationStartup: true,
        DefaultStopTimeout);

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopBackgroundSyncLifecycleOptions"/> class.
    /// </summary>
    /// <param name="startOnApplicationStartup">A value indicating whether the background sync loop should start on desktop startup.</param>
    /// <param name="stopTimeout">The maximum time allowed for stopping the background sync loop.</param>
    public DesktopBackgroundSyncLifecycleOptions(
        bool startOnApplicationStartup,
        TimeSpan stopTimeout)
    {
        if (stopTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(stopTimeout),
                stopTimeout,
                "Stop timeout must be greater than zero.");
        }

        StartOnApplicationStartup = startOnApplicationStartup;
        StopTimeout = stopTimeout;
    }

    /// <summary>
    /// Gets a value indicating whether the background sync loop should start on desktop startup.
    /// </summary>
    public bool StartOnApplicationStartup { get; }

    /// <summary>
    /// Gets the maximum time allowed for stopping the background sync loop.
    /// </summary>
    public TimeSpan StopTimeout { get; }
}