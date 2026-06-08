using Avalonia.Controls;
using Avalonia.Interactivity;
using GlucoDesk.Desktop.ViewModels.Settings;

namespace GlucoDesk.Desktop.Views.Settings;

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
}