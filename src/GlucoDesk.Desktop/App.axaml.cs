using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using GlucoDesk.Desktop.Bootstrap;
using GlucoDesk.Desktop.Views.Main;
using Microsoft.Extensions.DependencyInjection;

namespace GlucoDesk.Desktop;

public partial class App : Avalonia.Application
{
    private ServiceProvider? _serviceProvider;
    private IServiceScope? _applicationScope;

    /// <inheritdoc />
    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    /// <inheritdoc />
    public override void OnFrameworkInitializationCompleted()
    {
        _serviceProvider = DesktopServiceProviderBuilder.BuildServiceProvider();
        _applicationScope = _serviceProvider.CreateScope();

        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            desktop.MainWindow = _applicationScope
                .ServiceProvider
                .GetRequiredService<MainWindow>();

            desktop.Exit += (_, _) => DisposeServices();
        }

        base.OnFrameworkInitializationCompleted();
    }

    #region Helpers

    /// <summary>
    /// Disposes application-level dependency injection services.
    /// </summary>
    private void DisposeServices()
    {
        _applicationScope?.Dispose();
        _serviceProvider?.Dispose();

        _applicationScope = null;
        _serviceProvider = null;
    }

    #endregion
}