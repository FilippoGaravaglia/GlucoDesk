using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Clients;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Mapping;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Providers;

/// <summary>
/// Provides near real-time CGM readings through Dexcom Share.
/// </summary>
public sealed class DexcomShareCgmProvider : ICgmLiveProvider, ICgmHistoricalProvider, ICgmMetadataProvider
{
    private readonly IDexcomShareOptionsProvider _optionsProvider;
    private readonly IDexcomShareClient _client;
    private readonly DexcomShareGlucoseValueMapper _mapper;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomShareCgmProvider"/> class.
    /// </summary>
    /// <param name="optionsProvider">The Dexcom Share options provider.</param>
    /// <param name="client">The Dexcom Share client.</param>
    /// <param name="mapper">The Dexcom Share glucose value mapper.</param>
    /// <param name="timeProvider">The time provider.</param>
    public DexcomShareCgmProvider(
        IDexcomShareOptionsProvider optionsProvider,
        IDexcomShareClient client,
        DexcomShareGlucoseValueMapper mapper,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(optionsProvider);
        ArgumentNullException.ThrowIfNull(client);
        ArgumentNullException.ThrowIfNull(mapper);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _optionsProvider = optionsProvider;
        _client = client;
        _mapper = mapper;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(CancellationToken cancellationToken)
    {
        var optionsResult = await GetOptionsAsync(cancellationToken)
            .ConfigureAwait(false);

        if (optionsResult.IsFailure)
        {
            return Result<LatestGlucoseReadingResult>.Failure(optionsResult.Error);
        }

        var options = optionsResult.Value;
        var retrievedAt = _timeProvider.GetUtcNow();

        var readingsResult = await GetMappedReadingsAsync(
                options,
                options.LatestReadingLookback,
                maxCount: 1,
                cancellationToken)
            .ConfigureAwait(false);

        if (readingsResult.IsFailure)
        {
            return Result<LatestGlucoseReadingResult>.Failure(readingsResult.Error);
        }

        var latestReading = readingsResult.Value
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

        var optionsResult = await GetOptionsAsync(cancellationToken)
            .ConfigureAwait(false);

        if (optionsResult.IsFailure)
        {
            return Result<GlucoseReadingsResult>.Failure(optionsResult.Error);
        }

        var options = optionsResult.Value;
        var lookback = CalculateLookback(options, request);
        var maxCount = CalculateMaxCount(options, lookback, request.Limit);

        var readingsResult = await GetMappedReadingsAsync(
                options,
                lookback,
                maxCount,
                cancellationToken)
            .ConfigureAwait(false);

        if (readingsResult.IsFailure)
        {
            return Result<GlucoseReadingsResult>.Failure(readingsResult.Error);
        }

        var filteredReadings = readingsResult.Value
            .Where(reading => reading.Timestamp >= request.From && reading.Timestamp <= request.To)
            .OrderBy(reading => reading.Timestamp)
            .ToArray();

        return Result<GlucoseReadingsResult>.Success(
            new GlucoseReadingsResult(filteredReadings, _timeProvider.GetUtcNow()));
    }

    /// <inheritdoc />
    public Task<Result<GlucoseReadingsResult>> GetReadingsAsync(
        GlucoseReadingsRequest request,
        CancellationToken cancellationToken)
    {
        return GetRecentReadingsAsync(request, cancellationToken);
    }

    /// <inheritdoc />
    public Task<Result<CgmProviderMetadata>> GetMetadataAsync(CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var metadata = new CgmProviderMetadata(
            CgmProviderKind.DexcomShare,
            "Dexcom Share",
            GlucoseDataFreshness.NearRealTime,
            supportsLiveReadings: true,
            supportsHistoricalReadings: true);

        return Task.FromResult(Result<CgmProviderMetadata>.Success(metadata));
    }

    #region Helpers

    /// <summary>
    /// Gets the current Dexcom Share options.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current Dexcom Share options.</returns>
    private async Task<Result<DexcomShareOptions>> GetOptionsAsync(CancellationToken cancellationToken)
    {
        return await _optionsProvider
            .GetOptionsAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    /// <summary>
    /// Gets mapped Dexcom Share glucose readings.
    /// </summary>
    /// <param name="options">The Dexcom Share options.</param>
    /// <param name="lookback">The requested lookback window.</param>
    /// <param name="maxCount">The maximum number of readings.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The mapped glucose readings.</returns>
    private async Task<Result<IReadOnlyCollection<GlucoseReading>>> GetMappedReadingsAsync(
        DexcomShareOptions options,
        TimeSpan lookback,
        int maxCount,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(options);

        var sessionResult = await _client
            .AuthenticateAsync(cancellationToken)
            .ConfigureAwait(false);

        if (sessionResult.IsFailure)
        {
            return Result<IReadOnlyCollection<GlucoseReading>>.Failure(sessionResult.Error);
        }

        var valuesResult = await _client
            .GetLatestGlucoseValuesAsync(
                sessionResult.Value,
                CalculateMinutes(lookback),
                maxCount,
                cancellationToken)
            .ConfigureAwait(false);

        if (valuesResult.IsFailure)
        {
            return Result<IReadOnlyCollection<GlucoseReading>>.Failure(valuesResult.Error);
        }

        return _mapper.MapValues(valuesResult.Value);
    }

    /// <summary>
    /// Calculates the Dexcom Share lookback window for a glucose readings request.
    /// </summary>
    /// <param name="options">The Dexcom Share options.</param>
    /// <param name="request">The glucose readings request.</param>
    /// <returns>The calculated lookback window.</returns>
    private static TimeSpan CalculateLookback(
        DexcomShareOptions options,
        GlucoseReadingsRequest request)
    {
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(request);

        var requestLookback = request.To - request.From;

        if (requestLookback <= TimeSpan.Zero)
        {
            return options.RecentReadingsLookback;
        }

        return requestLookback > options.RecentReadingsLookback
            ? requestLookback
            : options.RecentReadingsLookback;
    }

    /// <summary>
    /// Calculates the number of readings to request from Dexcom Share for the selected lookback window.
    /// </summary>
    /// <param name="options">The Dexcom Share options.</param>
    /// <param name="lookback">The requested lookback window.</param>
    /// <param name="requestedLimit">The optional caller-provided limit.</param>
    /// <returns>The calculated maximum reading count.</returns>
    private static int CalculateMaxCount(
        DexcomShareOptions options,
        TimeSpan lookback,
        int? requestedLimit)
    {
        ArgumentNullException.ThrowIfNull(options);

        const int readingsPerHour = 12;
        const int bufferReadings = 12;

        var requestedHours = Math.Ceiling(lookback.TotalHours);
        var requiredCount = ((int)requestedHours * readingsPerHour) + bufferReadings;
        var effectiveRequestedLimit = requestedLimit.GetValueOrDefault(0);

        return Math.Clamp(
            Math.Max(requiredCount, effectiveRequestedLimit),
            1,
            options.MaximumRecentReadings);
    }

    /// <summary>
    /// Calculates the lookback minutes sent to Dexcom Share.
    /// </summary>
    /// <param name="lookback">The lookback window.</param>
    /// <returns>The lookback minutes.</returns>
    private static int CalculateMinutes(TimeSpan lookback)
    {
        return Math.Clamp(
            (int)Math.Ceiling(lookback.TotalMinutes),
            1,
            1440);
    }

    #endregion
}