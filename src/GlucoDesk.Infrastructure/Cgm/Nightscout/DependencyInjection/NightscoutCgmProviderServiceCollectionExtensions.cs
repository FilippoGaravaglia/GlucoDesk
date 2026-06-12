using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Clients;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Mappers;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Options;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Providers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Infrastructure.Cgm.Nightscout.DependencyInjection;

/// <summary>
/// Provides dependency injection registration for the Nightscout CGM provider.
/// </summary>
public static class NightscoutCgmProviderServiceCollectionExtensions
{
    /// <summary>
    /// Adds the Nightscout CGM provider to the service collection.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The Nightscout options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddNightscoutCgmProvider(
        this IServiceCollection services,
        NightscoutOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton(options);

        services.TryAddScoped<INightscoutEntryMapper, NightscoutEntryMapper>();

        services
            .AddHttpClient<INightscoutEntriesClient, NightscoutEntriesClient>(
                httpClient =>
                {
                    httpClient.Timeout = options.RequestTimeout;
                });

        services.TryAddScoped<NightscoutCgmProvider>();

        services.AddScoped<ICgmLiveProvider>(
            serviceProvider => serviceProvider.GetRequiredService<NightscoutCgmProvider>());

        services.AddScoped<ICgmHistoricalProvider>(
            serviceProvider => serviceProvider.GetRequiredService<NightscoutCgmProvider>());

        services.AddScoped<ICgmMetadataProvider>(
            serviceProvider => serviceProvider.GetRequiredService<NightscoutCgmProvider>());

        return services;
    }
}