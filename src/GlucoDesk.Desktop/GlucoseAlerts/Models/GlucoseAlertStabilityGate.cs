namespace GlucoDesk.Desktop.GlucoseAlerts.Models;

/// <summary>
/// Requires the same glucose alert condition to be observed repeatedly before it is presented.
/// </summary>
public sealed class GlucoseAlertStabilityGate
{
    private int _requiredConsecutiveObservations;
    private GlucoseAlertKind _currentKind = GlucoseAlertKind.None;
    private int _consecutiveObservations;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlucoseAlertStabilityGate"/> class.
    /// </summary>
    /// <param name="requiredConsecutiveObservations">The number of consecutive observations required before presenting the alert.</param>
    public GlucoseAlertStabilityGate(int requiredConsecutiveObservations = 2)
    {
        ValidateRequiredConsecutiveObservations(requiredConsecutiveObservations);

        _requiredConsecutiveObservations = requiredConsecutiveObservations;
    }

    /// <summary>
    /// Gets the required number of consecutive observations.
    /// </summary>
    public int RequiredConsecutiveObservations => _requiredConsecutiveObservations;

    /// <summary>
    /// Gets the currently tracked alert kind.
    /// </summary>
    public GlucoseAlertKind CurrentKind => _currentKind;

    /// <summary>
    /// Gets the number of consecutive observations for the current alert kind.
    /// </summary>
    public int ConsecutiveObservations => _consecutiveObservations;

    /// <summary>
    /// Updates the required number of consecutive observations.
    /// </summary>
    /// <param name="requiredConsecutiveObservations">The required number of consecutive observations.</param>
    public void Configure(int requiredConsecutiveObservations)
    {
        ValidateRequiredConsecutiveObservations(requiredConsecutiveObservations);

        if (_requiredConsecutiveObservations == requiredConsecutiveObservations)
        {
            return;
        }

        _requiredConsecutiveObservations = requiredConsecutiveObservations;
        Reset();
    }

    /// <summary>
    /// Records an alert kind observation and returns whether it is stable enough to present.
    /// </summary>
    /// <param name="kind">The observed alert kind.</param>
    /// <returns>True when the alert kind is stable enough to present; otherwise false.</returns>
    public bool ShouldPresent(GlucoseAlertKind kind)
    {
        if (kind == GlucoseAlertKind.None)
        {
            Reset();
            return false;
        }

        if (kind != _currentKind)
        {
            _currentKind = kind;
            _consecutiveObservations = 1;

            return _consecutiveObservations >= _requiredConsecutiveObservations;
        }

        _consecutiveObservations++;

        return _consecutiveObservations >= _requiredConsecutiveObservations;
    }

    /// <summary>
    /// Resets the tracked alert stability state.
    /// </summary>
    public void Reset()
    {
        _currentKind = GlucoseAlertKind.None;
        _consecutiveObservations = 0;
    }

    #region Helpers

    /// <summary>
    /// Validates the required number of consecutive observations.
    /// </summary>
    /// <param name="requiredConsecutiveObservations">The required number of consecutive observations.</param>
    private static void ValidateRequiredConsecutiveObservations(int requiredConsecutiveObservations)
    {
        if (requiredConsecutiveObservations < 1)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requiredConsecutiveObservations),
                requiredConsecutiveObservations,
                "At least one observation is required.");
        }
    }

    #endregion
}
