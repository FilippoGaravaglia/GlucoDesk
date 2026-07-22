using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Desktop.AboutSupport.Enums;
using GlucoDesk.Desktop.AboutSupport.Models;
using GlucoDesk.Desktop.AboutSupport.Services.Abstractions;
using GlucoDesk.Desktop.Tests.Localization;
using GlucoDesk.Desktop.ViewModels.Settings;

namespace GlucoDesk.Desktop.Tests.ViewModels.Settings;

public sealed class SettingsAboutSupportViewModelTests :
    EnglishLocalizationTestBase
{
    [Fact]
    public void Constructor_ShouldExposeApplicationVersion()
    {
        // Arrange
        var service = new FakeAboutSupportService(
            version: "0.4.0-preview");

        // Act
        var viewModel = CreateViewModel(service);

        // Assert
        Assert.Equal(
            "0.4.0-preview",
            viewModel.ApplicationVersionText);

        Assert.Equal(
            "Support links are ready.",
            viewModel.AboutSupportStatusMessage);

        Assert.False(viewModel.HasAboutSupportError);
        Assert.False(viewModel.IsAboutSupportBusy);
    }

    [Fact]
    public async Task OpenWebsiteCommand_ShouldOpenWebsite()
    {
        // Arrange
        var service = new FakeAboutSupportService();
        var viewModel = CreateViewModel(service);

        // Act
        await viewModel.OpenWebsiteCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            AboutSupportLinkKind.Website,
            service.LastOpenedLinkKind);

        Assert.Equal(
            "The selected link was opened in your default browser.",
            viewModel.AboutSupportStatusMessage);

        Assert.False(viewModel.HasAboutSupportError);
    }

    [Fact]
    public async Task OpenSourceCodeCommand_ShouldOpenSourceCode()
    {
        // Arrange
        var service = new FakeAboutSupportService();
        var viewModel = CreateViewModel(service);

        // Act
        await viewModel.OpenSourceCodeCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            AboutSupportLinkKind.SourceCode,
            service.LastOpenedLinkKind);
    }
[Fact]
    public async Task ReportIssueCommand_ShouldOpenIssueWorkflow()
    {
        // Arrange
        var service = new FakeAboutSupportService();
        var viewModel = CreateViewModel(service);

        // Act
        await viewModel.ReportIssueCommand
            .ExecuteAsync(null);

        // Assert
        Assert.Equal(
            AboutSupportLinkKind.ReportIssue,
            service.LastOpenedLinkKind);
    }

    [Fact]
    public async Task OpenWebsiteCommand_ShouldExposeSafeError_WhenServiceFails()
    {
        // Arrange
        var service = new FakeAboutSupportService
        {
            OpenResult = Result.Failure(
                new Error(
                    "AboutSupport.TechnicalFailure",
                    "Technical process failure details."))
        };

        var viewModel = CreateViewModel(service);

        // Act
        await viewModel.OpenWebsiteCommand
            .ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasAboutSupportError);

        Assert.Equal(
            "The link could not be opened.",
            viewModel.AboutSupportStatusMessage);

        Assert.Equal(
            "Your default browser could not be opened. "
            + "Check the system browser settings and try again.",
            viewModel.AboutSupportErrorMessage);

        Assert.DoesNotContain(
            "Technical",
            viewModel.AboutSupportErrorMessage,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpenWebsiteCommand_ShouldExposeUnavailableState_WhenServiceIsMissing()
    {
        // Arrange
        var viewModel = new SettingsViewModel(
            new FakeApplicationSettingsService());

        // Act
        await viewModel.OpenWebsiteCommand
            .ExecuteAsync(null);

        // Assert
        Assert.True(viewModel.HasAboutSupportError);

        Assert.Equal(
            "Product support services are not available "
            + "in the current desktop runtime.",
            viewModel.AboutSupportErrorMessage);
    }

    private static SettingsViewModel CreateViewModel(
        IAboutSupportService service)
    {
        return new SettingsViewModel(
            settingsService:
                new FakeApplicationSettingsService(),
            aboutSupportService:
                service);
    }

    private sealed class FakeAboutSupportService :
        IAboutSupportService
    {
        private readonly string _version;

        public FakeAboutSupportService(
            string version = "1.0.0")
        {
            _version = version;
        }

        public AboutSupportLinkKind? LastOpenedLinkKind
        {
            get;
            private set;
        }

        public Result OpenResult
        {
            get;
            init;
        } = Result.Success();

        public AboutSupportInformation GetInformation()
        {
            return new AboutSupportInformation(
                _version,
                new Uri("https://glucodesk.com/"),
                new Uri(
                    "https://github.com/"
                    + "FilippoGaravaglia/GlucoDesk"),
new Uri(
                    "https://github.com/"
                    + "FilippoGaravaglia/"
                    + "GlucoDesk/issues/new/choose"));
        }

        public Task<Result> OpenAsync(
            AboutSupportLinkKind linkKind,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            LastOpenedLinkKind = linkKind;

            return Task.FromResult(OpenResult);
        }
    }

    private sealed class FakeApplicationSettingsService :
        IApplicationSettingsService
    {
        public Task<Result<ApplicationSettings>>
            GetSettingsAsync(
                CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                Result<ApplicationSettings>.Success(
                    ApplicationSettings.Default));
        }

        public Task<Result> SaveSettingsAsync(
            ApplicationSettings settings,
            CancellationToken cancellationToken)
        {
            ArgumentNullException.ThrowIfNull(settings);
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                Result.Success());
        }
    }
}
