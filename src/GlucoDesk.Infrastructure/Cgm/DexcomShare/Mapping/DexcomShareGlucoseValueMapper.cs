using System.Globalization;
using System.Text.Json;
using System.Text.RegularExpressions;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Readings;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Mapping;

/// <summary>
/// Maps Dexcom Share glucose value DTOs to GlucoDesk glucose readings.
/// </summary>
public sealed partial class DexcomShareGlucoseValueMapper
{
    /// <summary>
    /// Maps Dexcom Share glucose values to normalized glucose readings.
    /// </summary>
    /// <param name="values">The Dexcom Share glucose values.</param>
    /// <returns>The mapped glucose readings.</returns>
    public Result<IReadOnlyCollection<GlucoseReading>> MapValues(
        IReadOnlyCollection<DexcomShareGlucoseValueDto> values)
    {
        ArgumentNullException.ThrowIfNull(values);

        var readings = new List<GlucoseReading>();

        foreach (var value in values)
        {
            var mappedReading = MapValue(value);

            if (mappedReading.IsFailure)
            {
                return Result<IReadOnlyCollection<GlucoseReading>>.Failure(mappedReading.Error);
            }

            readings.Add(mappedReading.Value);
        }

        return Result<IReadOnlyCollection<GlucoseReading>>.Success(
            readings
                .OrderBy(reading => reading.Timestamp)
                .ToArray());
    }

    #region Helpers

    /// <summary>
    /// Maps a single Dexcom Share glucose value to a normalized glucose reading.
    /// </summary>
    /// <param name="value">The Dexcom Share glucose value.</param>
    /// <returns>The mapped glucose reading.</returns>
    private static Result<GlucoseReading> MapValue(DexcomShareGlucoseValueDto value)
    {
        if (value.Value <= 0)
        {
            return Result<GlucoseReading>.Failure(
                new Error(
                    "DexcomShare.InvalidGlucoseValue",
                    "Dexcom Share returned an invalid glucose value."));
        }

        var timestampResult = ParseDexcomDate(value.SystemTime)
            .Or(() => ParseDexcomDate(value.DisplayTime))
            .Or(() => ParseDexcomDate(value.WallTime));

        if (timestampResult.IsFailure)
        {
            return Result<GlucoseReading>.Failure(timestampResult.Error);
        }

        return Result<GlucoseReading>.Success(
            new GlucoseReading(
                timestampResult.Value,
                new GlucoseValue(value.Value, GlucoseUnit.MgDl),
                MapTrend(value.Trend),
                CgmProviderKind.DexcomShare,
                GlucoseDataFreshness.NearRealTime));
    }

    /// <summary>
    /// Maps a Dexcom Share trend payload to a GlucoDesk trend direction.
    /// </summary>
    /// <param name="trend">The Dexcom Share trend payload.</param>
    /// <returns>The mapped trend direction.</returns>
    private static TrendDirection MapTrend(JsonElement trend)
    {
        return trend.ValueKind switch
        {
            JsonValueKind.Number when trend.TryGetInt32(out var numericTrend) =>
                MapNumericTrend(numericTrend),

            JsonValueKind.String =>
                MapTextTrend(trend.GetString()),

            _ =>
                TrendDirection.NotComputable
        };
    }

    /// <summary>
    /// Maps Dexcom Share numeric trend values to GlucoDesk trend directions.
    /// </summary>
    /// <param name="trend">The Dexcom Share numeric trend value.</param>
    /// <returns>The mapped trend direction.</returns>
    private static TrendDirection MapNumericTrend(int trend)
    {
        return trend switch
        {
            1 => TrendDirection.DoubleUp,
            2 => TrendDirection.SingleUp,
            3 => TrendDirection.FortyFiveUp,
            4 => TrendDirection.Flat,
            5 => TrendDirection.FortyFiveDown,
            6 => TrendDirection.SingleDown,
            7 => TrendDirection.DoubleDown,
            8 => TrendDirection.NotComputable,
            9 => TrendDirection.RateOutOfRange,
            _ => TrendDirection.NotComputable
        };
    }

    /// <summary>
    /// Maps Dexcom Share text trend values to GlucoDesk trend directions.
    /// </summary>
    /// <param name="trend">The Dexcom Share text trend value.</param>
    /// <returns>The mapped trend direction.</returns>
    private static TrendDirection MapTextTrend(string? trend)
    {
        return NormalizeTrendText(trend) switch
        {
            "doubleup" => TrendDirection.DoubleUp,
            "singleup" => TrendDirection.SingleUp,
            "fortyfiveup" => TrendDirection.FortyFiveUp,
            "flat" => TrendDirection.Flat,
            "fortyfivedown" => TrendDirection.FortyFiveDown,
            "singledown" => TrendDirection.SingleDown,
            "doubledown" => TrendDirection.DoubleDown,
            "notcomputable" => TrendDirection.NotComputable,
            "rateoutofrange" => TrendDirection.RateOutOfRange,
            "none" => TrendDirection.NotComputable,
            "" => TrendDirection.NotComputable,
            _ => TrendDirection.NotComputable
        };
    }

    /// <summary>
    /// Normalizes a Dexcom Share text trend value for stable comparisons.
    /// </summary>
    /// <param name="trend">The Dexcom Share text trend value.</param>
    /// <returns>The normalized trend text.</returns>
    private static string NormalizeTrendText(string? trend)
    {
        return string.IsNullOrWhiteSpace(trend)
            ? string.Empty
            : trend
                .Trim()
                .Replace(" ", string.Empty, StringComparison.Ordinal)
                .Replace("_", string.Empty, StringComparison.Ordinal)
                .Replace("-", string.Empty, StringComparison.Ordinal)
                .ToLowerInvariant();
    }

    /// <summary>
    /// Parses a Dexcom Share date value.
    /// </summary>
    /// <param name="value">The Dexcom date value.</param>
    /// <returns>The parsed timestamp.</returns>
    private static Result<DateTimeOffset> ParseDexcomDate(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return Result<DateTimeOffset>.Failure(
                new Error(
                    "DexcomShare.TimestampMissing",
                    "Dexcom Share timestamp is missing."));
        }

        var trimmedValue = value.Trim();

        var unixDateResult = TryParseDexcomUnixDate(trimmedValue);

        if (unixDateResult.IsSuccess)
        {
            return unixDateResult;
        }

        var dateTimeResult = TryParseDateTimeOffset(trimmedValue);

        if (dateTimeResult.IsSuccess)
        {
            return dateTimeResult;
        }

        return Result<DateTimeOffset>.Failure(
            new Error(
                "DexcomShare.TimestampInvalid",
                $"Dexcom Share timestamp is invalid: '{trimmedValue}'."));
    }

    /// <summary>
    /// Parses Dexcom Share timestamp values encoded as Date(milliseconds).
    /// </summary>
    /// <param name="value">The Dexcom timestamp value.</param>
    /// <returns>The parsed timestamp.</returns>
    private static Result<DateTimeOffset> TryParseDexcomUnixDate(string value)
    {
        var match = DexcomDateRegex().Match(value);

        if (!match.Success)
        {
            return Result<DateTimeOffset>.Failure(
                new Error(
                    "DexcomShare.TimestampInvalid",
                    "Dexcom Share timestamp is not a Dexcom Unix date."));
        }

        var millisecondsText = match.Groups["milliseconds"].Value;

        if (!long.TryParse(
                millisecondsText,
                NumberStyles.Integer,
                CultureInfo.InvariantCulture,
                out var milliseconds))
        {
            return Result<DateTimeOffset>.Failure(
                new Error(
                    "DexcomShare.TimestampInvalid",
                    "Dexcom Share timestamp milliseconds are invalid."));
        }

        return Result<DateTimeOffset>.Success(
            DateTimeOffset.FromUnixTimeMilliseconds(milliseconds));
    }

    /// <summary>
    /// Parses ISO-like Dexcom Share timestamp values.
    /// </summary>
    /// <param name="value">The Dexcom timestamp value.</param>
    /// <returns>The parsed timestamp.</returns>
    private static Result<DateTimeOffset> TryParseDateTimeOffset(string value)
    {
        if (DateTimeOffset.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var timestamp))
        {
            return Result<DateTimeOffset>.Success(timestamp.ToUniversalTime());
        }

        if (DateTime.TryParse(
                value,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeLocal,
                out var dateTime))
        {
            return Result<DateTimeOffset>.Success(
                new DateTimeOffset(dateTime).ToUniversalTime());
        }

        return Result<DateTimeOffset>.Failure(
            new Error(
                "DexcomShare.TimestampInvalid",
                "Dexcom Share timestamp is not an ISO-like date."));
    }

    [GeneratedRegex(@"\/?Date\((?<milliseconds>-?\d+)(?<offset>[+-]\d+)?\)\/?", RegexOptions.Compiled)]
    private static partial Regex DexcomDateRegex();

    #endregion
}