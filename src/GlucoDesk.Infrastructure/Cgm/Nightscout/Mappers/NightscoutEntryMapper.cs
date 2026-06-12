using System.Globalization;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Dtos;

namespace GlucoDesk.Infrastructure.Cgm.Nightscout.Mappers;

/// <summary>
/// Maps Nightscout entries to normalized GlucoDesk glucose readings.
/// </summary>
public sealed class NightscoutEntryMapper : INightscoutEntryMapper
{
    /// <inheritdoc />
    public Result<IReadOnlyList<GlucoseReading>> MapEntries(IReadOnlyCollection<NightscoutEntryDto> entries)
    {
        if (entries is null)
        {
            return Result<IReadOnlyList<GlucoseReading>>.Failure(
                new Error("Nightscout.EntriesNull", "Nightscout entries cannot be null."));
        }

        var readings = new List<GlucoseReading>(entries.Count);

        foreach (var entry in entries)
        {
            var readingResult = MapEntry(entry);

            if (readingResult.IsFailure)
            {
                return Result<IReadOnlyList<GlucoseReading>>.Failure(readingResult.Error);
            }

            readings.Add(readingResult.Value);
        }

        return Result<IReadOnlyList<GlucoseReading>>.Success(
            readings
                .OrderBy(reading => reading.Timestamp)
                .ToArray());
    }

    /// <inheritdoc />
    public Result<GlucoseReading> MapEntry(NightscoutEntryDto entry)
    {
        if (entry is null)
        {
            return Result<GlucoseReading>.Failure(
                new Error("Nightscout.EntryNull", "Nightscout entry cannot be null."));
        }

        var timestampResult = MapTimestamp(entry);

        if (timestampResult.IsFailure)
        {
            return Result<GlucoseReading>.Failure(timestampResult.Error);
        }

        var valueResult = MapGlucoseValue(entry);

        if (valueResult.IsFailure)
        {
            return Result<GlucoseReading>.Failure(valueResult.Error);
        }

        return Result<GlucoseReading>.Success(
            new GlucoseReading(
                timestampResult.Value,
                valueResult.Value,
                MapTrend(entry.Direction),
                CgmProviderKind.Nightscout,
                GlucoseDataFreshness.NearRealTime,
                NormalizeDisplayValue(entry.Device)));
    }

    #region Helpers

    /// <summary>
    /// Maps a Nightscout timestamp to UTC.
    /// </summary>
    /// <param name="entry">The Nightscout entry.</param>
    /// <returns>The mapped timestamp.</returns>
    private static Result<DateTimeOffset> MapTimestamp(NightscoutEntryDto entry)
    {
        if (!string.IsNullOrWhiteSpace(entry.DateString)
            && DateTimeOffset.TryParse(
                entry.DateString,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var parsedTimestamp))
        {
            return Result<DateTimeOffset>.Success(parsedTimestamp.ToUniversalTime());
        }

        if (entry.Date is not null && entry.Date > 0)
        {
            try
            {
                return Result<DateTimeOffset>.Success(
                    DateTimeOffset.FromUnixTimeMilliseconds(entry.Date.Value).ToUniversalTime());
            }
            catch (ArgumentOutOfRangeException exception)
            {
                return Result<DateTimeOffset>.Failure(
                    new Error("Nightscout.EntryInvalidDate", exception.Message));
            }
        }

        return Result<DateTimeOffset>.Failure(
            new Error("Nightscout.EntryMissingTimestamp", "Nightscout entry timestamp is missing or invalid."));
    }

    /// <summary>
    /// Maps the Nightscout glucose value.
    /// </summary>
    /// <param name="entry">The Nightscout entry.</param>
    /// <returns>The mapped glucose value.</returns>
    private static Result<GlucoseValue> MapGlucoseValue(NightscoutEntryDto entry)
    {
        if (entry.Sgv is null)
        {
            return Result<GlucoseValue>.Failure(
                new Error("Nightscout.EntryMissingSgv", "Nightscout SGV value is missing."));
        }

        if (entry.Sgv <= 0)
        {
            return Result<GlucoseValue>.Failure(
                new Error("Nightscout.EntryInvalidSgv", "Nightscout SGV value must be greater than zero."));
        }

        return Result<GlucoseValue>.Success(
            new GlucoseValue(entry.Sgv.Value, GlucoseUnit.MgDl));
    }

    /// <summary>
    /// Maps a Nightscout direction value to a GlucoDesk trend direction.
    /// </summary>
    /// <param name="direction">The Nightscout direction value.</param>
    /// <returns>The mapped trend direction.</returns>
    private static TrendDirection MapTrend(string? direction)
    {
        var normalizedDirection = NormalizeToken(direction);

        return normalizedDirection switch
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
            _ => TrendDirection.Unknown
        };
    }

    /// <summary>
    /// Normalizes a display value by trimming empty strings.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <returns>The normalized display value.</returns>
    private static string? NormalizeDisplayValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Normalizes provider tokens by removing separators and applying lowercase casing.
    /// </summary>
    /// <param name="value">The token to normalize.</param>
    /// <returns>The normalized token.</returns>
    private static string NormalizeToken(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return string.Empty;
        }

        var normalizedCharacters = value
            .Where(char.IsLetterOrDigit)
            .Select(char.ToLowerInvariant);

        return string.Concat(normalizedCharacters);
    }

    #endregion
}