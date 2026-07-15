using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.ViewModels.Onboarding;

/// <summary>
/// Represents the first-launch language selection experience.
/// </summary>
public sealed class LanguageOnboardingViewModel : ObservableObject
{
    private readonly Action<string> _previewLanguage;
    private readonly Action<string> _persistLanguage;

    private LanguageOnboardingOptionViewModel _selectedOption;
    private bool _hasError;
    private string _errorMessage = string.Empty;

    /// <summary>
    /// Initializes the production onboarding ViewModel.
    /// </summary>
    public LanguageOnboardingViewModel()
        : this(
            LocalizationManager.AvailableLanguages,
            CultureInfo.CurrentUICulture.Name,
            LocalizationManager.SetLanguageForCurrentProcess,
            LocalizationManager.SetLanguage)
    {
    }

    /// <summary>
    /// Initializes a testable onboarding ViewModel.
    /// </summary>
    public LanguageOnboardingViewModel(
        IReadOnlyList<AppLanguageOption> languages,
        string? suggestedLanguageCode,
        Action<string> previewLanguage,
        Action<string> persistLanguage)
    {
        ArgumentNullException.ThrowIfNull(languages);
        ArgumentNullException.ThrowIfNull(previewLanguage);
        ArgumentNullException.ThrowIfNull(persistLanguage);

        if (languages.Count == 0)
        {
            throw new ArgumentException(
                "At least one supported language is required.",
                nameof(languages));
        }

        var duplicateCode = languages
            .GroupBy(
                language => language.Code,
                StringComparer.OrdinalIgnoreCase)
            .FirstOrDefault(group => group.Count() > 1);

        if (duplicateCode is not null)
        {
            throw new ArgumentException(
                $"Duplicate language code '{duplicateCode.Key}'.",
                nameof(languages));
        }

        _previewLanguage = previewLanguage;
        _persistLanguage = persistLanguage;

        var options = languages
            .Select(language =>
                new LanguageOnboardingOptionViewModel(
                    language,
                    SelectOption))
            .ToArray();

        LanguageOptions = options;

        var normalizedSuggestion =
            TranslationCatalog.NormalizeLanguageCode(
                suggestedLanguageCode);

        _selectedOption =
            options.FirstOrDefault(option =>
                string.Equals(
                    option.Code,
                    normalizedSuggestion,
                    StringComparison.OrdinalIgnoreCase))
            ?? options[0];

        _selectedOption.IsSelected = true;

        ContinueCommand = new RelayCommand(
            CompleteSelection);

        _previewLanguage(_selectedOption.Code);
    }

    public event EventHandler<LanguageOnboardingCompletedEventArgs>?
        Completed;

    public IReadOnlyList<LanguageOnboardingOptionViewModel>
        LanguageOptions { get; }

    public LanguageOnboardingOptionViewModel SelectedOption
    {
        get => _selectedOption;
        private set
        {
            if (SetProperty(
                    ref _selectedOption,
                    value))
            {
                OnPropertyChanged(nameof(SelectedLanguageName));
            }
        }
    }

    public string SelectedLanguageName =>
        SelectedOption.NativeName;

    public bool HasError
    {
        get => _hasError;
        private set => SetProperty(
            ref _hasError,
            value);
    }

    public string ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(
            ref _errorMessage,
            value);
    }

    public IRelayCommand ContinueCommand { get; }

    private void SelectOption(
        LanguageOnboardingOptionViewModel option)
    {
        ArgumentNullException.ThrowIfNull(option);

        if (ReferenceEquals(
                SelectedOption,
                option))
        {
            return;
        }

        foreach (var languageOption in LanguageOptions)
        {
            languageOption.IsSelected =
                ReferenceEquals(languageOption, option);
        }

        SelectedOption = option;
        HasError = false;
        ErrorMessage = string.Empty;

        _previewLanguage(option.Code);
    }

    private void CompleteSelection()
    {
        try
        {
            _persistLanguage(SelectedOption.Code);

            HasError = false;
            ErrorMessage = string.Empty;

            Completed?.Invoke(
                this,
                new LanguageOnboardingCompletedEventArgs(
                    SelectedOption.Code));
        }
        catch
        {
            HasError = true;
            ErrorMessage = TranslationCatalog.Translate(
                SelectedOption.Code,
                "OnboardingSaveError");
        }
    }
}
