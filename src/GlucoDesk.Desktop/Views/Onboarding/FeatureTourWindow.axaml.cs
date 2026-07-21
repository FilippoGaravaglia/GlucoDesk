using Avalonia.Controls;
using GlucoDesk.Desktop.ViewModels.Onboarding;

namespace GlucoDesk.Desktop.Views.Onboarding;

/// <summary>
/// Displays the first-run GlucoDesk product tour.
/// </summary>
public partial class FeatureTourWindow : Window
{
    /// <summary>
    /// Initializes the XAML designer instance.
    /// </summary>
    public FeatureTourWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes the production feature tour window.
    /// </summary>
    public FeatureTourWindow(
        FeatureTourViewModel viewModel)
        : this()
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        DataContext = viewModel;
    }
}
