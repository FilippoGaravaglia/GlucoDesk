using GlucoDesk.Application.Cgm.History.Abstractions;
using GlucoDesk.Application.Cgm.History.Analytics.Services.Abstractions;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Cgm.History.Services.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Application.Cgm.Providers.Metadata;
using GlucoDesk.Application.Cgm.Readings.Requests;
using GlucoDesk.Application.Cgm.Readings.Results;
using GlucoDesk.Application.Cgm.Services.Abstractions;
using GlucoDesk.Application.Common.DependencyInjection;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Application.Tests.Common.DependencyInjection;

public sealed class ApplicationServiceCollectionExtensionsTests
{
    [Fact]
    public void AddGlucoDeskApplication_ShouldRegisterApplicationServices()
    {
        var services = new ServiceCollection();

        services.AddSingleton<ICgmLiveProvider, FakeCgmProvider>();
        services.AddSingleton<ICgmHistoricalProvider, FakeCgmProvider>();
        services.AddSingleton<ICgmMetadataProvider, FakeCgmProvider>();
        services.AddSingleton<IGlucoseHistoryStore, FakeGlucoseHistoryStore>();

        services.AddGlucoDeskApplication();

        using var serviceProvider = services.BuildServiceProvider();

        var glucoseDataService = serviceProvider.GetRequiredService<IGlucoseDataService>();
        var glucoseHistoryService = serviceProvider.GetRequiredService<IGlucoseHistoryService>();
        var glucoseHistoryAnalyticsService = serviceProvider.GetRequiredService<IGlucoseHistoryAnalyticsService>();

        Assert.NotNull(glucoseDataService);
        Assert.NotNull(glucoseHistoryService);
        Assert.NotNull(glucoseHistoryAnalyticsService);
    }

    [Fact]
    public void AddGlucoDeskApplication_ShouldRejectNullServiceCollection()
    {
        IServiceCollection services = null!;

        var exception = Assert.Throws<ArgumentNullException>(
            services.AddGlucoDeskApplication);

        Assert.Equal("services", exception.ParamName);
    }

    #region Helpers

    private sealed class FakeCgmProvider : ICgmLiveProvider, ICgmHistoricalProvider, ICgmMetadataProvider
    {
        /// <inheritdoc />
        public Task<Result<LatestGlucoseReadingResult>> GetLatestReadingAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<LatestGlucoseReadingResult>.Success(
                new LatestGlucoseReadingResult(CreateReading(), DateTimeOffset.UtcNow)));
        }

        /// <inheritdoc />
        public Task<Result<GlucoseReadingsResult>> GetRecentReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<GlucoseReadingsResult>.Success(
                new GlucoseReadingsResult([CreateReading()], DateTimeOffset.UtcNow)));
        }

        /// <inheritdoc />
        public Task<Result<GlucoseReadingsResult>> GetReadingsAsync(
            GlucoseReadingsRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<GlucoseReadingsResult>.Success(
                new GlucoseReadingsResult([CreateReading()], DateTimeOffset.UtcNow)));
        }

        /// <inheritdoc />
        public Task<Result<CgmProviderMetadata>> GetMetadataAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<CgmProviderMetadata>.Success(
                new CgmProviderMetadata(
                    CgmProviderKind.Mock,
                    "Mock",
                    GlucoseDataFreshness.NearRealTime,
                    supportsLiveReadings: true,
                    supportsHistoricalReadings: true)));
        }

        #region Helpers

        /// <summary>
        /// Creates a valid glucose reading used by dependency injection tests.
        /// </summary>
        /// <returns>A valid glucose reading.</returns>
        private static GlucoseReading CreateReading()
        {
            return new GlucoseReading(
                DateTimeOffset.UtcNow,
                new GlucoseValue(120, GlucoseUnit.MgDl),
                TrendDirection.Flat,
                CgmProviderKind.Mock,
                GlucoseDataFreshness.NearRealTime);
        }

        #endregion
    }

    private sealed class FakeGlucoseHistoryStore : IGlucoseHistoryStore
    {
        /// <inheritdoc />
        public Task<Result> SaveReadingsAsync(
            IReadOnlyCollection<GlucoseReading> readings,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }

        /// <inheritdoc />
        public Task<Result<GlucoseHistoryResult>> GetReadingsAsync(
            GlucoseHistoryRequest request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(Result<GlucoseHistoryResult>.Success(new GlucoseHistoryResult([])));
        }
    }

    #endregion
}