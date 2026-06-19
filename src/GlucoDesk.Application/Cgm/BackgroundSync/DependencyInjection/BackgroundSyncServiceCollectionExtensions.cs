using GlucoDesk.Application.Cgm.BackgroundSync.Options;
using GlucoDesk.Application.Cgm.BackgroundSync.Services;
using GlucoDesk.Application.Cgm.BackgroundSync.Services.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using GlucoDesk.Application.Cgm.BackgroundSync.State.Services;
using GlucoDesk.Application.Cgm.BackgroundSync.State.Services.Abstractions;

namespace GlucoDesk.Application.Cgm.BackgroundSync.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for CGM background sync services.
/// </summary>
public static class BackgroundSyncServiceCollectionExtensions
{
    /// <summary>
    /// Registers CGM background sync services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The optional background sync options.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddCgmBackgroundSync(
        this IServiceCollection services,
        BackgroundSyncOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);
    
        services.TryAddSingleton(options ?? BackgroundSyncOptions.Default);
        services.TryAddSingleton(TimeProvider.System);
        services.TryAddSingleton<ICgmBackgroundSyncService, CgmBackgroundSyncService>();
        services.TryAddSingleton<IBackgroundSyncLoopService, BackgroundSyncLoopService>();
        services.TryAddSingleton<IBackgroundSyncStateService, BackgroundSyncStateService>();
    
        return services;
    }
}