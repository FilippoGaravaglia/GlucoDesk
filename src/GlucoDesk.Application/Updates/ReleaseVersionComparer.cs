namespace GlucoDesk.Application.Updates;

/// <summary>
/// Compares GlucoDesk semantic release versions.
/// </summary>
public static class ReleaseVersionComparer
{
    /// <summary>
    /// Determines whether <paramref name="candidate"/> is newer than
    /// <paramref name="current"/>.
    /// </summary>
    public static bool IsNewer(string candidate, string current)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(candidate);
        ArgumentException.ThrowIfNullOrWhiteSpace(current);

        var candidateVersion = Parse(candidate);
        var currentVersion = Parse(current);

        var numericComparison = candidateVersion.Numeric.CompareTo(
            currentVersion.Numeric);

        if (numericComparison != 0)
        {
            return numericComparison > 0;
        }

        if (candidateVersion.IsPreview == currentVersion.IsPreview)
        {
            return false;
        }

        // Stable wins over preview when numeric versions match.
        return !candidateVersion.IsPreview && currentVersion.IsPreview;
    }

    #region Helpers

    /// <summary>
    /// Parses a GlucoDesk semantic release version.
    /// </summary>
    private static ParsedVersion Parse(string value)
    {
        var normalized = value.Trim();

        if (normalized.StartsWith('v'))
        {
            normalized = normalized[1..];
        }

        var separatorIndex = normalized.IndexOf('-');

        var numericPart = separatorIndex >= 0
            ? normalized[..separatorIndex]
            : normalized;

        var suffix = separatorIndex >= 0
            ? normalized[(separatorIndex + 1)..]
            : string.Empty;

        if (!Version.TryParse(numericPart, out var numericVersion))
        {
            throw new FormatException(
                $"'{value}' is not a supported GlucoDesk release version.");
        }

        return new ParsedVersion(
            numericVersion,
            suffix.Contains(
                "preview",
                StringComparison.OrdinalIgnoreCase));
    }

    private readonly record struct ParsedVersion(
        Version Numeric,
        bool IsPreview);

    #endregion
}
