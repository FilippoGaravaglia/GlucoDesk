using System.Diagnostics;
using GlucoDesk.Core.Updates;

namespace GlucoDesk.Application.Updates;

/// <summary>
/// Default application service used to discover and download GlucoDesk updates.
/// </summary>
public sealed class UpdateService : IUpdateService
{
    private readonly IReleaseCatalog _releaseCatalog;
    private readonly IApplicationVersionProvider _versionProvider;

    /// <summary>
    /// Initializes a new instance of the <see cref="UpdateService"/> class.
    /// </summary>
    public UpdateService(
        IReleaseCatalog releaseCatalog,
        IApplicationVersionProvider versionProvider)
    {
        _releaseCatalog = releaseCatalog;
        _versionProvider = versionProvider;
    }

    /// <inheritdoc />
    public async Task<UpdateCheckResult> CheckForUpdatesAsync(
        bool includePreviewReleases,
        CancellationToken cancellationToken)
    {
        try
        {
            var latestRelease = await _releaseCatalog.GetLatestAsync(
                includePreviewReleases,
                cancellationToken);

            if (latestRelease is null)
            {
                return new UpdateCheckResult
                {
                    CurrentVersion = _versionProvider.CurrentVersion,
                    LatestRelease = null,
                    IsUpdateAvailable = false
                };
            }

            return new UpdateCheckResult
            {
                CurrentVersion = _versionProvider.CurrentVersion,
                LatestRelease = latestRelease,
                IsUpdateAvailable = ReleaseVersionComparer.IsNewer(
                    latestRelease.Version,
                    _versionProvider.CurrentVersion)
            };
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (Exception exception)
        {
            return new UpdateCheckResult
            {
                CurrentVersion = _versionProvider.CurrentVersion,
                LatestRelease = null,
                IsUpdateAvailable = false,
                ErrorMessage = exception.Message
            };
        }
    }

    /// <inheritdoc />
    public void Download(UpdateRelease release)
    {
        ArgumentNullException.ThrowIfNull(release);

        Process.Start(
            new ProcessStartInfo
            {
                FileName = release.DownloadUrl.AbsoluteUri,
                UseShellExecute = true
            });
    }
}
