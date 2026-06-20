using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Application.Cgm.History.Continuity.Requests;

namespace GlucoDesk.Application.Tests.Cgm.History.Continuity.Requests;

public sealed class CgmHistoryContinuitySyncRequestTests
{
    [Fact]
    public void Constructor_ShouldCreateRequest_WhenInputIsValid()
    {
        // Arrange
        var lookback = TimeSpan.FromHours(6);

        // Act
        var request = new CgmHistoryContinuitySyncRequest(
            CgmHistoryContinuitySyncTrigger.Manual,
            lookback);

        // Assert
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Manual, request.Trigger);
        Assert.Equal(lookback, request.Lookback);
    }

    [Fact]
    public void Constructor_ShouldRejectInvalidTrigger()
    {
        // Arrange
        var trigger = (CgmHistoryContinuitySyncTrigger)999;

        // Act
        var exception = Assert.Throws<ArgumentException>(
            () => new CgmHistoryContinuitySyncRequest(
                trigger,
                TimeSpan.FromHours(1)));

        // Assert
        Assert.Equal("trigger", exception.ParamName);
    }

    [Fact]
    public void Constructor_ShouldRejectZeroLookback()
    {
        // Act
        var exception = Assert.Throws<ArgumentOutOfRangeException>(
            () => new CgmHistoryContinuitySyncRequest(
                CgmHistoryContinuitySyncTrigger.Startup,
                TimeSpan.Zero));

        // Assert
        Assert.Equal("lookback", exception.ParamName);
    }

    [Fact]
    public void ForStartup_ShouldCreateStartupRequestWithDefaultLookback()
    {
        // Act
        var request = CgmHistoryContinuitySyncRequest.ForStartup();

        // Assert
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Startup, request.Trigger);
        Assert.Equal(CgmHistoryContinuitySyncRequest.DefaultStartupLookback, request.Lookback);
    }

    [Fact]
    public void ForResume_ShouldCreateResumeRequestWithDefaultLookback()
    {
        // Act
        var request = CgmHistoryContinuitySyncRequest.ForResume();

        // Assert
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Resume, request.Trigger);
        Assert.Equal(CgmHistoryContinuitySyncRequest.DefaultResumeLookback, request.Lookback);
    }

    [Fact]
    public void ForManual_ShouldCreateManualRequestWithProvidedLookback()
    {
        // Arrange
        var lookback = TimeSpan.FromHours(3);

        // Act
        var request = CgmHistoryContinuitySyncRequest.ForManual(lookback);

        // Assert
        Assert.Equal(CgmHistoryContinuitySyncTrigger.Manual, request.Trigger);
        Assert.Equal(lookback, request.Lookback);
    }
}