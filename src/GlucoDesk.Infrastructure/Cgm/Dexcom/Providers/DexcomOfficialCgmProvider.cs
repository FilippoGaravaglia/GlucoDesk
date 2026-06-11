using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Clients;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Mappers;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Requests;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers.Options;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Providers;

/// <summary>
/// Provides Dexcom Official API CGM readings through GlucoDesk application provider abstractions.
/// </summary>
public sealed class DexcomOfficialCgmProvider : ICgmLiveProvider, ICgmHistoricalProvider, ICgmMetadataProvider
{
    private readonly DexcomApiOptions _apiOptions;
    private readonly DexcomCgmProviderOptions _providerOptions;
    private readonly IDexcomEgvClient _egvClient;
    private readonly IDexcomEgvMapper _egvMapper;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomOfficialCgmProvider"/> class.
    /// </summary>
    /// <param name="apiOptions">The Dexcom API options.</param>
    /// <param name="providerOptions">The Dexcom provider options.</param>
    /// <param name="egvClient">The Dexcom EGV client.</param>
    /// <param name="egvMapper">The Dexcom EGV mapper.</param>
    /// <param name="timeProvider">The time provider.</param>
    public DexcomOfficialCgmProvider(
        DexcomApiOptions apiOptions,
        DexcomCgmProviderOptions providerOptions,
        IDexcomEgvClient egvClient,
        IDexcomEgvMapper egvMapper,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(apiOptions);
        ArgumentNullException.ThrowIfNull(providerOptions);
        ArgumentNullException.ThrowIfNull(egvClient);
        ArgumentNullException.ThrowIfNull(egvMapper);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _apiOptions = apiOptions;
        _providerOptions = providerOptions;
        _egvClient = egvClient;
        _egvMapper = egvMapper;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var retrievedAt = _timeProvider.GetUtcNow();

        var request = GlucoseReadingsRequest.ForLast(
            _providerOptions.LatestReadingLookback,
            retrievedAt,
            limit: 1);

        var readingsResult = await GetMappedReadingsAsync(
                request,
                forceTokenRefresh: false,
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
        cancellationToken.ThrowIfCancellationRequested();

        var readingsResult = await GetMappedReadingsAsync(
                request,
                forceTokenRefresh: false,
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
                request,
                forceTokenRefresh: false,
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
            MapProviderKind(_apiOptions.Environment),
            _providerOptions.DisplayName,
            GlucoseDataFreshness.Delayed,
            supportsLiveReadings: true,
            supportsHistoricalReadings: true);

        return Task.FromResult(Result<CgmProviderMetadata>.Success(metadata));
    }

    #region Helpers

    /// <summary>
    /// Gets mapped glucose readings for the requested time range.
    /// </summary>
    /// <param name="request">The glucose readings request.</param>
    /// <param name="forceTokenRefresh">Whether to force an access token refresh.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The mapped glucose readings.</returns>
    private async Task<Result<IReadOnlyCollection<GlucoseReading>>> GetMappedReadingsAsync(
        GlucoseReadingsRequest request,
        bool forceTokenRefresh,
        CancellationToken cancellationToken)
    {
        var dexcomRequestResult = BuildDexcomRequest(request, forceTokenRefresh);

        if (dexcomRequestResult.IsFailure)
        {
            return Result<IReadOnlyCollection<GlucoseReading>>.Failure(dexcomRequestResult.Error);
        }

        var egvResponseResult = await _egvClient
            .GetEgvsAsync(dexcomRequestResult.Value, cancellationToken)
            .ConfigureAwait(false);

        if (egvResponseResult.IsFailure)
        {
            return Result<IReadOnlyCollection<GlucoseReading>>.Failure(egvResponseResult.Error);
        }

        var mappedReadingsResult = _egvMapper.MapResponse(
            egvResponseResult.Value,
            _apiOptions.Environment);

        if (mappedReadingsResult.IsFailure)
        {
            return Result<IReadOnlyCollection<GlucoseReading>>.Failure(mappedReadingsResult.Error);
        }

        return Result<IReadOnlyCollection<GlucoseReading>>.Success(
            ApplyLimit(mappedReadingsResult.Value, request.Limit));
    }

    /// <summary>
    /// Builds the Dexcom EGV request for the provided glucose readings request.
    /// </summary>
    /// <param name="request">The glucose readings request.</param>
    /// <param name="forceTokenRefresh">Whether to force an access token refresh.</param>
    /// <returns>The Dexcom EGV request.</returns>
    private Result<DexcomEgvRequest> BuildDexcomRequest(
        GlucoseReadingsRequest request,
        bool forceTokenRefresh)
    {
        if (!_providerOptions.HasClientSecret)
        {
            return Result<DexcomEgvRequest>.Failure(
                new Error(
                    "Dexcom.ProviderClientSecretMissing",
                    "Dexcom provider client secret is not configured."));
        }
    
        try
        {
            return Result<DexcomEgvRequest>.Success(
                new DexcomEgvRequest(
                    _providerOptions.ClientSecret!,
                    request.From,
                    request.To,
                    forceTokenRefresh));
        }
        catch (ArgumentException exception)
        {
            return Result<DexcomEgvRequest>.Failure(
                new Error("Dexcom.ProviderInvalidReadingsRequest", exception.Message));
        }
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

    /// <summary>
    /// Maps the Dexcom API environment to a provider kind.
    /// </summary>
    /// <param name="environment">The Dexcom API environment.</param>
    /// <returns>The provider kind.</returns>
    private static CgmProviderKind MapProviderKind(DexcomApiEnvironment environment)
    {
        return environment == DexcomApiEnvironment.Sandbox
            ? CgmProviderKind.DexcomSandbox
            : CgmProviderKind.DexcomOfficial;
    }

    #endregion
}