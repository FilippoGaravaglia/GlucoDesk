using GlucoDesk.Desktop.ViewModels.Onboarding;

namespace GlucoDesk.Desktop.Tests.ViewModels.Onboarding;

public sealed class LanguageOnboardingLaunchPolicyTests
{
    [Fact]
    public void ShouldShow_ShouldReturnTrue_WhenPreferenceIsMissing()
    {
        var result = LanguageOnboardingLaunchPolicy.ShouldShow(
            hasExplicitLanguagePreference: false,
            forceOnboardingValue: null);

        Assert.True(result);
    }

    [Fact]
    public void ShouldShow_ShouldReturnFalse_WhenPreferenceExists()
    {
        var result = LanguageOnboardingLaunchPolicy.ShouldShow(
            hasExplicitLanguagePreference: true,
            forceOnboardingValue: null);

        Assert.False(result);
    }

    [Theory]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("TRUE")]
    [InlineData("yes")]
    public void ShouldShow_ShouldReturnTrue_WhenQaOverrideIsEnabled(
        string value)
    {
        var result = LanguageOnboardingLaunchPolicy.ShouldShow(
            hasExplicitLanguagePreference: true,
            forceOnboardingValue: value);

        Assert.True(result);
    }

    [Theory]
    [InlineData("")]
    [InlineData("0")]
    [InlineData("false")]
    [InlineData("no")]
    public void ShouldShow_ShouldRespectPreference_WhenQaOverrideIsDisabled(
        string value)
    {
        var result = LanguageOnboardingLaunchPolicy.ShouldShow(
            hasExplicitLanguagePreference: true,
            forceOnboardingValue: value);

        Assert.False(result);
    }
}
