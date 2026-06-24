using GlucoDesk.Application.Cgm.Diary.Enums;
using GlucoDesk.Application.Cgm.Diary.Patterns.Enums;

namespace GlucoDesk.Application.Cgm.Diary.Patterns.Results;

/// <summary>
/// Represents a detected glycemic diary pattern.
/// </summary>
public sealed record GlycemicDiaryPattern
{
    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryPattern"/> class.
    /// </summary>
    /// <param name="kind">The pattern kind.</param>
    /// <param name="severity">The pattern severity.</param>
    /// <param name="title">The pattern title.</param>
    /// <param name="description">The pattern description.</param>
    /// <param name="supportingDaysCount">The number of days supporting the pattern.</param>
    /// <param name="timeBlockKind">The optional time block kind.</param>
    /// <param name="timeBlockLabel">The optional time block label.</param>
    public GlycemicDiaryPattern(
        GlycemicDiaryPatternKind kind,
        GlycemicDiaryPatternSeverity severity,
        string title,
        string description,
        int supportingDaysCount,
        GlycemicDiaryTimeBlockKind? timeBlockKind = null,
        string? timeBlockLabel = null)
    {
        if (kind == GlycemicDiaryPatternKind.Unknown)
        {
            throw new ArgumentOutOfRangeException(
                nameof(kind),
                kind,
                "Pattern kind must be specified.");
        }

        if (severity == GlycemicDiaryPatternSeverity.Unknown)
        {
            throw new ArgumentOutOfRangeException(
                nameof(severity),
                severity,
                "Pattern severity must be specified.");
        }

        if (supportingDaysCount < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(supportingDaysCount),
                supportingDaysCount,
                "Supporting days count cannot be negative.");
        }

        ArgumentException.ThrowIfNullOrWhiteSpace(title);
        ArgumentException.ThrowIfNullOrWhiteSpace(description);

        Kind = kind;
        Severity = severity;
        Title = title;
        Description = description;
        SupportingDaysCount = supportingDaysCount;
        TimeBlockKind = timeBlockKind;
        TimeBlockLabel = timeBlockLabel;
    }

    /// <summary>
    /// Gets the pattern kind.
    /// </summary>
    public GlycemicDiaryPatternKind Kind { get; }

    /// <summary>
    /// Gets the pattern severity.
    /// </summary>
    public GlycemicDiaryPatternSeverity Severity { get; }

    /// <summary>
    /// Gets the pattern title.
    /// </summary>
    public string Title { get; }

    /// <summary>
    /// Gets the pattern description.
    /// </summary>
    public string Description { get; }

    /// <summary>
    /// Gets the number of days supporting the pattern.
    /// </summary>
    public int SupportingDaysCount { get; }

    /// <summary>
    /// Gets the optional time block kind.
    /// </summary>
    public GlycemicDiaryTimeBlockKind? TimeBlockKind { get; }

    /// <summary>
    /// Gets the optional time block label.
    /// </summary>
    public string? TimeBlockLabel { get; }

    /// <summary>
    /// Gets whether the pattern is related to a specific time block.
    /// </summary>
    public bool IsTimeBlockPattern => TimeBlockKind.HasValue;
}
