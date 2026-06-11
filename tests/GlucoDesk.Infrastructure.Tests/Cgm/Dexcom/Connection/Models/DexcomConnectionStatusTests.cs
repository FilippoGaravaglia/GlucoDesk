using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Connection.Models;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Connection.Models;

public sealed class DexcomConnectionStatusTests
{
    [Fact]
    public void Constructor_ShouldCreateConnectionStatus_WhenValuesAreValid()
    {
        var checkedAtUtc = DateTimeOffset.Parse("2026-01-01T10:00:00Z");

        var status = new DexcomConnectionStatus(
            DexcomConnectionState.Connected,
            checkedAtUtc,
            " Dexcom is connected. ",
            checkedAtUtc.AddMinutes(30),
            checkedAtUtc.AddDays(30));

        Assert.Equal(DexcomConnectionState.Connected, status.State);
        Assert.Equal(checkedAtUtc, status.CheckedAtUtc);
        Assert.Equal("Dexcom is connected.", status.Message);
        Assert.True(status.IsConnected);
        Assert.False(status.CanAttemptRefresh);
        Assert.Equal(checkedAtUtc.AddMinutes(30), status.AccessTokenExpiresAtUtc);
        Assert.Equal(checkedAtUtc.AddDays(30), status.RefreshTokenExpiresAtUtc);
    }

    [Fact]
    public void CanAttemptRefresh_ShouldReturnTrue_WhenAccessTokenRefreshIsRequired()
    {
        var status = new DexcomConnectionStatus(
            DexcomConnectionState.AccessTokenRefreshRequired,
            DateTimeOffset.Parse("2026-01-01T10:00:00Z"),
            "Refresh required.");

        Assert.False(status.IsConnected);
        Assert.True(status.CanAttemptRefresh);
    }

    [Fact]
    public void Constructor_ShouldRejectUnknownState()
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomConnectionStatus(
                DexcomConnectionState.Unknown,
                DateTimeOffset.Parse("2026-01-01T10:00:00Z"),
                "Unknown."));

        Assert.Equal("state", exception.ParamName);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidMessage(string message)
    {
        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomConnectionStatus(
                DexcomConnectionState.TokenMissing,
                DateTimeOffset.Parse("2026-01-01T10:00:00Z"),
                message));

        Assert.Equal("message", exception.ParamName);
    }
}