using GlucoDesk.Application.Cgm.Providers.Abstractions;
using GlucoDesk.Infrastructure.Cgm.Mock.Options;
using GlucoDesk.Infrastructure.Cgm.Mock.Providers;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Infrastructure.Cgm.Mock.DependencyInjection;

/// <summary>
/// Provides dependency injection registrations for the mock CGM provider.
/// </summary>
public static class MockCgmProviderServiceCollectionExtensions
{
    /// <summary>
    /// Registers the mock CGM provider and its related provider interfaces.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <param name="options">The optional mock provider options.</param>
    /// <returns>The updated service collection.</returns>
    /// <exception cref="ArgumentNullException">Thrown when services is null.</exception>
    public static IServiceCollection AddMockCgmProvider(
        this IServiceCollection services,
        MockCgmProviderOptions? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        var effectiveOptions = options ?? MockCgmProviderOptions.Default;

        services.AddSingleton(effectiveOptions);
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<MockCgmProvider>();

        services.AddSingleton<ICgmLiveProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<MockCgmProvider>());

        services.AddSingleton<ICgmHistoricalProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<MockCgmProvider>());

        services.AddSingleton<ICgmMetadataProvider>(serviceProvider =>
            serviceProvider.GetRequiredService<MockCgmProvider>());

        return services;
    }
}