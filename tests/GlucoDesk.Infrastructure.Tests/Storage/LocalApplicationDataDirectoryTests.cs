using GlucoDesk.Infrastructure.Storage;

namespace GlucoDesk.Infrastructure.Tests.Storage;

public sealed class LocalApplicationDataDirectoryTests
{
    [Fact]
    public void GetApplicationDirectoryPath_ShouldReturnGlucoDeskDirectory()
    {
        // Act
        var directoryPath = LocalApplicationDataDirectory.GetApplicationDirectoryPath();

        // Assert
        Assert.False(string.IsNullOrWhiteSpace(directoryPath));
        Assert.EndsWith(
            Path.Combine("GlucoDesk"),
            directoryPath,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetApplicationDirectoryPath_ShouldUseLocalApplicationData_OnWindows()
    {
        // Arrange
        var expectedBaseDirectory = OperatingSystem.IsWindows()
            ? Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData)
            : Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);

        // Act
        var directoryPath = LocalApplicationDataDirectory.GetApplicationDirectoryPath();

        // Assert
        Assert.StartsWith(
            expectedBaseDirectory,
            directoryPath,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetFilePath_ShouldReturnPathInsideApplicationDirectory()
    {
        // Act
        var filePath = LocalApplicationDataDirectory.GetFilePath("test.json");

        // Assert
        Assert.EndsWith(
            Path.Combine("GlucoDesk", "test.json"),
            filePath,
            StringComparison.OrdinalIgnoreCase);
    }

    [Fact]
    public void GetFilePath_ShouldThrow_WhenFileNameIsEmpty()
    {
        // Act
        var act = () => LocalApplicationDataDirectory.GetFilePath(" ");

        // Assert
        Assert.Throws<ArgumentException>(act);
    }
}