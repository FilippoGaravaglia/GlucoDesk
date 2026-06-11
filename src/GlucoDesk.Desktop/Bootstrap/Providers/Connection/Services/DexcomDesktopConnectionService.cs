using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.Bootstrap.Providers.Connection.Models;
using GlucoDesk.Desktop.Bootstrap.Providers.Options;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Sessions;

namespace GlucoDesk.Desktop.Bootstrap.Providers.Connection.Services;

/// <summary>
/// Starts the Dexcom OAuth connection flow from the desktop runtime.
/// </summary>
public sealed class DexcomDesktopConnectionService : IDexcomDesktopConnectionService
{
    private static readonly TimeSpan DefaultAuthorizationTimeout = TimeSpan.FromMinutes(10);

    private readonly IDexcomOAuthAuthorizationSessionService _authorizationSessionService;
    private readonly DesktopDexcomProviderOptions _dexcomOptions;
    private readonly TimeProvider _timeProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomDesktopConnectionService"/> class.
    /// </summary>
    /// <param name="authorizationSessionService">The Dexcom OAuth authorization session service.</param>
    /// <param name="dexcomOptions">The desktop Dexcom provider options.</param>
    /// <param name="timeProvider">The time provider.</param>
    public DexcomDesktopConnectionService(
        IDexcomOAuthAuthorizationSessionService authorizationSessionService,
        DesktopDexcomProviderOptions dexcomOptions,
        TimeProvider timeProvider)
    {
        ArgumentNullException.ThrowIfNull(authorizationSessionService);
        ArgumentNullException.ThrowIfNull(dexcomOptions);
        ArgumentNullException.ThrowIfNull(timeProvider);

        _authorizationSessionService = authorizationSessionService;
        _dexcomOptions = dexcomOptions;
        _timeProvider = timeProvider;
    }

    /// <inheritdoc />
    public async Task<Result<DexcomDesktopConnectionResult>> ConnectAsync(
        CancellationToken cancellationToken)
    {
        var configurationResult = ValidateConfiguration();

        if (configurationResult.IsFailure)
        {
            return Result<DexcomDesktopConnectionResult>.Failure(configurationResult.Error);
        }

        var sessionResult = await _authorizationSessionService
            .StartAuthorizationSessionAsync(
                new DexcomOAuthAuthorizationSessionRequest(
                    _dexcomOptions.ClientSecret!,
                    DefaultAuthorizationTimeout),
                cancellationToken)
            .ConfigureAwait(false);

        if (sessionResult.IsFailure)
        {
            return Result<DexcomDesktopConnectionResult>.Failure(sessionResult.Error);
        }

        var connectionResult = new DexcomDesktopConnectionResult(
            _timeProvider.GetUtcNow(),
            sessionResult.Value.TokenSet.AccessTokenExpiresAtUtc,
            sessionResult.Value.TokenSet.RefreshTokenExpiresAtUtc);

        return Result<DexcomDesktopConnectionResult>.Success(connectionResult);
    }

    #region Helpers

    /// <summary>
    /// Validates that Dexcom is configured enough to start an OAuth connection flow.
    /// </summary>
    /// <returns>The validation result.</returns>
    private Result ValidateConfiguration()
    {
        if (!_dexcomOptions.IsEnabled)
        {
            return Result.Failure(
                new Error(
                    "Dexcom.DesktopConnectionDisabled",
                    "Dexcom is not enabled in the current desktop runtime."));
        }

        if (string.IsNullOrWhiteSpace(_dexcomOptions.ClientSecret))
        {
            return Result.Failure(
                new Error(
                    "Dexcom.ClientSecretMissing",
                    "Dexcom client secret is required to start the OAuth connection flow."));
        }

        return Result.Success();
    }

    #endregion
}