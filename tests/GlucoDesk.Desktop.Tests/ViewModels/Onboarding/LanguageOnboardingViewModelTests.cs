using GlucoDesk.Desktop.Localization;
using GlucoDesk.Desktop.ViewModels.Onboarding;

namespace GlucoDesk.Desktop.Tests.ViewModels.Onboarding;

public sealed class LanguageOnboardingViewModelTests
{
    private static readonly IReadOnlyList<AppLanguageOption>
        Languages =
        [
            new AppLanguageOption(
                "en",
                "English",
                "English"),

            new AppLanguageOption(
                "it",
                "Italian",
                "Italiano")
        ];

    [Fact]
    public void Constructor_ShouldExposeEverySupportedLanguage()
    {
        var previewedLanguages = new List<string>();

        var viewModel = CreateViewModel(
            suggestedLanguageCode: "en",
            previewLanguage: previewedLanguages.Add);

        Assert.Equal(
            Languages.Count,
            viewModel.LanguageOptions.Count);

        Assert.Equal(
            ["en", "it"],
            viewModel.LanguageOptions
                .Select(option => option.Code)
                .ToArray());

        Assert.Equal("en", viewModel.SelectedOption.Code);
        Assert.Equal(["en"], previewedLanguages);
    }

    [Fact]
    public void Constructor_ShouldSuggestMatchingOperatingSystemLanguage()
    {
        var previewedLanguages = new List<string>();

        var viewModel = CreateViewModel(
            suggestedLanguageCode: "it-IT",
            previewLanguage: previewedLanguages.Add);

        Assert.Equal("it", viewModel.SelectedOption.Code);
        Assert.Equal("Italiano", viewModel.SelectedLanguageName);
        Assert.Equal(["it"], previewedLanguages);
    }

    [Fact]
    public void SelectingLanguage_ShouldPreviewWithoutPersisting()
    {
        var previewedLanguages = new List<string>();
        var persistedLanguages = new List<string>();

        var viewModel = CreateViewModel(
            suggestedLanguageCode: "en",
            previewLanguage: previewedLanguages.Add,
            persistLanguage: persistedLanguages.Add);

        var italianOption = viewModel.LanguageOptions.Single(
            option => option.Code == "it");

        italianOption.SelectCommand.Execute(null);

        Assert.Equal("it", viewModel.SelectedOption.Code);
        Assert.True(italianOption.IsSelected);
        Assert.False(
            viewModel.LanguageOptions.Single(
                option => option.Code == "en")
            .IsSelected);

        Assert.Equal(
            ["en", "it"],
            previewedLanguages);

        Assert.Empty(persistedLanguages);
    }

    [Fact]
    public void Continue_ShouldPersistAndRaiseCompletedEvent()
    {
        var persistedLanguages = new List<string>();
        string? completedLanguage = null;

        var viewModel = CreateViewModel(
            suggestedLanguageCode: "it",
            persistLanguage: persistedLanguages.Add);

        viewModel.Completed += (_, eventArgs) =>
        {
            completedLanguage = eventArgs.LanguageCode;
        };

        viewModel.ContinueCommand.Execute(null);

        Assert.Equal(["it"], persistedLanguages);
        Assert.Equal("it", completedLanguage);
        Assert.False(viewModel.HasError);
    }

    [Fact]
    public void Continue_ShouldExposeLocalizedError_WhenPersistenceFails()
    {
        var completedCount = 0;

        var viewModel = CreateViewModel(
            suggestedLanguageCode: "it",
            persistLanguage: _ =>
                throw new IOException(
                    "Simulated persistence failure."));

        viewModel.Completed += (_, _) =>
        {
            completedCount++;
        };

        viewModel.ContinueCommand.Execute(null);

        Assert.True(viewModel.HasError);
        Assert.Equal(
            TranslationCatalog.Translate(
                "it",
                "OnboardingSaveError"),
            viewModel.ErrorMessage);

        Assert.Equal(0, completedCount);
    }

    private static LanguageOnboardingViewModel CreateViewModel(
        string suggestedLanguageCode,
        Action<string>? previewLanguage = null,
        Action<string>? persistLanguage = null)
    {
        return new LanguageOnboardingViewModel(
            Languages,
            suggestedLanguageCode,
            previewLanguage ?? (_ => { }),
            persistLanguage ?? (_ => { }));
    }
}
