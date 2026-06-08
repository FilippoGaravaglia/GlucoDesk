using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Infrastructure.Settings.DependencyInjection;
using GlucoDesk.Infrastructure.Settings.Options;
using GlucoDesk.Infrastructure.Settings.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Infrastructure.Tests.Settings.DependencyInjection;

public sealed class LocalSettingsServiceCollectionExtensionsTests
{
    [Fact]
    public void AddJsonApplicationSettingsStore_ShouldRegisterSettingsStore()
    {
        var services = new ServiceCollection();

        services.AddJsonApplicationSettingsStore(
            new LocalSettingsStorageOptions("/tmp/glucodesk/settings.json"));

        using var serviceProvider = services.BuildServiceProvider();

        var store = serviceProvider.GetRequiredService<IApplicationSettingsStore>();

        Assert.IsType<JsonApplicationSettingsStore>(store);
    }

    [Fact]
    public void AddJsonApplicationSettingsStore_ShouldRejectNullServiceCollection()
    {
        IServiceCollection services = null!;

        var exception = Assert.Throws<ArgumentNullException>(
            () => services.AddJsonApplicationSettingsStore());

        Assert.Equal("services", exception.ParamName);
    }
}