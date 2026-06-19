using GlucoDesk.Application.Cgm.BackgroundSync.Enums;
using GlucoDesk.Application.Cgm.BackgroundSync.Results;
using GlucoDesk.Application.Cgm.BackgroundSync.State;
using GlucoDesk.Application.Cgm.BackgroundSync.State.Services;
using GlucoDesk.Core.Glucose.Enums;

namespace GlucoDesk.Application.Tests.Cgm.BackgroundSync.State.Services;

public sealed class BackgroundSyncStateServiceTests
{
    [Fact]
    public void CurrentSnapshot_ShouldReturnInitialState_ByDefault()
    {
        // Arrange
        var service = new BackgroundSyncStateService();

        // Act
        var snapshot = service.CurrentSnapshot;

        // Assert
        Assert.False(snapshot.IsRunning);
        Assert.Equal(BackgroundSyncStatus.Unknown, snapshot.LastStatus);
        Assert.Equal(CgmProviderKind.Unknown, snapshot.LastProviderKind);
        Assert.Equal(0, snapshot.LastReadingsCount);
    }

    [Fact]
    public void MarkStarted_ShouldSetRunningState()
    {
        // Arrange
        var service = new BackgroundSyncStateService();
        var timestamp = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);

        // Act
        service.MarkStarted(timestamp);

        // Assert
        Assert.True(service.CurrentSnapshot.IsRunning);
        Assert.Equal("Background sync is running.", service.CurrentSnapshot.StatusMessage);
    }

    [Fact]
    public void MarkStopped_ShouldSetStoppedState()
    {
        // Arrange
        var service = new BackgroundSyncStateService();
        var timestamp = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);

        // Act
        service.MarkStopped(timestamp);

        // Assert
        Assert.False(service.CurrentSnapshot.IsRunning);
        Assert.Equal(timestamp, service.CurrentSnapshot.LastStoppedAt);
        Assert.Equal("Background sync is stopped.", service.CurrentSnapshot.StatusMessage);
    }

    [Fact]
    public void RecordIteration_ShouldUpdateSuccessfulSyncState()
    {
        // Arrange
        var service = new BackgroundSyncStateService();
        var syncedAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);

        // Act
        service.RecordIteration(
            BackgroundSyncIterationResult.Succeeded(
                CgmProviderKind.DexcomShare,
                12,
                syncedAt));

        // Assert
        var snapshot = service.CurrentSnapshot;

        Assert.Equal(BackgroundSyncStatus.Succeeded, snapshot.LastStatus);
        Assert.Equal(CgmProviderKind.DexcomShare, snapshot.LastProviderKind);
        Assert.Equal(12, snapshot.LastReadingsCount);
        Assert.Equal(syncedAt, snapshot.LastAttemptedAt);
        Assert.Equal(syncedAt, snapshot.LastSucceededAt);
        Assert.Null(snapshot.LastErrorMessage);
    }

    [Fact]
    public void RecordIteration_ShouldUpdateFailureSyncState()
    {
        // Arrange
        var service = new BackgroundSyncStateService();
        var syncedAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);

        // Act
        service.RecordIteration(
            BackgroundSyncIterationResult.ProviderFailed(
                syncedAt,
                "Provider failed."));

        // Assert
        var snapshot = service.CurrentSnapshot;

        Assert.Equal(BackgroundSyncStatus.ProviderFailed, snapshot.LastStatus);
        Assert.Equal(CgmProviderKind.Unknown, snapshot.LastProviderKind);
        Assert.Equal(syncedAt, snapshot.LastAttemptedAt);
        Assert.Equal("Provider failed.", snapshot.LastErrorMessage);
    }

    [Fact]
    public void RecordFailure_ShouldUpdateUnexpectedFailureState()
    {
        // Arrange
        var service = new BackgroundSyncStateService();
        var attemptedAt = new DateTimeOffset(2026, 6, 19, 10, 0, 0, TimeSpan.Zero);

        // Act
        service.RecordFailure(attemptedAt, "Unexpected error.");

        // Assert
        var snapshot = service.CurrentSnapshot;

        Assert.Equal(BackgroundSyncStatus.Failed, snapshot.LastStatus);
        Assert.Equal(CgmProviderKind.Unknown, snapshot.LastProviderKind);
        Assert.Equal(attemptedAt, snapshot.LastAttemptedAt);
        Assert.Equal("Unexpected error.", snapshot.LastErrorMessage);
    }

    [Fact]
    public void SnapshotChanged_ShouldBeRaised_WhenStateChanges()
    {
        // Arrange
        var service = new BackgroundSyncStateService();
        BackgroundSyncStateSnapshot? receivedSnapshot = null;

        service.SnapshotChanged += (_, snapshot) =>
        {
            receivedSnapshot = snapshot;
        };

        // Act
        service.MarkStarted(DateTimeOffset.UtcNow);

        // Assert
        Assert.NotNull(receivedSnapshot);
        Assert.True(receivedSnapshot.IsRunning);
    }
}