using Avalonia;
using Avalonia.Controls;
using Avalonia.Threading;

namespace GlucoDesk.Desktop.Views.Settings;

public partial class SettingsView : UserControl
{
    private const double CompactLayoutBreakpoint = 1240;
    private const int MaximumTargetValueLength = 3;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsView"/> class.
    /// </summary>
    public SettingsView()
    {
        InitializeComponent();

        AttachedToVisualTree += OnAttachedToVisualTree;
        SizeChanged += OnSizeChanged;
    }

    #region Helpers

    /// <summary>
    /// Applies the initial responsive state when the view is attached.
    /// </summary>
    private void OnAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        UpdateResponsiveState(Bounds.Width);
    }

    /// <summary>
    /// Updates the responsive state when the view size changes.
    /// </summary>
    private void OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        UpdateResponsiveState(e.NewSize.Width);
    }

    /// <summary>
    /// Updates all adaptive Settings sections according to the available width.
    /// </summary>
    /// <param name="width">The current view width.</param>
    private void UpdateResponsiveState(double width)
    {
        var isCompact = width < CompactLayoutBreakpoint;

        UpdateSectionLayout(
            ProvidersSectionGrid,
            ProvidersRightPanel,
            isCompact);

        UpdateSectionLayout(
            PreferencesSectionGrid,
            PreferencesRightPanel,
            isCompact);

        UpdateSectionLayout(
            NotificationsSectionGrid,
            NotificationsRightPanel,
            isCompact);
    }

    /// <summary>
    /// Updates a two-column section to either wide or compact layout.
    /// </summary>
    /// <param name="sectionGrid">The section grid.</param>
    /// <param name="rightPanel">The right-side panel.</param>
    /// <param name="isCompact">Whether compact layout should be used.</param>
    private static void UpdateSectionLayout(
        Grid sectionGrid,
        Border rightPanel,
        bool isCompact)
    {
        if (isCompact)
        {
            sectionGrid.ColumnDefinitions = new ColumnDefinitions("*");
            sectionGrid.RowDefinitions = new RowDefinitions("Auto,Auto");

            Grid.SetColumn(rightPanel, 0);
            Grid.SetRow(rightPanel, 1);

            rightPanel.BorderThickness = new Thickness(0, 1, 0, 0);
            rightPanel.Padding = new Thickness(0, 24, 0, 0);

            return;
        }

        sectionGrid.ColumnDefinitions = new ColumnDefinitions("1.15*,0.85*");
        sectionGrid.RowDefinitions = new RowDefinitions("Auto");

        Grid.SetColumn(rightPanel, 1);
        Grid.SetRow(rightPanel, 0);

        rightPanel.BorderThickness = new Thickness(1, 0, 0, 0);
        rightPanel.Padding = new Thickness(28, 0, 0, 0);
    }

    /// <summary>
    /// Keeps target glucose inputs valid for the currently selected glucose unit while preserving the existing view-model binding.
    /// </summary>
    /// <param name="sender">The text box sender.</param>
    /// <param name="e">The text changed event args.</param>
    private void OnTargetValueTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox ||
            string.IsNullOrEmpty(textBox.Text) ||
            DataContext is not GlucoDesk.Desktop.ViewModels.Settings.SettingsViewModel viewModel)
        {
            return;
        }

        var originalText = textBox.Text;
        var sanitizedText = viewModel.SanitizeTargetValueInput(originalText);

        if (string.Equals(originalText, sanitizedText, StringComparison.Ordinal))
        {
            return;
        }

        var caretIndex = textBox.CaretIndex;
        var removedCharacters = originalText.Length - sanitizedText.Length;

        textBox.Text = sanitizedText;
        textBox.CaretIndex = Math.Clamp(
            caretIndex - Math.Max(removedCharacters, 0),
            0,
            sanitizedText.Length);
    }

    #endregion
    private void OnPositiveIntegerSettingsTextChanged(
        object? sender,
        Avalonia.Controls.TextChangedEventArgs e)
    {
        _ = e;

        if (sender is not Avalonia.Controls.TextBox textBox ||
            DataContext is not GlucoDesk.Desktop.ViewModels.Settings.SettingsViewModel viewModel)
        {
            return;
        }

        var sanitizedText = SanitizePositiveIntegerText(textBox.Text);

        if (!string.Equals(textBox.Text, sanitizedText, System.StringComparison.Ordinal))
        {
            var caretIndex = textBox.CaretIndex;
            textBox.Text = sanitizedText;
            textBox.CaretIndex = System.Math.Min(caretIndex, sanitizedText.Length);
            return;
        }

        switch (textBox.Name)
        {
            case "DashboardRefreshIntervalSecondsTextBox":
                viewModel.DashboardRefreshIntervalSecondsText = sanitizedText;
                break;

            case "GlucoseAlertRepeatIntervalMinutesTextBox":
                viewModel.GlucoseAlertRepeatIntervalMinutesText = sanitizedText;
                break;

            case "GlucoseAlertRequiredConsecutiveReadingsTextBox":
                viewModel.GlucoseAlertRequiredConsecutiveReadingsText = sanitizedText;
                break;
        }
    }

    private static string SanitizePositiveIntegerText(string? text)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return string.Empty;
        }

        var builder = new System.Text.StringBuilder();

        foreach (var character in text)
        {
            if (char.IsDigit(character))
            {
                builder.Append(character);
            }
        }

        return builder.ToString();
    }

    protected override void OnAttachedToVisualTree(
        Avalonia.VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);

        Dispatcher.UIThread.Post(LoadPersistedSettingsWhenAttached);
    }

    private async void LoadPersistedSettingsWhenAttached()
    {
        if (DataContext is not GlucoDesk.Desktop.ViewModels.Settings.SettingsViewModel viewModel)
        {
            return;
        }

        if (!viewModel.LoadCommand.CanExecute(null))
        {
            return;
        }

        await viewModel.LoadCommand.ExecuteAsync(null);
    }

}
