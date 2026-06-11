using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Models;

namespace GlucoDesk.Desktop.Tests.Bootstrap.Providers.Connection.Models;

public sealed class DexcomDesktopConnectionResultTests
{
    [Fact]
    public void Constructor_ShouldCreateResult_WhenValuesAreValid()
    {
        var connectedAtUtc = DateTimeOffset.Parse("2026-01-01T10:00:00Z");

        var result = new DexcomDesktopConnectionResult(
            connectedAtUtc,
            connectedAtUtc.AddHours(1),
            connectedAtUtc.AddDays(30));

        Assert.Equal(connectedAtUtc, result.ConnectedAtUtc);
        Assert.Equal(connectedAtUtc.AddHours(1), result.AccessTokenExpiresAtUtc);
        Assert.Equal(connectedAtUtc.AddDays(30), result.RefreshTokenExpiresAtUtc);
    }

    [Fact]
    public void Constructor_ShouldRejectAccessTokenExpirationBeforeConnectionTimestamp()
    {
        var connectedAtUtc = DateTimeOffset.Parse("2026-01-01T10:00:00Z");

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomDesktopConnectionResult(
                connectedAtUtc,
                connectedAtUtc,
                connectedAtUtc.AddDays(30)));

        Assert.Equal("accessTokenExpiresAtUtc", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectRefreshTokenExpirationBeforeConnectionTimestamp()
    {
        var connectedAtUtc = DateTimeOffset.Parse("2026-01-01T10:00:00Z");

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomDesktopConnectionResult(
                connectedAtUtc,
                connectedAtUtc.AddHours(1),
                connectedAtUtc));

        Assert.Equal("refreshTokenExpiresAtUtc", exception.ParamName);
    }
}