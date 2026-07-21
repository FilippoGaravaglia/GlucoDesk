using GlucoDesk.Desktop.Onboarding;
using GlucoDesk.Desktop.ViewModels.Onboarding;

namespace GlucoDesk.Desktop.Tests.ViewModels.Onboarding;

public sealed class FeatureTourViewModelProgressTests
{
    [Fact]
    public void ProgressValue_ShouldStartFromFirstStep()
    {
        using var viewModel =
            new FeatureTourViewModel(
                FeatureTourCatalog.GetSteps(),
                () => { });

        Assert.Equal(
            1D,
            viewModel.ProgressValue);

        Assert.Equal(
            (double)viewModel.TotalSteps,
            viewModel.ProgressMaximum);

        Assert.Equal(
            $"1/{viewModel.TotalSteps}",
            viewModel.StepCounterText);
    }

    [Fact]
    public void ProgressValue_ShouldReachMaximum_OnLastStep()
    {
        using var viewModel =
            new FeatureTourViewModel(
                FeatureTourCatalog.GetSteps(),
                () => { });

        for (var index = 1;
             index < viewModel.TotalSteps;
             index++)
        {
            viewModel.NextCommand.Execute(null);
        }

        Assert.Equal(
            viewModel.ProgressMaximum,
            viewModel.ProgressValue);

        Assert.Equal(
            $"{viewModel.TotalSteps}/{viewModel.TotalSteps}",
            viewModel.StepCounterText);
    }
}
