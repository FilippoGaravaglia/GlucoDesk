using GlucoDesk.Application.Cgm.BackgroundSync.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.BackgroundSync.Options;
using GlucoDesk.Desktop.BackgroundSync.Services;

namespace GlucoDesk.Desktop.Tests.BackgroundSync.Services;

public sealed class DesktopBackgroundSyncLifecycleServiceTests
{
    [Fact]
    public async Task StartAsync_ShouldStartLoop_WhenStartupIsEnabled()
    {
        // Arrange
        var loopService = new FakeBackgroundSyncLoopService();

        var lifecycleService = new DesktopBackgroundSyncLifecycleService(
            loopService,
            DesktopBackgroundSyncLifecycleOptions.Default);

        // Act
        var result = await lifecycleService.StartAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(loopService.StartWasCalled);
        Assert.True(lifecycleService.IsRunning);
    }

    [Fact]
    public async Task StartAsync_ShouldNotStartLoop_WhenStartupIsDisabled()
    {
        // Arrange
        var loopService = new FakeBackgroundSyncLoopService();

        var lifecycleService = new DesktopBackgroundSyncLifecycleService(
            loopService,
            new DesktopBackgroundSyncLifecycleOptions(
                startOnApplicationStartup: false,
                TimeSpan.FromSeconds(5)));

        // Act
        var result = await lifecycleService.StartAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(loopService.StartWasCalled);
        Assert.False(lifecycleService.IsRunning);
    }

    [Fact]
    public async Task StopAsync_ShouldStopLoop()
    {
        // Arrange
        var loopService = new FakeBackgroundSyncLoopService();

        var lifecycleService = new DesktopBackgroundSyncLifecycleService(
            loopService,
            DesktopBackgroundSyncLifecycleOptions.Default);

        await lifecycleService.StartAsync(CancellationToken.None);

        // Act
        var result = await lifecycleService.StopAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(loopService.StopWasCalled);
        Assert.False(lifecycleService.IsRunning);
    }

    [Fact]
    public async Task StartAsync_ShouldReturnFailure_WhenLoopStartFails()
    {
        // Arrange
        var lifecycleService = new DesktopBackgroundSyncLifecycleService(
            new FailingStartBackgroundSyncLoopService(),
            DesktopBackgroundSyncLifecycleOptions.Default);

        // Act
        var result = await lifecycleService.StartAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("BackgroundSync.StartFailed", result.Error.Code);
    }

    [Fact]
    public async Task StopAsync_ShouldReturnFailure_WhenLoopStopFails()
    {
        // Arrange
        var lifecycleService = new DesktopBackgroundSyncLifecycleService(
            new FailingStopBackgroundSyncLoopService(),
            DesktopBackgroundSyncLifecycleOptions.Default);

        // Act
        var result = await lifecycleService.StopAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsFailure);
        Assert.Equal("BackgroundSync.StopFailed", result.Error.Code);
    }

    private sealed class FakeBackgroundSyncLoopService : IBackgroundSyncLoopService
    {
        public bool StartWasCalled { get; private set; }

        public bool StopWasCalled { get; private set; }

        public bool IsRunning { get; private set; }

        public Task<Result> StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            StartWasCalled = true;
            IsRunning = true;

            return Task.FromResult(Result.Success());
        }

        public Task<Result> StopAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            StopWasCalled = true;
            IsRunning = false;

            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FailingStartBackgroundSyncLoopService : IBackgroundSyncLoopService
    {
        public bool IsRunning => false;

        public Task<Result> StartAsync(CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Loop start failed.");
        }

        public Task<Result> StopAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }
    }

    private sealed class FailingStopBackgroundSyncLoopService : IBackgroundSyncLoopService
    {
        public bool IsRunning => true;

        public Task<Result> StartAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult(Result.Success());
        }

        public Task<Result> StopAsync(CancellationToken cancellationToken)
        {
            throw new InvalidOperationException("Loop stop failed.");
        }
    }
}