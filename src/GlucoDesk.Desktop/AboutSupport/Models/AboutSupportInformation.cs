namespace GlucoDesk.Desktop.AboutSupport.Models;

/// <summary>
/// Describes the public product and support information shown by GlucoDesk.
/// </summary>
/// <param name="Version">The user-facing application version.</param>
/// <param name="WebsiteUri">The official product website.</param>
/// <param name="SourceCodeUri">The public source-code repository.</param>
/// <param name="ReportIssueUri">The public issue-reporting destination.</param>
public sealed record AboutSupportInformation(
    string Version,
    Uri WebsiteUri,
    Uri SourceCodeUri,
    Uri ReportIssueUri);
