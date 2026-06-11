using System.Globalization;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Dtos;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Mappers;

/// <summary>
/// Maps Dexcom EGV DTOs to normalized GlucoDesk glucose readings.
/// </summary>
public sealed class DexcomEgvMapper : IDexcomEgvMapper
{
    /// <inheritdoc />
    public Result<IReadOnlyList<GlucoseReading>> MapResponse(
        DexcomEgvResponseDto response,
        DexcomApiEnvironment environment)
    {
        if (response is null)
        {
            return Result<IReadOnlyList<GlucoseReading>>.Failure(
                new Error("Dexcom.EgvResponseNull", "Dexcom EGV response cannot be null."));
        }

        if (response.Records is null)
        {
            return Result<IReadOnlyList<GlucoseReading>>.Failure(
                new Error("Dexcom.EgvRecordsMissing", "Dexcom EGV response records are missing."));
        }

        var readings = new List<GlucoseReading>(response.Records.Count);

        foreach (var record in response.Records)
        {
            var readingResult = MapRecord(record, environment);

            if (readingResult.IsFailure)
            {
                return Result<IReadOnlyList<GlucoseReading>>.Failure(readingResult.Error);
            }

            readings.Add(readingResult.Value);
        }

        return Result<IReadOnlyList<GlucoseReading>>.Success(readings);
    }

    /// <inheritdoc />
    public Result<GlucoseReading> MapRecord(
        DexcomEgvRecordDto record,
        DexcomApiEnvironment environment)
    {
        if (record is null)
        {
            return Result<GlucoseReading>.Failure(
                new Error("Dexcom.EgvRecordNull", "Dexcom EGV record cannot be null."));
        }

        var timestampResult = MapTimestamp(record);

        if (timestampResult.IsFailure)
        {
            return Result<GlucoseReading>.Failure(timestampResult.Error);
        }

        var valueResult = MapGlucoseValue(record);

        if (valueResult.IsFailure)
        {
            return Result<GlucoseReading>.Failure(valueResult.Error);
        }

        return Result<GlucoseReading>.Success(
            new GlucoseReading(
                timestampResult.Value,
                valueResult.Value,
                MapTrend(record.Trend),
                MapProvider(environment),
                GlucoseDataFreshness.Delayed,
                BuildDeviceName(record)));
    }

    #region Helpers

    /// <summary>
    /// Maps the Dexcom system time to a UTC timestamp.
    /// </summary>
    /// <param name="record">The Dexcom EGV record.</param>
    /// <returns>The mapped timestamp.</returns>
    private static Result<DateTimeOffset> MapTimestamp(DexcomEgvRecordDto record)
    {
        if (string.IsNullOrWhiteSpace(record.SystemTime))
        {
            return Result<DateTimeOffset>.Failure(
                new Error("Dexcom.EgvMissingSystemTime", "Dexcom EGV systemTime is missing."));
        }

        if (!DateTimeOffset.TryParse(
                record.SystemTime,
                CultureInfo.InvariantCulture,
                DateTimeStyles.AssumeUniversal | DateTimeStyles.AdjustToUniversal,
                out var timestamp))
        {
            return Result<DateTimeOffset>.Failure(
                new Error("Dexcom.EgvInvalidSystemTime", "Dexcom EGV systemTime is invalid."));
        }

        return Result<DateTimeOffset>.Success(timestamp.ToUniversalTime());
    }

    /// <summary>
    /// Maps the Dexcom glucose value and unit.
    /// </summary>
    /// <param name="record">The Dexcom EGV record.</param>
    /// <returns>The mapped glucose value.</returns>
    private static Result<GlucoseValue> MapGlucoseValue(DexcomEgvRecordDto record)
    {
        if (record.Value is null)
        {
            return Result<GlucoseValue>.Failure(
                new Error("Dexcom.EgvMissingValue", "Dexcom EGV value is missing."));
        }

        if (record.Value <= 0)
        {
            return Result<GlucoseValue>.Failure(
                new Error("Dexcom.EgvInvalidValue", "Dexcom EGV value must be greater than zero."));
        }

        var unitResult = MapUnit(record.Unit);

        if (unitResult.IsFailure)
        {
            return Result<GlucoseValue>.Failure(unitResult.Error);
        }

        return Result<GlucoseValue>.Success(
            new GlucoseValue(record.Value.Value, unitResult.Value));
    }

    /// <summary>
    /// Maps a Dexcom unit to a GlucoDesk glucose unit.
    /// </summary>
    /// <param name="unit">The Dexcom unit.</param>
    /// <returns>The mapped glucose unit.</returns>
    private static Result<GlucoseUnit> MapUnit(string? unit)
    {
        var normalizedUnit = NormalizeToken(unit);

        return normalizedUnit switch
        {
            "mgdl" => Result<GlucoseUnit>.Success(GlucoseUnit.MgDl),
            "mmoll" => Result<GlucoseUnit>.Success(GlucoseUnit.MmolL),

            _ => Result<GlucoseUnit>.Failure(
                new Error("Dexcom.EgvUnsupportedUnit", "Dexcom EGV unit is missing or unsupported."))
        };
    }

    /// <summary>
    /// Maps a Dexcom trend string to a GlucoDesk trend direction.
    /// </summary>
    /// <param name="trend">The Dexcom trend.</param>
    /// <returns>The mapped trend direction.</returns>
    private static TrendDirection MapTrend(string? trend)
    {
        var normalizedTrend = NormalizeToken(trend);

        return normalizedTrend switch
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
    /// Maps a Dexcom API environment to the GlucoDesk provider kind.
    /// </summary>
    /// <param name="environment">The Dexcom API environment.</param>
    /// <returns>The mapped provider kind.</returns>
    private static CgmProviderKind MapProvider(DexcomApiEnvironment environment)
    {
        return environment == DexcomApiEnvironment.Sandbox
            ? CgmProviderKind.DexcomSandbox
            : CgmProviderKind.DexcomOfficial;
    }

    /// <summary>
    /// Builds a readable device name from Dexcom display fields.
    /// </summary>
    /// <param name="record">The Dexcom EGV record.</param>
    /// <returns>The optional device name.</returns>
    private static string? BuildDeviceName(DexcomEgvRecordDto record)
    {
        var parts = new[]
            {
                NormalizeDisplayValue(record.DisplayDevice),
                NormalizeDisplayValue(record.DisplayApp)
            }
            .Where(value => value is not null)
            .Select(value => value!)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        if (parts.Length > 0)
        {
            return string.Join(" / ", parts);
        }

        return NormalizeDisplayValue(record.TransmitterGeneration);
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