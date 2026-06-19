using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using GlucoDesk.Application.Cgm.WidgetState.Abstractions;
using GlucoDesk.Application.Cgm.WidgetState.Enums;
using GlucoDesk.Application.Cgm.WidgetState.Results;
using GlucoDesk.Application.Cgm.WidgetState.Snapshots;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Infrastructure.Cgm.WidgetState.Options;

namespace GlucoDesk.Infrastructure.Cgm.WidgetState.Stores;

/// <summary>
/// Stores the local glucose widget state in a JSON file.
/// </summary>
public sealed class JsonWidgetStateStore : IWidgetStateStore
{
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    private readonly LocalWidgetStateStorageOptions _options;
    private readonly SemaphoreSlim _storageLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonWidgetStateStore"/> class.
    /// </summary>
    /// <param name="options">The local widget state storage options.</param>
    public JsonWidgetStateStore(LocalWidgetStateStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <inheritdoc />
    public async Task<Result> SaveAsync(
        GlucoseWidgetState state,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(state);

        await _storageLock
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            await SaveStateAsync(state, cancellationToken)
                .ConfigureAwait(false);

            return Result.Success();
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            return Result.Failure(
                new Error("WidgetState.SaveFailed", "Unable to save widget state."));
        }
        finally
        {
            _storageLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseWidgetStateReadResult>> ReadAsync(
        CancellationToken cancellationToken)
    {
        await _storageLock
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var state = await LoadStateAsync(cancellationToken)
                .ConfigureAwait(false);

            return Result<GlucoseWidgetStateReadResult>.Success(
                state is null
                    ? GlucoseWidgetStateReadResult.Empty()
                    : new GlucoseWidgetStateReadResult(state));
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            return Result<GlucoseWidgetStateReadResult>.Failure(
                new Error("WidgetState.LoadFailed", "Unable to load widget state."));
        }
        finally
        {
            _storageLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<Result> ClearAsync(CancellationToken cancellationToken)
    {
        await _storageLock
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            DeleteFileSafely(_options.StateFilePath);
            DeleteFileSafely(BuildBackupFilePath(_options.StateFilePath));

            return Result.Success();
        }
        finally
        {
            _storageLock.Release();
        }
    }

    #region Helpers

    /// <summary>
    /// Loads the widget state from local storage, recovering from backup when possible.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The widget state, when available.</returns>
    private async Task<GlucoseWidgetState?> LoadStateAsync(CancellationToken cancellationToken)
    {
        var stateFilePath = _options.StateFilePath;
        var backupFilePath = BuildBackupFilePath(stateFilePath);

        if (File.Exists(stateFilePath))
        {
            var primaryState = await TryLoadStateFromPathAsync(
                    stateFilePath,
                    cancellationToken)
                .ConfigureAwait(false);

            if (primaryState is not null)
            {
                return primaryState;
            }

            QuarantineFileSafely(stateFilePath);
        }

        if (File.Exists(backupFilePath))
        {
            var backupState = await TryLoadStateFromPathAsync(
                    backupFilePath,
                    cancellationToken)
                .ConfigureAwait(false);

            if (backupState is not null)
            {
                RestoreBackupSafely(backupFilePath, stateFilePath);
                return backupState;
            }

            QuarantineFileSafely(backupFilePath);
        }

        return null;
    }

    /// <summary>
    /// Attempts to load the widget state from a file path.
    /// </summary>
    /// <param name="filePath">The state file path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The widget state, or null when the file is invalid.</returns>
    private static async Task<GlucoseWidgetState?> TryLoadStateFromPathAsync(
        string filePath,
        CancellationToken cancellationToken)
    {
        try
        {
            await using var stream = new FileStream(
                filePath,
                FileMode.Open,
                FileAccess.Read,
                FileShare.Read,
                bufferSize: 4096,
                useAsync: true);

            return await JsonSerializer
                .DeserializeAsync<GlucoseWidgetState>(
                    stream,
                    SerializerOptions,
                    cancellationToken)
                .ConfigureAwait(false);
        }
        catch (JsonException)
        {
            return null;
        }
        catch (ArgumentException)
        {
            return null;
        }
    }

    /// <summary>
    /// Saves the widget state using a temporary file and backup.
    /// </summary>
    /// <param name="state">The widget state.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task SaveStateAsync(
        GlucoseWidgetState state,
        CancellationToken cancellationToken)
    {
        EnsureStateDirectoryExists(_options.StateFilePath);

        var temporaryFilePath = BuildTemporaryFilePath(_options.StateFilePath);
        var backupFilePath = BuildBackupFilePath(_options.StateFilePath);

        try
        {
            await using (var stream = new FileStream(
                             temporaryFilePath,
                             FileMode.CreateNew,
                             FileAccess.Write,
                             FileShare.None,
                             bufferSize: 4096,
                             useAsync: true))
            {
                await JsonSerializer
                    .SerializeAsync(stream, state, SerializerOptions, cancellationToken)
                    .ConfigureAwait(false);

                await stream
                    .FlushAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            CreateBackupSafely(_options.StateFilePath, backupFilePath);

            File.Move(temporaryFilePath, _options.StateFilePath, overwrite: true);
        }
        finally
        {
            DeleteFileSafely(temporaryFilePath);
        }
    }

    /// <summary>
    /// Creates JSON serializer options used by the widget state store.
    /// </summary>
    /// <returns>The JSON serializer options.</returns>
    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter<GlucoseUnit>());
        options.Converters.Add(new JsonStringEnumConverter<TrendDirection>());
        options.Converters.Add(new JsonStringEnumConverter<CgmProviderKind>());
        options.Converters.Add(new JsonStringEnumConverter<GlucoseDataFreshness>());
        options.Converters.Add(new JsonStringEnumConverter<WidgetGlucoseStatusLevel>());

        return options;
    }

    /// <summary>
    /// Creates a backup of the current state file when it exists.
    /// </summary>
    /// <param name="stateFilePath">The current state file path.</param>
    /// <param name="backupFilePath">The backup file path.</param>
    private static void CreateBackupSafely(
        string stateFilePath,
        string backupFilePath)
    {
        if (!File.Exists(stateFilePath))
        {
            return;
        }

        File.Copy(stateFilePath, backupFilePath, overwrite: true);
    }

    /// <summary>
    /// Restores a backup file into the primary state file path.
    /// </summary>
    /// <param name="backupFilePath">The backup file path.</param>
    /// <param name="stateFilePath">The primary state file path.</param>
    private static void RestoreBackupSafely(
        string backupFilePath,
        string stateFilePath)
    {
        try
        {
            EnsureStateDirectoryExists(stateFilePath);
            File.Copy(backupFilePath, stateFilePath, overwrite: true);
        }
        catch
        {
            // Recovery is best-effort. The caller can still use the in-memory backup state.
        }
    }

    /// <summary>
    /// Moves an invalid state file to a timestamped quarantine file.
    /// </summary>
    /// <param name="filePath">The invalid file path.</param>
    private static void QuarantineFileSafely(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return;
            }

            var quarantinedFilePath = BuildCorruptFilePath(filePath);
            File.Move(filePath, quarantinedFilePath, overwrite: true);
        }
        catch
        {
            // Quarantine is best-effort and must never prevent the app from starting.
        }
    }

    /// <summary>
    /// Deletes a file without throwing when cleanup fails.
    /// </summary>
    /// <param name="filePath">The file path.</param>
    private static void DeleteFileSafely(string filePath)
    {
        try
        {
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }
        catch
        {
            // File cleanup is best-effort.
        }
    }

    /// <summary>
    /// Ensures the widget state directory exists.
    /// </summary>
    /// <param name="stateFilePath">The widget state file path.</param>
    private static void EnsureStateDirectoryExists(string stateFilePath)
    {
        var directoryPath = Path.GetDirectoryName(stateFilePath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return;
        }

        Directory.CreateDirectory(directoryPath);
    }

    /// <summary>
    /// Builds a temporary file path used for safer widget state writes.
    /// </summary>
    /// <param name="stateFilePath">The final state file path.</param>
    /// <returns>The temporary state file path.</returns>
    private static string BuildTemporaryFilePath(string stateFilePath)
    {
        return $"{stateFilePath}.{Guid.NewGuid():N}.tmp";
    }

    /// <summary>
    /// Builds the backup widget state file path.
    /// </summary>
    /// <param name="stateFilePath">The primary state file path.</param>
    /// <returns>The backup state file path.</returns>
    private static string BuildBackupFilePath(string stateFilePath)
    {
        return $"{stateFilePath}.bak";
    }

    /// <summary>
    /// Builds a timestamped corrupt file path.
    /// </summary>
    /// <param name="filePath">The invalid file path.</param>
    /// <returns>The corrupt file path.</returns>
    private static string BuildCorruptFilePath(string filePath)
    {
        var timestamp = DateTimeOffset.UtcNow.ToString(
            "yyyyMMddHHmmssfff",
            CultureInfo.InvariantCulture);

        return $"{filePath}.corrupt.{timestamp}";
    }

    /// <summary>
    /// Returns whether the exception is related to local widget state storage access.
    /// </summary>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns>True when the exception is storage-related; otherwise false.</returns>
    private static bool IsStorageException(Exception exception)
    {
        return exception is IOException or UnauthorizedAccessException or NotSupportedException;
    }

    #endregion
}