using Avalonia.Controls;
using Avalonia.Interactivity;
using GlucoDesk.Desktop.ViewModels.Dashboard;

namespace GlucoDesk.Desktop.Views.Dashboard;

public partial class DashboardView : UserControl
{
    private bool _hasLoaded;

    /// <summary>
    /// Initializes a new instance of the <see cref="DashboardView"/> class.
    /// </summary>
    public DashboardView()
    {
        InitializeComponent();
    }

    protected override async void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        if (_hasLoaded)
        {
            return;
        }

        _hasLoaded = true;

        if (DataContext is DashboardViewModel viewModel)
        {
            await viewModel.RefreshCommand.ExecuteAsync(null);
        }
    }
}