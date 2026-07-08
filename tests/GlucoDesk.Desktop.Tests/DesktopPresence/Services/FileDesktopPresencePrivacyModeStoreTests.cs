using GlucoDesk.Desktop.DesktopPresence.Services;

namespace GlucoDesk.Desktop.Tests.DesktopPresence.Services;

public sealed class FileDesktopPresencePrivacyModeStoreTests
{
    [Fact]
    public void Load_ShouldReturnFalse_WhenStateFileDoesNotExist()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var store = new FileDesktopPresencePrivacyModeStore(
            Path.Combine(temporaryDirectory.Path, "privacy-mode.txt"));

        var result = store.Load();

        Assert.False(result);
    }

    [Fact]
    public void Save_ShouldPersistEnabledState()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var store = new FileDesktopPresencePrivacyModeStore(
            Path.Combine(temporaryDirectory.Path, "privacy-mode.txt"));

        store.Save(true);

        Assert.True(store.Load());
    }

    [Fact]
    public void Save_ShouldPersistDisabledState()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var store = new FileDesktopPresencePrivacyModeStore(
            Path.Combine(temporaryDirectory.Path, "privacy-mode.txt"));

        store.Save(true);
        store.Save(false);

        Assert.False(store.Load());
    }

    [Fact]
    public void Load_ShouldReturnFalse_WhenStateFileContainsInvalidValue()
    {
        using var temporaryDirectory = new TemporaryDirectory();
        var filePath = Path.Combine(temporaryDirectory.Path, "privacy-mode.txt");

        File.WriteAllText(filePath, "invalid");

        var store = new FileDesktopPresencePrivacyModeStore(filePath);

        Assert.False(store.Load());
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                $"glucodesk-privacy-mode-{Guid.NewGuid():N}");

            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
