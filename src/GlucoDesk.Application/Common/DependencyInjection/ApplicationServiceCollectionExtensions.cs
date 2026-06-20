using GlucoDesk.Application.Cgm.Backfill.Options;
using GlucoDesk.Application.Cgm.Backfill.Services;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Cgm.Diary.DependencyInjection;
using GlucoDesk.Application.Cgm.History.Analytics.Services;
using GlucoDesk.Application.Cgm.History.Analytics.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.DependencyInjection;
using GlucoDesk.Application.Cgm.History.Continuity.Services;
using GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Services;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Resolution.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Resolution.Services;
using GlucoDesk.Application.Cgm.Services;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Cgm.Statistics.Services;
using GlucoDesk.Application.Cgm.Statistics.Services.Abstractions;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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

        services.AddApplicationCoreServices();
        services.AddCgmApplicationServices();
        services.AddCgmBackfillAndContinuityServices();

        services.AddGlucoseHistoryContinuity();
        services.AddGlycemicDiary();

        return services;
    }

    #region Helpers

    /// <summary>
    /// Registers shared application services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void AddApplicationCoreServices(this IServiceCollection services)
    {
        services.TryAddSingleton<TimeProvider>(TimeProvider.System);
        services.TryAddSingleton<IApplicationSettingsChangeNotifier, ApplicationSettingsChangeNotifier>();

        services.TryAddScoped<IApplicationSettingsService, ApplicationSettingsService>();
    }

    /// <summary>
    /// Registers CGM application services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void AddCgmApplicationServices(this IServiceCollection services)
    {
        services.TryAddScoped<ICgmProviderResolver, CgmProviderResolver>();

        services.TryAddScoped<IGlucoseDataService>(serviceProvider =>
            new GlucoseDataService(
                serviceProvider.GetRequiredService<ICgmProviderResolver>(),
                serviceProvider.GetRequiredService<TimeProvider>()));

        services.TryAddScoped<IGlucoseHistoryService, GlucoseHistoryService>();
        services.TryAddScoped<IGlucoseHistoryAnalyticsService, GlucoseHistoryAnalyticsService>();
        services.TryAddScoped<IGlucoseStatisticsService, GlucoseStatisticsService>();
    }

    /// <summary>
    /// Registers CGM backfill and history continuity application services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    private static void AddCgmBackfillAndContinuityServices(this IServiceCollection services)
    {
        services.TryAddSingleton(CgmBackfillCapabilityOptions.Default);

        services.TryAddScoped<ICgmBackfillCapabilityService, CgmBackfillCapabilityService>();
        services.TryAddScoped<ICgmBackfillPlanService, CgmBackfillPlanService>();
        services.TryAddScoped<ICgmBackfillPlanQueryService, CgmBackfillPlanQueryService>();
        services.TryAddScoped<ICgmBackfillRunService, CgmBackfillRunService>();
        services.TryAddScoped<ICgmBackfillHistoricalReadingsFetcher, CgmBackfillHistoricalReadingsFetcher>();
        services.TryAddScoped<ICgmBackfillExecutionService, CgmBackfillExecutionService>();
        services.TryAddScoped<ICgmBackfillHistorySyncService, CgmBackfillHistorySyncService>();

        services.TryAddScoped<ICgmHistoryContinuitySyncService, CgmHistoryContinuitySyncService>();
    }

    #endregion
}