using GlucoDesk.Application.Updates;
using GlucoDesk.Core.Updates;

namespace GlucoDesk.Application.Tests.Updates;

public sealed class UpdateCoordinatorTests
{
    [Fact]
    public async Task CheckAsync_AutomaticCheckDisabled_ReturnsIdle()
    {
        var preferencesStore = new FakePreferencesStore(
            new UpdatePreferences
            {
                CheckAutomatically = false
            });

        var updateService = new FakeUpdateService();

        var sut = CreateSut(
            updateService,
            preferencesStore);

        var result = await sut.CheckAsync(
            UpdateCheckTrigger.Automatic,
            CancellationToken.None);

        Assert.Equal(
            UpdateCenterState.Idle,
            result.State);

        Assert.Equal(
            0,
            updateService.CheckCount);
    }

    [Fact]
    public async Task CheckAsync_NoUpdate_ReturnsUpToDateAndPersistsTimestamp()
    {
        var preferencesStore = new FakePreferencesStore(
            new UpdatePreferences());

        var updateService = new FakeUpdateService
        {
            Result = new UpdateCheckResult
            {
                CurrentVersion = "0.4.0-preview",
                IsUpdateAvailable = false
            }
        };

        var sut = CreateSut(
            updateService,
            preferencesStore);

        var result = await sut.CheckAsync(
            UpdateCheckTrigger.Manual,
            CancellationToken.None);

        Assert.Equal(
            UpdateCenterState.UpToDate,
            result.State);

        Assert.NotNull(result.LastSuccessfulCheckAt);
        Assert.NotNull(
            preferencesStore.Preferences.LastSuccessfulCheckAt);
    }

    [Fact]
    public async Task CheckAsync_UpdateAvailableFromStartup_ShowsDialog()
    {
        var preferencesStore = new FakePreferencesStore(
            new UpdatePreferences());

        var release = CreateRelease(
            "0.5.0-preview");

        var updateService = new FakeUpdateService
        {
            Result = new UpdateCheckResult
            {
                CurrentVersion = "0.4.0-preview",
                LatestRelease = release,
                IsUpdateAvailable = true
            }
        };

        var sut = CreateSut(
            updateService,
            preferencesStore);

        var result = await sut.CheckAsync(
            UpdateCheckTrigger.Automatic,
            CancellationToken.None);

        Assert.Equal(
            UpdateCenterState.UpdateAvailable,
            result.State);

        Assert.True(result.ShouldShowDialog);
        Assert.Same(
            release,
            result.LatestRelease);
    }

    [Fact]
    public async Task CheckAsync_UpdateAvailableFromManualCheck_DoesNotShowDialog()
    {
        var preferencesStore = new FakePreferencesStore(
            new UpdatePreferences());

        var updateService = new FakeUpdateService
        {
            Result = new UpdateCheckResult
            {
                CurrentVersion = "0.4.0-preview",
                LatestRelease = CreateRelease(
                    "0.5.0-preview"),
                IsUpdateAvailable = true
            }
        };

        var sut = CreateSut(
            updateService,
            preferencesStore);

        var result = await sut.CheckAsync(
            UpdateCheckTrigger.Manual,
            CancellationToken.None);

        Assert.Equal(
            UpdateCenterState.UpdateAvailable,
            result.State);

        Assert.False(result.ShouldShowDialog);
    }

    [Fact]
    public async Task CheckAsync_Error_PreservesLastSuccessfulCheck()
    {
        var previousCheck =
            DateTimeOffset.UtcNow.AddDays(-1);

        var preferencesStore = new FakePreferencesStore(
            new UpdatePreferences
            {
                LastSuccessfulCheckAt = previousCheck
            });

        var updateService = new FakeUpdateService
        {
            Result = new UpdateCheckResult
            {
                CurrentVersion = "0.4.0-preview",
                IsUpdateAvailable = false,
                ErrorMessage = "Network unavailable."
            }
        };

        var sut = CreateSut(
            updateService,
            preferencesStore);

        var result = await sut.CheckAsync(
            UpdateCheckTrigger.Automatic,
            CancellationToken.None);

        Assert.Equal(
            UpdateCenterState.Error,
            result.State);

        Assert.Equal(
            previousCheck,
            result.LastSuccessfulCheckAt);

        Assert.Equal(
            "Network unavailable.",
            result.ErrorMessage);
    }

    [Fact]
    public async Task DismissAsync_PersistsVersionAndTimestamp()
    {
        var preferencesStore = new FakePreferencesStore(
            new UpdatePreferences());

        var sut = CreateSut(
            new FakeUpdateService(),
            preferencesStore);

        await sut.DismissAsync(
            "0.5.0-preview",
            CancellationToken.None);

        Assert.Equal(
            "0.5.0-preview",
            preferencesStore.Preferences.LastDismissedVersion);

        Assert.NotNull(
            preferencesStore.Preferences.LastDismissedAt);
    }

    #region Helpers

    /// <summary>
    /// Creates the system under test.
    /// </summary>
    private static UpdateCoordinator CreateSut(
        IUpdateService updateService,
        IUpdatePreferencesStore preferencesStore)
    {
        return new UpdateCoordinator(
            updateService,
            preferencesStore,
            new FakeVersionProvider());
    }

    /// <summary>
    /// Creates a release used by update-coordinator tests.
    /// </summary>
    private static UpdateRelease CreateRelease(
        string version)
    {
        return new UpdateRelease(
            Version: version,
            TagName: $"v{version}",
            ReleaseName: $"v{version}",
            ReleaseNotes: "Release notes.",
            PublishedAt: DateTimeOffset.UtcNow,
            ReleasePageUrl:
                new Uri("https://example.com/release"),
            DownloadUrl:
                new Uri("https://example.com/download.zip"),
            AssetName: "download.zip",
            IsPreview: true);
    }

    private sealed class FakeVersionProvider :
        IApplicationVersionProvider
    {
        public string CurrentVersion =>
            "0.4.0-preview";
    }

    private sealed class FakeUpdateService :
        IUpdateService
    {
        public int CheckCount { get; private set; }

        public UpdateCheckResult Result { get; set; } =
            new()
            {
                CurrentVersion = "0.4.0-preview",
                IsUpdateAvailable = false
            };

        public Task<UpdateCheckResult> CheckForUpdatesAsync(
            bool includePreviewReleases,
            CancellationToken cancellationToken)
        {
            _ = includePreviewReleases;
            cancellationToken.ThrowIfCancellationRequested();

            CheckCount++;

            return Task.FromResult(Result);
        }

        public void Download(
            UpdateRelease release)
        {
            _ = release;
        }
    }

    private sealed class FakePreferencesStore :
        IUpdatePreferencesStore
    {
        public FakePreferencesStore(
            UpdatePreferences preferences)
        {
            Preferences = preferences;
        }

        public UpdatePreferences Preferences { get; private set; }

        public Task<UpdatePreferences> LoadAsync(
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            return Task.FromResult(Preferences);
        }

        public Task SaveAsync(
            UpdatePreferences preferences,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            Preferences = preferences;

            return Task.CompletedTask;
        }
    }

    #endregion
}
