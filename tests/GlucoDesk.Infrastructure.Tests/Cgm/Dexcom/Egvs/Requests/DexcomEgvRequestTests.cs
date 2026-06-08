using GlucoDesk.Infrastructure.Cgm.Dexcom.Egvs.Requests;

namespace GlucoDesk.Infrastructure.Tests.Cgm.Dexcom.Egvs.Requests;

public sealed class DexcomEgvRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest_WhenValuesAreValid()
    {
        var startDateUtc = new DateTimeOffset(2026, 6, 8, 10, 0, 0, TimeSpan.Zero);
        var endDateUtc = startDateUtc.AddHours(2);

        var request = new DexcomEgvRequest(
            " client-secret ",
            startDateUtc,
            endDateUtc,
            forceTokenRefresh: true);

        Assert.Equal("client-secret", request.ClientSecret);
        Assert.Equal(startDateUtc, request.StartDateUtc);
        Assert.Equal(endDateUtc, request.EndDateUtc);
        Assert.True(request.ForceTokenRefresh);
    }

    [Fact]
    public void Constructor_ShouldNormalizeDatesToUtc()
    {
        var startDate = new DateTimeOffset(2026, 6, 8, 12, 0, 0, TimeSpan.FromHours(2));
        var endDate = startDate.AddHours(2);

        var request = new DexcomEgvRequest(
            "client-secret",
            startDate,
            endDate);

        Assert.Equal(TimeSpan.Zero, request.StartDateUtc.Offset);
        Assert.Equal(TimeSpan.Zero, request.EndDateUtc.Offset);
        Assert.Equal(new DateTimeOffset(2026, 6, 8, 10, 0, 0, TimeSpan.Zero), request.StartDateUtc);
        Assert.Equal(new DateTimeOffset(2026, 6, 8, 12, 0, 0, TimeSpan.Zero), request.EndDateUtc);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Constructor_ShouldRejectInvalidClientSecret(string clientSecret)
    {
        var now = DateTimeOffset.UtcNow;

        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomEgvRequest(
                clientSecret,
                now,
                now.AddHours(1)));

        Assert.Equal("clientSecret", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectStartDateEqualToEndDate()
    {
        var now = DateTimeOffset.UtcNow;

        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomEgvRequest(
                "client-secret",
                now,
                now));

        Assert.Equal("startDateUtc", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectStartDateAfterEndDate()
    {
        var now = DateTimeOffset.UtcNow;

        var exception = Assert.Throws<ArgumentException>(
            () => new DexcomEgvRequest(
                "client-secret",
                now,
                now.AddMinutes(-1)));

        Assert.Equal("startDateUtc", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectDateRangeGreaterThanThirtyDays()
    {
        var now = DateTimeOffset.UtcNow;

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new DexcomEgvRequest(
                "client-secret",
                now,
                now.AddDays(30).AddTicks(1)));

        Assert.Equal("endDateUtc", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldAllowDateRangeEqualToThirtyDays()
    {
        var now = DateTimeOffset.UtcNow;

        var request = new DexcomEgvRequest(
            "client-secret",
            now,
            now.AddDays(30));

        Assert.Equal(now.ToUniversalTime(), request.StartDateUtc);
        Assert.Equal(now.AddDays(30).ToUniversalTime(), request.EndDateUtc);
    }
}