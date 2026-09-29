using GlucoDesk.Desktop.Updates.Presentation;

namespace GlucoDesk.Desktop.Tests.Updates.Presentation;

public sealed class ReleaseNotesPresenterTests
{
    [Fact]
    public void Format_English_ReturnsEnglishInAppHighlights()
    {
        var result = ReleaseNotesPresenter.Format(
            CreateReleaseNotes(),
            "en");

        Assert.Contains(
            "first public preview",
            result,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "• Keep your current glucose",
            result,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "cronologia glicemica",
            result,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "Technical notes",
            result,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "##",
            result,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Format_Italian_ReturnsItalianInAppHighlights()
    {
        var result = ReleaseNotesPresenter.Format(
            CreateReleaseNotes(),
            "it");

        Assert.Contains(
            "prima preview pubblica",
            result,
            StringComparison.OrdinalIgnoreCase);

        Assert.Contains(
            "• Tieni glicemia attuale",
            result,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "Keep your current glucose",
            result,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "Full release notes",
            result,
            StringComparison.OrdinalIgnoreCase);

        Assert.DoesNotContain(
            "##",
            result,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Format_PreferredSectionMissing_FallsBackToOtherLanguage()
    {
        const string releaseNotes = """
## In-app highlights EN

A friendly update.

- Better reliability.

## Full release notes

Technical content.
""";

        var result = ReleaseNotesPresenter.Format(
            releaseNotes,
            "it");

        Assert.Contains(
            "A friendly update.",
            result,
            StringComparison.Ordinal);

        Assert.Contains(
            "• Better reliability.",
            result,
            StringComparison.Ordinal);
    }

    [Fact]
    public void Format_NoInAppSections_ReturnsSafeFallback()
    {
        const string releaseNotes = """
# Version 1

A short description.

## Technical notes

- Internal packaging update.
""";

        var result = ReleaseNotesPresenter.Format(
            releaseNotes,
            "en");

        Assert.Contains(
            "A short description.",
            result,
            StringComparison.Ordinal);

        Assert.DoesNotContain(
            "# Version 1",
            result,
            StringComparison.Ordinal);
    }

    #region Helpers

    /// <summary>
    /// Creates release notes matching the GitHub convention used by GlucoDesk.
    /// </summary>
    private static string CreateReleaseNotes()
    {
        return """
## In-app highlights EN

GlucoDesk v0.3.0-preview is the first public preview.

### What's new

- Keep your current glucose visible.
- Build a private local history.

> GlucoDesk is not a medical device.

## In-app highlights IT

GlucoDesk v0.3.0-preview è la prima preview pubblica.

### Novità

- Tieni glicemia attuale sempre visibile.
- Costruisci una cronologia glicemica locale.

> GlucoDesk non è un dispositivo medico.

## Full release notes

Technical notes and packaging details.
""";
    }

    #endregion
}
