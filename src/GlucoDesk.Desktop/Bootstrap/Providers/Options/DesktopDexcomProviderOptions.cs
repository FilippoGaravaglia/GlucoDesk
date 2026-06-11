using GlucoDesk.Infrastructure.Cgm.Dexcom.Enums;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Providers.Options;

namespace GlucoDesk.Desktop.Bootstrap.Providers.Options;

/// <summary>
/// Represents desktop bootstrap options used to optionally register the Dexcom Official CGM provider.
/// </summary>
public sealed record DesktopDexcomProviderOptions
{
    private static readonly string[] DefaultScopes =
    [
        "egv",
        "offline_access"
    ];

    /// <summary>
    /// Initializes a new instance of the <see cref="DesktopDexcomProviderOptions"/> class.
    /// </summary>
    /// <param name="isEnabled">Whether Dexcom provider registration is enabled.</param>
    /// <param name="environment">The Dexcom API environment.</param>
    /// <param name="clientId">The Dexcom application client id.</param>
    /// <param name="clientSecret">The Dexcom application client secret.</param>
    /// <param name="redirectUri">The Dexcom OAuth redirect URI.</param>
    /// <param name="scopes">The Dexcom OAuth scopes.</param>
    /// <param name="latestReadingLookback">The latest reading lookback window.</param>
    /// <param name="displayName">The provider display name.</param>
    public DesktopDexcomProviderOptions(
        bool isEnabled = false,
        DexcomApiEnvironment environment = DexcomApiEnvironment.Sandbox,
        string? clientId = null,
        string? clientSecret = null,
        Uri? redirectUri = null,
        IReadOnlyCollection<string>? scopes = null,
        TimeSpan? latestReadingLookback = null,
        string displayName = "Dexcom Official API")
    {
        var effectiveScopes = NormalizeScopes(scopes ?? DefaultScopes);
        var effectiveRedirectUri = redirectUri ?? new Uri("http://127.0.0.1:51234/callback");

        if (!Enum.IsDefined(environment))
        {
            throw new ArgumentException("Dexcom API environment is not valid.", nameof(environment));
        }

        if (string.IsNullOrWhiteSpace(displayName))
        {
            throw new ArgumentException("Provider display name must be specified.", nameof(displayName));
        }

        if (latestReadingLookback is not null && latestReadingLookback <= TimeSpan.Zero)
        {
            throw new ArgumentOutOfRangeException(
                nameof(latestReadingLookback),
                latestReadingLookback,
                "Latest reading lookback must be greater than zero.");
        }

        if (latestReadingLookback is not null && latestReadingLookback > TimeSpan.FromDays(30))
        {
            throw new ArgumentOutOfRangeException(
                nameof(latestReadingLookback),
                latestReadingLookback,
                "Latest reading lookback cannot exceed 30 days.");
        }

        if (isEnabled)
        {
            ValidateEnabledOptions(clientId, clientSecret, effectiveRedirectUri, effectiveScopes);
        }

        IsEnabled = isEnabled;
        Environment = environment;
        ClientId = NormalizeOptionalValue(clientId);
        ClientSecret = NormalizeOptionalValue(clientSecret);
        RedirectUri = effectiveRedirectUri;
        Scopes = effectiveScopes;
        LatestReadingLookback = latestReadingLookback ?? TimeSpan.FromHours(24);
        DisplayName = displayName.Trim();
    }

    /// <summary>
    /// Gets disabled Dexcom desktop provider options.
    /// </summary>
    public static DesktopDexcomProviderOptions Disabled { get; } = new();

    /// <summary>
    /// Gets a value indicating whether Dexcom provider registration is enabled.
    /// </summary>
    public bool IsEnabled { get; }

    /// <summary>
    /// Gets the Dexcom API environment.
    /// </summary>
    public DexcomApiEnvironment Environment { get; }

    /// <summary>
    /// Gets the Dexcom application client id.
    /// </summary>
    public string? ClientId { get; }

    /// <summary>
    /// Gets the Dexcom application client secret.
    /// </summary>
    public string? ClientSecret { get; }

    /// <summary>
    /// Gets the Dexcom OAuth redirect URI.
    /// </summary>
    public Uri RedirectUri { get; }

    /// <summary>
    /// Gets the Dexcom OAuth scopes.
    /// </summary>
    public IReadOnlyCollection<string> Scopes { get; }

    /// <summary>
    /// Gets the latest reading lookback window.
    /// </summary>
    public TimeSpan LatestReadingLookback { get; }

    /// <summary>
    /// Gets the provider display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Creates desktop Dexcom provider options from environment variables.
    /// </summary>
    /// <returns>The desktop Dexcom provider options.</returns>
    public static DesktopDexcomProviderOptions FromEnvironmentVariables()
    {
        var isEnabled = IsTruthy(System.Environment.GetEnvironmentVariable("GLUCODESK_DEXCOM_ENABLED"));
    
        if (!isEnabled)
        {
            return Disabled;
        }
    
        return new DesktopDexcomProviderOptions(
            isEnabled: true,
            environment: ParseEnvironment(System.Environment.GetEnvironmentVariable("GLUCODESK_DEXCOM_ENVIRONMENT")),
            clientId: System.Environment.GetEnvironmentVariable("GLUCODESK_DEXCOM_CLIENT_ID"),
            clientSecret: System.Environment.GetEnvironmentVariable("GLUCODESK_DEXCOM_CLIENT_SECRET"),
            redirectUri: ParseRedirectUri(System.Environment.GetEnvironmentVariable("GLUCODESK_DEXCOM_REDIRECT_URI")),
            scopes: ParseScopes(System.Environment.GetEnvironmentVariable("GLUCODESK_DEXCOM_SCOPES")),
            latestReadingLookback: ParseLatestReadingLookback(System.Environment.GetEnvironmentVariable("GLUCODESK_DEXCOM_LATEST_LOOKBACK_MINUTES")),
            displayName: System.Environment.GetEnvironmentVariable("GLUCODESK_DEXCOM_DISPLAY_NAME") ?? "Dexcom Official API");
    }
    /// <summary>
    /// Creates Dexcom API options.
    /// </summary>
    /// <returns>The Dexcom API options.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provider is not enabled.</exception>
    public DexcomApiOptions ToApiOptions()
    {
        EnsureEnabled();

        return new DexcomApiOptions(
            Environment,
            ClientId!,
            RedirectUri,
            Scopes);
    }

    /// <summary>
    /// Creates Dexcom CGM provider options.
    /// </summary>
    /// <returns>The Dexcom CGM provider options.</returns>
    /// <exception cref="InvalidOperationException">Thrown when the provider is not enabled.</exception>
    public DexcomCgmProviderOptions ToProviderOptions()
    {
        EnsureEnabled();

        return new DexcomCgmProviderOptions(
            ClientSecret,
            LatestReadingLookback,
            DisplayName);
    }

    #region Helpers

    /// <summary>
    /// Ensures the provider options are enabled before conversion.
    /// </summary>
    /// <exception cref="InvalidOperationException">Thrown when the provider is not enabled.</exception>
    private void EnsureEnabled()
    {
        if (!IsEnabled)
        {
            throw new InvalidOperationException("Dexcom provider options are disabled.");
        }
    }

    /// <summary>
    /// Validates options required when Dexcom provider registration is enabled.
    /// </summary>
    /// <param name="clientId">The Dexcom client id.</param>
    /// <param name="clientSecret">The Dexcom client secret.</param>
    /// <param name="redirectUri">The redirect URI.</param>
    /// <param name="scopes">The OAuth scopes.</param>
    /// <exception cref="ArgumentException">Thrown when a required value is invalid.</exception>
    private static void ValidateEnabledOptions(
        string? clientId,
        string? clientSecret,
        Uri redirectUri,
        IReadOnlyCollection<string> scopes)
    {
        if (string.IsNullOrWhiteSpace(clientId))
        {
            throw new ArgumentException("Dexcom client id must be specified when Dexcom is enabled.", nameof(clientId));
        }

        if (string.IsNullOrWhiteSpace(clientSecret))
        {
            throw new ArgumentException("Dexcom client secret must be specified when Dexcom is enabled.", nameof(clientSecret));
        }

        if (!redirectUri.IsAbsoluteUri)
        {
            throw new ArgumentException("Dexcom redirect URI must be absolute.", nameof(redirectUri));
        }

        if (scopes.Count == 0)
        {
            throw new ArgumentException("At least one Dexcom OAuth scope must be specified.", nameof(scopes));
        }
    }

    /// <summary>
    /// Normalizes an optional string value.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <returns>The normalized value.</returns>
    private static string? NormalizeOptionalValue(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    /// <summary>
    /// Normalizes OAuth scopes.
    /// </summary>
    /// <param name="scopes">The scopes to normalize.</param>
    /// <returns>The normalized scopes.</returns>
    private static IReadOnlyCollection<string> NormalizeScopes(IEnumerable<string> scopes)
    {
        return scopes
            .Where(scope => !string.IsNullOrWhiteSpace(scope))
            .Select(scope => scope.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    /// <summary>
    /// Parses whether an environment variable value should be treated as true.
    /// </summary>
    /// <param name="value">The environment variable value.</param>
    /// <returns>True when the value is truthy; otherwise false.</returns>
    private static bool IsTruthy(string? value)
    {
        return string.Equals(value, "true", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "1", StringComparison.OrdinalIgnoreCase)
            || string.Equals(value, "yes", StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Parses a Dexcom API environment value.
    /// </summary>
    /// <param name="value">The environment value.</param>
    /// <returns>The parsed Dexcom API environment.</returns>
    private static DexcomApiEnvironment ParseEnvironment(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return DexcomApiEnvironment.Sandbox;
        }

        return Enum.TryParse<DexcomApiEnvironment>(value.Trim(), ignoreCase: true, out var environment)
            ? environment
            : throw new ArgumentException("Dexcom API environment is invalid.", nameof(value));
    }

    /// <summary>
    /// Parses a redirect URI value.
    /// </summary>
    /// <param name="value">The redirect URI value.</param>
    /// <returns>The parsed redirect URI.</returns>
    private static Uri? ParseRedirectUri(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : new Uri(value.Trim(), UriKind.Absolute);
    }

    /// <summary>
    /// Parses an OAuth scopes value.
    /// </summary>
    /// <param name="value">The scopes value.</param>
    /// <returns>The parsed scopes.</returns>
    private static IReadOnlyCollection<string>? ParseScopes(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .ToArray();
    }

    /// <summary>
    /// Parses the latest reading lookback value expressed in minutes.
    /// </summary>
    /// <param name="value">The lookback value.</param>
    /// <returns>The parsed lookback window.</returns>
    private static TimeSpan? ParseLatestReadingLookback(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return int.TryParse(value.Trim(), out var minutes)
            ? TimeSpan.FromMinutes(minutes)
            : throw new ArgumentException("Dexcom latest lookback minutes value is invalid.", nameof(value));
    }

    #endregion
}