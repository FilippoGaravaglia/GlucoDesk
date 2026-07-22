using System.ComponentModel;
using System.Diagnostics;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.AboutSupport.Services.Abstractions;

namespace GlucoDesk.Desktop.AboutSupport.Services;

/// <summary>
/// Opens trusted HTTPS destinations with the operating-system default browser.
/// </summary>
public sealed class OperatingSystemExternalUriLauncher :
    IExternalUriLauncher
{
    private const string InvalidUriErrorCode =
        "AboutSupport.InvalidUri";

    private const string OpenFailedErrorCode =
        "AboutSupport.OpenFailed";

    /// <inheritdoc />
    public Task<Result> OpenAsync(
        Uri uri,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(uri);
        cancellationToken.ThrowIfCancellationRequested();

        if (!uri.IsAbsoluteUri ||
            !string.Equals(
                uri.Scheme,
                Uri.UriSchemeHttps,
                StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(
                Result.Failure(
                    new Error(
                        InvalidUriErrorCode,
                        "Only absolute HTTPS support links can be opened.")));
        }

        try
        {
            var process = Process.Start(
                new ProcessStartInfo
                {
                    FileName = uri.AbsoluteUri,
                    UseShellExecute = true
                });

            if (process is null)
            {
                return Task.FromResult(
                    CreateOpenFailure());
            }

            process.Dispose();

            return Task.FromResult(
                Result.Success());
        }
        catch (Win32Exception)
        {
            return Task.FromResult(
                CreateOpenFailure());
        }
        catch (InvalidOperationException)
        {
            return Task.FromResult(
                CreateOpenFailure());
        }
        catch (PlatformNotSupportedException)
        {
            return Task.FromResult(
                CreateOpenFailure());
        }
    }

    private static Result CreateOpenFailure()
    {
        return Result.Failure(
            new Error(
                OpenFailedErrorCode,
                "The operating system could not open the selected link."));
    }
}
