using GlucoDesk.Application.Cgm.Backfill.Options;
using GlucoDesk.Application.Cgm.Backfill.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Resolution.Abstractions;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Cgm.Statistics.Services.Abstractions;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Desktop.BackgroundSync.Dispatching.Abstractions;
using GlucoDesk.Desktop.BackgroundSync.Services.Abstractions;
using GlucoDesk.Desktop.Bootstrap;
using GlucoDesk.Desktop.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Desktop.Diary.Services.Abstractions;
using GlucoDesk.Desktop.ViewModels.Account;
using GlucoDesk.Desktop.ViewModels.BackgroundSync;
using GlucoDesk.Desktop.ViewModels.Dashboard;
using GlucoDesk.Desktop.ViewModels.Diary;
using GlucoDesk.Desktop.ViewModels.Main;
using GlucoDesk.Desktop.ViewModels.Settings;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Desktop.Tests.Bootstrap;

public sealed class DesktopServiceProviderBuilderTests
{
    [Fact]
    public async Task BuildServiceProvider_ShouldBuildProvider_WithValidationEnabled()
    {
        // Act
        await using var serviceProvider = BuildProvider();

        // Assert
        Assert.NotNull(serviceProvider);
    }

    [Fact]
    public async Task BuildServiceProvider_ShouldResolveDesktopSingletonServices()
    {
        // Arrange
        await using var serviceProvider = BuildProvider();

        // Act
        var backgroundSyncLifecycleService = serviceProvider
            .GetRequiredService<IDesktopBackgroundSyncLifecycleService>();

        var backgroundSyncUiDispatcher = serviceProvider
            .GetRequiredService<IBackgroundSyncUiDispatcher>();

        var historyContinuityCoordinator = serviceProvider
            .GetRequiredService<IDesktopHistoryContinuitySyncCoordinator>();

        var diaryExportFileSaveService = serviceProvider
            .GetRequiredService<IDiaryExportFileSaveService>();

        var backgroundSyncStatusViewModel = serviceProvider
            .GetRequiredService<BackgroundSyncStatusViewModel>();
        
        var historyContinuityStatusStore = serviceProvider
            .GetRequiredService<IDesktopHistoryContinuitySyncStatusStore>();

        // Assert
        Assert.NotNull(backgroundSyncLifecycleService);
        Assert.NotNull(backgroundSyncUiDispatcher);
        Assert.NotNull(historyContinuityCoordinator);
        Assert.NotNull(diaryExportFileSaveService);
        Assert.NotNull(backgroundSyncStatusViewModel);
        Assert.NotNull(historyContinuityStatusStore);
    }

    [Fact]
    public async Task BuildServiceProvider_ShouldResolveApplicationScopedServices_FromScope()
    {
        // Arrange
        await using var serviceProvider = BuildProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        var scopedProvider = scope.ServiceProvider;

        // Act
        var applicationSettingsService = scopedProvider
            .GetRequiredService<IApplicationSettingsService>();

        var glucoseDataService = scopedProvider
            .GetRequiredService<IGlucoseDataService>();

        var glucoseHistoryService = scopedProvider
            .GetRequiredService<IGlucoseHistoryService>();

        var glucoseStatisticsService = scopedProvider
            .GetRequiredService<IGlucoseStatisticsService>();

        var cgmProviderResolver = scopedProvider
            .GetRequiredService<ICgmProviderResolver>();

        var historyContinuitySyncService = scopedProvider
            .GetRequiredService<ICgmHistoryContinuitySyncService>();

        // Assert
        Assert.NotNull(applicationSettingsService);
        Assert.NotNull(glucoseDataService);
        Assert.NotNull(glucoseHistoryService);
        Assert.NotNull(glucoseStatisticsService);
        Assert.NotNull(cgmProviderResolver);
        Assert.NotNull(historyContinuitySyncService);
    }

    [Fact]
    public async Task BuildServiceProvider_ShouldResolveBackfillServiceGraph_FromScope()
    {
        // Arrange
        await using var serviceProvider = BuildProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        var scopedProvider = scope.ServiceProvider;

        // Act
        var capabilityOptions = scopedProvider
            .GetRequiredService<CgmBackfillCapabilityOptions>();

        var capabilityService = scopedProvider
            .GetRequiredService<ICgmBackfillCapabilityService>();

        var planService = scopedProvider
            .GetRequiredService<ICgmBackfillPlanService>();

        var planQueryService = scopedProvider
            .GetRequiredService<ICgmBackfillPlanQueryService>();

        var runService = scopedProvider
            .GetRequiredService<ICgmBackfillRunService>();

        var historicalReadingsFetcher = scopedProvider
            .GetRequiredService<ICgmBackfillHistoricalReadingsFetcher>();

        var executionService = scopedProvider
            .GetRequiredService<ICgmBackfillExecutionService>();

        var historySyncService = scopedProvider
            .GetRequiredService<ICgmBackfillHistorySyncService>();

        // Assert
        Assert.NotNull(capabilityOptions);
        Assert.NotNull(capabilityService);
        Assert.NotNull(planService);
        Assert.NotNull(planQueryService);
        Assert.NotNull(runService);
        Assert.NotNull(historicalReadingsFetcher);
        Assert.NotNull(executionService);
        Assert.NotNull(historySyncService);
    }

    [Fact]
    public async Task BuildServiceProvider_ShouldResolveDesktopViewModels()
    {
        // Arrange
        await using var serviceProvider = BuildProvider();
        await using var scope = serviceProvider.CreateAsyncScope();

        var scopedProvider = scope.ServiceProvider;

        // Act
        var mainWindowViewModel = scopedProvider
            .GetRequiredService<MainWindowViewModel>();

        var dashboardViewModel = scopedProvider
            .GetRequiredService<DashboardViewModel>();

        var diaryViewModel = scopedProvider
            .GetRequiredService<DiaryViewModel>();

        var accountViewModel = scopedProvider
            .GetRequiredService<AccountViewModel>();

        var settingsViewModel = scopedProvider
            .GetRequiredService<SettingsViewModel>();

        // Assert
        Assert.NotNull(mainWindowViewModel);
        Assert.NotNull(dashboardViewModel);
        Assert.NotNull(diaryViewModel);
        Assert.NotNull(accountViewModel);
        Assert.NotNull(settingsViewModel);
    }

    #region Helpers

    /// <summary>
    /// Builds the desktop service provider using the real production composition root.
    /// </summary>
    /// <returns>The configured desktop service provider.</returns>
    private static ServiceProvider BuildProvider()
    {
        return DesktopServiceProviderBuilder.BuildServiceProvider();
    }

    #endregion
}