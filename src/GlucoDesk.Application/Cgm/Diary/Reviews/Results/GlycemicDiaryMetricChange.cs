using GlucoDesk.Application.Cgm.Diary.Reviews.Enums;

namespace GlucoDesk.Application.Cgm.Diary.Reviews.Results;

/// <summary>
/// Represents a metric change between a previous and current glycemic diary period.
/// </summary>
public sealed record GlycemicDiaryMetricChange
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryMetricChange"/> class.
    /// </summary>
    /// <param name="kind">The metric kind.</param>
    /// <param name="displayName">The user-facing metric name.</param>
    /// <param name="previousValueText">The previous period value.</param>
    /// <param name="currentValueText">The current period value.</param>
    /// <param name="deltaText">The formatted delta.</param>
    /// <param name="direction">The change direction.</param>
    /// <param name="severity">The change severity.</param>
    /// <param name="description">The user-facing change description.</param>
    public GlycemicDiaryMetricChange(
        GlycemicDiaryReviewMetricKind kind,
        string displayName,
        string previousValueText,
        string currentValueText,
        string deltaText,
        GlycemicDiaryReviewChangeDirection direction,
        GlycemicDiaryReviewSignalSeverity severity,
        string description)
    {
        if (kind == GlycemicDiaryReviewMetricKind.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(kind), kind, "Metric kind must be specified.");
        }

        if (direction == GlycemicDiaryReviewChangeDirection.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(direction), direction, "Change direction must be specified.");
        }

        if (severity == GlycemicDiaryReviewSignalSeverity.Unknown)
        {
            throw new ArgumentOutOfRangeException(nameof(severity), severity, "Signal severity must be specified.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(displayName);
        ArgumentException.ThrowIfNullOrWhiteSpace(previousValueText);
        ArgumentException.ThrowIfNullOrWhiteSpace(currentValueText);
        ArgumentException.ThrowIfNullOrWhiteSpace(deltaText);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Kind = kind;
        DisplayName = displayName;
        PreviousValueText = previousValueText;
        CurrentValueText = currentValueText;
        DeltaText = deltaText;
        Direction = direction;
        Severity = severity;
        Description = description;
    }

    /// <summary>
    /// Gets the metric kind.
    /// </summary>
    public GlycemicDiaryReviewMetricKind Kind { get; }

    /// <summary>
    /// Gets the user-facing metric name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the previous period value.
    /// </summary>
    public string PreviousValueText { get; }

    /// <summary>
    /// Gets the current period value.
    /// </summary>
    public string CurrentValueText { get; }

    /// <summary>
    /// Gets the formatted delta.
    /// </summary>
    public string DeltaText { get; }

    /// <summary>
    /// Gets the change direction.
    /// </summary>
    public GlycemicDiaryReviewChangeDirection Direction { get; }

    /// <summary>
    /// Gets the signal severity.
    /// </summary>
    public GlycemicDiaryReviewSignalSeverity Severity { get; }

    /// <summary>
    /// Gets the user-facing change description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets whether the metric changed meaningfully.
    /// </summary>
    public bool HasMeaningfulChange => Direction != GlycemicDiaryReviewChangeDirection.Unchanged;
}
