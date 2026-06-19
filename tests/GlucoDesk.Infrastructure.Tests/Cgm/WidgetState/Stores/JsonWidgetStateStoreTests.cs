using GlucoDesk.Application.Cgm.WidgetState.Enums;
using GlucoDesk.Application.Cgm.WidgetState.Snapshots;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Infrastructure.Cgm.WidgetState.Options;
using GlucoDesk.Infrastructure.Cgm.WidgetState.Stores;

namespace GlucoDesk.Infrastructure.Tests.Cgm.WidgetState.Stores;

public sealed class JsonWidgetStateStoreTests : IDisposable
{
    private readonly string _temporaryDirectoryPath;

    public JsonWidgetStateStoreTests()
    {
        _temporaryDirectoryPath = Path.Combine(
            Path.GetTempPath(),
            "GlucoDesk.WidgetState.Tests",
            Guid.NewGuid().ToString("N"));

        Directory.CreateDirectory(_temporaryDirectoryPath);
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnEmptyResult_WhenFileDoesNotExist()
    {
        // Arrange
        var store = CreateStore();

        // Act
        var result = await store.ReadAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.HasState);
        Assert.Null(result.Value.State);
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateWidgetStateFile()
    {
        // Arrange
        var store = CreateStore();
        var state = CreateState(120m);

        // Act
        var result = await store.SaveAsync(state, CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(File.Exists(BuildStateFilePath()));
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnSavedWidgetState()
    {
        // Arrange
        var store = CreateStore();
        var state = CreateState(120m);

        await store.SaveAsync(state, CancellationToken.None);

        // Act
        var result = await store.ReadAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasState);

        var restoredState = result.Value.State!;

        Assert.Equal(state.SchemaVersion, restoredState.SchemaVersion);
        Assert.Equal(state.GeneratedAt, restoredState.GeneratedAt);
        Assert.Equal(state.ReadingTimestamp, restoredState.ReadingTimestamp);
        Assert.Equal(state.ExpiresAt, restoredState.ExpiresAt);
        Assert.Equal(state.GlucoseAmount, restoredState.GlucoseAmount);
        Assert.Equal(state.GlucoseUnit, restoredState.GlucoseUnit);
        Assert.Equal(state.Trend, restoredState.Trend);
        Assert.Equal(state.ProviderKind, restoredState.ProviderKind);
        Assert.Equal(state.Freshness, restoredState.Freshness);
        Assert.Equal(state.StatusLevel, restoredState.StatusLevel);
        Assert.Equal(state.DisplayValue, restoredState.DisplayValue);
        Assert.Equal(state.UnitLabel, restoredState.UnitLabel);
        Assert.Equal(state.TrendLabel, restoredState.TrendLabel);
        Assert.Equal(state.StatusMessage, restoredState.StatusMessage);
    }

    [Fact]
    public async Task SaveAsync_ShouldCreateBackup_WhenStateFileAlreadyExists()
    {
        // Arrange
        var store = CreateStore();

        await store.SaveAsync(CreateState(120m), CancellationToken.None);

        // Act
        var result = await store.SaveAsync(CreateState(130m), CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(File.Exists(BuildStateFilePath()));
        Assert.True(File.Exists($"{BuildStateFilePath()}.bak"));
    }

    [Fact]
    public async Task ReadAsync_ShouldReturnEmptyResult_AndQuarantineFile_WhenJsonIsInvalid()
    {
        // Arrange
        var stateFilePath = BuildStateFilePath();

        await File.WriteAllTextAsync(
            stateFilePath,
            "{ invalid-json",
            CancellationToken.None);

        var store = CreateStore();

        // Act
        var result = await store.ReadAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(result.Value.HasState);
        Assert.False(File.Exists(stateFilePath));

        var quarantinedFiles = Directory.GetFiles(
            _temporaryDirectoryPath,
            "glucodesk-widget-state.json.corrupt.*");

        Assert.Single(quarantinedFiles);
    }

    [Fact]
    public async Task ReadAsync_ShouldRecoverFromBackup_WhenPrimaryJsonIsInvalid()
    {
        // Arrange
        var stateFilePath = BuildStateFilePath();
        var backupFilePath = $"{stateFilePath}.bak";

        await File.WriteAllTextAsync(
            stateFilePath,
            "{ invalid-json",
            CancellationToken.None);

        var backupJson = """
        {
          "schemaVersion": 1,
          "generatedAt": "2026-06-19T10:01:00+00:00",
          "readingTimestamp": "2026-06-19T10:00:00+00:00",
          "expiresAt": "2026-06-19T10:15:00+00:00",
          "glucoseAmount": 120,
          "glucoseUnit": "MgDl",
          "trend": "Flat",
          "providerKind": "DexcomShare",
          "freshness": "NearRealTime",
          "statusLevel": "InRange",
          "displayValue": "120",
          "unitLabel": "mg/dL",
          "trendLabel": "Flat",
          "statusMessage": "Glucose in range"
        }
        """;

        await File.WriteAllTextAsync(
            backupFilePath,
            backupJson,
            CancellationToken.None);

        var store = CreateStore();

        // Act
        var result = await store.ReadAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.True(result.Value.HasState);

        var state = result.Value.State!;

        Assert.Equal(120m, state.GlucoseAmount);
        Assert.Equal(WidgetGlucoseStatusLevel.InRange, state.StatusLevel);
        Assert.True(File.Exists(stateFilePath));
    }

    [Fact]
    public async Task ClearAsync_ShouldDeleteStateAndBackupFiles()
    {
        // Arrange
        var store = CreateStore();

        await store.SaveAsync(CreateState(120m), CancellationToken.None);
        await store.SaveAsync(CreateState(130m), CancellationToken.None);

        // Act
        var result = await store.ClearAsync(CancellationToken.None);

        // Assert
        Assert.True(result.IsSuccess);
        Assert.False(File.Exists(BuildStateFilePath()));
        Assert.False(File.Exists($"{BuildStateFilePath()}.bak"));
    }

    /// <inheritdoc />
    public void Dispose()
    {
        DeleteDirectorySafely(_temporaryDirectoryPath);
    }

    #region Helpers

    /// <summary>
    /// Creates a JSON widget state store for the current test.
    /// </summary>
    /// <returns>The JSON widget state store.</returns>
    private JsonWidgetStateStore CreateStore()
    {
        return new JsonWidgetStateStore(
            new LocalWidgetStateStorageOptions(BuildStateFilePath()));
    }

    /// <summary>
    /// Builds the widget state file path for the current test.
    /// </summary>
    /// <returns>The widget state file path.</returns>
    private string BuildStateFilePath()
    {
        return Path.Combine(_temporaryDirectoryPath, "glucodesk-widget-state.json");
    }

    /// <summary>
    /// Creates a widget state for tests.
    /// </summary>
    /// <param name="valueMgDl">The glucose value in mg/dL.</param>
    /// <returns>The widget state.</returns>
    private static GlucoseWidgetState CreateState(decimal valueMgDl)
    {
        var readingTimestamp = new DateTimeOffset(
            2026,
            6,
            19,
            10,
            0,
            0,
            TimeSpan.Zero);

        return new GlucoseWidgetState(
            1,
            readingTimestamp.AddMinutes(1),
            readingTimestamp,
            readingTimestamp.AddMinutes(15),
            valueMgDl,
            GlucoseUnit.MgDl,
            TrendDirection.Flat,
            CgmProviderKind.DexcomShare,
            GlucoseDataFreshness.NearRealTime,
            WidgetGlucoseStatusLevel.InRange,
            valueMgDl.ToString("0"),
            "mg/dL",
            "Flat",
            "Glucose in range");
    }

    /// <summary>
    /// Deletes a directory without failing the test when cleanup is not possible.
    /// </summary>
    /// <param name="directoryPath">The directory path.</param>
    private static void DeleteDirectorySafely(string directoryPath)
    {
        try
        {
            if (Directory.Exists(directoryPath))
            {
                Directory.Delete(directoryPath, recursive: true);
            }
        }
        catch
        {
            // Test cleanup is best-effort.
        }
    }

    #endregion
}