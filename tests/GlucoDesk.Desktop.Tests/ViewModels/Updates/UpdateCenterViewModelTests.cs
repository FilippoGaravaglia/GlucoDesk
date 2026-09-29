using GlucoDesk.Application.Updates;
using GlucoDesk.Core.Updates;
using GlucoDesk.Desktop.ViewModels.Updates;

namespace GlucoDesk.Desktop.Tests.ViewModels.Updates;

public sealed class UpdateCenterViewModelTests
{
    [Fact]
    public async Task CheckNowCommand_UpToDate_UpdatesStore()
    {
        var coordinator = new FakeUpdateCoordinator
        {
            CheckResult = new UpdateCenterSnapshot
            {
                State = UpdateCenterState.UpToDate,
                CurrentVersion = "0.4.0-preview",
                LastSuccessfulCheckAt = DateTimeOffset.UtcNow
            }
        };

        var store = new UpdateCenterStore();

        using var sut = CreateSut(
            coordinator,
            new FakeUpdateService(),
            store);

        await sut.CheckNowCommand.ExecuteAsync(null);

        Assert.Equal(
            UpdateCenterState.UpToDate,
            store.Snapshot.State);

        Assert.True(sut.IsUpToDate);

        Assert.Equal(
            UpdateCheckTrigger.Manual,
            coordinator.LastTrigger);
    }

    [Fact]
    public async Task CheckOnStartupAsync_UsesAutomaticTrigger()
    {
        var coordinator = new FakeUpdateCoordinator
        {
            CheckResult = new UpdateCenterSnapshot
            {
                State = UpdateCenterState.UpToDate,
                CurrentVersion = "0.4.0-preview"
            }
        };

        using var sut = CreateSut(
            coordinator,
            new FakeUpdateService(),
            new UpdateCenterStore());

        await sut.CheckOnStartupAsync(
            CancellationToken.None);

        Assert.Equal(
            UpdateCheckTrigger.Automatic,
            coordinator.LastTrigger);
    }

    [Fact]
    public void DownloadUpdateCommand_UpdateAvailable_DownloadsRelease()
    {
        var release = CreateRelease();

        var store = new UpdateCenterStore
        {
            Snapshot = new UpdateCenterSnapshot
            {
                State = UpdateCenterState.UpdateAvailable,
                CurrentVersion = "0.3.0-preview",
                LatestRelease = release
            }
        };

        var updateService = new FakeUpdateService();

        using var sut = CreateSut(
            new FakeUpdateCoordinator(),
            updateService,
            store);

        sut.DownloadUpdateCommand.Execute(null);

        Assert.Same(
            release,
            updateService.DownloadedRelease);
    }

    [Fact]
    public async Task DismissCommand_UpdateAvailable_DismissesDialog()
    {
        var release = CreateRelease();

        var coordinator = new FakeUpdateCoordinator();

        var store = new UpdateCenterStore
        {
            Snapshot = new UpdateCenterSnapshot
            {
                State = UpdateCenterState.UpdateAvailable,
                CurrentVersion = "0.3.0-preview",
                LatestRelease = release,
                ShouldShowDialog = true
            }
        };

        using var sut = CreateSut(
            coordinator,
            new FakeUpdateService(),
            store);

        await sut.DismissCommand.ExecuteAsync(null);

        Assert.Equal(
            release.Version,
            coordinator.DismissedVersion);

        Assert.False(
            store.Snapshot.ShouldShowDialog);
    }

    [Fact]
    public void Dispose_DetachesFromStore()
    {
        var store = new UpdateCenterStore();

        var sut = CreateSut(
            new FakeUpdateCoordinator(),
            new FakeUpdateService(),
            store);

        var notificationCount = 0;

        sut.PropertyChanged += (_, _) =>
            notificationCount++;

        sut.Dispose();

        store.Snapshot = new UpdateCenterSnapshot
        {
            State = UpdateCenterState.UpToDate,
            CurrentVersion = "0.4.0-preview"
        };

        Assert.Equal(
            0,
            notificationCount);
    }

    #region Helpers

    /// <summary>
    /// Creates the system under test.
    /// </summary>
    private static UpdateCenterViewModel CreateSut(
        IUpdateCoordinator coordinator,
        IUpdateService updateService,
        UpdateCenterStore store)
    {
        return new UpdateCenterViewModel(
            coordinator,
            updateService,
            store);
    }

    /// <summary>
    /// Creates a release used by view-model tests.
    /// </summary>
    private static UpdateRelease CreateRelease()
    {
        return new UpdateRelease(
            Version: "0.4.0-preview",
            TagName: "v0.4.0-preview",
            ReleaseName: "v0.4.0-preview",
            ReleaseNotes: "Release notes.",
            PublishedAt: DateTimeOffset.UtcNow,
            ReleasePageUrl:
                new Uri("https://example.com/release"),
            DownloadUrl:
                new Uri("https://example.com/download.zip"),
            AssetName: "download.zip",
            IsPreview: true);
    }

    private sealed class FakeUpdateCoordinator :
        IUpdateCoordinator
    {
        public UpdateCheckTrigger? LastTrigger { get; private set; }

        public string? DismissedVersion { get; private set; }

        public UpdateCenterSnapshot CheckResult { get; set; } =
            new()
            {
                State = UpdateCenterState.UpToDate,
                CurrentVersion = "0.4.0-preview"
            };

        public Task<UpdateCenterSnapshot> CheckAsync(
            UpdateCheckTrigger trigger,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            LastTrigger = trigger;

            return Task.FromResult(CheckResult);
        }

        public Task DismissAsync(
            string version,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            DismissedVersion = version;

            return Task.CompletedTask;
        }

        public Task<UpdatePreferences> GetPreferencesAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                new UpdatePreferences());
        }

        public Task SavePreferencesAsync(
            UpdatePreferences preferences,
            CancellationToken cancellationToken)
        {
            _ = preferences;

            cancellationToken.ThrowIfCancellationRequested();

            return Task.CompletedTask;
        }
    }

    private sealed class FakeUpdateService :
        IUpdateService
    {
        public UpdateRelease? DownloadedRelease { get; private set; }

        public Task<UpdateCheckResult> CheckForUpdatesAsync(
            bool includePreviewReleases,
            CancellationToken cancellationToken)
        {
            _ = includePreviewReleases;

            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(
                new UpdateCheckResult
                {
                    CurrentVersion = "0.4.0-preview",
                    IsUpdateAvailable = false
                });
        }

        public void Download(
            UpdateRelease release)
        {
            DownloadedRelease = release;
        }
    }

    #endregion
}
