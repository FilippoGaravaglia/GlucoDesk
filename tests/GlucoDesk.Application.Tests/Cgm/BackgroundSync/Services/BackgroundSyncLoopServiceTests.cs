using GlucoDesk.Application.Cgm.BackgroundSync.Options;
using GlucoDesk.Application.Cgm.BackgroundSync.Results;
using GlucoDesk.Application.Cgm.BackgroundSync.Services;
using GlucoDesk.Application.Cgm.BackgroundSync.Services.Abstractions;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Tests.Cgm.BackgroundSync.Services;

public sealed class BackgroundSyncLoopServiceTests
{
    [Fact]
    public async Task StartAsync_ShouldStartLoop()
    {
        // Arrange
        await using var loopService = new BackgroundSyncLoopService(
            new FakeCgmBackgroundSyncService(),
            new BackgroundSyncOptions(
                TimeSpan.FromMilliseconds(50),
                true,
                true));

        // Act
        var result = await loopService.StartAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(loopService.IsRunning);
    }

    [Fact]
    public async Task StopAsync_ShouldStopLoop()
    {
        // Arrange
        await using var loopService = new BackgroundSyncLoopService(
            new FakeCgmBackgroundSyncService(),
            new BackgroundSyncOptions(
                TimeSpan.FromMilliseconds(50),
                true,
                true));

        await loopService.StartAsync(CancellationToken.None);

        // Act
        var result = await loopService.StopAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(loopService.IsRunning);
    }

    [Fact]
    public async Task StartAsync_ShouldBeIdempotent_WhenLoopIsAlreadyRunning()
    {
        // Arrange
        var syncService = new FakeCgmBackgroundSyncService();

        await using var loopService = new BackgroundSyncLoopService(
            syncService,
            new BackgroundSyncOptions(
                TimeSpan.FromMilliseconds(50),
                true,
                true));

        // Act
        var firstStartResult = await loopService.StartAsync(CancellationToken.None);
        var secondStartResult = await loopService.StartAsync(CancellationToken.None);

        await Task.Delay(120);

        // Assert
        Assert.True(firstStartResult.IsSuccess);
        Assert.True(secondStartResult.IsSuccess);
        Assert.True(loopService.IsRunning);
        Assert.True(syncService.RunCount >= 1);
    }

    [Fact]
    public async Task Loop_ShouldContinue_WhenSingleIterationFails()
    {
        // Arrange
        var syncService = new FailingOnceCgmBackgroundSyncService();

        await using var loopService = new BackgroundSyncLoopService(
            syncService,
            new BackgroundSyncOptions(
                TimeSpan.FromMilliseconds(30),
                true,
                true));

        // Act
        await loopService.StartAsync(CancellationToken.None);
        await Task.Delay(140);
        await loopService.StopAsync(CancellationToken.None);

        // Assert
        Assert.True(syncService.RunCount >= 2);
    }

    [Fact]
    public async Task DisposeAsync_ShouldStopRunningLoop()
    {
        // Arrange
        var loopService = new BackgroundSyncLoopService(
            new FakeCgmBackgroundSyncService(),
            new BackgroundSyncOptions(
                TimeSpan.FromMilliseconds(50),
                true,
                true));

        await loopService.StartAsync(CancellationToken.None);

        // Act
        await loopService.DisposeAsync();

        // Assert
        Assert.False(loopService.IsRunning);
    }

    private sealed class FakeCgmBackgroundSyncService : ICgmBackgroundSyncService
    {
        public int RunCount { get; private set; }

        public Task<Result<BackgroundSyncIterationResult>> RunOnceAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RunCount++;

            return Task.FromResult(Result<BackgroundSyncIterationResult>.Success(
                BackgroundSyncIterationResult.Succeeded(
                    CgmProviderKind.Mock,
                    1,
                    DateTimeOffset.UtcNow)));
        }
    }

    private sealed class FailingOnceCgmBackgroundSyncService : ICgmBackgroundSyncService
    {
        public int RunCount { get; private set; }

        public Task<Result<BackgroundSyncIterationResult>> RunOnceAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            RunCount++;

            if (RunCount == 1)
            {
                throw new IOException("Transient sync failure.");
            }

            return Task.FromResult(Result<BackgroundSyncIterationResult>.Success(
                BackgroundSyncIterationResult.Succeeded(
                    CgmProviderKind.Mock,
                    1,
                    DateTimeOffset.UtcNow)));
        }
    }
}