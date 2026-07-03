using Avalonia;
using Avalonia.Controls;

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
    /// Keeps target glucose inputs numeric while preserving the existing view-model binding.
    /// </summary>
    /// <param name="sender">The text box sender.</param>
    /// <param name="e">The text changed event args.</param>
    private void OnTargetValueTextChanged(object? sender, TextChangedEventArgs e)
    {
        if (sender is not TextBox textBox || string.IsNullOrEmpty(textBox.Text))
        {
            return;
        }

        var originalText = textBox.Text;
        var sanitizedText = new string(
            originalText
                .Where(char.IsDigit)
                .Take(MaximumTargetValueLength)
                .ToArray());

        if (string.Equals(originalText, sanitizedText, StringComparison.Ordinal))
        {
            return;
        }

        var caretIndex = textBox.CaretIndex;

        textBox.Text = sanitizedText;
        textBox.CaretIndex = Math.Clamp(
            caretIndex - 1,
            0,
            sanitizedText.Length);
    }

    #endregion
}
