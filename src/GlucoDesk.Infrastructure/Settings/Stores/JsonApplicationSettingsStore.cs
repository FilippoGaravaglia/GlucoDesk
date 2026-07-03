using System.Text.Json;
using System.Text.Json.Serialization;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Application.Settings.Abstractions;
using GlucoDesk.Application.Settings.Models;
using GlucoDesk.Core.Glucose.Enums;
using GlucoDesk.Infrastructure.Settings.Options;

namespace GlucoDesk.Infrastructure.Settings.Stores;

/// <summary>
/// Stores GlucoDesk application settings in a local JSON file.
/// </summary>
public sealed class JsonApplicationSettingsStore : IApplicationSettingsStore
{
    private static readonly JsonSerializerOptions SerializerOptions = CreateSerializerOptions();

    private readonly LocalSettingsStorageOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="JsonApplicationSettingsStore"/> class.
    /// </summary>
    /// <param name="options">The local settings storage options.</param>
    public JsonApplicationSettingsStore(LocalSettingsStorageOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _options = options;
    }

    /// <inheritdoc />
    public async Task<Result<ApplicationSettings>> LoadAsync(CancellationToken cancellationToken)
    {
        try
        {
            if (!File.Exists(_options.SettingsFilePath))
            {
                return Result<ApplicationSettings>.Success(ApplicationSettings.Default);
            }

            await using var stream = File.OpenRead(_options.SettingsFilePath);

            var document = await JsonSerializer
                .DeserializeAsync<ApplicationSettingsDocument>(stream, SerializerOptions, cancellationToken)
                .ConfigureAwait(false);

            if (document is null)
            {
                return Result<ApplicationSettings>.Failure(
                    new Error("Settings.EmptyFile", "The settings file is empty or invalid."));
            }

            return Result<ApplicationSettings>.Success(document.ToApplicationSettings());
        }
        catch (JsonException)
        {
            return Result<ApplicationSettings>.Failure(
                new Error("Settings.InvalidFormat", "The settings file contains invalid JSON."));
        }
        catch (ArgumentException)
        {
            return Result<ApplicationSettings>.Failure(
                new Error("Settings.InvalidContent", "The settings file contains invalid settings values."));
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            return Result<ApplicationSettings>.Failure(
                new Error("Settings.LoadFailed", "Unable to load the settings file."));
        }
    }

    /// <inheritdoc />
    public async Task<Result> SaveAsync(
        ApplicationSettings settings,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(settings);

        try
        {
            EnsureSettingsDirectoryExists(_options.SettingsFilePath);

            var temporaryFilePath = BuildTemporaryFilePath(_options.SettingsFilePath);
            var document = ApplicationSettingsDocument.FromApplicationSettings(settings);

            await using (var stream = File.Create(temporaryFilePath))
            {
                await JsonSerializer
                    .SerializeAsync(stream, document, SerializerOptions, cancellationToken)
                    .ConfigureAwait(false);
            }

            File.Move(temporaryFilePath, _options.SettingsFilePath, overwrite: true);

            return Result.Success();
        }
        catch (Exception exception) when (IsStorageException(exception))
        {
            return Result.Failure(
                new Error("Settings.SaveFailed", "Unable to save the settings file."));
        }
    }

    #region Helpers

    /// <summary>
    /// Creates JSON serializer options used by the settings store.
    /// </summary>
    /// <returns>The JSON serializer options.</returns>
    private static JsonSerializerOptions CreateSerializerOptions()
    {
        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true
        };

        options.Converters.Add(new JsonStringEnumConverter<CgmProviderKind>());
        options.Converters.Add(new JsonStringEnumConverter<GlucoseUnit>());

        return options;
    }

    /// <summary>
    /// Ensures the settings directory exists.
    /// </summary>
    /// <param name="settingsFilePath">The settings file path.</param>
    private static void EnsureSettingsDirectoryExists(string settingsFilePath)
    {
        var directoryPath = Path.GetDirectoryName(settingsFilePath);

        if (string.IsNullOrWhiteSpace(directoryPath))
        {
            return;
        }

        Directory.CreateDirectory(directoryPath);
    }

    /// <summary>
    /// Builds a temporary file path used for atomic settings writes.
    /// </summary>
    /// <param name="settingsFilePath">The final settings file path.</param>
    /// <returns>The temporary settings file path.</returns>
    private static string BuildTemporaryFilePath(string settingsFilePath)
    {
        return $"{settingsFilePath}.{Guid.NewGuid():N}.tmp";
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

    private sealed record ApplicationSettingsDocument
    {
        /// <summary>
        /// Gets the active live CGM provider.
        /// </summary>
        public CgmProviderKind ActiveLiveProvider { get; init; } = CgmProviderKind.Mock;

        /// <summary>
        /// Gets the active historical CGM provider.
        /// </summary>
        public CgmProviderKind HistoricalProvider { get; init; } = CgmProviderKind.Mock;

        /// <summary>
        /// Gets the preferred glucose display unit.
        /// </summary>
        public GlucoseUnit PreferredUnit { get; init; } = GlucoseUnit.MgDl;

        /// <summary>
        /// Gets the lower glucose target expressed in mg/dL.
        /// </summary>
        public int TargetLowMgDl { get; init; } = 70;

        /// <summary>
        /// Gets the upper glucose target expressed in mg/dL.
        /// </summary>
        public int TargetHighMgDl { get; init; } = 180;

        /// <summary>
        /// Gets the maximum chart value expressed in mg/dL.
        /// </summary>
        public int ChartMaximumMgDl { get; init; } = 300;

        /// <summary>
        /// Gets the dashboard refresh interval.
        /// </summary>
        public TimeSpan DashboardRefreshInterval { get; init; } = TimeSpan.FromSeconds(30);

        /// <summary>
        /// Gets a value indicating whether in-app glucose awareness alerts are enabled.
        /// </summary>
        public bool GlucoseAlertsEnabled { get; init; } = true;

        /// <summary>
        /// Gets a value indicating whether below-target glucose awareness alerts are enabled.
        /// </summary>
        public bool LowGlucoseAlertsEnabled { get; init; } = true;

        /// <summary>
        /// Gets a value indicating whether above-target glucose awareness alerts are enabled.
        /// </summary>
        public bool HighGlucoseAlertsEnabled { get; init; } = true;

        /// <summary>
        /// Gets a value indicating whether native OS glucose awareness notifications are enabled.
        /// </summary>
        public bool NativeGlucoseNotificationsEnabled { get; init; }

        /// <summary>
        /// Gets a value indicating whether glucose alert privacy mode is enabled.
        /// </summary>
        public bool GlucoseAlertPrivacyModeEnabled { get; init; } = true;

        /// <summary>
        /// Gets the glucose alert repeat interval.
        /// </summary>
        public TimeSpan GlucoseAlertRepeatInterval { get; init; } =
            ApplicationSettings.DefaultGlucoseAlertRepeatInterval;

        /// <summary>
        /// Gets the number of consecutive out-of-range readings required before showing a glucose alert.
        /// </summary>
        public int GlucoseAlertRequiredConsecutiveReadings { get; init; } =
            ApplicationSettings.DefaultGlucoseAlertRequiredConsecutiveReadings;

        /// <summary>
        /// Creates a settings document from application settings.
        /// </summary>
        /// <param name="settings">The application settings.</param>
        /// <returns>The settings document.</returns>
        public static ApplicationSettingsDocument FromApplicationSettings(ApplicationSettings settings)
        {
            ArgumentNullException.ThrowIfNull(settings);

            return new ApplicationSettingsDocument
            {
                ActiveLiveProvider = settings.ActiveLiveProvider,
                HistoricalProvider = settings.HistoricalProvider,
                PreferredUnit = settings.PreferredUnit,
                TargetLowMgDl = settings.TargetLowMgDl,
                TargetHighMgDl = settings.TargetHighMgDl,
                DashboardRefreshInterval = settings.DashboardRefreshInterval,
                ChartMaximumMgDl = settings.ChartMaximumMgDl,
                GlucoseAlertsEnabled = settings.GlucoseAlertsEnabled,
                LowGlucoseAlertsEnabled = settings.LowGlucoseAlertsEnabled,
                HighGlucoseAlertsEnabled = settings.HighGlucoseAlertsEnabled,
                NativeGlucoseNotificationsEnabled = settings.NativeGlucoseNotificationsEnabled,
                GlucoseAlertPrivacyModeEnabled = settings.GlucoseAlertPrivacyModeEnabled,
                GlucoseAlertRepeatInterval = settings.GlucoseAlertRepeatInterval,
                GlucoseAlertRequiredConsecutiveReadings = settings.GlucoseAlertRequiredConsecutiveReadings
            };
        }

        /// <summary>
        /// Converts the settings document to application settings.
        /// </summary>
        /// <returns>The application settings.</returns>
        public ApplicationSettings ToApplicationSettings()
        {
            return new ApplicationSettings(
                activeLiveProvider: ActiveLiveProvider,
                historicalProvider: HistoricalProvider,
                preferredUnit: PreferredUnit,
                targetLowMgDl: TargetLowMgDl,
                targetHighMgDl: TargetHighMgDl,
                dashboardRefreshInterval: DashboardRefreshInterval,
                chartMaximumMgDl: NormalizeChartMaximumMgDl(ChartMaximumMgDl),
                glucoseAlertsEnabled: GlucoseAlertsEnabled,
                lowGlucoseAlertsEnabled: LowGlucoseAlertsEnabled,
                highGlucoseAlertsEnabled: HighGlucoseAlertsEnabled,
                nativeGlucoseNotificationsEnabled: NativeGlucoseNotificationsEnabled,
                glucoseAlertPrivacyModeEnabled: GlucoseAlertPrivacyModeEnabled,
                glucoseAlertRepeatInterval: NormalizeGlucoseAlertRepeatInterval(GlucoseAlertRepeatInterval),
                glucoseAlertRequiredConsecutiveReadings: NormalizeGlucoseAlertRequiredConsecutiveReadings(
                    GlucoseAlertRequiredConsecutiveReadings));
        }

        /// <summary>
        /// Normalizes persisted chart maximum values to supported options.
        /// </summary>
        /// <param name="chartMaximumMgDl">The persisted chart maximum value.</param>
        /// <returns>The normalized chart maximum value.</returns>
        private static int NormalizeChartMaximumMgDl(int chartMaximumMgDl)
        {
            return chartMaximumMgDl is 400
                ? 400
                : 300;
        }

        /// <summary>
        /// Normalizes persisted glucose alert repeat intervals to supported options.
        /// </summary>
        /// <param name="repeatInterval">The persisted repeat interval.</param>
        /// <returns>The normalized repeat interval.</returns>
        private static TimeSpan NormalizeGlucoseAlertRepeatInterval(TimeSpan repeatInterval)
        {
            if (repeatInterval < ApplicationSettings.MinimumGlucoseAlertRepeatInterval ||
                repeatInterval > ApplicationSettings.MaximumGlucoseAlertRepeatInterval)
            {
                return ApplicationSettings.DefaultGlucoseAlertRepeatInterval;
            }

            return repeatInterval;
        }

        /// <summary>
        /// Normalizes the persisted glucose alert stability value.
        /// </summary>
        /// <param name="requiredConsecutiveReadings">The persisted required consecutive readings.</param>
        /// <returns>The normalized required consecutive readings.</returns>
        private static int NormalizeGlucoseAlertRequiredConsecutiveReadings(int requiredConsecutiveReadings)
        {
            if (requiredConsecutiveReadings is < ApplicationSettings.MinimumGlucoseAlertRequiredConsecutiveReadings or > ApplicationSettings.MaximumGlucoseAlertRequiredConsecutiveReadings)
            {
                return ApplicationSettings.DefaultGlucoseAlertRequiredConsecutiveReadings;
            }

            return requiredConsecutiveReadings;
        }
    }
}
