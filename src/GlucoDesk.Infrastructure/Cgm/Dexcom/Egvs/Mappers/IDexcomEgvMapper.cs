using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Dtos;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Mappers;

/// <summary>
/// Defines mapping operations from Dexcom EGV DTOs to normalized GlucoDesk readings.
/// </summary>
public interface IDexcomEgvMapper
{
    /// <summary>
    /// Maps a Dexcom EGV response to normalized glucose readings.
    /// </summary>
    /// <param name="response">The Dexcom EGV response.</param>
    /// <param name="environment">The Dexcom API environment.</param>
    /// <returns>The normalized glucose readings.</returns>
    Result<IReadOnlyList<GlucoseReading>> MapResponse(
        DexcomEgvResponseDto response,
        DexcomApiEnvironment environment);

    /// <summary>
    /// Maps a single Dexcom EGV record to a normalized glucose reading.
    /// </summary>
    /// <param name="record">The Dexcom EGV record.</param>
    /// <param name="environment">The Dexcom API environment.</param>
    /// <returns>The normalized glucose reading.</returns>
    Result<GlucoseReading> MapRecord(
        DexcomEgvRecordDto record,
        DexcomApiEnvironment environment);
}