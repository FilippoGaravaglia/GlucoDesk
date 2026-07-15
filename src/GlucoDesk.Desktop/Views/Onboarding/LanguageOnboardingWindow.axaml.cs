using Avalonia.Controls;
using GlucoDesk.Desktop.ViewModels.Onboarding;

namespace GlucoDesk.Desktop.Views.Onboarding;

/// <summary>
/// Displays the first-launch language selection experience.
/// </summary>
public partial class LanguageOnboardingWindow : Window
{
    public LanguageOnboardingWindow()
    {
        InitializeComponent();
    }

    public LanguageOnboardingWindow(
        LanguageOnboardingViewModel viewModel)
        : this()
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        DataContext = viewModel;
    }
}
