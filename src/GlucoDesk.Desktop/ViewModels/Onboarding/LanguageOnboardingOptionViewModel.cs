using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.ViewModels.Onboarding;

/// <summary>
/// Represents a selectable language card in the first-launch experience.
/// </summary>
public sealed class LanguageOnboardingOptionViewModel : ObservableObject
{
    private bool _isSelected;

    /// <summary>
    /// Initializes a language option.
    /// </summary>
    /// <param name="language">The supported language metadata.</param>
    /// <param name="select">The selection callback.</param>
    public LanguageOnboardingOptionViewModel(
        AppLanguageOption language,
        Action<LanguageOnboardingOptionViewModel> select)
    {
        Language = language
            ?? throw new ArgumentNullException(nameof(language));

        ArgumentNullException.ThrowIfNull(select);

        SelectCommand = new RelayCommand(
            () => select(this));
    }

    public AppLanguageOption Language { get; }

    public string Code => Language.Code;

    public string ShortCode =>
        Language.Code
            .Split(
                '-',
                StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)[0]
            .ToUpperInvariant();

    public string NativeName => Language.NativeName;

    public string DisplayName => Language.DisplayName;

    public string AccessibleName =>
        string.Equals(
            NativeName,
            DisplayName,
            StringComparison.OrdinalIgnoreCase)
            ? NativeName
            : $"{NativeName} ({DisplayName})";

    public bool IsSelected
    {
        get => _isSelected;
        internal set => SetProperty(
            ref _isSelected,
            value);
    }

    public IRelayCommand SelectCommand { get; }
}
