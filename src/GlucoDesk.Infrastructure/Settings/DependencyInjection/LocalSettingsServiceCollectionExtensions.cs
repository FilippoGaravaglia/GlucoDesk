using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Infrastructure.Settings.Options;
using GlucoDesk.Infrastructure.Settings.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Infrastructure.Settings.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for local settings infrastructure.
/// </summary>
public static class LocalSettingsServiceCollectionExtensions
{
    /// <summary>
    /// Registers the local JSON application settings store.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The optional local settings storage options.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddJsonApplicationSettingsStore(
        this IServiceCollection services,
        LocalSettingsStorageOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(options ?? LocalSettingsStorageOptions.Default);
        services.TryAddSingleton<IApplicationSettingsStore, JsonApplicationSettingsStore>();

        return services;
    }
}