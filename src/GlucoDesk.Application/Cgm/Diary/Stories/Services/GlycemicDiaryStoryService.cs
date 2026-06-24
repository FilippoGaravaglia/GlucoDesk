using GlucoDesk.Application.Cgm.Diary.Results;
using GlucoDesk.Application.Cgm.Diary.Stories.Enums;
using GlucoDesk.Application.Cgm.Diary.Stories.Results;
using GlucoDesk.Application.Cgm.Diary.Stories.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Completeness.Results;
using GlucoDesk.Application.Cgm.History.Completeness.Services.Abstractions;

namespace GlucoDesk.Application.Cgm.Diary.Stories.Services;

/// <summary>
/// Builds user-facing glycemic diary stories from diary reports.
/// </summary>
public sealed class GlycemicDiaryStoryService : IGlycemicDiaryStoryService
{
    private const decimal LowGlucoseThresholdMgDl = 70m;
    private const decimal HighGlucoseThresholdMgDl = 180m;
    private const decimal VeryHighGlucoseThresholdMgDl = 250m;
    private const decimal ExcellentTimeInRangeThreshold = 90m;
    private const decimal StableTimeInRangeThreshold = 80m;
    private const decimal PartialDailyCoverageThreshold = 90m;
    private const decimal PoorDailyCoverageThreshold = 50m;

    private readonly IGlucoseHistoryCompletenessScoringService _completenessScoringService;

    /// <summary>
    /// Initializes a new instance of the <see cref="GlycemicDiaryStoryService"/> class.
    /// </summary>
    /// <param name="completenessScoringService">The history completeness scoring service.</param>
    public GlycemicDiaryStoryService(
        IGlucoseHistoryCompletenessScoringService completenessScoringService)
    {
        ArgumentNullException.ThrowIfNull(completenessScoringService);

        _completenessScoringService = completenessScoringService;
    }

    /// <inheritdoc />
    public GlycemicDiaryStory CreateStory(GlycemicDiaryReport report)
    {
        ArgumentNullException.ThrowIfNull(report);

        var completenessScore = _completenessScoringService.Calculate(report.OverallContinuity);
        var level = CalculateOverallLevel(report, completenessScore);
        var dailyStories = report.DailyEntries
            .OrderBy(entry => entry.Date)
            .Select(CreateDailyStory)
            .ToArray();

        return new GlycemicDiaryStory(
            report.PeriodStartsAt,
            report.PeriodEndsAt,
            level,
            BuildOverallHeadline(level),
            BuildOverallSummary(report, completenessScore, level),
            BuildHistoryReliabilityText(completenessScore),
            dailyStories);
    }

    #region Helpers

    /// <summary>
    /// Creates a daily glucose story.
    /// </summary>
    /// <param name="entry">The daily diary entry.</param>
    /// <returns>The generated daily story.</returns>
    private static GlycemicDiaryDailyStory CreateDailyStory(GlycemicDiaryDailyEntry entry)
    {
        var level = CalculateDailyLevel(entry);

        return new GlycemicDiaryDailyStory(
            entry.Date,
            level,
            BuildDailyHeadline(entry, level),
            BuildDailySummary(entry),
            BuildDailyDataQualityText(entry),
            BuildDailyHighlights(entry));
    }

    /// <summary>
    /// Calculates the overall story level.
    /// </summary>
    /// <param name="report">The diary report.</param>
    /// <param name="completenessScore">The completeness score.</param>
    /// <returns>The overall story level.</returns>
    private static GlycemicDiaryStoryLevel CalculateOverallLevel(
        GlycemicDiaryReport report,
        GlucoseHistoryCompletenessScore completenessScore)
    {
        if (report.ReadingsCount == 0)
        {
            return GlycemicDiaryStoryLevel.NoData;
        }

        if (completenessScore.RequiresCaution)
        {
            return GlycemicDiaryStoryLevel.Caution;
        }

        if (HasLowGlucose(report.MinimumMgDl) || HasVeryHighGlucose(report.MaximumMgDl))
        {
            return GlycemicDiaryStoryLevel.Variable;
        }

        if (report.TimeInRangePercentage >= ExcellentTimeInRangeThreshold &&
            report.MinimumMgDl >= LowGlucoseThresholdMgDl &&
            report.MaximumMgDl <= HighGlucoseThresholdMgDl)
        {
            return GlycemicDiaryStoryLevel.Excellent;
        }

        if (report.TimeInRangePercentage >= StableTimeInRangeThreshold)
        {
            return GlycemicDiaryStoryLevel.Stable;
        }

        return GlycemicDiaryStoryLevel.Variable;
    }

    /// <summary>
    /// Calculates the daily story level.
    /// </summary>
    /// <param name="entry">The daily entry.</param>
    /// <returns>The daily story level.</returns>
    private static GlycemicDiaryStoryLevel CalculateDailyLevel(GlycemicDiaryDailyEntry entry)
    {
        if (!entry.HasData)
        {
            return GlycemicDiaryStoryLevel.NoData;
        }

        if (entry.DataCoveragePercentage < PoorDailyCoverageThreshold)
        {
            return GlycemicDiaryStoryLevel.Caution;
        }

        if (!entry.IsDataComplete ||
            entry.GapCount > 0 ||
            entry.DataCoveragePercentage < PartialDailyCoverageThreshold)
        {
            return GlycemicDiaryStoryLevel.Caution;
        }

        if (HasLowGlucose(entry.MinimumMgDl) || HasVeryHighGlucose(entry.MaximumMgDl))
        {
            return GlycemicDiaryStoryLevel.Variable;
        }

        if (entry.TimeInRangePercentage >= ExcellentTimeInRangeThreshold &&
            entry.MinimumMgDl >= LowGlucoseThresholdMgDl &&
            entry.MaximumMgDl <= HighGlucoseThresholdMgDl)
        {
            return GlycemicDiaryStoryLevel.Excellent;
        }

        if (entry.TimeInRangePercentage >= StableTimeInRangeThreshold)
        {
            return GlycemicDiaryStoryLevel.Stable;
        }

        return GlycemicDiaryStoryLevel.Variable;
    }

    /// <summary>
    /// Builds the overall headline.
    /// </summary>
    /// <param name="level">The overall story level.</param>
    /// <returns>The headline.</returns>
    private static string BuildOverallHeadline(GlycemicDiaryStoryLevel level)
    {
        return level switch
        {
            GlycemicDiaryStoryLevel.NoData => "No local glucose history available",
            GlycemicDiaryStoryLevel.Caution => "Glucose story limited by data gaps",
            GlycemicDiaryStoryLevel.Variable => "Variable glucose period",
            GlycemicDiaryStoryLevel.Stable => "Mostly stable glucose period",
            GlycemicDiaryStoryLevel.Excellent => "Stable glucose period",
            _ => "Glucose period summary"
        };
    }

    /// <summary>
    /// Builds the overall summary text.
    /// </summary>
    /// <param name="report">The diary report.</param>
    /// <param name="completenessScore">The completeness score.</param>
    /// <param name="level">The overall story level.</param>
    /// <returns>The summary text.</returns>
    private static string BuildOverallSummary(
        GlycemicDiaryReport report,
        GlucoseHistoryCompletenessScore completenessScore,
        GlycemicDiaryStoryLevel level)
    {
        if (level == GlycemicDiaryStoryLevel.NoData)
        {
            return "No local glucose readings are available for the selected period.";
        }

        if (level == GlycemicDiaryStoryLevel.Caution)
        {
            return $"The selected period has {completenessScore.CoverageText} local history coverage. Interpret averages, time-in-range, and daily summaries carefully.";
        }

        return $"Average glucose was {FormatMgDl(report.AverageMgDl)}, time in range was {FormatPercentage(report.TimeInRangePercentage)}, and the observed range was {FormatMgDl(report.MinimumMgDl)} - {FormatMgDl(report.MaximumMgDl)}.";
    }

    /// <summary>
    /// Builds the history reliability text.
    /// </summary>
    /// <param name="completenessScore">The completeness score.</param>
    /// <returns>The reliability text.</returns>
    private static string BuildHistoryReliabilityText(
        GlucoseHistoryCompletenessScore completenessScore)
    {
        return $"History reliability: {completenessScore.StatusText} · {completenessScore.CoverageText}. {completenessScore.DetailText}";
    }

    /// <summary>
    /// Builds the daily headline.
    /// </summary>
    /// <param name="entry">The daily entry.</param>
    /// <param name="level">The daily story level.</param>
    /// <returns>The daily headline.</returns>
    private static string BuildDailyHeadline(
        GlycemicDiaryDailyEntry entry,
        GlycemicDiaryStoryLevel level)
    {
        if (level == GlycemicDiaryStoryLevel.NoData)
        {
            return "No local glucose data";
        }

        if (level == GlycemicDiaryStoryLevel.Caution)
        {
            return "Partial local history";
        }

        if (HasLowGlucose(entry.MinimumMgDl) && HasVeryHighGlucose(entry.MaximumMgDl))
        {
            return "Variable day with low and high excursions";
        }

        if (HasLowGlucose(entry.MinimumMgDl))
        {
            return "Day with low glucose episodes";
        }

        if (HasVeryHighGlucose(entry.MaximumMgDl))
        {
            return "Day with high glucose peaks";
        }

        return level switch
        {
            GlycemicDiaryStoryLevel.Excellent => "Stable day",
            GlycemicDiaryStoryLevel.Stable => "Mostly stable day",
            GlycemicDiaryStoryLevel.Variable => "Variable day",
            _ => "Daily glucose summary"
        };
    }

    /// <summary>
    /// Builds the daily summary text.
    /// </summary>
    /// <param name="entry">The daily entry.</param>
    /// <returns>The daily summary text.</returns>
    private static string BuildDailySummary(GlycemicDiaryDailyEntry entry)
    {
        if (!entry.HasData)
        {
            return "No readings are available for this day.";
        }

        return $"Average glucose was {FormatMgDl(entry.AverageMgDl)}, time in range was {FormatPercentage(entry.TimeInRangePercentage)}, and the observed range was {FormatMgDl(entry.MinimumMgDl)} - {FormatMgDl(entry.MaximumMgDl)}.";
    }

    /// <summary>
    /// Builds the daily data quality text.
    /// </summary>
    /// <param name="entry">The daily entry.</param>
    /// <returns>The data quality text.</returns>
    private static string BuildDailyDataQualityText(GlycemicDiaryDailyEntry entry)
    {
        if (!entry.HasData)
        {
            return "No readings are available for this day.";
        }

        if (entry.IsDataComplete)
        {
            return "Daily local history appears complete.";
        }

        return $"Daily local history is partial: {FormatPercentage(entry.DataCoveragePercentage)} coverage and {entry.GapCount} {Pluralize("gap", entry.GapCount)}.";
    }

    /// <summary>
    /// Builds the daily highlights.
    /// </summary>
    /// <param name="entry">The daily entry.</param>
    /// <returns>The daily highlights.</returns>
    private static IReadOnlyCollection<string> BuildDailyHighlights(GlycemicDiaryDailyEntry entry)
    {
        if (!entry.HasData)
        {
            return ["No local readings available."];
        }

        var highlights = new List<string>
        {
            $"Average: {FormatMgDl(entry.AverageMgDl)}.",
            $"Time in range: {FormatPercentage(entry.TimeInRangePercentage)}."
        };

        if (HasLowGlucose(entry.MinimumMgDl))
        {
            highlights.Add($"Low glucose observed: {FormatMgDl(entry.MinimumMgDl)}.");
        }

        if (HasHighGlucose(entry.MaximumMgDl))
        {
            highlights.Add($"High glucose observed: {FormatMgDl(entry.MaximumMgDl)}.");
        }

        if (!entry.IsDataComplete)
        {
            highlights.Add($"Data coverage: {FormatPercentage(entry.DataCoveragePercentage)} with {entry.GapCount} {Pluralize("gap", entry.GapCount)}.");
        }

        return highlights;
    }

    /// <summary>
    /// Gets whether a glucose value is below the low threshold.
    /// </summary>
    /// <param name="valueMgDl">The glucose value.</param>
    /// <returns>True when the value is low; otherwise false.</returns>
    private static bool HasLowGlucose(decimal? valueMgDl)
    {
        return valueMgDl < LowGlucoseThresholdMgDl;
    }

    /// <summary>
    /// Gets whether a glucose value is above the high threshold.
    /// </summary>
    /// <param name="valueMgDl">The glucose value.</param>
    /// <returns>True when the value is high; otherwise false.</returns>
    private static bool HasHighGlucose(decimal? valueMgDl)
    {
        return valueMgDl > HighGlucoseThresholdMgDl;
    }

    /// <summary>
    /// Gets whether a glucose value is above the very high threshold.
    /// </summary>
    /// <param name="valueMgDl">The glucose value.</param>
    /// <returns>True when the value is very high; otherwise false.</returns>
    private static bool HasVeryHighGlucose(decimal? valueMgDl)
    {
        return valueMgDl > VeryHighGlucoseThresholdMgDl;
    }

    /// <summary>
    /// Formats a glucose value in mg/dL.
    /// </summary>
    /// <param name="valueMgDl">The glucose value.</param>
    /// <returns>The formatted glucose value.</returns>
    private static string FormatMgDl(decimal? valueMgDl)
    {
        return valueMgDl.HasValue
            ? $"{Math.Round(valueMgDl.Value, 0, MidpointRounding.AwayFromZero):0} mg/dL"
            : "—";
    }

    /// <summary>
    /// Formats a percentage value.
    /// </summary>
    /// <param name="percentage">The percentage value.</param>
    /// <returns>The formatted percentage.</returns>
    private static string FormatPercentage(decimal? percentage)
    {
        return percentage.HasValue
            ? $"{Math.Round(percentage.Value, 2, MidpointRounding.AwayFromZero):0.##}%"
            : "—";
    }

    /// <summary>
    /// Pluralizes a singular noun.
    /// </summary>
    /// <param name="singular">The singular noun.</param>
    /// <param name="count">The item count.</param>
    /// <returns>The pluralized text.</returns>
    private static string Pluralize(string singular, int count)
    {
        return count == 1
            ? singular
            : $"{singular}s";
    }

    #endregion
}
