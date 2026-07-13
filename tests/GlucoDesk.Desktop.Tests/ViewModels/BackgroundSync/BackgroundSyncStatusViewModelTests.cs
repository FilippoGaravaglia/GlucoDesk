using GlucoDesk.Application.Cgm.BackgroundSync.Results;
using GlucoDesk.Application.Cgm.BackgroundSync.State.Services;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Desktop.BackgroundSync.Dispatching.Abstractions;
using GlucoDesk.Desktop.ViewModels.BackgroundSync;
using GlucoDesk.Desktop.Tests.Localization;

namespace GlucoDesk.Desktop.Tests.ViewModels.BackgroundSync;

public sealed class BackgroundSyncStatusViewModelTests : EnglishLocalizationTestBase
{
    [Fact]
    public void Constructor_ShouldExposeUserFriendlyInitialState()
    {
        // Arrange
        var stateService = new BackgroundSyncStateService();

        // Act
        using var viewModel = new BackgroundSyncStatusViewModel(
            stateService,
            new ImmediateBackgroundSyncUiDispatcher());

        // Assert
        Assert.False(viewModel.IsRunning);
        Assert.False(viewModel.NeedsAttention);
        Assert.False(viewModel.HasSuccessfulSync);
        Assert.Equal("Automatic sync", viewModel.Title);
        Assert.Equal("Waiting to start", viewModel.StatusText);
        Assert.Equal("Your local history will update automatically while GlucoDesk is open.", viewModel.SummaryText);
        Assert.Equal("No updates yet", viewModel.LastUpdateText);
    }

    [Fact]
    public void ViewModel_ShouldExposePreparingState_WhenSyncStarts()
    {
        // Arrange
        var stateService = new BackgroundSyncStateService();

        using var viewModel = new BackgroundSyncStatusViewModel(
            stateService,
            new ImmediateBackgroundSyncUiDispatcher());

        // Act
        stateService.MarkStarted(DateTimeOffset.UtcNow);

        // Assert
        Assert.True(viewModel.IsRunning);
        Assert.False(viewModel.NeedsAttention);
        Assert.Equal("Preparing sync", viewModel.StatusText);
        Assert.Equal("Looking for recent readings", viewModel.SummaryText);
    }

    [Fact]
    public void ViewModel_ShouldExposeActiveState_WhenSuccessfulIterationIsRecorded()
    {
        // Arrange
        var stateService = new BackgroundSyncStateService();
        var syncedAt = new DateTimeOffset(2026, 6, 19, 10, 30, 0, TimeSpan.Zero);

        using var viewModel = new BackgroundSyncStatusViewModel(
            stateService,
            new ImmediateBackgroundSyncUiDispatcher());

        // Act
        stateService.MarkStarted(syncedAt);
        stateService.RecordIteration(
            BackgroundSyncIterationResult.Succeeded(
                CgmProviderKind.DexcomShare,
                12,
                syncedAt));

        // Assert
        Assert.True(viewModel.IsRunning);
        Assert.False(viewModel.NeedsAttention);
        Assert.True(viewModel.HasSuccessfulSync);
        Assert.Equal("Sync active", viewModel.StatusText);
        Assert.StartsWith("Updated at ", viewModel.SummaryText);
        Assert.StartsWith("Last successful update: ", viewModel.LastUpdateText);
    }

    [Fact]
    public void ViewModel_ShouldExposeAttentionState_WhenProviderFails()
    {
        // Arrange
        var stateService = new BackgroundSyncStateService();
        var syncedAt = new DateTimeOffset(2026, 6, 19, 10, 30, 0, TimeSpan.Zero);

        using var viewModel = new BackgroundSyncStatusViewModel(
            stateService,
            new ImmediateBackgroundSyncUiDispatcher());

        // Act
        stateService.MarkStarted(syncedAt);
        stateService.RecordIteration(
            BackgroundSyncIterationResult.ProviderFailed(
                syncedAt,
                "Provider failed."));

        // Assert
        Assert.True(viewModel.IsRunning);
        Assert.True(viewModel.NeedsAttention);
        Assert.Equal("Needs attention", viewModel.StatusText);
        Assert.Equal("Last update failed", viewModel.SummaryText);
        Assert.Equal(
            "GlucoDesk will try again automatically. If the issue continues, check your account or connection.",
            viewModel.SupportingText);
    }

    [Fact]
    public void ViewModel_ShouldExposePausedState_WhenSyncStopsAfterSuccess()
    {
        // Arrange
        var stateService = new BackgroundSyncStateService();
        var syncedAt = new DateTimeOffset(2026, 6, 19, 10, 30, 0, TimeSpan.Zero);

        using var viewModel = new BackgroundSyncStatusViewModel(
            stateService,
            new ImmediateBackgroundSyncUiDispatcher());

        // Act
        stateService.MarkStarted(syncedAt);
        stateService.RecordIteration(
            BackgroundSyncIterationResult.Succeeded(
                CgmProviderKind.DexcomShare,
                12,
                syncedAt));
        stateService.MarkStopped(syncedAt.AddMinutes(1));

        // Assert
        Assert.False(viewModel.IsRunning);
        Assert.True(viewModel.HasSuccessfulSync);
        Assert.Equal("Sync paused", viewModel.StatusText);
        Assert.StartsWith("Updated at ", viewModel.SummaryText);
    }

    [Fact]
    public void Dispose_ShouldUnsubscribeFromStateChanges()
    {
        // Arrange
        var stateService = new BackgroundSyncStateService();

        var viewModel = new BackgroundSyncStatusViewModel(
            stateService,
            new ImmediateBackgroundSyncUiDispatcher());

        viewModel.Dispose();

        // Act
        stateService.MarkStarted(DateTimeOffset.UtcNow);

        // Assert
        Assert.False(viewModel.IsRunning);
        Assert.Equal("Waiting to start", viewModel.StatusText);
    }

    private sealed class ImmediateBackgroundSyncUiDispatcher : IBackgroundSyncUiDispatcher
    {
        /// <inheritdoc />
        public void Post(Action action)
        {
            action();
        }
    }
}