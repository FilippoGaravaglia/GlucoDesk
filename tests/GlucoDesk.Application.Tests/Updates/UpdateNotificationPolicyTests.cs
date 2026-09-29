using GlucoDesk.Application.Updates;
using GlucoDesk.Core.Updates;

namespace GlucoDesk.Application.Tests.Updates;

public sealed class UpdateNotificationPolicyTests
{
    [Fact]
    public void ShouldNotify_NewVersion_ReturnsTrue()
    {
        var preferences = new UpdatePreferences
        {
            LastDismissedVersion = "0.3.0-preview",
            LastDismissedAt = DateTimeOffset.UtcNow
        };

        var result = UpdateNotificationPolicy.ShouldNotify(
            CreateRelease("0.4.0-preview"),
            preferences,
            DateTimeOffset.UtcNow);

        Assert.True(result);
    }

    [Fact]
    public void ShouldNotify_RecentlyDismissedVersion_ReturnsFalse()
    {
        var now = DateTimeOffset.UtcNow;

        var preferences = new UpdatePreferences
        {
            LastDismissedVersion = "0.4.0-preview",
            LastDismissedAt = now.AddHours(-2)
        };

        var result = UpdateNotificationPolicy.ShouldNotify(
            CreateRelease("0.4.0-preview"),
            preferences,
            now);

        Assert.False(result);
    }

    [Fact]
    public void ShouldNotify_DismissedMoreThanOneDayAgo_ReturnsTrue()
    {
        var now = DateTimeOffset.UtcNow;

        var preferences = new UpdatePreferences
        {
            LastDismissedVersion = "0.4.0-preview",
            LastDismissedAt = now.AddDays(-2)
        };

        var result = UpdateNotificationPolicy.ShouldNotify(
            CreateRelease("0.4.0-preview"),
            preferences,
            now);

        Assert.True(result);
    }

    #region Helpers

    /// <summary>
    /// Creates a minimal release for policy tests.
    /// </summary>
    private static UpdateRelease CreateRelease(string version)
    {
        return new UpdateRelease(
            Version: version,
            TagName: $"v{version}",
            ReleaseName: $"v{version}",
            ReleaseNotes: string.Empty,
            PublishedAt: DateTimeOffset.UtcNow,
            ReleasePageUrl: new Uri("https://example.com/release"),
            DownloadUrl: new Uri("https://example.com/download.zip"),
            AssetName: "download.zip",
            IsPreview: true);
    }

    #endregion
}
