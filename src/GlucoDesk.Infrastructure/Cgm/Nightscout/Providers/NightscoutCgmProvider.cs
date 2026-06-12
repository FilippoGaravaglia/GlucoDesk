using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Clients;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Mappers;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Options;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Requests;

namespace GlucoDesk.Infrastructure.Cgm.Nightscout.Providers;

/// <summary>
/// Provides Nightscout CGM readings through GlucoDesk application provider abstractions.
/// </summary>
public sealed class NightscoutCgmProvider : ICgmLiveProvider, ICgmHistoricalProvider, ICgmMetadataProvider
{
    private readonly NightscoutOptions _options;
    private readonly INightscoutEntriesClient _entriesClient;
    private readonly INightscoutEntryMapper _entryMapper;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="NightscoutCgmProvider"/> class.
    /// </summary>
    /// <param name="options">The Nightscout options.</param>
    /// <param name="entriesClient">The Nightscout entries client.</param>
    /// <param name="entryMapper">The Nightscout entry mapper.</param>
    /// <param name="timeProvider">The time provider.</param>
    public NightscoutCgmProvider(
        NightscoutOptions options,
        INightscoutEntriesClient entriesClient,
        INightscoutEntryMapper entryMapper,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(entriesClient);
        ArgumentNullException.ThrowIfNull(entryMapper);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _options = options;
        _entriesClient = entriesClient;
        _entryMapper = entryMapper;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var retrievedAt = _timeProvider.GetUtcNow();

        var entriesResult = await GetMappedReadingsAsync(
                retrievedAt.Subtract(_options.LatestReadingLookback),
                retrievedAt,
                count: 1,
                limit: 1,
                cancellationToken)
            .ConfigureAwait(false);

        if (entriesResult.IsFailure)
        {
            return Result<LatestGlucoseReadingResult>.Failure(entriesResult.Error);
        }

        var latestReading = entriesResult.Value
            .OrderBy(reading => reading.Timestamp)
            .LastOrDefault();

        return Result<LatestGlucoseReadingResult>.Success(
            new LatestGlucoseReadingResult(latestReading, retrievedAt));
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var readingsResult = await GetMappedReadingsAsync(
                request.From,
                request.To,
                ResolveRequestCount(request.Limit),
                request.Limit,
                cancellationToken)
            .ConfigureAwait(false);

        if (readingsResult.IsFailure)
        {
            return Result<GlucoseReadingsResult>.Failure(readingsResult.Error);
        }

        return Result<GlucoseReadingsResult>.Success(
            new GlucoseReadingsResult(readingsResult.Value, _timeProvider.GetUtcNow()));
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseReadingsResult>> GetReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var readingsResult = await GetMappedReadingsAsync(
                request.From,
                request.To,
                ResolveRequestCount(request.Limit),
                request.Limit,
                cancellationToken)
            .ConfigureAwait(false);

        if (readingsResult.IsFailure)
        {
            return Result<GlucoseReadingsResult>.Failure(readingsResult.Error);
        }

        return Result<GlucoseReadingsResult>.Success(
            new GlucoseReadingsResult(readingsResult.Value, _timeProvider.GetUtcNow()));
    }

    /// <inheritdoc />
    public Task<Result<CgmProviderMetadata>> GetMetadataAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var metadata = new CgmProviderMetadata(
            CgmProviderKind.Nightscout,
            _options.DisplayName,
            GlucoseDataFreshness.NearRealTime,
            supportsLiveReadings: true,
            supportsHistoricalReadings: true);

        return Task.FromResult(Result<CgmProviderMetadata>.Success(metadata));
    }

    #region Helpers

    /// <summary>
    /// Gets mapped glucose readings for the supplied range.
    /// </summary>
    /// <param name="from">The inclusive range start.</param>
    /// <param name="to">The inclusive range end.</param>
    /// <param name="count">The Nightscout request count.</param>
    /// <param name="limit">The optional result limit.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The mapped glucose readings.</returns>
    private async Task<Result<IReadOnlyCollection<GlucoseReading>>> GetMappedReadingsAsync(
        DateTimeOffset from,
        DateTimeOffset to,
        int count,
        int? limit,
        CancellationToken cancellationToken)
    {
        NightscoutEntriesRequest nightscoutRequest;

        try
        {
            nightscoutRequest = new NightscoutEntriesRequest(from, to, count);
        }
        catch (ArgumentException exception)
        {
            return Result<IReadOnlyCollection<GlucoseReading>>.Failure(
                new Error("Nightscout.ProviderInvalidReadingsRequest", exception.Message));
        }

        var entriesResult = await _entriesClient
            .GetEntriesAsync(nightscoutRequest, cancellationToken)
            .ConfigureAwait(false);

        if (entriesResult.IsFailure)
        {
            return Result<IReadOnlyCollection<GlucoseReading>>.Failure(entriesResult.Error);
        }

        var mappedReadingsResult = _entryMapper.MapEntries(entriesResult.Value);

        if (mappedReadingsResult.IsFailure)
        {
            return Result<IReadOnlyCollection<GlucoseReading>>.Failure(mappedReadingsResult.Error);
        }

        return Result<IReadOnlyCollection<GlucoseReading>>.Success(
            ApplyLimit(mappedReadingsResult.Value, limit));
    }

    /// <summary>
    /// Resolves the Nightscout request count from a GlucoDesk request limit.
    /// </summary>
    /// <param name="limit">The optional GlucoDesk request limit.</param>
    /// <returns>The Nightscout request count.</returns>
    private int ResolveRequestCount(int? limit)
    {
        return Math.Min(limit ?? _options.MaxReadingsPerRequest, _options.MaxReadingsPerRequest);
    }

    /// <summary>
    /// Applies the optional request limit to mapped readings.
    /// </summary>
    /// <param name="readings">The mapped readings.</param>
    /// <param name="limit">The optional readings limit.</param>
    /// <returns>The limited readings.</returns>
    private static IReadOnlyCollection<GlucoseReading> ApplyLimit(
        IReadOnlyList<GlucoseReading> readings,
        int? limit)
    {
        IEnumerable<GlucoseReading> orderedReadings = readings
            .OrderBy(reading => reading.Timestamp);

        if (limit is not null)
        {
            orderedReadings = orderedReadings.TakeLast(limit.Value);
        }

        return orderedReadings.ToArray();
    }

    #endregion
}