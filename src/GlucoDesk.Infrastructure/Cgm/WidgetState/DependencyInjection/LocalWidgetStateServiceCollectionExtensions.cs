using GlucoDesk.Application.Cgm.WidgetState.Abstractions;
using GlucoDesk.Application.Cgm.WidgetState.Options;
using GlucoDesk.Application.Cgm.WidgetState.Services;
using GlucoDesk.Application.Cgm.WidgetState.Services.Abstractions;
using GlucoDesk.Infrastructure.Cgm.WidgetState.Options;
using GlucoDesk.Infrastructure.Cgm.WidgetState.Stores;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Infrastructure.Cgm.WidgetState.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for local glucose widget state services.
/// </summary>
public static class LocalWidgetStateServiceCollectionExtensions
{
    /// <summary>
    /// Registers the local JSON widget state store and widget state publisher.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The optional local widget state storage options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddJsonWidgetStateStore(
        this IServiceCollection services,
        LocalWidgetStateStorageOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton(options ?? LocalWidgetStateStorageOptions.Default);
        services.TryAddSingleton(WidgetStatePublisherOptions.Default);
        services.TryAddSingleton<IWidgetStateStore, JsonWidgetStateStore>();
        services.TryAddSingleton<IWidgetStatePublisher, GlucoseWidgetStatePublisher>();

        return services;
    }
}