namespace GlucoDesk.Application.Cgm.History.Continuity.Options;

/// <summary>
/// Provides options for glucose history continuity analysis.
/// </summary>
public sealed record HistoryContinuityOptions
{
    /// <summary>
    /// Gets the default expected CGM reading interval.
    /// </summary>
    public static readonly TimeSpan DefaultExpectedReadingInterval = TimeSpan.FromMinutes(5);

    /// <summary>
    /// Gets the default maximum allowed gap before a discontinuity is reported.
    /// </summary>
    public static readonly TimeSpan DefaultMaximumAllowedGap = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Gets the default history continuity options.
    /// </summary>
    public static HistoryContinuityOptions Default => new(
        DefaultExpectedReadingInterval,
        DefaultMaximumAllowedGap);

    /// <summary>
    /// Initializes a new instance of the <see cref="HistoryContinuityOptions"/> class.
    /// </summary>
    /// <param name="expectedReadingInterval">The expected reading interval.</param>
    /// <param name="maximumAllowedGap">The maximum allowed gap before a discontinuity is reported.</param>
    public HistoryContinuityOptions(
        TimeSpan expectedReadingInterval,
        TimeSpan maximumAllowedGap)
    {
        if (expectedReadingInterval <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(expectedReadingInterval),
                expectedReadingInterval,
                "Expected reading interval must be greater than zero.");
        }

        if (maximumAllowedGap < expectedReadingInterval)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maximumAllowedGap),
                maximumAllowedGap,
                "Maximum allowed gap must be greater than or equal to the expected reading interval.");
        }

        ExpectedReadingInterval = expectedReadingInterval;
        MaximumAllowedGap = maximumAllowedGap;
    }

    /// <summary>
    /// Gets the expected reading interval.
    /// </summary>
    public TimeSpan ExpectedReadingInterval { get; }

    /// <summary>
    /// Gets the maximum allowed gap before a discontinuity is reported.
    /// </summary>
    public TimeSpan MaximumAllowedGap { get; }
}