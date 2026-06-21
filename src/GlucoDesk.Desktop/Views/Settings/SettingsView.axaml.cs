using Avalonia.Controls;
using Avalonia.Interactivity;
using GlucoDesk.Desktop.ViewModels.Settings;

namespace GlucoDesk.Desktop.Views.Settings;

/// <summary>
/// Interaction logic for the settings view.
/// </summary>
public partial class SettingsView : UserControl
{
    private bool _hasLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsView"/> class.
    /// </summary>
    public SettingsView()
    {
        InitializeComponent();
    }

    /// <inheritdoc />
    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (_hasLoaded)
        {
            return;
        }

        _hasLoaded = true;

        if (DataContext is SettingsViewModel viewModel)
        {
            await viewModel.LoadCommand.ExecuteAsync(null);
        }
    }

    #region Helpers

    /// <summary>
    /// Sanitizes target value text boxes while the user is typing.
    /// </summary>
    /// <param name="sender">The event sender.</param>
    /// <param name="e">The text changed event arguments.</param>
    private void OnTargetValueTextChanged(
        object? sender,
        TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox ||
            DataContext is not SettingsViewModel viewModel)
        {
            return;
        }

        var originalText = textBox.Text ?? string.Empty;
        var sanitizedText = viewModel.SanitizeTargetValueInput(originalText);

        if (sanitizedText == originalText)
        {
            return;
        }

        var originalCaretIndex = textBox.CaretIndex;
        var removedCharactersCount = originalText.Length - sanitizedText.Length;

        textBox.Text = sanitizedText;
        textBox.CaretIndex = Math.Clamp(
            originalCaretIndex - removedCharactersCount,
            0,
            sanitizedText.Length);
    }

    #endregion
}