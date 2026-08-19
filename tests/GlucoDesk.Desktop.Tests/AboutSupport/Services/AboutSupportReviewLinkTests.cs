using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.AboutSupport.Enums;
using GlucoDesk.Desktop.AboutSupport.Services;
using GlucoDesk.Desktop.AboutSupport.Services.Abstractions;

namespace GlucoDesk.Desktop.Tests.AboutSupport.Services;

public sealed class AboutSupportReviewLinkTests
{
    [Fact]
    public async Task OpenAsync_ShouldOpenSecureTallyReviewForm()
    {
        // Arrange
        var launcher = new RecordingExternalUriLauncher();

        var service = new AboutSupportService(
            new FakeApplicationVersionProvider("1.0.0"),
            launcher);

        // Act
        var result = await service.OpenAsync(
            AboutSupportLinkKind.LeaveReview,
            CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.NotNull(launcher.LastOpenedUri);

        Assert.Equal(
            Uri.UriSchemeHttps,
            launcher.LastOpenedUri.Scheme);

        Assert.Equal(
            "tally.so",
            launcher.LastOpenedUri.Host);

        Assert.StartsWith(
            "/r/",
            launcher.LastOpenedUri.AbsolutePath,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "REPLACE_ME",
            launcher.LastOpenedUri.AbsoluteUri,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public async Task OpenAsync_ShouldForwardCancellation_ForReviewLink()
    {
        // Arrange
        var launcher = new RecordingExternalUriLauncher();

        var service = new AboutSupportService(
            new FakeApplicationVersionProvider("1.0.0"),
            launcher);

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        // Act + Assert
        await Assert.ThrowsAsync<OperationCanceledException>(
            () => service.OpenAsync(
                AboutSupportLinkKind.LeaveReview,
                cancellationTokenSource.Token));

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
