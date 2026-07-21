using System.Text.Json;
using GlucoDesk.Desktop.Onboarding;

namespace GlucoDesk.Desktop.Tests.Onboarding;

public sealed class FeatureTourPreferenceStoreTests : IDisposable
{
    private readonly string _temporaryDirectory =
        Path.Combine(
            Path.GetTempPath(),
            "GlucoDesk.Tests",
            Guid.NewGuid().ToString("N"));

    [Fact]
    public void HasCompletedCurrentTour_ShouldReturnFalse_WhenFileDoesNotExist()
    {
        var store = CreateStore();

        Assert.False(store.HasCompletedCurrentTour());
    }

    [Fact]
    public void MarkCurrentTourCompleted_ShouldPersistCurrentVersion()
    {
        var filePath = GetFilePath();
        var store = new FeatureTourPreferenceStore(filePath);

        store.MarkCurrentTourCompleted();

        Assert.True(File.Exists(filePath));
        Assert.True(store.HasCompletedCurrentTour());

        using var document =
            JsonDocument.Parse(File.ReadAllText(filePath));

        Assert.True(
            document.RootElement
                .GetProperty("completed")
                .GetBoolean());

        Assert.Equal(
            FeatureTourPreferenceStore.CurrentTourVersion,
            document.RootElement
                .GetProperty("tourVersion")
                .GetInt32());
    }

    [Fact]
    public void HasCompletedCurrentTour_ShouldReturnFalse_ForOlderVersion()
    {
        var filePath = GetFilePath();

        Directory.CreateDirectory(
            Path.GetDirectoryName(filePath)!);

        File.WriteAllText(
            filePath,
            """
            {
              "completed": true,
              "tourVersion": 0
            }
            """);

        var store =
            new FeatureTourPreferenceStore(filePath);

        Assert.False(store.HasCompletedCurrentTour());
    }

    [Fact]
    public void HasCompletedCurrentTour_ShouldReturnFalse_ForInvalidJson()
    {
        var filePath = GetFilePath();

        Directory.CreateDirectory(
            Path.GetDirectoryName(filePath)!);

        File.WriteAllText(
            filePath,
            "{ invalid");

        var store =
            new FeatureTourPreferenceStore(filePath);

        Assert.False(store.HasCompletedCurrentTour());
    }

    public void Dispose()
    {
        if (Directory.Exists(_temporaryDirectory))
        {
            Directory.Delete(
                _temporaryDirectory,
                recursive: true);
        }
    }

    private FeatureTourPreferenceStore CreateStore()
    {
        return new FeatureTourPreferenceStore(
            GetFilePath());
    }

    private string GetFilePath()
    {
        return Path.Combine(
            _temporaryDirectory,
            "feature-tour.json");
    }
}
