using GlucoDesk.Application.Cgm.Diary.Enums;
using GlucoDesk.Application.Cgm.Diary.Patterns.Enums;
using GlucoDesk.Application.Cgm.Diary.Patterns.Results;
using GlucoDesk.Application.Cgm.Diary.Patterns.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.History.Completeness.Services.Abstractions;

namespace GlucoDesk.Application.Cgm.Diary.Patterns.Services;

/// <summary>
/// Analyzes glycemic diary reports to detect local recurring glucose patterns.
/// </summary>
public sealed class GlycemicDiaryPatternAnalysisService : IGlycemicDiaryPatternAnalysisService
{
    private const decimal LowThresholdMgDl = 70m;
    private const decimal HighThresholdMgDl = 180m;
    private const decimal VariableRangeThresholdMgDl = 80m;
    private const decimal StableRangeThresholdMgDl = 45m;
    private const decimal StableTimeInRangeThreshold = 85m;
    private const int MinimumSupportingDays = 2;
    private const int MinimumStableSupportingDays = 3;

    private readonly IGlucoseHistoryCompletenessScoringService _completenessScoringService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryPatternAnalysisService"/> class.
    /// </summary>
    /// <param name="completenessScoringService">The history completeness scoring service.</param>
    public GlycemicDiaryPatternAnalysisService(
        IGlucoseHistoryCompletenessScoringService completenessScoringService)
    {
        ArgumentNullException.ThrowIfNull(completenessScoringService);

        _completenessScoringService = completenessScoringService;
    }

    /// <inheritdoc />
    public GlycemicDiaryPatternAnalysis Analyze(GlycemicDiaryReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var patterns = new List<GlycemicDiaryPattern>();
        var dailyEntries = report.DailyEntries
            .OrderBy(day => day.Date)
            .ToArray();

        var daysWithDataCount = dailyEntries.Count(day => day.HasData);

        AddCoveragePattern(report, patterns);
        AddTimeBlockPatterns(dailyEntries, patterns);

        return new GlycemicDiaryPatternAnalysis(
            report.PeriodStartsAt,
            report.PeriodEndsAt,
            dailyEntries.Length,
            daysWithDataCount,
            patterns);
    }

    #region Helpers

    /// <summary>
    /// Adds a data coverage pattern when local history is incomplete.
    /// </summary>
    /// <param name="report">The diary report.</param>
    /// <param name="patterns">The detected pattern collection.</param>
    private void AddCoveragePattern(
        GlycemicDiaryReport report,
        ICollection<GlycemicDiaryPattern> patterns)
    {
        var completenessScore = _completenessScoringService.Calculate(report.OverallContinuity);

        if (!completenessScore.RequiresCaution)
        {
            return;
        }

        patterns.Add(new GlycemicDiaryPattern(
            GlycemicDiaryPatternKind.LimitedDataCoverage,
            GlycemicDiaryPatternSeverity.Caution,
            "Limited local history coverage",
            $"Local history coverage is {completenessScore.CoverageText}. Detected patterns should be interpreted carefully.",
            report.IncompleteDaysCount));
    }

    /// <summary>
    /// Adds recurring time block patterns.
    /// </summary>
    /// <param name="dailyEntries">The daily entries.</param>
    /// <param name="patterns">The detected pattern collection.</param>
    private static void AddTimeBlockPatterns(
        IReadOnlyCollection<GlycemicDiaryDailyEntry> dailyEntries,
        ICollection<GlycemicDiaryPattern> patterns)
    {
        var blockGroups = dailyEntries
            .SelectMany(day => day.TimeBlocks.Select(block => new DailyBlock(day, block)))
            .Where(item => item.Block.HasData)
            .GroupBy(item => new
            {
                item.Block.Kind,
                item.Block.Label
            })
            .OrderBy(group => GetTimeBlockSortOrder(group.Key.Kind))
            .ToArray();

        foreach (var group in blockGroups)
        {
            var values = group.ToArray();

            AddLowPattern(group.Key.Kind, group.Key.Label, values, patterns);
            AddHighPattern(group.Key.Kind, group.Key.Label, values, patterns);
            AddVariabilityPattern(group.Key.Kind, group.Key.Label, values, patterns);
            AddStablePattern(group.Key.Kind, group.Key.Label, values, patterns);
        }
    }

    /// <summary>
    /// Adds a recurring low pattern.
    /// </summary>
    /// <param name="kind">The time block kind.</param>
    /// <param name="label">The time block label.</param>
    /// <param name="values">The block values.</param>
    /// <param name="patterns">The detected pattern collection.</param>
    private static void AddLowPattern(
        GlycemicDiaryTimeBlockKind kind,
        string label,
        IReadOnlyCollection<DailyBlock> values,
        ICollection<GlycemicDiaryPattern> patterns)
    {
        var supportingDays = values
            .Count(item => item.Block.RepresentativeValueMgDl < LowThresholdMgDl);

        if (supportingDays < MinimumSupportingDays)
        {
            return;
        }

        patterns.Add(new GlycemicDiaryPattern(
            GlycemicDiaryPatternKind.RecurringLow,
            GlycemicDiaryPatternSeverity.Important,
            $"Recurring low tendency around {label}",
            $"{supportingDays} days show representative glucose below {LowThresholdMgDl:0} mg/dL around {label}.",
            supportingDays,
            kind,
            label));
    }

    /// <summary>
    /// Adds a recurring high pattern.
    /// </summary>
    /// <param name="kind">The time block kind.</param>
    /// <param name="label">The time block label.</param>
    /// <param name="values">The block values.</param>
    /// <param name="patterns">The detected pattern collection.</param>
    private static void AddHighPattern(
        GlycemicDiaryTimeBlockKind kind,
        string label,
        IReadOnlyCollection<DailyBlock> values,
        ICollection<GlycemicDiaryPattern> patterns)
    {
        var supportingDays = values
            .Count(item => item.Block.RepresentativeValueMgDl > HighThresholdMgDl);

        if (supportingDays < MinimumSupportingDays)
        {
            return;
        }

        patterns.Add(new GlycemicDiaryPattern(
            GlycemicDiaryPatternKind.RecurringHigh,
            GlycemicDiaryPatternSeverity.Caution,
            $"Recurring high tendency around {label}",
            $"{supportingDays} days show representative glucose above {HighThresholdMgDl:0} mg/dL around {label}.",
            supportingDays,
            kind,
            label));
    }

    /// <summary>
    /// Adds a recurring variability pattern.
    /// </summary>
    /// <param name="kind">The time block kind.</param>
    /// <param name="label">The time block label.</param>
    /// <param name="values">The block values.</param>
    /// <param name="patterns">The detected pattern collection.</param>
    private static void AddVariabilityPattern(
        GlycemicDiaryTimeBlockKind kind,
        string label,
        IReadOnlyCollection<DailyBlock> values,
        ICollection<GlycemicDiaryPattern> patterns)
    {
        var supportingDays = values
            .Count(item => CalculateRange(item.Block) >= VariableRangeThresholdMgDl);

        if (supportingDays < MinimumSupportingDays)
        {
            return;
        }

        patterns.Add(new GlycemicDiaryPattern(
            GlycemicDiaryPatternKind.RecurringVariability,
            GlycemicDiaryPatternSeverity.Caution,
            $"Recurring variability around {label}",
            $"{supportingDays} days show a glucose spread of at least {VariableRangeThresholdMgDl:0} mg/dL around {label}.",
            supportingDays,
            kind,
            label));
    }

    /// <summary>
    /// Adds a stable time block pattern.
    /// </summary>
    /// <param name="kind">The time block kind.</param>
    /// <param name="label">The time block label.</param>
    /// <param name="values">The block values.</param>
    /// <param name="patterns">The detected pattern collection.</param>
    private static void AddStablePattern(
        GlycemicDiaryTimeBlockKind kind,
        string label,
        IReadOnlyCollection<DailyBlock> values,
        ICollection<GlycemicDiaryPattern> patterns)
    {
        var stableDays = values
            .Count(item =>
                CalculateRange(item.Block) <= StableRangeThresholdMgDl &&
                item.Day.TimeInRangePercentage >= StableTimeInRangeThreshold);

        if (stableDays < MinimumStableSupportingDays)
        {
            return;
        }

        patterns.Add(new GlycemicDiaryPattern(
            GlycemicDiaryPatternKind.StableTimeBlock,
            GlycemicDiaryPatternSeverity.Info,
            $"Stable pattern around {label}",
            $"{stableDays} days show a relatively stable glucose profile around {label}.",
            stableDays,
            kind,
            label));
    }

    /// <summary>
    /// Calculates the glucose range for a time block.
    /// </summary>
    /// <param name="block">The time block.</param>
    /// <returns>The glucose range in mg/dL.</returns>
    private static decimal CalculateRange(GlycemicDiaryTimeBlockEntry block)
    {
        if (!block.MinimumMgDl.HasValue || !block.MaximumMgDl.HasValue)
        {
            return 0m;
        }

        return block.MaximumMgDl.Value - block.MinimumMgDl.Value;
    }

    /// <summary>
    /// Gets a deterministic sort order for known time blocks.
    /// </summary>
    /// <param name="kind">The time block kind.</param>
    /// <returns>The sort order.</returns>
    private static int GetTimeBlockSortOrder(GlycemicDiaryTimeBlockKind kind)
    {
        return kind switch
        {
            GlycemicDiaryTimeBlockKind.Breakfast => 1,
            GlycemicDiaryTimeBlockKind.Lunch => 2,
            GlycemicDiaryTimeBlockKind.Dinner => 3,
            GlycemicDiaryTimeBlockKind.Bedtime => 4,
            _ => 99
        };
    }

    private sealed record DailyBlock(
        GlycemicDiaryDailyEntry Day,
        GlycemicDiaryTimeBlockEntry Block);

    #endregion
}
