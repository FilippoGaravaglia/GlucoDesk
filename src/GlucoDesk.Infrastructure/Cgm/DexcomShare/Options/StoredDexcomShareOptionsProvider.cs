using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.DexcomShare.Credentials;

namespace GlucoDesk.Infrastructure.Cgm.DexcomShare.Options;

/// <summary>
/// Builds Dexcom Share options from persisted credentials.
/// </summary>
public sealed class StoredDexcomShareOptionsProvider : IDexcomShareOptionsProvider
{
    private readonly IDexcomShareCredentialStore _credentialStore;

    /// <summary>
    /// Initializes a new instance of the <see cref="StoredDexcomShareOptionsProvider"/> class.
    /// </summary>
    /// <param name="credentialStore">The credential store.</param>
    public StoredDexcomShareOptionsProvider(IDexcomShareCredentialStore credentialStore)
    {
        ArgumentNullException.ThrowIfNull(credentialStore);

        _credentialStore = credentialStore;
    }

    /// <inheritdoc />
    public async Task<Result<DexcomShareOptions>> GetOptionsAsync(CancellationToken cancellationToken)
    {
        var credentials = await _credentialStore
            .ReadAsync(cancellationToken)
            .ConfigureAwait(false);

        if (credentials?.IsConfigured != true)
        {
            return Result<DexcomShareOptions>.Failure(
                new Error(
                    "DexcomShare.NotConfigured",
                    "Dexcom Share account is not configured. Open Account and save your Dexcom Share credentials."));
        }

        return Result<DexcomShareOptions>.Success(
            new DexcomShareOptions(
                credentials.Username,
                credentials.Password,
                credentials.Region,
                displayName: "Dexcom Share"));
    }
}