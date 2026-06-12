using GlucoDesk.Infrastructure.Cgm.Nightscout.Enums;

namespace GlucoDesk.Infrastructure.Cgm.Nightscout.Options;

/// <summary>
/// Represents Nightscout provider configuration.
/// </summary>
public sealed record NightscoutOptions
{
    /// <summary>
    /// Initializes a new instance of the <see cref="NightscoutOptions"/> class.
    /// </summary>
    /// <param name="baseUri">The Nightscout base URI.</param>
    /// <param name="displayName">The provider display name.</param>
    /// <param name="authenticationMode">The authentication mode.</param>
    /// <param name="apiSecretSha1">The optional SHA1-hashed Nightscout API secret.</param>
    /// <param name="accessToken">The optional Nightscout access token.</param>
    /// <param name="latestReadingLookback">The latest reading lookback window.</param>
    /// <param name="requestTimeout">The HTTP request timeout.</param>
    /// <param name="maxReadingsPerRequest">The maximum number of readings requested from Nightscout.</param>
    /// <exception cref="ArgumentNullException">Thrown when baseUri is null.</exception>
    /// <exception cref="ArgumentException">Thrown when a string option is invalid.</exception>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when a numeric or duration option is invalid.</exception>
    public NightscoutOptions(
        Uri baseUri,
        string displayName = "Nightscout",
        NightscoutAuthenticationMode authenticationMode = NightscoutAuthenticationMode.None,
        string? apiSecretSha1 = null,
        string? accessToken = null,
        TimeSpan? latestReadingLookback = null,
        TimeSpan? requestTimeout = null,
        int maxReadingsPerRequest = 288)
    {
        ArgumentNullException.ThrowIfNull(baseUri);

        if (!baseUri.IsAbsoluteUri)
        {
            throw new ArgumentException("Nightscout base URI must be absolute.", nameof(baseUri));
        }

        if (baseUri.Scheme != Uri.UriSchemeHttps && baseUri.Scheme != Uri.UriSchemeHttp)
        {
            throw new ArgumentException("Nightscout base URI must use HTTP or HTTPS.", nameof(baseUri));
        }

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

        ValidateAuthentication(authenticationMode, apiSecretSha1, accessToken);

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
    /// Gets the Nightscout base URI.
    /// </summary>
    public Uri BaseUri { get; }

    /// <summary>
    /// Gets the provider display name.
    /// </summary>
    public string DisplayName { get; }

    /// <summary>
    /// Gets the Nightscout authentication mode.
    /// </summary>
    public NightscoutAuthenticationMode AuthenticationMode { get; }

    /// <summary>
    /// Gets the optional SHA1-hashed Nightscout API secret.
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

    #region Helpers

    /// <summary>
    /// Validates authentication-specific option consistency.
    /// </summary>
    /// <param name="authenticationMode">The authentication mode.</param>
    /// <param name="apiSecretSha1">The optional SHA1-hashed API secret.</param>
    /// <param name="accessToken">The optional access token.</param>
    private static void ValidateAuthentication(
        NightscoutAuthenticationMode authenticationMode,
        string? apiSecretSha1,
        string? accessToken)
    {
        switch (authenticationMode)
        {
            case NightscoutAuthenticationMode.None:
                return;

            case NightscoutAuthenticationMode.ApiSecretSha1Header:
                if (string.IsNullOrWhiteSpace(apiSecretSha1))
                {
                    throw new ArgumentException(
                        "A SHA1-hashed Nightscout API secret is required when api-secret header authentication is enabled.",
                        nameof(apiSecretSha1));
                }

                return;

            case NightscoutAuthenticationMode.AccessTokenQueryString:
                if (string.IsNullOrWhiteSpace(accessToken))
                {
                    throw new ArgumentException(
                        "A Nightscout access token is required when access-token query-string authentication is enabled.",
                        nameof(accessToken));
                }

                return;

            default:
                throw new ArgumentOutOfRangeException(
                    nameof(authenticationMode),
                    authenticationMode,
                    "Unsupported Nightscout authentication mode.");
        }
    }

    /// <summary>
    /// Normalizes optional secret values.
    /// </summary>
    /// <param name="value">The value to normalize.</param>
    /// <returns>The normalized value.</returns>
    private static string? NormalizeSecret(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    #endregion
}