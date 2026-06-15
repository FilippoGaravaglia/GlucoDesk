using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Clients;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Endpoints;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Mapping;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for the Dexcom Share CGM provider.
/// </summary>
public static class DexcomShareCgmProviderServiceCollectionExtensions
{
    /// <summary>
    /// Registers the Dexcom Share CGM provider.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The Dexcom Share options.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or options are null.</exception>
    public static IServiceCollection AddDexcomShareCgmProvider(
        this IServiceCollection services,
        DexcomShareOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.AddSingleton(options);
        services.AddSingleton<TimeProvider>(TimeProvider.System);
        services.AddSingleton<DexcomShareEndpointProvider>();
        services.AddSingleton<DexcomShareGlucoseValueMapper>();

        services.AddHttpClient<IDexcomShareClient, DexcomShareClient>();

        services.AddSingleton<DexcomShareCgmProvider>();

        services.AddSingleton<ICgmLiveProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<DexcomShareCgmProvider>());

        services.AddSingleton<ICgmHistoricalProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<DexcomShareCgmProvider>());

        services.AddSingleton<ICgmMetadataProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<DexcomShareCgmProvider>());

        return services;
    }
}