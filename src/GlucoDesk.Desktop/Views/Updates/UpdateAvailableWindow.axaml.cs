using Avalonia.Controls;
using Avalonia.Interactivity;
using GlucoDesk.Desktop.ViewModels.Updates;

namespace GlucoDesk.Desktop.Views.Updates;

/// <summary>
/// Displays the update notification shown when a newer compatible
/// GlucoDesk release is available.
/// </summary>
public partial class UpdateAvailableWindow : Window
{
    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="UpdateAvailableWindow"/> class.
    /// </summary>
    public UpdateAvailableWindow()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Initializes a new instance of the
    /// <see cref="UpdateAvailableWindow"/> class.
    /// </summary>
    /// <param name="viewModel">
    /// The shared update center view model.
    /// </param>
    public UpdateAvailableWindow(
        UpdateCenterViewModel viewModel)
        : this()
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        DataContext = viewModel;
    }

    #region Helpers

    /// <summary>
    /// Dismisses the current release notification and closes the dialog.
    /// </summary>
    private async void OnLaterClicked(
        object? sender,
        RoutedEventArgs eventArgs)
    {
        _ = sender;
        _ = eventArgs;

        var viewModel = GetViewModel();

        if (!viewModel.DismissCommand.CanExecute(null))
        {
            Close();
            return;
        }

        await viewModel.DismissCommand.ExecuteAsync(null);

        Close();
    }

    /// <summary>
    /// Starts downloading the current release and closes the dialog.
    /// </summary>
    private void OnDownloadClicked(
        object? sender,
        RoutedEventArgs eventArgs)
    {
        _ = sender;
        _ = eventArgs;

        var viewModel = GetViewModel();

        if (viewModel.DownloadUpdateCommand.CanExecute(null))
        {
            viewModel.DownloadUpdateCommand.Execute(null);
        }

        Close();
    }

    /// <summary>
    /// Gets the update center view model assigned to the dialog.
    /// </summary>
    private UpdateCenterViewModel GetViewModel()
    {
        return DataContext as UpdateCenterViewModel
               ?? throw new InvalidOperationException(
                   "The update notification window requires "
                   + "an UpdateCenterViewModel data context.");
    }

    #endregion
}
