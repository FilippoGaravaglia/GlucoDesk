using GlucoDesk.Application.Cgm.History.Continuity.Enums;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Desktop.Cgm.History.Continuity.Enums;
using GlucoDesk.Desktop.Cgm.History.Continuity.Results;
using GlucoDesk.Desktop.Cgm.History.Continuity.Services.Abstractions;
using GlucoDesk.Desktop.Cgm.History.Continuity.ViewModels;
using GlucoDesk.Desktop.Common.Dispatching.Abstractions;

namespace GlucoDesk.Desktop.Tests.Cgm.History.Continuity.ViewModels;

public sealed class DesktopHistoryContinuitySyncStatusViewModelTests
{
    [Fact]
    public void Constructor_ShouldExposeInitialIdleStatus()
    {
        // Arrange
        var statusStore = new FakeStatusStore();

        // Act
        using var viewModel = CreateViewModel(statusStore);

        // Assert
        Assert.Equal(DesktopHistoryContinuitySyncRunState.Idle, viewModel.State);
        Assert.Equal("Idle", viewModel.StateText);
        Assert.Equal("Not available", viewModel.TriggerText);
        Assert.False(viewModel.IsRunning);
        Assert.False(viewModel.HasError);
        Assert.False(viewModel.HasLastSuccessfulSync);
        Assert.False(viewModel.HasNewReadings);
    }

    [Fact]
    public void StatusChanged_ShouldExposeRunningStatus()
    {
        // Arrange
        var statusStore = new FakeStatusStore();
        using var viewModel = CreateViewModel(statusStore);

        var startedAt = new DateTimeOffset(2026, 6, 20, 10, 0, 0, TimeSpan.Zero);

        var snapshot = new DesktopHistoryContinuitySyncStatusSnapshot(
            DesktopHistoryContinuitySyncRunState.Running,
            CgmHistoryContinuitySyncTrigger.Startup,
            StartedAtUtc: startedAt,
            CompletedAtUtc: null,
            LastSuccessfulSyncAtUtc: null,
            Message: "History continuity synchronization started by Startup.",
            ErrorCode: null,
            ErrorDescription: null,
            TotalFetchedReadings: 0,
            AddedReadingsCount: 0,
            DuplicateReadingsCount: 0,
            StoredReadingsCount: 0,
            HasNewReadings: false);

        // Act
        statusStore.Publish(snapshot);

        // Assert
        Assert.Equal(DesktopHistoryContinuitySyncRunState.Running, viewModel.State);
        Assert.Equal("Syncing history", viewModel.StateText);
        Assert.Equal("Startup", viewModel.TriggerText);
        Assert.True(viewModel.IsRunning);
        Assert.False(viewModel.HasError);
        Assert.Equal(snapshot.Message, viewModel.Message);
        Assert.NotEqual("Not available", viewModel.StartedAtText);
    }

    [Fact]
    public void StatusChanged_ShouldExposeSuccessfulStatusWithReadingSummary()
    {
        // Arrange
        var statusStore = new FakeStatusStore();
        using var viewModel = CreateViewModel(statusStore);

        var completedAt = new DateTimeOffset(2026, 6, 20, 10, 5, 0, TimeSpan.Zero);

        var snapshot = new DesktopHistoryContinuitySyncStatusSnapshot(
            DesktopHistoryContinuitySyncRunState.Succeeded,
            CgmHistoryContinuitySyncTrigger.Startup,
            StartedAtUtc: completedAt.AddMinutes(-1),
            CompletedAtUtc: completedAt,
            LastSuccessfulSyncAtUtc: completedAt,
            Message: "History continuity synchronization completed successfully.",
            ErrorCode: null,
            ErrorDescription: null,
            TotalFetchedReadings: 5,
            AddedReadingsCount: 3,
            DuplicateReadingsCount: 2,
            StoredReadingsCount: 50,
            HasNewReadings: true);

        // Act
        statusStore.Publish(snapshot);

        // Assert
        Assert.Equal(DesktopHistoryContinuitySyncRunState.Succeeded, viewModel.State);
        Assert.Equal("History synced", viewModel.StateText);
        Assert.Equal("Startup", viewModel.TriggerText);
        Assert.False(viewModel.IsRunning);
        Assert.False(viewModel.HasError);
        Assert.True(viewModel.HasReadingSummary);
        Assert.True(viewModel.HasLastSuccessfulSync);
        Assert.True(viewModel.HasNewReadings);
        Assert.Equal("Fetched: 5, added: 3, duplicates: 2, stored: 50", viewModel.ReadingSummaryText);
        Assert.NotEqual("Not available", viewModel.CompletedAtText);
        Assert.NotEqual("Not available", viewModel.LastSuccessfulSyncAtText);
    }

    [Fact]
    public void StatusChanged_ShouldExposeFailedStatusWithError()
    {
        // Arrange
        var statusStore = new FakeStatusStore();
        using var viewModel = CreateViewModel(statusStore);

        var snapshot = new DesktopHistoryContinuitySyncStatusSnapshot(
            DesktopHistoryContinuitySyncRunState.Failed,
            CgmHistoryContinuitySyncTrigger.Resume,
            StartedAtUtc: new DateTimeOffset(2026, 6, 20, 10, 0, 0, TimeSpan.Zero),
            CompletedAtUtc: new DateTimeOffset(2026, 6, 20, 10, 1, 0, TimeSpan.Zero),
            LastSuccessfulSyncAtUtc: null,
            Message: "History continuity synchronization failed.",
            ErrorCode: "HistoryContinuity.SyncFailed",
            ErrorDescription: "History continuity synchronization failed.",
            TotalFetchedReadings: 0,
            AddedReadingsCount: 0,
            DuplicateReadingsCount: 0,
            StoredReadingsCount: 0,
            HasNewReadings: false);

        // Act
        statusStore.Publish(snapshot);

        // Assert
        Assert.Equal(DesktopHistoryContinuitySyncRunState.Failed, viewModel.State);
        Assert.Equal("Sync failed", viewModel.StateText);
        Assert.Equal("Resume", viewModel.TriggerText);
        Assert.False(viewModel.IsRunning);
        Assert.True(viewModel.HasError);
        Assert.False(viewModel.HasNewReadings);
        Assert.Equal(
            "HistoryContinuity.SyncFailed: History continuity synchronization failed.",
            viewModel.ErrorText);
    }

    [Fact]
    public void Dispose_ShouldUnsubscribeFromStatusStore()
    {
        // Arrange
        var statusStore = new FakeStatusStore();
        var viewModel = CreateViewModel(statusStore);

        viewModel.Dispose();

        var snapshot = new DesktopHistoryContinuitySyncStatusSnapshot(
            DesktopHistoryContinuitySyncRunState.Running,
            CgmHistoryContinuitySyncTrigger.Manual,
            StartedAtUtc: DateTimeOffset.UtcNow,
            CompletedAtUtc: null,
            LastSuccessfulSyncAtUtc: null,
            Message: "Manual sync started.",
            ErrorCode: null,
            ErrorDescription: null,
            TotalFetchedReadings: 0,
            AddedReadingsCount: 0,
            DuplicateReadingsCount: 0,
            StoredReadingsCount: 0,
            HasNewReadings: false);

        // Act
        statusStore.Publish(snapshot);

        // Assert
        Assert.Equal(DesktopHistoryContinuitySyncRunState.Idle, viewModel.State);
        Assert.Equal("Idle", viewModel.StateText);
    }

    #region Helpers

    /// <summary>
    /// Creates the status ViewModel under test.
    /// </summary>
    /// <param name="statusStore">The fake status store.</param>
    /// <returns>The status ViewModel.</returns>
    private static DesktopHistoryContinuitySyncStatusViewModel CreateViewModel(
        IDesktopHistoryContinuitySyncStatusStore statusStore)
    {
        return new DesktopHistoryContinuitySyncStatusViewModel(
            statusStore,
            new ImmediateDesktopUiDispatcher());
    }

    #endregion

    private sealed class ImmediateDesktopUiDispatcher : IDesktopUiDispatcher
    {
        /// <inheritdoc />
        public void Post(Action action)
        {
            ArgumentNullException.ThrowIfNull(action);

            action();
        }
    }

    private sealed class FakeStatusStore : IDesktopHistoryContinuitySyncStatusStore
    {
        public event EventHandler<DesktopHistoryContinuitySyncStatusSnapshot>? StatusChanged;

        public DesktopHistoryContinuitySyncStatusSnapshot Current { get; private set; } =
            DesktopHistoryContinuitySyncStatusSnapshot.Idle;

        public void Publish(DesktopHistoryContinuitySyncStatusSnapshot snapshot)
        {
            Current = snapshot;
            StatusChanged?.Invoke(this, snapshot);
        }

        /// <inheritdoc />
        public void MarkRunning(CgmHistoryContinuitySyncTrigger trigger)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void MarkSucceeded(
            CgmHistoryContinuitySyncTrigger trigger,
            DesktopHistoryContinuitySyncRunResult runResult)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void MarkSkipped(
            CgmHistoryContinuitySyncTrigger trigger,
            string message)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void MarkFailed(
            CgmHistoryContinuitySyncTrigger trigger,
            Error error)
        {
            throw new NotSupportedException();
        }

        /// <inheritdoc />
        public void MarkCanceled(CgmHistoryContinuitySyncTrigger trigger)
        {
            throw new NotSupportedException();
        }
    }
}