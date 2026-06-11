using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Infrastructure.Cgm.Dexcom.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers.Options;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Providers.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for the Dexcom Official CGM provider.
/// </summary>
public static class DexcomOfficialCgmProviderServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Dexcom Official API infrastructure and CGM provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="apiOptions">The Dexcom API options.</param>
    /// <param name="providerOptions">The optional Dexcom provider options.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or apiOptions is null.</exception>
    public static IServiceCollection AddDexcomOfficialCgmProvider(
        this IServiceCollection services,
        DexcomApiOptions apiOptions,
        DexcomCgmProviderOptions? providerOptions = null)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(apiOptions);

        services.AddDexcomOfficialApi(apiOptions);

        var effectiveProviderOptions = providerOptions ?? DexcomCgmProviderOptions.Default;

        services.TryAddSingleton(effectiveProviderOptions);
        services.TryAddSingleton<DexcomOfficialCgmProvider>();

        services.AddSingleton<ICgmLiveProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<DexcomOfficialCgmProvider>());

        services.AddSingleton<ICgmHistoricalProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<DexcomOfficialCgmProvider>());

        services.AddSingleton<ICgmMetadataProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<DexcomOfficialCgmProvider>());

        return services;
    }
}