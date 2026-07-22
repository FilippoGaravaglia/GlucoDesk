using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.AboutSupport.Enums;
using GlucoDesk.Desktop.AboutSupport.Services;
using GlucoDesk.Desktop.AboutSupport.Services.Abstractions;

namespace GlucoDesk.Desktop.Tests.AboutSupport.Services;

public sealed class AboutSupportServiceTests
{
    [Fact]
    public void GetInformation_ShouldExposeCurrentProductDestinations()
    {
        // Arrange
        var service = new AboutSupportService(
            new FakeApplicationVersionProvider("0.3.0-preview"),
            new RecordingExternalUriLauncher());

        // Act
        var information = service.GetInformation();

        // Assert
        Assert.Equal(
            "0.3.0-preview",
            information.Version);

        Assert.Equal(
            new Uri("https://glucodesk.com/"),
            information.WebsiteUri);

        Assert.Equal(
            new Uri(
                "https://github.com/"
                + "FilippoGaravaglia/GlucoDesk"),
            information.SourceCodeUri);
Assert.Equal(
            new Uri(
                "https://github.com/"
                + "FilippoGaravaglia/"
                + "GlucoDesk/issues/new/choose"),
            information.ReportIssueUri);
    }

    [Theory]
    [InlineData(
        AboutSupportLinkKind.Website,
        "https://glucodesk.com/")]
    [InlineData(
        AboutSupportLinkKind.SourceCode,
        "https://github.com/FilippoGaravaglia/GlucoDesk")]
[InlineData(
        AboutSupportLinkKind.ReportIssue,
        "https://github.com/FilippoGaravaglia/"
        + "GlucoDesk/issues/new/choose")]
    public async Task OpenAsync_ShouldOpenMappedDestination(
        AboutSupportLinkKind linkKind,
        string expectedUri)
    {
        // Arrange
        var launcher = new RecordingExternalUriLauncher();

        var service = new AboutSupportService(
            new FakeApplicationVersionProvider("1.0.0"),
            launcher);

        // Act
        var result = await service.OpenAsync(
            linkKind,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.Equal(
            new Uri(expectedUri),
            launcher.LastOpenedUri);
    }

    [Fact]
    public async Task OpenAsync_ShouldReturnFailure_WhenLinkIsUnsupported()
    {
        // Arrange
        var launcher = new RecordingExternalUriLauncher();

        var service = new AboutSupportService(
            new FakeApplicationVersionProvider("1.0.0"),
            launcher);

        // Act
        var result = await service.OpenAsync(
            (AboutSupportLinkKind)999,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            "AboutSupport.UnsupportedLink",
            result.Error.Code);

        Assert.Null(launcher.LastOpenedUri);
    }

    private sealed class FakeApplicationVersionProvider :
        IApplicationVersionProvider
    {
        private readonly string _version;

        public FakeApplicationVersionProvider(
            string version)
        {
            _version = version;
        }

        public string GetVersion()
        {
            return _version;
        }
    }

    private sealed class RecordingExternalUriLauncher :
        IExternalUriLauncher
    {
        public Uri? LastOpenedUri
        {
            get;
            private set;
        }

        public Task<Result> OpenAsync(
            Uri uri,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            LastOpenedUri = uri;

            return Task.FromResult(
                Result.Success());
        }
    }
}
