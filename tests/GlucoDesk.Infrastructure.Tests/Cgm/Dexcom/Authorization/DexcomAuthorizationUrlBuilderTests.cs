using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Endpoints;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Authorization;

public sealed class DexcomAuthorizationUrlBuilderTests
{
    [Fact]
    public void BuildAuthorizationUri_ShouldBuildExpectedAuthorizationUri()
    {
        var builder = new DexcomAuthorizationUrlBuilder(new DexcomApiEndpointProvider());

        var uri = builder.BuildAuthorizationUri(
            DexcomApiEnvironment.Sandbox,
            new DexcomAuthorizationRequest(
                "client-id",
                new Uri("http://127.0.0.1:51234/callback"),
                ["egv", "offline_access"],
                "state-value"));

        Assert.Equal("sandbox-api.dexcom.com", uri.Host);
        Assert.Equal("/v3/oauth2/login", uri.AbsolutePath);

        var query = Uri.UnescapeDataString(uri.Query);

        Assert.Contains("response_type=code", query, StringComparison.Ordinal);
        Assert.Contains("client_id=client-id", query, StringComparison.Ordinal);
        Assert.Contains("redirect_uri=http://127.0.0.1:51234/callback", query, StringComparison.Ordinal);
        Assert.Contains("scope=egv offline_access", query, StringComparison.Ordinal);
        Assert.Contains("state=state-value", query, StringComparison.Ordinal);
    }

    [Fact]
    public void BuildAuthorizationUri_ShouldRejectNullRequest()
    {
        var builder = new DexcomAuthorizationUrlBuilder(new DexcomApiEndpointProvider());

        var exception = Assert.Throws<ArgumentNullException>(
            () => builder.BuildAuthorizationUri(DexcomApiEnvironment.Sandbox, null!));

        Assert.Equal("request", exception.ParamName);
    }
}