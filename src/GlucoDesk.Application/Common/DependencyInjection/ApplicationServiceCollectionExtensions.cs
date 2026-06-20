using GlucoDesk.Application.Cgm.History.Analytics.Services;
using GlucoDesk.Application.Cgm.History.Analytics.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Services;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Resolution.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Resolution.Services;
using GlucoDesk.Application.Cgm.Services;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using GlucoDesk.Application.Cgm.Statistics.Services;
using GlucoDesk.Application.Cgm.Statistics.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.DependencyInjection;
using GlucoDesk.Application.Cgm.Diary.DependencyInjection;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Cgm.Backfill.Services;

namespace GlucoDesk.Application.Common.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for the application layer.
/// </summary>
public static class ApplicationServiceCollectionExtensions
{
    /// <summary>
    /// Registers GlucoDesk application services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddGlucoDeskApplication(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.TryAddSingleton<IApplicationSettingsChangeNotifier, ApplicationSettingsChangeNotifier>();

        services.AddScoped<IGlucoseDataService>(serviceProvider =>
        new GlucoseDataService(
            serviceProvider.GetRequiredService<ICgmProviderResolver>(),
        serviceProvider.GetRequiredService<TimeProvider>()));
        services.AddScoped<IApplicationSettingsService, ApplicationSettingsService>();
        services.AddScoped<IGlucoseHistoryService, GlucoseHistoryService>();
        services.AddScoped<IGlucoseHistoryAnalyticsService, GlucoseHistoryAnalyticsService>();
        services.AddScoped<ICgmProviderResolver, CgmProviderResolver>();
        services.TryAddScoped<IGlucoseStatisticsService, GlucoseStatisticsService>();
        services.AddGlucoseHistoryContinuity();
        services.AddGlycemicDiary();
        services.AddScoped<ICgmBackfillHistorySyncService, CgmBackfillHistorySyncService>();

        return services;
    }
}