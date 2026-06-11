using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for Dexcom connection status services.
/// </summary>
public static class DexcomConnectionStatusServiceCollectionExtensions
{
    /// <summary>
    /// Registers Dexcom connection status services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddDexcomConnectionStatus(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<IDexcomConnectionStatusService, DexcomConnectionStatusService>();

        return services;
    }
}