using System.Text;

namespace GlucoDesk.Desktop.Updates.Presentation;

/// <summary>
/// Extracts the user-facing localized section from GitHub release notes
/// and converts the supported Markdown subset into desktop-friendly text.
/// </summary>
public static class ReleaseNotesPresenter
{
    private const string EnglishSection = "## In-app highlights EN";
    private const string ItalianSection = "## In-app highlights IT";

    /// <summary>
    /// Creates release notes suitable for display inside GlucoDesk.
    /// </summary>
    /// <param name="releaseNotes">
    /// The complete Markdown release body returned by GitHub.
    /// </param>
    /// <param name="languageCode">
    /// The active GlucoDesk language code.
    /// </param>
    /// <returns>
    /// Localized, user-facing release notes without raw Markdown syntax.
    /// </returns>
    public static string Format(
        string? releaseNotes,
        string? languageCode)
    {
        if (string.IsNullOrWhiteSpace(releaseNotes))
        {
            return string.Empty;
        }

        var preferredHeading = IsItalian(languageCode)
            ? ItalianSection
            : EnglishSection;

        var fallbackHeading = IsItalian(languageCode)
            ? EnglishSection
            : ItalianSection;

        var section =
            TryExtractSection(releaseNotes, preferredHeading)
            ?? TryExtractSection(releaseNotes, fallbackHeading);

        if (string.IsNullOrWhiteSpace(section))
        {
            return CreateFallback(releaseNotes);
        }

        return NormalizeMarkdown(section);
    }

    #region Helpers

    /// <summary>
    /// Determines whether the supplied language code represents Italian.
    /// </summary>
    private static bool IsItalian(string? languageCode)
    {
        return string.Equals(
            languageCode,
            "it",
            StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Extracts a second-level Markdown section from the supplied document.
    /// </summary>
    private static string? TryExtractSection(
        string releaseNotes,
        string heading)
    {
        var normalized = NormalizeLineEndings(releaseNotes);

        var headingIndex = normalized.IndexOf(
            heading,
            StringComparison.OrdinalIgnoreCase);

        if (headingIndex < 0)
        {
            return null;
        }

        var contentStart = normalized.IndexOf(
            '\n',
            headingIndex + heading.Length);

        if (contentStart < 0)
        {
            return string.Empty;
        }

        contentStart++;

        var nextHeadingIndex = FindNextLevelTwoHeading(
            normalized,
            contentStart);

        var content = nextHeadingIndex >= 0
            ? normalized[contentStart..nextHeadingIndex]
            : normalized[contentStart..];

        return content.Trim();
    }

    /// <summary>
    /// Finds the next second-level Markdown heading.
    /// </summary>
    private static int FindNextLevelTwoHeading(
        string value,
        int startIndex)
    {
        var searchIndex = startIndex;

        while (searchIndex < value.Length)
        {
            var candidateIndex = value.IndexOf(
                "\n## ",
                searchIndex,
                StringComparison.Ordinal);

            if (candidateIndex < 0)
            {
                return -1;
            }

            return candidateIndex + 1;
        }

        return -1;
    }

    /// <summary>
    /// Converts the supported user-facing Markdown subset into plain text.
    /// </summary>
    private static string NormalizeMarkdown(string value)
    {
        var normalized = NormalizeLineEndings(value);
        var lines = normalized.Split('\n');

        var builder = new StringBuilder();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (line.Length == 0)
            {
                AppendBlankLine(builder);
                continue;
            }

            if (line.Equals(
                    "---",
                    StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith("### ", StringComparison.Ordinal))
            {
                AppendLine(
                    builder,
                    line[4..].Trim());

                continue;
            }

            if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                AppendLine(
                    builder,
                    $"• {line[2..].Trim()}");

                continue;
            }

            if (line.StartsWith("> ", StringComparison.Ordinal))
            {
                AppendLine(
                    builder,
                    line[2..].Trim());

                continue;
            }

            AppendLine(
                builder,
                RemoveInlineMarkdown(line));
        }

        return builder
            .ToString()
            .Trim();
    }

    /// <summary>
    /// Produces a safe fallback when the expected in-app sections are absent.
    /// </summary>
    private static string CreateFallback(string releaseNotes)
    {
        var normalized = NormalizeLineEndings(releaseNotes);
        var lines = normalized.Split('\n');

        var builder = new StringBuilder();

        foreach (var rawLine in lines)
        {
            var line = rawLine.Trim();

            if (line.Length == 0)
            {
                AppendBlankLine(builder);
                continue;
            }

            if (line.StartsWith("# ", StringComparison.Ordinal) ||
                line.StartsWith("## ", StringComparison.Ordinal) ||
                line.Equals("---", StringComparison.Ordinal) ||
                line.StartsWith("```", StringComparison.Ordinal))
            {
                continue;
            }

            if (line.StartsWith("- ", StringComparison.Ordinal))
            {
                AppendLine(
                    builder,
                    $"• {line[2..].Trim()}");
            }
            else if (line.StartsWith("> ", StringComparison.Ordinal))
            {
                AppendLine(
                    builder,
                    line[2..].Trim());
            }
            else
            {
                AppendLine(
                    builder,
                    RemoveInlineMarkdown(line));
            }

            if (builder.Length >= 700)
            {
                break;
            }
        }

        return builder
            .ToString()
            .Trim();
    }

    /// <summary>
    /// Removes the small inline Markdown subset used by release notes.
    /// </summary>
    private static string RemoveInlineMarkdown(string value)
    {
        return value
            .Replace("**", string.Empty, StringComparison.Ordinal)
            .Replace("__", string.Empty, StringComparison.Ordinal)
            .Replace("`", string.Empty, StringComparison.Ordinal);
    }

    /// <summary>
    /// Normalizes line endings before parsing Markdown.
    /// </summary>
    private static string NormalizeLineEndings(string value)
    {
        return value
            .Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n');
    }

    /// <summary>
    /// Appends a line to the presentation buffer.
    /// </summary>
    private static void AppendLine(
        StringBuilder builder,
        string value)
    {
        if (builder.Length > 0 &&
            builder[^1] != '\n')
        {
            builder.AppendLine();
        }

        builder.AppendLine(value);
    }

    /// <summary>
    /// Appends one logical blank line without producing repeated spacing.
    /// </summary>
    private static void AppendBlankLine(StringBuilder builder)
    {
        if (builder.Length == 0)
        {
            return;
        }

        var value = builder.ToString();

        if (value.EndsWith(
                "\n\n",
                StringComparison.Ordinal))
        {
            return;
        }

        builder.AppendLine();
    }

    #endregion
}
