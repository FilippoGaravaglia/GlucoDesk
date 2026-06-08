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
        ArgumentNullException.ThrowIfNull(readings);

        if (readings.Count == 0)
        {
            return Result.Success();
        }

        try
        {
            var existingDocuments = await LoadDocumentsAsync(cancellationToken)
                .ConfigureAwait(false);

            var newDocuments = readings
                .Select(GlucoseHistoryDocument.FromReading);

            var mergedDocuments = existingDocuments
                .Concat(newDocuments)
                .GroupBy(document => document.BuildIdentityKey(), StringComparer.Ordinal)
                .Select(group => group.Last())
                .OrderBy(document => document.Timestamp)
                .ToArray();

            await SaveDocumentsAsync(mergedDocuments, cancellationToken)
                .ConfigureAwait(false);

            return Result.Success();
        }
        catch (JsonException)
        {
            return Result.Failure(
                new Error("History.InvalidFormat", "The glucose history file contains invalid JSON."));
        }
        catch (ArgumentException)
        {
            return Result.Failure(
                new Error("History.InvalidContent", "The glucose history file contains invalid glucose readings."));
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            return Result.Failure(
                new Error("History.SaveFailed", "Unable to save glucose history."));
        }
    }

    /// <inheritdoc />
    public async Task<Result<GlucoseHistoryResult>> GetReadingsAsync(
        GlucoseHistoryRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        try
        {
            if (!File.Exists(_options.HistoryFilePath))
            {
                return Result<GlucoseHistoryResult>.Success(new GlucoseHistoryResult([]));
            }

            var documents = await LoadDocumentsAsync(cancellationToken)
                .ConfigureAwait(false);

            var readings = documents
                .Where(document => document.Timestamp >= request.From && document.Timestamp <= request.To)
                .OrderBy(document => document.Timestamp)
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
    }

    #region Helpers

    /// <summary>
    /// Loads all glucose history documents from local storage.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The stored glucose history documents.</returns>
    private async Task<IReadOnlyCollection<GlucoseHistoryDocument>> LoadDocumentsAsync(
        CancellationToken cancellationToken)
    {
        if (!File.Exists(_options.HistoryFilePath))
        {
            return [];
        }

        await using var stream = File.OpenRead(_options.HistoryFilePath);

        var documents = await JsonSerializer
            .DeserializeAsync<IReadOnlyCollection<GlucoseHistoryDocument>>(
                stream,
                SerializerOptions,
                cancellationToken)
            .ConfigureAwait(false);

        return documents ?? [];
    }

    /// <summary>
    /// Saves glucose history documents to local storage using an atomic replacement.
    /// </summary>
    /// <param name="documents">The documents to save.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private async Task SaveDocumentsAsync(
        IReadOnlyCollection<GlucoseHistoryDocument> documents,
        CancellationToken cancellationToken)
    {
        EnsureHistoryDirectoryExists(_options.HistoryFilePath);

        var temporaryFilePath = BuildTemporaryFilePath(_options.HistoryFilePath);

        await using (var stream = File.Create(temporaryFilePath))
        {
            await JsonSerializer
                .SerializeAsync(stream, documents, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);
        }

        File.Move(temporaryFilePath, _options.HistoryFilePath, overwrite: true);
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
    /// Builds a temporary file path used for atomic history writes.
    /// </summary>
    /// <param name="historyFilePath">The final history file path.</param>
    /// <returns>The temporary history file path.</returns>
    private static string BuildTemporaryFilePath(string historyFilePath)
    {
        return $"{historyFilePath}.{Guid.NewGuid():N}.tmp";
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