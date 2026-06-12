using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Dtos;

namespace GlucoDesk.Infrastructure.Cgm.Nightscout.Mappers;

/// <summary>
/// Maps Nightscout entries to normalized GlucoDesk glucose readings.
/// </summary>
public interface INightscoutEntryMapper
{
    /// <summary>
    /// Maps Nightscout entries to glucose readings.
    /// </summary>
    /// <param name="entries">The Nightscout entries.</param>
    /// <returns>The mapped glucose readings.</returns>
    Result<IReadOnlyList<GlucoseReading>> MapEntries(IReadOnlyCollection<NightscoutEntryDto> entries);

    /// <summary>
    /// Maps a Nightscout entry to a glucose reading.
    /// </summary>
    /// <param name="entry">The Nightscout entry.</param>
    /// <returns>The mapped glucose reading.</returns>
    Result<GlucoseReading> MapEntry(NightscoutEntryDto entry);
}