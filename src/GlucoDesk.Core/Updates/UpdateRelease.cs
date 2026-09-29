namespace GlucoDesk.Core.Updates;

/// <summary>
/// Represents a downloadable GlucoDesk release discovered from the official
/// release channel.
/// </summary>
public sealed record UpdateRelease(
    string Version,
    string TagName,
    string ReleaseName,
    string ReleaseNotes,
    DateTimeOffset PublishedAt,
    Uri ReleasePageUrl,
    Uri DownloadUrl,
    string AssetName,
    bool IsPreview);
