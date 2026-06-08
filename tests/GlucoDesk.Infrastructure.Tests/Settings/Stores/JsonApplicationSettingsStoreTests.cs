using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Infrastructure.Settings.Options;
using GlucoDesk.Infrastructure.Settings.Stores;

namespace GlucoDesk.Infrastructure.Tests.Settings.Stores;

public sealed class JsonApplicationSettingsStoreTests : IDisposable
{
    private readonly string _temporaryDirectoryPath;

    public JsonApplicationSettingsStoreTests()
    {
        _temporaryDirectoryPath = Path.Combine(
            Path.GetTempPath(),
            "GlucoDesk.Tests",
            Guid.NewGuid().ToString("N"));
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnDefaultSettings_WhenFileDoesNotExist()
    {
        var store = CreateStore();

        var result = await store.LoadAsync(CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.Equal(ApplicationSettings.Default, result.Value);
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateSettingsFile()
    {
        var store = CreateStore();
        var settings = new ApplicationSettings(
            preferredUnit: GlucoseUnit.MgDl,
            dashboardRefreshInterval: TimeSpan.FromSeconds(45));

        var result = await store.SaveAsync(settings, CancellationToken.None);

        Assert.True(result.IsSuccess);
        Assert.True(File.Exists(BuildSettingsFilePath()));
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnSavedSettings()
    {
        var store = CreateStore();
        var expectedSettings = new ApplicationSettings(
            activeLiveProvider: CgmProviderKind.Mock,
            historicalProvider: CgmProviderKind.Mock,
            preferredUnit: GlucoseUnit.MgDl,
            targetLowMgDl: 75,
            targetHighMgDl: 170,
            dashboardRefreshInterval: TimeSpan.FromSeconds(45));

        var saveResult = await store.SaveAsync(expectedSettings, CancellationToken.None);
        var loadResult = await store.LoadAsync(CancellationToken.None);

        Assert.True(saveResult.IsSuccess);
        Assert.True(loadResult.IsSuccess);
        Assert.Equal(expectedSettings, loadResult.Value);
    }

    [Fact]
    public async Task LoadAsync_ShouldReturnFailure_WhenJsonIsInvalid()
    {
        Directory.CreateDirectory(_temporaryDirectoryPath);
        await File.WriteAllTextAsync(BuildSettingsFilePath(), "{ invalid json");

        var store = CreateStore();

        var result = await store.LoadAsync(CancellationToken.None);

        Assert.True(result.IsFailure);
        Assert.Equal("Settings.InvalidFormat", result.Error.Code);
    }

    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectoryPath))
        {
            Directory.Delete(_temporaryDirectoryPath, recursive: true);
        }
    }

    #region Helpers

    /// <summary>
    /// Creates a JSON application settings store using a test file path.
    /// </summary>
    /// <returns>The JSON application settings store.</returns>
    private JsonApplicationSettingsStore CreateStore()
    {
        return new JsonApplicationSettingsStore(
            new LocalSettingsStorageOptions(BuildSettingsFilePath()));
    }

    /// <summary>
    /// Builds the test settings file path.
    /// </summary>
    /// <returns>The test settings file path.</returns>
    private string BuildSettingsFilePath()
    {
        return Path.Combine(_temporaryDirectoryPath, "settings.json");
    }

    #endregion
}