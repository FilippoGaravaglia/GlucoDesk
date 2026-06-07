using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Infrastructure.Cgm.Mock.Generators;
using GlucoDesk.Infrastructure.Cgm.Mock.Options;

namespace GlucoDesk.Infrastructure.Cgm.Mock.Providers;

/// <summary>
/// Provides deterministic mock CGM readings for local development, tests and demos.
/// </summary>
public sealed class MockCgmProvider : ICgmLiveProvider, ICgmHistoricalProvider, ICgmMetadataProvider
{
    private readonly MockCgmProviderOptions _options;
    private readonly TimeProvider _timeProvider;
    private readonly MockGlucoseReadingGenerator _generator;

    /// <summary>
    /// Initializes a new instance of the <see cref="MockCgmProvider"/> class.
    /// </summary>
    /// <param name="options">The mock provider options.</param>
    /// <param name="timeProvider">The time provider.</param>
    public MockCgmProvider(
        MockCgmProviderOptions? options = null,
        TimeProvider? timeProvider = null)
    {
        _options = options ?? MockCgmProviderOptions.Default;
        _timeProvider = timeProvider ?? TimeProvider.System;
        _generator = new MockGlucoseReadingGenerator(_options);
    }

    /// <inheritdoc />
    public Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var now = _timeProvider.GetUtcNow();
        var alignedTimestamp = AlignTimestamp(now);
        var reading = _generator.Generate(alignedTimestamp, GlucoseDataFreshness.NearRealTime);

        var result = new LatestGlucoseReadingResult(reading, now);

        return Task.FromResult(Result<LatestGlucoseReadingResult>.Success(result));
    }

    /// <inheritdoc />
    public Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var result = BuildReadingsResult(
            request,
            _timeProvider.GetUtcNow(),
            GlucoseDataFreshness.NearRealTime);

        return Task.FromResult(Result<GlucoseReadingsResult>.Success(result));
    }

    /// <inheritdoc />
    public Task<Result<GlucoseReadingsResult>> GetReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);
        cancellationToken.ThrowIfCancellationRequested();

        var result = BuildReadingsResult(
            request,
            _timeProvider.GetUtcNow(),
            GlucoseDataFreshness.Historical);

        return Task.FromResult(Result<GlucoseReadingsResult>.Success(result));
    }

    /// <inheritdoc />
    public Task<Result<CgmProviderMetadata>> GetMetadataAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var metadata = new CgmProviderMetadata(
            CgmProviderKind.Mock,
            "Mock CGM Provider",
            GlucoseDataFreshness.NearRealTime,
            supportsLiveReadings: true,
            supportsHistoricalReadings: true);

        return Task.FromResult(Result<CgmProviderMetadata>.Success(metadata));
    }

    #region Helpers

    /// <summary>
    /// Builds a glucose readings result for the requested range.
    /// </summary>
    /// <param name="request">The glucose readings request.</param>
    /// <param name="retrievedAt">The retrieval timestamp.</param>
    /// <param name="freshness">The freshness assigned to generated readings.</param>
    /// <returns>The generated glucose readings result.</returns>
    private GlucoseReadingsResult BuildReadingsResult(
        GlucoseReadingsRequest request,
        DateTimeOffset retrievedAt,
        GlucoseDataFreshness freshness)
    {
        var readings = GenerateReadings(request, freshness);

        return new GlucoseReadingsResult(readings, retrievedAt);
    }

    /// <summary>
    /// Generates deterministic glucose readings for the requested range.
    /// </summary>
    /// <param name="request">The glucose readings request.</param>
    /// <param name="freshness">The freshness assigned to generated readings.</param>
    /// <returns>The generated glucose readings.</returns>
    private IReadOnlyCollection<GlucoseReading> GenerateReadings(
        GlucoseReadingsRequest request,
        GlucoseDataFreshness freshness)
    {
        var timestamps = EnumerateTimestamps(request.From, request.To)
            .ToArray();

        if (request.Limit is not null)
        {
            timestamps = timestamps
                .TakeLast(request.Limit.Value)
                .ToArray();
        }

        return timestamps
            .Select(timestamp => _generator.Generate(timestamp, freshness))
            .ToArray();
    }

    /// <summary>
    /// Enumerates aligned timestamps inside the requested range.
    /// </summary>
    /// <param name="from">The inclusive start timestamp.</param>
    /// <param name="to">The exclusive end timestamp.</param>
    /// <returns>The aligned timestamps.</returns>
    private IEnumerable<DateTimeOffset> EnumerateTimestamps(DateTimeOffset from, DateTimeOffset to)
    {
        var current = AlignTimestamp(from);

        if (current < from)
        {
            current = current.Add(_options.ReadingInterval);
        }

        while (current < to)
        {
            yield return current;
            current = current.Add(_options.ReadingInterval);
        }
    }

    /// <summary>
    /// Aligns a timestamp to the configured mock reading interval.
    /// </summary>
    /// <param name="timestamp">The timestamp to align.</param>
    /// <returns>The aligned timestamp.</returns>
    private DateTimeOffset AlignTimestamp(DateTimeOffset timestamp)
    {
        var intervalSeconds = Math.Max(1, (long)_options.ReadingInterval.TotalSeconds);
        var unixTimeSeconds = timestamp.ToUnixTimeSeconds();
        var alignedUnixTimeSeconds = unixTimeSeconds - (unixTimeSeconds % intervalSeconds);

        return DateTimeOffset.FromUnixTimeSeconds(alignedUnixTimeSeconds);
    }

    #endregion
}