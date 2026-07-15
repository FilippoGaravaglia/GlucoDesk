using System.Text.Json;
using GlucoDesk.Desktop.Localization;

namespace GlucoDesk.Desktop.Tests.Localization;

public sealed class LanguagePreferenceFileStoreTests
{
    [Fact]
    public void Read_ShouldReturnNonExplicitDefault_WhenFileDoesNotExist()
    {
        using var directory = new TemporaryDirectory();

        var store = new LanguagePreferenceFileStore(
            directory.GetPath("language-preferences.json"));

        var result = store.Read();

        Assert.False(result.HasExplicitPreference);
        Assert.Equal("en", result.LanguageCode);
    }

    [Fact]
    public void Read_ShouldSupportExistingLegacyPreferenceFile()
    {
        using var directory = new TemporaryDirectory();

        var filePath = directory.GetPath(
            "language-preferences.json");

        File.WriteAllText(
            filePath,
            """
            {
              "LanguageCode": "it"
            }
            """);

        var store = new LanguagePreferenceFileStore(filePath);

        var result = store.Read();

        Assert.True(result.HasExplicitPreference);
        Assert.Equal("it", result.LanguageCode);
    }

    [Fact]
    public void Read_ShouldReturnNonExplicitDefault_WhenLanguageIsUnsupported()
    {
        using var directory = new TemporaryDirectory();

        var filePath = directory.GetPath(
            "language-preferences.json");

        File.WriteAllText(
            filePath,
            """
            {
              "SchemaVersion": 1,
              "LanguageCode": "fr"
            }
            """);

        var store = new LanguagePreferenceFileStore(filePath);

        var result = store.Read();

        Assert.False(result.HasExplicitPreference);
        Assert.Equal("en", result.LanguageCode);
    }

    [Fact]
    public void Read_ShouldReturnNonExplicitDefault_WhenJsonIsInvalid()
    {
        using var directory = new TemporaryDirectory();

        var filePath = directory.GetPath(
            "language-preferences.json");

        File.WriteAllText(
            filePath,
            "{ invalid json");

        var store = new LanguagePreferenceFileStore(filePath);

        var result = store.Read();

        Assert.False(result.HasExplicitPreference);
        Assert.Equal("en", result.LanguageCode);
    }

    [Fact]
    public void Save_ShouldPersistNormalizedSupportedLanguageAtomically()
    {
        using var directory = new TemporaryDirectory();

        var filePath = directory.GetPath(
            "language-preferences.json");

        var store = new LanguagePreferenceFileStore(filePath);

        store.Save("it-IT");

        var result = store.Read();

        Assert.True(result.HasExplicitPreference);
        Assert.Equal("it", result.LanguageCode);

        Assert.Empty(
            Directory.GetFiles(
                directory.Path,
                "*.tmp",
                SearchOption.TopDirectoryOnly));

        using var document = JsonDocument.Parse(
            File.ReadAllText(filePath));

        Assert.Equal(
            1,
            document.RootElement
                .GetProperty("SchemaVersion")
                .GetInt32());

        Assert.Equal(
            "it",
            document.RootElement
                .GetProperty("LanguageCode")
                .GetString());
    }

    [Fact]
    public void Save_ShouldRejectUnsupportedLanguage()
    {
        using var directory = new TemporaryDirectory();

        var store = new LanguagePreferenceFileStore(
            directory.GetPath("language-preferences.json"));

        Assert.Throws<ArgumentException>(
            () => store.Save("fr"));
    }

    private sealed class TemporaryDirectory : IDisposable
    {
        public TemporaryDirectory()
        {
            Path = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "GlucoDesk.Tests",
                Guid.NewGuid().ToString("N"));

            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public string GetPath(string fileName)
        {
            return System.IO.Path.Combine(
                Path,
                fileName);
        }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(
                    Path,
                    recursive: true);
            }
        }
    }
}
