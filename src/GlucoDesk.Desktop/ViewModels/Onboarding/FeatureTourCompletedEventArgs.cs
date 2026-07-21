namespace GlucoDesk.Desktop.ViewModels.Onboarding;

/// <summary>
/// Provides information about completion of the first-run feature tour.
/// </summary>
public sealed class FeatureTourCompletedEventArgs : EventArgs
{
    /// <summary>
    /// Initializes a new event instance.
    /// </summary>
    /// <param name="wasSkipped">
    /// Whether the user completed the flow through the skip action.
    /// </param>
    public FeatureTourCompletedEventArgs(bool wasSkipped)
    {
        WasSkipped = wasSkipped;
    }

    /// <summary>
    /// Gets a value indicating whether the tour was skipped.
    /// </summary>
    public bool WasSkipped { get; }
}
