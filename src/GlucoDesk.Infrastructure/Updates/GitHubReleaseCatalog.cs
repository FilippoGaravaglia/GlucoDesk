using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using GlucoDesk.Application.Updates;
using GlucoDesk.Core.Updates;

namespace GlucoDesk.Infrastructure.Updates;

/// <summary>
/// Retrieves GlucoDesk releases from the official GitHub repository.
/// </summary>
public sealed class GitHubReleaseCatalog : IReleaseCatalog
{
    private const string ReleasesEndpoint =
        "https://api.github.com/repos/FilippoGaravaglia/GlucoDesk/releases"
        + "?per_page=20";

    private readonly HttpClient _httpClient;

    /// <summary>
    /// Initializes a new instance of the <see cref="GitHubReleaseCatalog"/>
    /// class.
    /// </summary>
    public GitHubReleaseCatalog(HttpClient httpClient)
    {
        _httpClient = httpClient;
    }

    /// <inheritdoc />
    public async Task<UpdateRelease?> GetLatestAsync(
        bool includePreviewReleases,
        CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            ReleasesEndpoint);

        request.Headers.UserAgent.ParseAdd("GlucoDesk-UpdateChecker");
        request.Headers.Accept.ParseAdd(
            "application/vnd.github+json");

        using var response = await _httpClient.SendAsync(
            request,
            HttpCompletionOption.ResponseHeadersRead,
            cancellationToken);

        response.EnsureSuccessStatusCode();

        await using var contentStream =
            await response.Content.ReadAsStreamAsync(cancellationToken);

        var releases = await JsonSerializer.DeserializeAsync<
            IReadOnlyList<GitHubReleaseDto>>(
            contentStream,
            cancellationToken: cancellationToken);

        if (releases is null)
        {
            return null;
        }

        foreach (var release in releases)
        {
            if (release.Draft)
            {
                continue;
            }

            if (release.Prerelease && !includePreviewReleases)
            {
                continue;
            }

            var asset = FindCompatibleAsset(release.Assets);

            if (asset is null)
            {
                continue;
            }

            var version = NormalizeVersion(release.TagName);

            return new UpdateRelease(
                Version: version,
                TagName: release.TagName,
                ReleaseName: string.IsNullOrWhiteSpace(release.Name)
                    ? release.TagName
                    : release.Name,
                ReleaseNotes: release.Body ?? string.Empty,
                PublishedAt:
                    release.PublishedAt ?? release.CreatedAt,
                ReleasePageUrl: new Uri(release.HtmlUrl),
                DownloadUrl: new Uri(asset.BrowserDownloadUrl),
                AssetName: asset.Name,
                IsPreview: release.Prerelease);
        }

        return null;
    }

    #region Helpers

    /// <summary>
    /// Finds the installer bundle compatible with the current runtime.
    /// </summary>
    private static GitHubAssetDto? FindCompatibleAsset(
        IReadOnlyList<GitHubAssetDto> assets)
    {
        var expectedSuffix = GetExpectedAssetSuffix();

        return assets.FirstOrDefault(
            asset => asset.Name.EndsWith(
                expectedSuffix,
                StringComparison.OrdinalIgnoreCase));
    }

    /// <summary>
    /// Gets the expected GitHub release asset suffix for the current runtime.
    /// </summary>
    private static string GetExpectedAssetSuffix()
    {
        if (OperatingSystem.IsWindows()
            && RuntimeInformation.OSArchitecture == Architecture.X64)
        {
            return "windows-x64-installable.zip";
        }

        if (OperatingSystem.IsMacOS())
        {
            return RuntimeInformation.OSArchitecture switch
            {
                Architecture.Arm64 => "macos-arm64-installable.zip",
                Architecture.X64 => "macos-x64-installable.zip",
                _ => throw new PlatformNotSupportedException(
                    "This macOS architecture is not supported.")
            };
        }

        throw new PlatformNotSupportedException(
            "Automatic GlucoDesk update downloads are currently supported "
            + "only on Windows x64 and macOS.");
    }

    /// <summary>
    /// Normalizes a GitHub release tag into the application version format.
    /// </summary>
    private static string NormalizeVersion(string tagName)
    {
        return tagName.StartsWith('v')
            ? tagName[1..]
            : tagName;
    }

    #endregion

    private sealed record GitHubReleaseDto(
        [property: JsonPropertyName("tag_name")]
        string TagName,

        [property: JsonPropertyName("name")]
        string? Name,

        [property: JsonPropertyName("body")]
        string? Body,

        [property: JsonPropertyName("html_url")]
        string HtmlUrl,

        [property: JsonPropertyName("draft")]
        bool Draft,

        [property: JsonPropertyName("prerelease")]
        bool Prerelease,

        [property: JsonPropertyName("created_at")]
        DateTimeOffset CreatedAt,

        [property: JsonPropertyName("published_at")]
        DateTimeOffset? PublishedAt,

        [property: JsonPropertyName("assets")]
        IReadOnlyList<GitHubAssetDto> Assets);

    private sealed record GitHubAssetDto(
        [property: JsonPropertyName("name")]
        string Name,

        [property: JsonPropertyName("browser_download_url")]
        string BrowserDownloadUrl);
}
