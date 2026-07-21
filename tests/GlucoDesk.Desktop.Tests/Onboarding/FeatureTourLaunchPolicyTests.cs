using GlucoDesk.Desktop.Onboarding;

namespace GlucoDesk.Desktop.Tests.Onboarding;

public sealed class FeatureTourLaunchPolicyTests
{
    [Fact]
    public void ShouldShow_ShouldReturnTrue_WhenTourIsIncomplete()
    {
        var result =
            FeatureTourLaunchPolicy.ShouldShow(
                hasCompletedCurrentTour: false,
                forceTourValue: null);

        Assert.True(result);
    }

    [Fact]
    public void ShouldShow_ShouldReturnFalse_WhenTourIsComplete()
    {
        var result =
            FeatureTourLaunchPolicy.ShouldShow(
                hasCompletedCurrentTour: true,
                forceTourValue: null);

        Assert.False(result);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("TRUE")]
    [InlineData("yes")]
    public void ShouldShow_ShouldReturnTrue_WhenForced(
        string forceValue)
    {
        var result =
            FeatureTourLaunchPolicy.ShouldShow(
                hasCompletedCurrentTour: true,
                forceTourValue: forceValue);

        Assert.True(result);
    }
}
