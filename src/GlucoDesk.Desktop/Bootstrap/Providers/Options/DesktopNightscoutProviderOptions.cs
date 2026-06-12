using GlucoDesk.Infrastructure.Cgm.Nightscout.Enums;
using GlucoDesk.Infrastructure.Cgm.Nightscout.Options;

namespace GlucoDesk.Desktop.Bootstrap.Providers.Options;

/// <summary>
/// Represents desktop runtime options for the Nightscout provider.
/// </summary>
public sealed record DesktopNightscoutProviderOptions
{
    private const string EnabledEnvironmentVariable = "GLUCODESK_NIGHTSCOUT_ENABLED";
    private const string BaseUriEnvironmentVariable = "GLUCODESK_NIGHTSCOUT_BASE_URI";
    private const string DisplayNameEnvironmentVariable = "GLUCODESK_NIGHTSCOUT_DISPLAY_NAME";
    private const string AuthenticationModeEnvironmentVariable = "GLUCODESK_NIGHTSCOUT_AUTH_MODE";
    private const string ApiSecretSha1EnvironmentVariable = "GLUCODESK_NIGHTSCOUT_API_SECRET_SHA1";
    private const string AccessTokenEnvironmentVariable = "GLUCODESK_NIGHTSCOUT_ACCESS_TOKEN";
    private const string LatestLookbackMinutesEnvironmentVariable = "GLUCODESK_NIGHTSCOUT_LATEST_LOOKBACK_MINUTES";
    private const string RequestTimeoutSecondsEnvironmentVariable = "GLUCODESK_NIGHTSCOUT_REQUEST_TIMEOUT_SECONDS";
    private const string MaxReadingsPerRequestEnvironmentVariable = "GLUCODESK_NIGHTSCOUT_MAX_READINGS_PER_REQUEST";

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopNightscoutProviderOptions"/> class.
    /// </summary>
    /// <param name="isEnabled">Whether Nightscout is enabled in the desktop runtime.</param>
    /// <param name="baseUri">The Nightscout base URI.</param>
    /// <param name="displayName">The provider display name.</param>
    /// <param name="authenticationMode">The Nightscout authentication mode.</param>
    /// <param name="apiSecretSha1">The optional SHA1-hashed API secret.</param>
    /// <param name="accessToken">The optional Nightscout access token.</param>
    /// <param name="latestReadingLookback">The latest reading lookback window.</param>
    /// <param name="requestTimeout">The HTTP request timeout.</param>
    /// <param name="maxReadingsPerRequest">The maximum number of readings requested from Nightscout.</param>
    public DesktopNightscoutProviderOptions(
        bool isEnabled,
        Uri? baseUri,
        string displayName = "Nightscout",
        NightscoutAuthenticationMode authenticationMode = NightscoutAuthenticationMode.None,
        string? apiSecretSha1 = null,
        string? accessToken = null,
        TimeSpan? latestReadingLookback = null,
        TimeSpan? requestTimeout = null,
        int maxReadingsPerRequest = 288)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Nightscout display name must be specified.", nameof(displayName));
        }

        if (latestReadingLookback is not null && latestReadingLookback <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(latestReadingLookback),
                latestReadingLookback,
                "Latest reading lookback must be greater than zero.");
        }

        if (requestTimeout is not null && requestTimeout <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(requestTimeout),
                requestTimeout,
                "Request timeout must be greater than zero.");
        }

        if (maxReadingsPerRequest <= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(maxReadingsPerRequest),
                maxReadingsPerRequest,
                "Maximum readings per request must be greater than zero.");
        }

        IsEnabled = isEnabled;
        BaseUri = baseUri;
        DisplayName = displayName.Trim();
        AuthenticationMode = authenticationMode;
        ApiSecretSha1 = NormalizeSecret(apiSecretSha1);
        AccessToken = NormalizeSecret(accessToken);
        LatestReadingLookback = latestReadingLookback ?? TimeSpan.FromMinutes(20);
        RequestTimeout = requestTimeout ?? TimeSpan.FromSeconds(15);
        MaxReadingsPerRequest = maxReadingsPerRequest;
    }

    /// <summary>
    /// Gets a value indicating whether Nightscout is enabled in the desktop runtime.
    /// </summary>
    public bool IsEnabled { get; }

    /// <summary>
    /// Gets the Nightscout base URI.
    /// </summary>
    public Uri? BaseUri { get; }

    /// <summary>
    /// Gets the provider display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the Nightscout authentication mode.
    /// </summary>
    public NightscoutAuthenticationMode AuthenticationMode { get; }

    /// <summary>
    /// Gets the optional SHA1-hashed API secret.
    /// </summary>
    public string? ApiSecretSha1 { get; }

    /// <summary>
    /// Gets the optional Nightscout access token.
    /// </summary>
    public string? AccessToken { get; }

    /// <summary>
    /// Gets the latest reading lookback window.
    /// </summary>
    public TimeSpan LatestReadingLookback { get; }

    /// <summary>
    /// Gets the HTTP request timeout.
    /// </summary>
    public TimeSpan RequestTimeout { get; }

    /// <summary>
    /// Gets the maximum number of readings requested from Nightscout.
    /// </summary>
    public int MaxReadingsPerRequest { get; }

    /// <summary>
    /// Creates desktop Nightscout options from environment variables.
    /// </summary>
    /// <returns>The desktop Nightscout options.</returns>
    public static DesktopNightscoutProviderOptions FromEnvironmentVariables()
    {
        var isEnabled = ParseBoolean(System.Environment.GetEnvironmentVariable(EnabledEnvironmentVariable));

        var baseUri = ParseUri(System.Environment.GetEnvironmentVariable(BaseUriEnvironmentVariable));
        var displayName = System.Environment.GetEnvironmentVariable(DisplayNameEnvironmentVariable) ?? "Nightscout";
        var authenticationMode = ParseAuthenticationMode(
            System.Environment.GetEnvironmentVariable(AuthenticationModeEnvironmentVariable));

        var latestReadingLookback = ParseMinutes(
            System.Environment.GetEnvironmentVariable(LatestLookbackMinutesEnvironmentVariable),
            TimeSpan.FromMinutes(20));

        var requestTimeout = ParseSeconds(
            System.Environment.GetEnvironmentVariable(RequestTimeoutSecondsEnvironmentVariable),
            TimeSpan.FromSeconds(15));

        var maxReadingsPerRequest = ParsePositiveInteger(
            System.Environment.GetEnvironmentVariable(MaxReadingsPerRequestEnvironmentVariable),
            defaultValue: 288);

        return new DesktopNightscoutProviderOptions(
            isEnabled,
            baseUri,
            displayName,
            authenticationMode,
            System.Environment.GetEnvironmentVariable(ApiSecretSha1EnvironmentVariable),
            System.Environment.GetEnvironmentVariable(AccessTokenEnvironmentVariable),
            latestReadingLookback,
            requestTimeout,
            maxReadingsPerRequest);
    }

    /// <summary>
    /// Converts the desktop options to infrastructure Nightscout options.
    /// </summary>
    /// <returns>The infrastructure Nightscout options.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provider is enabled but the base URI is missing.</exception>
    public NightscoutOptions ToNightscoutOptions()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Nightscout provider is not enabled.");
        }

        if (BaseUri is null)
        {
            throw new InvalidOperationException("Nightscout provider is enabled but the base URI is not configured.");
        }

        return new NightscoutOptions(
            BaseUri,
            DisplayName,
            AuthenticationMode,
            ApiSecretSha1,
            AccessToken,
            LatestReadingLookback,
            RequestTimeout,
            MaxReadingsPerRequest);
    }

    #region Helpers

    /// <summary>
    /// Parses a boolean environment variable.
    /// </summary>
    /// <param name="value">The environment variable value.</param>
    /// <returns>True when enabled; otherwise false.</returns>
    private static bool ParseBoolean(string? value)
    {
        return bool.TryParse(value, out var parsedValue) && parsedValue;
    }

    /// <summary>
    /// Parses a URI environment variable.
    /// </summary>
    /// <param name="value">The environment variable value.</param>
    /// <returns>The parsed URI, when valid.</returns>
    private static Uri? ParseUri(string? value)
    {
        return Uri.TryCreate(value, UriKind.Absolute, out var uri)
            ? uri
            : null;
    }

    /// <summary>
    /// Parses the Nightscout authentication mode.
    /// </summary>
    /// <param name="value">The environment variable value.</param>
    /// <returns>The parsed authentication mode.</returns>
    private static NightscoutAuthenticationMode ParseAuthenticationMode(string? value)
    {
        return Enum.TryParse<NightscoutAuthenticationMode>(
            value,
            ignoreCase: true,
            out var parsedValue)
            ? parsedValue
            : NightscoutAuthenticationMode.None;
    }

    /// <summary>
    /// Parses a duration expressed in minutes.
    /// </summary>
    /// <param name="value">The environment variable value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The parsed duration.</returns>
    private static TimeSpan ParseMinutes(string? value, TimeSpan defaultValue)
    {
        return int.TryParse(value, out var minutes) && minutes > 0
            ? TimeSpan.FromMinutes(minutes)
            : defaultValue;
    }

    /// <summary>
    /// Parses a duration expressed in seconds.
    /// </summary>
    /// <param name="value">The environment variable value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The parsed duration.</returns>
    private static TimeSpan ParseSeconds(string? value, TimeSpan defaultValue)
    {
        return int.TryParse(value, out var seconds) && seconds > 0
            ? TimeSpan.FromSeconds(seconds)
            : defaultValue;
    }

    /// <summary>
    /// Parses a positive integer environment variable.
    /// </summary>
    /// <param name="value">The environment variable value.</param>
    /// <param name="defaultValue">The default value.</param>
    /// <returns>The parsed integer.</returns>
    private static int ParsePositiveInteger(string? value, int defaultValue)
    {
        return int.TryParse(value, out var parsedValue) && parsedValue > 0
            ? parsedValue
            : defaultValue;
    }

    /// <summary>
    /// Normalizes optional secret values.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <returns>The normalized secret value.</returns>
    private static string? NormalizeSecret(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    #endregion
}