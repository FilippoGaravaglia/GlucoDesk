using System.ComponentModel;
using System.Diagnostics;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Browsers;

/// <summary>
/// Opens Dexcom OAuth authorization URLs using the operating system default browser.
/// </summary>
public sealed class DexcomSystemBrowser : IDexcomSystemBrowser
{
    /// <inheritdoc />
    public Task<Result<Uri>> OpenAsync(
        Uri authorizationUri,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(authorizationUri);

        if (!authorizationUri.IsAbsoluteUri)
        {
            return Task.FromResult(Result<Uri>.Failure(
                new Error("Dexcom.BrowserInvalidUri", "Dexcom authorization URI must be absolute.")));
        }

        if (cancellationToken.IsCancellationRequested)
        {
            return Task.FromResult(Result<Uri>.Failure(
                new Error("Dexcom.BrowserOpenCancelled", "Dexcom browser opening was cancelled.")));
        }

        try
        {
            using var process = Process.Start(BuildStartInfo(authorizationUri));

            if (process is null)
            {
                return Task.FromResult(Result<Uri>.Failure(BuildOpenFailure()));
            }

            return Task.FromResult(Result<Uri>.Success(authorizationUri));
        }
        catch (Win32Exception)
        {
            return Task.FromResult(Result<Uri>.Failure(BuildOpenFailure()));
        }
        catch (InvalidOperationException)
        {
            return Task.FromResult(Result<Uri>.Failure(BuildOpenFailure()));
        }
        catch (PlatformNotSupportedException)
        {
            return Task.FromResult(Result<Uri>.Failure(BuildOpenFailure()));
        }
    }

    #region Helpers

    /// <summary>
    /// Builds the process start information used to open the system browser.
    /// </summary>
    /// <param name="authorizationUri">The authorization URI.</param>
    /// <returns>The process start information.</returns>
    private static ProcessStartInfo BuildStartInfo(Uri authorizationUri)
    {
        return new ProcessStartInfo
        {
            FileName = authorizationUri.ToString(),
            UseShellExecute = true
        };
    }

    /// <summary>
    /// Builds a browser open failure error.
    /// </summary>
    /// <returns>The application error.</returns>
    private static Error BuildOpenFailure()
    {
        return new Error(
            "Dexcom.BrowserOpenFailed",
            "Unable to open the Dexcom authorization URL in the system browser.");
    }

    #endregion
}