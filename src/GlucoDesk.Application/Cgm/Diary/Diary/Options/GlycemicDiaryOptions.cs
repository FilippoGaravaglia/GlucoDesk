using GlucoDesk.Application.Cgm.Diary.Enums;

namespace GlucoDesk.Application.Cgm.Diary.Options;

/// <summary>
/// Provides options for glycemic diary generation.
/// </summary>
public sealed record GlycemicDiaryOptions
{
    /// <summary>
    /// Gets the default low threshold for the awareness range.
    /// </summary>
    public const decimal DefaultLowRangeMgDl = 70m;

    /// <summary>
    /// Gets the default high threshold for the awareness range.
    /// </summary>
    public const decimal DefaultHighRangeMgDl = 180m;

    /// <summary>
    /// Gets the default glycemic diary options.
    /// </summary>
    public static GlycemicDiaryOptions Default => new(
        TimeZoneInfo.Local,
        DefaultLowRangeMgDl,
        DefaultHighRangeMgDl,
        [
            new GlycemicDiaryTimeBlockDefinition(
                GlycemicDiaryTimeBlockKind.Breakfast,
                "Breakfast",
                new TimeOnly(6, 0),
                new TimeOnly(10, 59, 59),
                1),
            new GlycemicDiaryTimeBlockDefinition(
                GlycemicDiaryTimeBlockKind.Lunch,
                "Lunch",
                new TimeOnly(11, 0),
                new TimeOnly(15, 59, 59),
                2),
            new GlycemicDiaryTimeBlockDefinition(
                GlycemicDiaryTimeBlockKind.Dinner,
                "Dinner",
                new TimeOnly(18, 0),
                new TimeOnly(21, 59, 59),
                3),
            new GlycemicDiaryTimeBlockDefinition(
                GlycemicDiaryTimeBlockKind.Bedtime,
                "Pre-night",
                new TimeOnly(22, 0),
                new TimeOnly(23, 59, 59),
                4)
        ]);

    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryOptions"/> class.
    /// </summary>
    /// <param name="timeZone">The diary time zone.</param>
    /// <param name="lowRangeMgDl">The low range threshold in mg/dL.</param>
    /// <param name="highRangeMgDl">The high range threshold in mg/dL.</param>
    /// <param name="timeBlocks">The diary time blocks.</param>
    public GlycemicDiaryOptions(
        TimeZoneInfo timeZone,
        decimal lowRangeMgDl,
        decimal highRangeMgDl,
        IReadOnlyCollection<GlycemicDiaryTimeBlockDefinition> timeBlocks)
    {
        ArgumentNullException.ThrowIfNull(timeZone);
        ArgumentNullException.ThrowIfNull(timeBlocks);

        if (lowRangeMgDl <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(lowRangeMgDl),
                lowRangeMgDl,
                "Low range threshold must be greater than zero.");
        }

        if (highRangeMgDl <= lowRangeMgDl)
        {
            throw new ArgumentOutOfRangeException(
                nameof(highRangeMgDl),
                highRangeMgDl,
                "High range threshold must be greater than low range threshold.");
        }

        if (timeBlocks.Count == 0)
        {
            throw new ArgumentException(
                "At least one time block must be configured.",
                nameof(timeBlocks));
        }

        TimeZone = timeZone;
        LowRangeMgDl = lowRangeMgDl;
        HighRangeMgDl = highRangeMgDl;
        TimeBlocks = timeBlocks
            .OrderBy(block => block.SortOrder)
            .ToArray();
    }

    /// <summary>
    /// Gets the diary time zone.
    /// </summary>
    public TimeZoneInfo TimeZone { get; }

    /// <summary>
    /// Gets the low range threshold in mg/dL.
    /// </summary>
    public decimal LowRangeMgDl { get; }

    /// <summary>
    /// Gets the high range threshold in mg/dL.
    /// </summary>
    public decimal HighRangeMgDl { get; }

    /// <summary>
    /// Gets the diary time blocks.
    /// </summary>
    public IReadOnlyCollection<GlycemicDiaryTimeBlockDefinition> TimeBlocks { get; }
}