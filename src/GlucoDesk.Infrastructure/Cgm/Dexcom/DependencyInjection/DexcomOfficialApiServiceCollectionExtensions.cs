using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Clients;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for Dexcom Official API infrastructure.
/// </summary>
public static class DexcomOfficialApiServiceCollectionExtensions
{
    /// <summary>
    /// Registers Dexcom Official API infrastructure services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The Dexcom API options.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services or options is null.</exception>
    public static IServiceCollection AddDexcomOfficialApi(
        this IServiceCollection services,
        DexcomApiOptions options)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentNullException.ThrowIfNull(options);

        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton(options);
        services.TryAddSingleton<IDexcomApiEndpointProvider, DexcomApiEndpointProvider>();
        services.TryAddSingleton<IDexcomAuthorizationUrlBuilder, DexcomAuthorizationUrlBuilder>();

        services.AddHttpClient<IDexcomTokenClient, DexcomTokenClient>();

        return services;
    }
}