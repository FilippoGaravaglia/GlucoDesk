using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.DependencyInjection;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Services;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Services;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Tokens.Stores;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Connection.DependencyInjection;

public sealed class DexcomConnectionStatusServiceCollectionExtensionsTests
{
    [Fact]
    public void AddDexcomConnectionStatus_ShouldRegisterConnectionStatusService()
    {
        var services = new ServiceCollection();

        services.AddSingleton<IDexcomOAuthTokenStore, InMemoryDexcomOAuthTokenStore>();
        services.AddSingleton(DexcomOAuthTokenRefreshOptions.Default);
        services.AddSingleton<TimeProvider>(TimeProvider.System);

        services.AddDexcomConnectionStatus();

        using var serviceProvider = services.BuildServiceProvider();

        var service = serviceProvider.GetRequiredService<IDexcomConnectionStatusService>();

        Assert.IsType<DexcomConnectionStatusService>(service);
    }

    [Fact]
    public void AddDexcomConnectionStatus_ShouldRejectNullServices()
    {
        IServiceCollection services = null!;

        var exception = Assert.Throws<ArgumentNullException>(
            services.AddDexcomConnectionStatus);

        Assert.Equal("services", exception.ParamName);
    }
}