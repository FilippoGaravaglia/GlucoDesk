using GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Endpoints;

public sealed class DexcomApiEndpointProviderTests
{
    [Theory]
    [InlineData(DexcomApiEnvironment.Sandbox, "https://sandbox-api.dexcom.com/")]
    [InlineData(DexcomApiEnvironment.ProductionUs, "https://api.dexcom.com/")]
    [InlineData(DexcomApiEnvironment.ProductionEu, "https://api.dexcom.eu/")]
    [InlineData(DexcomApiEnvironment.ProductionJapan, "https://api.dexcom.jp/")]
    public void GetEndpoints_ShouldReturnExpectedApiBaseUri(
        DexcomApiEnvironment environment,
        string expectedBaseUri)
    {
        var provider = new DexcomApiEndpointProvider();

        var endpoints = provider.GetEndpoints(environment);

        Assert.Equal(new Uri(expectedBaseUri), endpoints.ApiBaseUri);
        Assert.Equal("/v3/users/self/egvs", endpoints.EgvsPath);
        Assert.Equal(new Uri(new Uri(expectedBaseUri), "/v3/oauth2/login"), endpoints.AuthorizationUri);
        Assert.Equal(new Uri(new Uri(expectedBaseUri), "/v3/oauth2/token"), endpoints.TokenUri);
    }

    [Fact]
    public void GetEndpoints_ShouldRejectUnsupportedEnvironment()
    {
        var provider = new DexcomApiEndpointProvider();

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => provider.GetEndpoints((DexcomApiEnvironment)999));

        Assert.Equal("environment", exception.ParamName);
    }
}