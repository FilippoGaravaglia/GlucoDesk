using GlucoDesk.Application.Cgm.History.Analytics.Requests;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.ValueObjects;

namespace GlucoDesk.Application.Tests.Cgm.History.Analytics.Requests;

public sealed class GlucoseHistorySummaryRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest_WhenRangeIsValid()
    {
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);
        var to = from.AddHours(1);
        var targetRange = CreateTargetRange();

        var request = new GlucoseHistorySummaryRequest(from, to, targetRange);

        Assert.Equal(from, request.From);
        Assert.Equal(to, request.To);
        Assert.Same(targetRange, request.TargetRange);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidRange()
    {
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new GlucoseHistorySummaryRequest(from, from, CreateTargetRange()));

        Assert.Equal("to", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectNullTargetRange()
    {
        var from = new DateTimeOffset(2026, 6, 8, 8, 0, 0, TimeSpan.Zero);

        var exception = Assert.Throws<ArgumentNullException>(
            () => new GlucoseHistorySummaryRequest(from, from.AddHours(1), null!));

        Assert.Equal("targetRange", exception.ParamName);
    }

    #region Helpers

    /// <summary>
    /// Creates the test target range.
    /// </summary>
    /// <returns>The target glucose range.</returns>
    private static GlucoseRange CreateTargetRange()
    {
        return new GlucoseRange(
            new GlucoseValue(70, GlucoseUnit.MgDl),
            new GlucoseValue(180, GlucoseUnit.MgDl));
    }

    #endregion
}