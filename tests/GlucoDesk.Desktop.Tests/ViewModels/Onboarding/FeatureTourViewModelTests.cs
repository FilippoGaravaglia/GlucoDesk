using GlucoDesk.Desktop.Localization;
using GlucoDesk.Desktop.Onboarding;
using GlucoDesk.Desktop.Tests.Localization;
using GlucoDesk.Desktop.ViewModels.Onboarding;

namespace GlucoDesk.Desktop.Tests.ViewModels.Onboarding;

public sealed class FeatureTourViewModelTests : EnglishLocalizationTestBase
{
    private static readonly IReadOnlyList<FeatureTourStep> Steps =
    [
        CreateStep("01", "One"),
        CreateStep("02", "Two"),
        CreateStep("03", "Three")
    ];

    [Fact]
    public void Constructor_ShouldExposeFirstStep()
    {
        using var viewModel =
            new FeatureTourViewModel(
                Steps,
                () => { });

        Assert.Equal(0, viewModel.CurrentStepIndex);
        Assert.Equal(1, viewModel.CurrentStepNumber);
        Assert.Equal(3, viewModel.TotalSteps);
        Assert.Equal("01", viewModel.StepNumber);
        Assert.True(viewModel.IsFirstStep);
        Assert.False(viewModel.IsLastStep);
        Assert.False(viewModel.CanGoBack);
        Assert.Equal("1 / 3", viewModel.ProgressText);
    }

    [Fact]
    public void Next_ShouldAdvanceThroughSteps()
    {
        using var viewModel =
            new FeatureTourViewModel(
                Steps,
                () => { });

        viewModel.NextCommand.Execute(null);

        Assert.Equal(1, viewModel.CurrentStepIndex);
        Assert.Equal("02", viewModel.StepNumber);
        Assert.True(viewModel.CanGoBack);
        Assert.False(viewModel.IsLastStep);

        viewModel.NextCommand.Execute(null);

        Assert.Equal(2, viewModel.CurrentStepIndex);
        Assert.True(viewModel.IsLastStep);
    }

    [Fact]
    public void Back_ShouldReturnToPreviousStep()
    {
        using var viewModel =
            new FeatureTourViewModel(
                Steps,
                () => { });

        viewModel.NextCommand.Execute(null);
        viewModel.BackCommand.Execute(null);

        Assert.Equal(0, viewModel.CurrentStepIndex);
        Assert.True(viewModel.IsFirstStep);
    }

    [Fact]
    public void FinalNext_ShouldPersistAndRaiseCompleted()
    {
        var persistenceCalls = 0;
        FeatureTourCompletedEventArgs? completed = null;

        using var viewModel =
            new FeatureTourViewModel(
                Steps,
                () => persistenceCalls++);

        viewModel.Completed += (_, eventArgs) =>
        {
            completed = eventArgs;
        };

        viewModel.NextCommand.Execute(null);
        viewModel.NextCommand.Execute(null);
        viewModel.NextCommand.Execute(null);

        Assert.Equal(1, persistenceCalls);
        Assert.NotNull(completed);
        Assert.False(completed.WasSkipped);
    }

    [Fact]
    public void Skip_ShouldPersistAndRaiseSkippedCompletion()
    {
        var persistenceCalls = 0;
        FeatureTourCompletedEventArgs? completed = null;

        using var viewModel =
            new FeatureTourViewModel(
                Steps,
                () => persistenceCalls++);

        viewModel.Completed += (_, eventArgs) =>
        {
            completed = eventArgs;
        };

        viewModel.SkipCommand.Execute(null);

        Assert.Equal(1, persistenceCalls);
        Assert.NotNull(completed);
        Assert.True(completed.WasSkipped);
    }

    [Fact]
    public void CompletionFailure_ShouldExposeLocalizedError()
    {
        using var viewModel =
            new FeatureTourViewModel(
                Steps,
                () => throw new IOException(
                    "Simulated failure."));

        viewModel.SkipCommand.Execute(null);

        Assert.True(viewModel.HasError);
        Assert.Equal(
            TranslationCatalog.Translate(
                "en",
                "FeatureTourSaveError"),
            viewModel.ErrorMessage);
    }

    private static FeatureTourStep CreateStep(
        string number,
        string visualKind)
    {
        return new FeatureTourStep(
            Number: number,
            EyebrowKey: "FeatureTourWelcomeEyebrow",
            TitleKey: "FeatureTourWelcomeTitle",
            DescriptionKey: "FeatureTourWelcomeDescription",
            FirstHighlightKey: "FeatureTourWelcomeHighlightOne",
            SecondHighlightKey: "FeatureTourWelcomeHighlightTwo",
            ThirdHighlightKey: "FeatureTourWelcomeHighlightThree",
            VisualKind: visualKind);
    }
}
