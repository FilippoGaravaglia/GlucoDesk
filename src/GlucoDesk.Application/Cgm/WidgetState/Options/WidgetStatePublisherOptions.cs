namespace GlucoDesk.Application.Cgm.WidgetState.Options;

/// <summary>
/// Provides options for publishing glucose widget state snapshots.
/// </summary>
public sealed record WidgetStatePublisherOptions
{
    /// <summary>
    /// Gets the default duration after which a widget reading is considered stale.
    /// </summary>
    public static readonly TimeSpan DefaultStaleAfter = TimeSpan.FromMinutes(15);

    /// <summary>
    /// Initializes a new instance of the <see cref="WidgetStatePublisherOptions"/> class.
    /// </summary>
    /// <param name="staleAfter">The duration after which a reading should be considered stale.</param>
    /// <param name="unavailableStatusMessage">The default unavailable status message.</param>
    public WidgetStatePublisherOptions(
        TimeSpan staleAfter,
        string unavailableStatusMessage)
    {
        if (staleAfter <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(staleAfter),
                staleAfter,
                "Stale duration must be greater than zero.");
        }

        if (string.IsNullOrWhiteSpace(unavailableStatusMessage))
        {
            throw new ArgumentException(
                "Unavailable status message cannot be empty.",
                nameof(unavailableStatusMessage));
        }

        StaleAfter = staleAfter;
        UnavailableStatusMessage = unavailableStatusMessage;
    }

    /// <summary>
    /// Gets the duration after which a reading should be considered stale.
    /// </summary>
    public TimeSpan StaleAfter { get; }

    /// <summary>
    /// Gets the default unavailable status message.
    /// </summary>
    public string UnavailableStatusMessage { get; }

    /// <summary>
    /// Creates the default widget state publisher options.
    /// </summary>
    /// <returns>The default widget state publisher options.</returns>
    public static WidgetStatePublisherOptions Default()
    {
        return new WidgetStatePublisherOptions(
            DefaultStaleAfter,
            "Glucose unavailable");
    }
}