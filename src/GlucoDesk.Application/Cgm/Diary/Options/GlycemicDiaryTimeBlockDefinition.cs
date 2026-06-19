using GlucoDesk.Application.Cgm.Diary.Enums;

namespace GlucoDesk.Application.Cgm.Diary.Options;

/// <summary>
/// Defines a standard glycemic diary time block.
/// </summary>
public sealed record GlycemicDiaryTimeBlockDefinition
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryTimeBlockDefinition"/> class.
    /// </summary>
    /// <param name="kind">The time block kind.</param>
    /// <param name="label">The user-facing label.</param>
    /// <param name="startsAt">The local start time.</param>
    /// <param name="endsAt">The local end time.</param>
    /// <param name="sortOrder">The sort order.</param>
    public GlycemicDiaryTimeBlockDefinition(
        GlycemicDiaryTimeBlockKind kind,
        string label,
        TimeOnly startsAt,
        TimeOnly endsAt,
        int sortOrder)
    {
        if (kind == GlycemicDiaryTimeBlockKind.Unknown)
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "Time block kind must be specified.");
        }

        if (string.IsNullOrWhiteSpace(label))
        {
            throw new ArgumentException(
                "Time block label cannot be empty.",
                nameof(label));
        }

        if (endsAt <= startsAt)
        {
            throw new ArgumentOutOfRangeException(
                nameof(endsAt),
                endsAt,
                "Time block end time must be greater than start time.");
        }

        if (sortOrder <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(sortOrder),
                sortOrder,
                "Sort order must be greater than zero.");
        }

        Kind = kind;
        Label = label;
        StartsAt = startsAt;
        EndsAt = endsAt;
        SortOrder = sortOrder;
    }

    /// <summary>
    /// Gets the time block kind.
    /// </summary>
    public GlycemicDiaryTimeBlockKind Kind { get; }

    /// <summary>
    /// Gets the user-facing label.
    /// </summary>
    public string Label { get; }

    /// <summary>
    /// Gets the local start time.
    /// </summary>
    public TimeOnly StartsAt { get; }

    /// <summary>
    /// Gets the local end time.
    /// </summary>
    public TimeOnly EndsAt { get; }

    /// <summary>
    /// Gets the sort order.
    /// </summary>
    public int SortOrder { get; }
}