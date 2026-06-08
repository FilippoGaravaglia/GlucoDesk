using GlucoDesk.Application.Cgm.History.Abstractions;
using GlucoDesk.Infrastructure.Cgm.History.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.History.Options;
using GlucoDesk.Infrastructure.Cgm.History.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Infrastructure.Tests.Cgm.History.DependencyInjection;

public sealed class LocalGlucoseHistoryServiceCollectionExtensionsTests
{
    [Fact]
    public void AddJsonGlucoseHistoryStore_ShouldRegisterHistoryStore()
    {
        var services = new ServiceCollection();

        services.AddJsonGlucoseHistoryStore(
            new LocalGlucoseHistoryStorageOptions("/tmp/glucodesk/glucose-history.json"));

        using var serviceProvider = services.BuildServiceProvider();

        var store = serviceProvider.GetRequiredService<IGlucoseHistoryStore>();

        Assert.IsType<JsonGlucoseHistoryStore>(store);
    }

    [Fact]
    public void AddJsonGlucoseHistoryStore_ShouldRejectNullServiceCollection()
    {
        IServiceCollection services = null!;

        var exception = Assert.Throws<ArgumentNullException>(
            () => services.AddJsonGlucoseHistoryStore());

        Assert.Equal("services", exception.ParamName);
    }
}