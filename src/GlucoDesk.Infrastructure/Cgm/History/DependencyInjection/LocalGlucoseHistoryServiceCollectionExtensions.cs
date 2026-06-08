using GlucoDesk.Application.Cgm.History.Abstractions;
using GlucoDesk.Infrastructure.Cgm.History.Options;
using GlucoDesk.Infrastructure.Cgm.History.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Infrastructure.Cgm.History.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for local glucose history infrastructure.
/// </summary>
public static class LocalGlucoseHistoryServiceCollectionExtensions
{
    /// <summary>
    /// Registers the local JSON glucose history store.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The optional local glucose history storage options.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddJsonGlucoseHistoryStore(
        this IServiceCollection services,
        LocalGlucoseHistoryStorageOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(options ?? LocalGlucoseHistoryStorageOptions.Default);
        services.TryAddSingleton<IGlucoseHistoryStore, JsonGlucoseHistoryStore>();

        return services;
    }
}