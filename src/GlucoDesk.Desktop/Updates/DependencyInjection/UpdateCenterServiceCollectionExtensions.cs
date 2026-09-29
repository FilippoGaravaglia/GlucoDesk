using GlucoDesk.Application.Updates;
using GlucoDesk.Desktop.ViewModels.Updates;
using GlucoDesk.Infrastructure.Updates;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace GlucoDesk.Desktop.Updates.DependencyInjection;

/// <summary>
/// Registers services required by the desktop update center.
/// </summary>
internal static class UpdateCenterServiceCollectionExtensions
{
    /// <summary>
    /// Adds the GlucoDesk desktop update-center services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The supplied service collection.</returns>
    public static IServiceCollection AddDesktopUpdateCenter(
        this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient<IReleaseCatalog, GitHubReleaseCatalog>();

        services.TryAddSingleton<
            GlucoDesk.Application.Updates.IApplicationVersionProvider,
            GlucoDesk.Infrastructure.Updates.ApplicationVersionProvider>();

        services.TryAddSingleton<IUpdatePreferencesStore>(
            _ => JsonUpdatePreferencesStore.CreateDefault());

        services.TryAddTransient<IUpdateService, UpdateService>();
        services.TryAddTransient<IUpdateCoordinator, UpdateCoordinator>();

        services.TryAddSingleton<UpdateCenterStore>();
        services.AddTransient<UpdateCenterViewModel>();

        return services;
    }
}
