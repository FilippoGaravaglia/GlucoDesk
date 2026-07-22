using GlucoDesk.Desktop.AboutSupport.Services;

namespace GlucoDesk.Desktop.Tests.AboutSupport.Services;

public sealed class OperatingSystemExternalUriLauncherTests
{
    [Theory]
    [InlineData("http://glucodesk.com/")]
    [InlineData("file:///tmp/glucodesk")]
    [InlineData("ftp://example.com/glucodesk")]
    public async Task OpenAsync_ShouldRejectNonHttpsUri(
        string uriText)
    {
        // Arrange
        var launcher =
            new OperatingSystemExternalUriLauncher();

        // Act
        var result = await launcher.OpenAsync(
            new Uri(uriText),
            CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal(
            "AboutSupport.InvalidUri",
            result.Error.Code);
    }

    [Fact]
    public async Task OpenAsync_ShouldHonorCancellation()
    {
        // Arrange
        var launcher =
            new OperatingSystemExternalUriLauncher();

        using var cancellationTokenSource =
            new CancellationTokenSource();

        cancellationTokenSource.Cancel();

        // Act
        var exception = await Record.ExceptionAsync(
            () => launcher.OpenAsync(
                new Uri("https://glucodesk.com/"),
                cancellationTokenSource.Token));

        // Assert
        Assert.IsType<OperationCanceledException>(
            exception);
    }
}
