using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Resolution.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Resolution.Models;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Cgm.Providers.Resolution.Services;

/// <summary>
/// Resolves CGM providers based on the current application settings.
/// </summary>
public sealed class CgmProviderResolver : ICgmProviderResolver
{
    private readonly IApplicationSettingsService _settingsService;
    private readonly IReadOnlyCollection<ICgmLiveProvider> _liveProviders;
    private readonly IReadOnlyCollection<ICgmHistoricalProvider> _historicalProviders;
    private readonly IReadOnlyCollection<ICgmMetadataProvider> _metadataProviders;

    /// <summary>
    /// Initializes a new instance of the <see cref="CgmProviderResolver"/> class.
    /// </summary>
    /// <param name="settingsService">The application settings service.</param>
    /// <param name="liveProviders">The registered live providers.</param>
    /// <param name="historicalProviders">The registered historical providers.</param>
    /// <param name="metadataProviders">The registered metadata providers.</param>
    public CgmProviderResolver(
        IApplicationSettingsService settingsService,
        IEnumerable<ICgmLiveProvider> liveProviders,
        IEnumerable<ICgmHistoricalProvider> historicalProviders,
        IEnumerable<ICgmMetadataProvider> metadataProviders)
    {
        ArgumentNullException.ThrowIfNull(settingsService);
        ArgumentNullException.ThrowIfNull(liveProviders);
        ArgumentNullException.ThrowIfNull(historicalProviders);
        ArgumentNullException.ThrowIfNull(metadataProviders);

        _settingsService = settingsService;
        _liveProviders = liveProviders.ToArray();
        _historicalProviders = historicalProviders.ToArray();
        _metadataProviders = metadataProviders.ToArray();
    }

    /// <inheritdoc />
    public async Task<Result<CgmLiveProviderResolution>> ResolveActiveLiveProviderAsync(
        CancellationToken cancellationToken)
    {
        var settings = await GetSettingsOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var requestedProvider = settings.ActiveLiveProvider;

        var requestedProviderResult = await TryResolveLiveProviderAsync(
                requestedProvider,
                cancellationToken)
            .ConfigureAwait(false);

        if (requestedProviderResult.IsSuccess)
        {
            return requestedProviderResult;
        }

        if (requestedProvider != CgmProviderKind.Mock)
        {
            var fallbackResult = await TryResolveLiveProviderAsync(
                    CgmProviderKind.Mock,
                    cancellationToken)
                .ConfigureAwait(false);

            if (fallbackResult.IsSuccess)
            {
                return fallbackResult;
            }
        }

        return Result<CgmLiveProviderResolution>.Failure(
            new Error(
                "Cgm.LiveProviderUnavailable",
                $"No live CGM provider is available for provider kind '{requestedProvider}'."));
    }

    /// <inheritdoc />
    public async Task<Result<CgmHistoricalProviderResolution>> ResolveActiveHistoricalProviderAsync(
        CancellationToken cancellationToken)
    {
        var settings = await GetSettingsOrDefaultAsync(cancellationToken)
            .ConfigureAwait(false);

        var requestedProvider = settings.HistoricalProvider;

        var requestedProviderResult = await TryResolveHistoricalProviderAsync(
                requestedProvider,
                cancellationToken)
            .ConfigureAwait(false);

        if (requestedProviderResult.IsSuccess)
        {
            return requestedProviderResult;
        }

        if (requestedProvider != CgmProviderKind.Mock)
        {
            var fallbackResult = await TryResolveHistoricalProviderAsync(
                    CgmProviderKind.Mock,
                    cancellationToken)
                .ConfigureAwait(false);

            if (fallbackResult.IsSuccess)
            {
                return fallbackResult;
            }
        }

        return Result<CgmHistoricalProviderResolution>.Failure(
            new Error(
                "Cgm.HistoricalProviderUnavailable",
                $"No historical CGM provider is available for provider kind '{requestedProvider}'."));
    }

    #region Helpers

    /// <summary>
    /// Gets application settings, falling back to defaults when settings cannot be loaded.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The current application settings or defaults.</returns>
    private async Task<ApplicationSettings> GetSettingsOrDefaultAsync(CancellationToken cancellationToken)
    {
        var settingsResult = await _settingsService
            .GetSettingsAsync(cancellationToken)
            .ConfigureAwait(false);

        return settingsResult.IsSuccess
            ? settingsResult.Value
            : ApplicationSettings.Default;
    }

    /// <summary>
    /// Attempts to resolve a live provider by provider kind.
    /// </summary>
    /// <param name="providerKind">The requested provider kind.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The live provider resolution.</returns>
    private async Task<Result<CgmLiveProviderResolution>> TryResolveLiveProviderAsync(
        CgmProviderKind providerKind,
        CancellationToken cancellationToken)
    {
        foreach (var metadataProvider in _metadataProviders)
        {
            var metadataResult = await metadataProvider
                .GetMetadataAsync(cancellationToken)
                .ConfigureAwait(false);

            if (metadataResult.IsFailure)
            {
                continue;
            }

            var metadata = metadataResult.Value;

            if (metadata.ProviderKind != providerKind || !metadata.SupportsLiveReadings)
            {
                continue;
            }

            var liveProvider = TryResolveProvider(metadataProvider, _liveProviders);

            if (liveProvider is null)
            {
                continue;
            }

            return Result<CgmLiveProviderResolution>.Success(
                new CgmLiveProviderResolution(
                    metadata,
                    liveProvider,
                    metadataProvider));
        }

        return Result<CgmLiveProviderResolution>.Failure(
            new Error(
                "Cgm.LiveProviderNotFound",
                $"Live CGM provider '{providerKind}' is not registered."));
    }

    /// <summary>
    /// Attempts to resolve a historical provider by provider kind.
    /// </summary>
    /// <param name="providerKind">The requested provider kind.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The historical provider resolution.</returns>
    private async Task<Result<CgmHistoricalProviderResolution>> TryResolveHistoricalProviderAsync(
        CgmProviderKind providerKind,
        CancellationToken cancellationToken)
    {
        foreach (var metadataProvider in _metadataProviders)
        {
            var metadataResult = await metadataProvider
                .GetMetadataAsync(cancellationToken)
                .ConfigureAwait(false);

            if (metadataResult.IsFailure)
            {
                continue;
            }

            var metadata = metadataResult.Value;

            if (metadata.ProviderKind != providerKind || !metadata.SupportsHistoricalReadings)
            {
                continue;
            }

            var historicalProvider = TryResolveProvider(metadataProvider, _historicalProviders);

            if (historicalProvider is null)
            {
                continue;
            }

            return Result<CgmHistoricalProviderResolution>.Success(
                new CgmHistoricalProviderResolution(
                    metadata,
                    historicalProvider,
                    metadataProvider));
        }

        return Result<CgmHistoricalProviderResolution>.Failure(
            new Error(
                "Cgm.HistoricalProviderNotFound",
                $"Historical CGM provider '{providerKind}' is not registered."));
    }

    /// <summary>
    /// Resolves the provider implementation associated with a metadata provider.
    /// </summary>
    /// <param name="metadataProvider">The metadata provider.</param>
    /// <param name="providers">The candidate providers.</param>
    /// <typeparam name="TProvider">The provider interface type.</typeparam>
    /// <returns>The matching provider, when available.</returns>
    private static TProvider? TryResolveProvider<TProvider>(
        ICgmMetadataProvider metadataProvider,
        IEnumerable<TProvider> providers)
        where TProvider : class
    {
        if (metadataProvider is TProvider providerFromMetadata)
        {
            return providerFromMetadata;
        }

        return providers.FirstOrDefault(provider =>
            ReferenceEquals(provider, metadataProvider));
    }

    #endregion
}