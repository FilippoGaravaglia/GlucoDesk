using GlucoDesk.Core.Updates;
using GlucoDesk.Infrastructure.Updates;

namespace GlucoDesk.Infrastructure.Tests.Updates;

public sealed class JsonUpdatePreferencesStoreTests : IDisposable
{
    private readonly string _directoryPath =
        Path.Combine(
            Path.GetTempPath(),
            $"glucodesk-update-tests-{Guid.NewGuid():N}");

    [Fact]
    public async Task LoadAsync_FileDoesNotExist_ReturnsDefaults()
    {
        var store = CreateStore();

        var result = await store.LoadAsync(
            CancellationToken.None);

        Assert.True(result.CheckAutomatically);
        Assert.True(result.IncludePreviewReleases);
        Assert.Null(result.LastDismissedVersion);
        Assert.Null(result.LastDismissedAt);
        Assert.Null(result.LastSuccessfulCheckAt);
    }

    [Fact]
    public async Task SaveAndLoadAsync_PersistsPreferences()
    {
        var store = CreateStore();

        var expected = new UpdatePreferences
        {
            CheckAutomatically = false,
            IncludePreviewReleases = false,
            LastDismissedVersion = "0.4.0-preview",
            LastDismissedAt =
                new DateTimeOffset(
                    2026,
                    9,
                    28,
                    10,
                    30,
                    0,
                    TimeSpan.Zero),
            LastSuccessfulCheckAt =
                new DateTimeOffset(
                    2026,
                    9,
                    28,
                    10,
                    25,
                    0,
                    TimeSpan.Zero)
        };

        await store.SaveAsync(
            expected,
            CancellationToken.None);

        var actual = await store.LoadAsync(
            CancellationToken.None);

        Assert.Equal(
            expected.CheckAutomatically,
            actual.CheckAutomatically);

        Assert.Equal(
            expected.IncludePreviewReleases,
            actual.IncludePreviewReleases);

        Assert.Equal(
            expected.LastDismissedVersion,
            actual.LastDismissedVersion);

        Assert.Equal(
            expected.LastDismissedAt,
            actual.LastDismissedAt);

        Assert.Equal(
            expected.LastSuccessfulCheckAt,
            actual.LastSuccessfulCheckAt);
    }

    /// <inheritdoc />
    public void Dispose()
    {
        if (Directory.Exists(_directoryPath))
        {
            Directory.Delete(
                _directoryPath,
                recursive: true);
        }
    }

    #region Helpers

    /// <summary>
    /// Creates the store used by the current test.
    /// </summary>
    private JsonUpdatePreferencesStore CreateStore()
    {
        return new JsonUpdatePreferencesStore(
            Path.Combine(
                _directoryPath,
                "update-settings.json"));
    }

    #endregion
}
