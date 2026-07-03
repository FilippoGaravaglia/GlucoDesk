using Avalonia.Controls;
using GlucoDesk.Desktop.ViewModels.Main;

namespace GlucoDesk.Desktop.Views.Main;

public partial class MainWindow : Window
{
    private const double MinimumReleaseWidth = 1180;
    private const double MinimumReleaseHeight = 760;


    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    public MainWindow()
    {
        InitializeComponent();

        MinWidth = MinimumReleaseWidth;
        MinHeight = MinimumReleaseHeight;

        if (Width < MinimumReleaseWidth)
        {
            Width = MinimumReleaseWidth;
        }

        if (Height < MinimumReleaseHeight)
        {
            Height = MinimumReleaseHeight;
        }
}

    /// <summary>
    /// Initializes a new instance of the <see cref="MainWindow"/> class.
    /// </summary>
    /// <param name="viewModel">The main window view model.</param>
    public MainWindow(MainWindowViewModel viewModel)
        : this()
    {
        ArgumentNullException.ThrowIfNull(viewModel);

        DataContext = viewModel;
    }
}
