using System.Globalization;
using System.Text.Json;
using System.Text.Json.Serialization;
using GlucoDesk.Application.Cgm.History.Abstractions;
using GlucoDesk.Application.Cgm.History.Requests;
using GlucoDesk.Application.Cgm.History.Results;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Core.Glucose.Readings;
using GlucoDesk.Core.Glucose.ValueObjects;
using GlucoDesk.Infrastructure.Cgm.History.Options;

namespace GlucoDesk.Infrastructure.Cgm.History.Stores;

/// <summary>
/// Stores local glucose history in a JSON file.
/// </summary>
public sealed class JsonGlucoseHistoryStore : IGlucoseHistoryStore
{
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    private readonly LocalGlucoseHistoryStorageOptions _options;
    private readonly SemaphoreSlim _storageLock = new(1, 1);

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonGlucoseHistoryStore"/> class.
    /// </summary>
    /// <param name="options">The local glucose history storage options.</param>
    public JsonGlucoseHistoryStore(LocalGlucoseHistoryStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <inheritdoc />
    public async Task<Result> SaveReadingsAsync(
        IReadOnlyCollection<GlucoseReading> readings,
        CancellationToken cancellationToken)
    {
        var result = await SaveReadingsWithSummaryAsync(readings, cancellationToken)
            .ConfigureAwait(false);

        return result.IsSuccess
            ? Result.Success()
            : Result.Failure(result.Error);
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseHistorySaveResult>> SaveReadingsWithSummaryAsync(
        IReadOnlyCollection<GlucoseReading> readings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(readings);

        await _storageLock
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            if (readings.Count == 0)
            {
                var storedReadingsCount = await GetStoredReadingsCountSafelyAsync(cancellationToken)
                    .ConfigureAwait(false);

                return Result<GlucoseHistorySaveResult>.Success(
                    new GlucoseHistorySaveResult(
                        CgmProviderKind.Unknown,
                        0,
                        0,
                        0,
                        storedReadingsCount));
            }

            var existingDocuments = await LoadDocumentsAsync(cancellationToken)
                .ConfigureAwait(false);

            var existingDocumentsByKey = existingDocuments
                .GroupBy(document => document.BuildIdentityKey(), StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Last(),
                    StringComparer.Ordinal);

            var incomingDocuments = readings
                .Select(GlucoseHistoryDocument.FromReading)
                .ToArray();

            var providerKind = ResolveBatchProviderKind(incomingDocuments);

            var incomingDistinctDocumentsByKey = incomingDocuments
                .GroupBy(document => document.BuildIdentityKey(), StringComparer.Ordinal)
                .ToDictionary(
                    group => group.Key,
                    group => group.Last(),
                    StringComparer.Ordinal);

            var incomingDuplicateCount = incomingDocuments.Length - incomingDistinctDocumentsByKey.Count;

            var addedReadingsCount = 0;
            var existingDuplicateCount = 0;

            foreach (var incomingDocument in incomingDistinctDocumentsByKey)
            {
                if (existingDocumentsByKey.ContainsKey(incomingDocument.Key))
                {
                    existingDuplicateCount++;
                }
                else
                {
                    addedReadingsCount++;
                }

                existingDocumentsByKey[incomingDocument.Key] = incomingDocument.Value;
            }

            var mergedDocuments = NormalizeDocuments(existingDocumentsByKey.Values);

            await SaveDocumentsAsync(mergedDocuments, cancellationToken)
                .ConfigureAwait(false);

            var saveResult = new GlucoseHistorySaveResult(
                providerKind,
                incomingDocuments.Length,
                addedReadingsCount,
                incomingDuplicateCount + existingDuplicateCount,
                mergedDocuments.Count);

            return Result<GlucoseHistorySaveResult>.Success(saveResult);
        }
        catch (JsonException)
        {
            return Result<GlucoseHistorySaveResult>.Failure(
                new Error("History.InvalidFormat", "The glucose history file contains invalid JSON."));
        }
        catch (ArgumentException)
        {
            return Result<GlucoseHistorySaveResult>.Failure(
                new Error("History.InvalidContent", "The glucose history file contains invalid glucose readings."));
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            return Result<GlucoseHistorySaveResult>.Failure(
                new Error("History.SaveFailed", "Unable to save glucose history."));
        }
        finally
        {
            _storageLock.Release();
        }
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseHistoryResult>> GetReadingsAsync(
        GlucoseHistoryRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        await _storageLock
            .WaitAsync(cancellationToken)
            .ConfigureAwait(false);

        try
        {
            var documents = await LoadDocumentsAsync(cancellationToken)
                .ConfigureAwait(false);

            var readings = documents
                .Where(document => document.Timestamp >= request.From && document.Timestamp <= request.To)
                .OrderBy(document => document.Timestamp)
                .ThenBy(document => document.ProviderKind.ToString(), StringComparer.Ordinal)
                .Select(document => document.ToReading())
                .ToArray();

            return Result<GlucoseHistoryResult>.Success(new GlucoseHistoryResult(readings));
        }
        catch (JsonException)
        {
            return Result<GlucoseHistoryResult>.Failure(
                new Error("History.InvalidFormat", "The glucose history file contains invalid JSON."));
        }
        catch (ArgumentException)
        {
            return Result<GlucoseHistoryResult>.Failure(
                new Error("History.InvalidContent", "The glucose history file contains invalid glucose readings."));
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            return Result<GlucoseHistoryResult>.Failure(
                new Error("History.LoadFailed", "Unable to load glucose history."));
        }
        finally
        {
            _storageLock.Release();
        }
    }

    #region Helpers

    /// <summary>
    /// Loads all glucose history documents from local storage, recovering from backup when possible.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stored glucose history documents.</returns>
    private async Task<IReadOnlyCollection<GlucoseHistoryDocument>> LoadDocumentsAsync(
        CancellationToken cancellationToken)
    {
        var historyFilePath = _options.HistoryFilePath;
        var backupFilePath = BuildBackupFilePath(historyFilePath);

        if (File.Exists(historyFilePath))
        {
            var primaryDocuments = await TryLoadDocumentsFromPathAsync(
                    historyFilePath,
                    cancellationToken)
                .ConfigureAwait(false);

            if (primaryDocuments is not null)
            {
                return primaryDocuments;
            }

            QuarantineFileSafely(historyFilePath);
        }

        if (File.Exists(backupFilePath))
        {
            var backupDocuments = await TryLoadDocumentsFromPathAsync(
                    backupFilePath,
                    cancellationToken)
                .ConfigureAwait(false);

            if (backupDocuments is not null)
            {
                RestoreBackupSafely(backupFilePath, historyFilePath);
                return backupDocuments;
            }

            QuarantineFileSafely(backupFilePath);
        }

        return [];
    }

    /// <summary>
    /// Attempts to load glucose history documents from a file path.
    /// </summary>
    /// <param name="filePath">The history file path.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The loaded documents, or null when the file is invalid.</returns>
    private static async Task<IReadOnlyCollection<GlucoseHistoryDocument>?> TryLoadDocumentsFromPathAsync(
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

            var documents = await JsonSerializer
                .DeserializeAsync<IReadOnlyCollection<GlucoseHistoryDocument>>(
                    stream,
                    SerializerOptions,
                    cancellationToken)
                .ConfigureAwait(false);

            var normalizedDocuments = NormalizeDocuments(documents ?? []);

            ValidateDocuments(normalizedDocuments);

            return normalizedDocuments;
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
    /// Saves glucose history documents to local storage using a temporary file and a backup of the previous file.
    /// </summary>
    /// <param name="documents">The documents to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task SaveDocumentsAsync(
        IReadOnlyCollection<GlucoseHistoryDocument> documents,
        CancellationToken cancellationToken)
    {
        EnsureHistoryDirectoryExists(_options.HistoryFilePath);

        var normalizedDocuments = NormalizeDocuments(documents);
        var temporaryFilePath = BuildTemporaryFilePath(_options.HistoryFilePath);
        var backupFilePath = BuildBackupFilePath(_options.HistoryFilePath);

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
                    .SerializeAsync(stream, normalizedDocuments, SerializerOptions, cancellationToken)
                    .ConfigureAwait(false);

                await stream
                    .FlushAsync(cancellationToken)
                    .ConfigureAwait(false);
            }

            CreateBackupSafely(_options.HistoryFilePath, backupFilePath);

            File.Move(temporaryFilePath, _options.HistoryFilePath, overwrite: true);
        }
        finally
        {
            DeleteFileSafely(temporaryFilePath);
        }
    }

    /// <summary>
    /// Gets the current stored readings count without failing the empty-readings save path.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stored readings count.</returns>
    private async Task<int> GetStoredReadingsCountSafelyAsync(CancellationToken cancellationToken)
    {
        try
        {
            var documents = await LoadDocumentsAsync(cancellationToken)
                .ConfigureAwait(false);

            return documents.Count;
        }
        catch
        {
            return 0;
        }
    }

    /// <summary>
    /// Creates a backup of the current history file when it exists.
    /// </summary>
    /// <param name="historyFilePath">The current history file path.</param>
    /// <param name="backupFilePath">The backup file path.</param>
    private static void CreateBackupSafely(
        string historyFilePath,
        string backupFilePath)
    {
        if (!File.Exists(historyFilePath))
        {
            return;
        }

        File.Copy(historyFilePath, backupFilePath, overwrite: true);
    }

    /// <summary>
    /// Restores a backup file into the primary history file path.
    /// </summary>
    /// <param name="backupFilePath">The backup file path.</param>
    /// <param name="historyFilePath">The primary history file path.</param>
    private static void RestoreBackupSafely(
        string backupFilePath,
        string historyFilePath)
    {
        try
        {
            EnsureHistoryDirectoryExists(historyFilePath);
            File.Copy(backupFilePath, historyFilePath, overwrite: true);
        }
        catch
        {
            // Recovery is best-effort. The caller can still use the in-memory backup data.
        }
    }

    /// <summary>
    /// Moves an invalid history file to a timestamped quarantine file.
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
            // Temporary file cleanup is best-effort.
        }
    }

    /// <summary>
    /// Normalizes glucose history documents by de-duplicating and ordering them.
    /// </summary>
    /// <param name="documents">The documents to normalize.</param>
    /// <returns>The normalized documents.</returns>
    private static IReadOnlyCollection<GlucoseHistoryDocument> NormalizeDocuments(
        IEnumerable<GlucoseHistoryDocument> documents)
    {
        ArgumentNullException.ThrowIfNull(documents);

        return documents
            .GroupBy(document => document.BuildIdentityKey(), StringComparer.Ordinal)
            .Select(group => group.Last())
            .OrderBy(document => document.Timestamp)
            .ThenBy(document => document.ProviderKind.ToString(), StringComparer.Ordinal)
            .ToArray();
    }

    /// <summary>
    /// Validates glucose history documents by converting them to domain readings.
    /// </summary>
    /// <param name="documents">The documents to validate.</param>
    private static void ValidateDocuments(IReadOnlyCollection<GlucoseHistoryDocument> documents)
    {
        foreach (var document in documents)
        {
            _ = document.ToReading();
        }
    }

    /// <summary>
    /// Resolves the provider kind represented by an incoming batch.
    /// </summary>
    /// <param name="documents">The incoming documents.</param>
    /// <returns>The provider kind, or Unknown when the batch contains multiple providers.</returns>
    private static CgmProviderKind ResolveBatchProviderKind(
        IReadOnlyCollection<GlucoseHistoryDocument> documents)
    {
        if (documents.Count == 0)
        {
            return CgmProviderKind.Unknown;
        }

        var distinctProviders = documents
            .Select(document => document.ProviderKind)
            .Distinct()
            .ToArray();

        return distinctProviders.Length == 1
            ? distinctProviders[0]
            : CgmProviderKind.Unknown;
    }

    /// <summary>
    /// Creates JSON serializer options used by the glucose history store.
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

        return options;
    }

    /// <summary>
    /// Ensures the history directory exists.
    /// </summary>
    /// <param name="historyFilePath">The glucose history file path.</param>
    private static void EnsureHistoryDirectoryExists(string historyFilePath)
    {
        var directoryPath = Path.GetDirectoryName(historyFilePath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return;
        }

        Directory.CreateDirectory(directoryPath);
    }

    /// <summary>
    /// Builds a temporary file path used for safer history writes.
    /// </summary>
    /// <param name="historyFilePath">The final history file path.</param>
    /// <returns>The temporary history file path.</returns>
    private static string BuildTemporaryFilePath(string historyFilePath)
    {
        return $"{historyFilePath}.{Guid.NewGuid():N}.tmp";
    }

    /// <summary>
    /// Builds the backup history file path.
    /// </summary>
    /// <param name="historyFilePath">The primary history file path.</param>
    /// <returns>The backup history file path.</returns>
    private static string BuildBackupFilePath(string historyFilePath)
    {
        return $"{historyFilePath}.bak";
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
    /// Returns whether the exception is related to local storage access.
    /// </summary>
    /// <param name="exception">The exception to evaluate.</param>
    /// <returns>True when the exception is storage-related; otherwise false.</returns>
    private static bool IsStorageException(Exception exception)
    {
        return exception is IOException or UnauthorizedAccessException or NotSupportedException;
    }

    #endregion

    private sealed record GlucoseHistoryDocument
    {
        /// <summary>
        /// Gets the reading timestamp.
        /// </summary>
        public DateTimeOffset Timestamp { get; init; }

        /// <summary>
        /// Gets the glucose value amount.
        /// </summary>
        public decimal Amount { get; init; }

        /// <summary>
        /// Gets the glucose unit.
        /// </summary>
        public GlucoseUnit Unit { get; init; }

        /// <summary>
        /// Gets the trend direction.
        /// </summary>
        public TrendDirection Trend { get; init; }

        /// <summary>
        /// Gets the CGM provider kind.
        /// </summary>
        public CgmProviderKind ProviderKind { get; init; }

        /// <summary>
        /// Gets the glucose data freshness.
        /// </summary>
        public GlucoseDataFreshness Freshness { get; init; }

        /// <summary>
        /// Creates a history document from a glucose reading.
        /// </summary>
        /// <param name="reading">The glucose reading.</param>
        /// <returns>The glucose history document.</returns>
        public static GlucoseHistoryDocument FromReading(GlucoseReading reading)
        {
            ArgumentNullException.ThrowIfNull(reading);

            return new GlucoseHistoryDocument
            {
                Timestamp = reading.Timestamp,
                Amount = reading.Value.Amount,
                Unit = reading.Value.Unit,
                Trend = reading.Trend,
                ProviderKind = reading.Provider,
                Freshness = reading.Freshness
            };
        }

        /// <summary>
        /// Converts the document to a glucose reading.
        /// </summary>
        /// <returns>The glucose reading.</returns>
        public GlucoseReading ToReading()
        {
            return new GlucoseReading(
                Timestamp,
                new GlucoseValue(Amount, Unit),
                Trend,
                ProviderKind,
                Freshness);
        }

        /// <summary>
        /// Builds a stable identity key used to de-duplicate readings.
        /// </summary>
        /// <returns>The reading identity key.</returns>
        public string BuildIdentityKey()
        {
            return string.Join(
                "|",
                Timestamp.ToUniversalTime().Ticks.ToString(CultureInfo.InvariantCulture),
                ProviderKind.ToString());
        }
    }
}