using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Desktop.AboutSupport.Enums;
using GlucoDesk.Desktop.AboutSupport.Models;
using GlucoDesk.Desktop.AboutSupport.Services.Abstractions;

namespace GlucoDesk.Desktop.AboutSupport.Services;

/// <summary>
/// Provides immutable GlucoDesk product information and support navigation.
/// </summary>
public sealed class AboutSupportService :
    IAboutSupportService
{
    private static readonly Uri WebsiteUri =
        new("https://glucodesk.com/");

    private static readonly Uri SourceCodeUri =
        new("https://github.com/FilippoGaravaglia/GlucoDesk");
private static readonly Uri ReportIssueUri =
        new(
            "https://github.com/FilippoGaravaglia/"
            + "GlucoDesk/issues/new/choose");

    private readonly IApplicationVersionProvider
        _versionProvider;

    private readonly IExternalUriLauncher
        _externalUriLauncher;

    /// <summary>
    /// Initializes the service.
    /// </summary>
    /// <param name="versionProvider">
    /// The application version provider.
    /// </param>
    /// <param name="externalUriLauncher">
    /// The trusted external URI launcher.
    /// </param>
    public AboutSupportService(
        IApplicationVersionProvider versionProvider,
        IExternalUriLauncher externalUriLauncher)
    {
        ArgumentNullException.ThrowIfNull(versionProvider);
        ArgumentNullException.ThrowIfNull(externalUriLauncher);

        _versionProvider = versionProvider;
        _externalUriLauncher = externalUriLauncher;
    }

    /// <inheritdoc />
    public AboutSupportInformation GetInformation()
    {
        return new AboutSupportInformation(
            Version: _versionProvider.GetVersion(),
            WebsiteUri,
            SourceCodeUri,
            ReportIssueUri);
    }

    /// <inheritdoc />
    public Task<Result> OpenAsync(
        AboutSupportLinkKind linkKind,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var uriResult = ResolveUri(linkKind);

        if (uriResult.IsFailure)
        {
            return Task.FromResult(
                Result.Failure(uriResult.Error));
        }

        return _externalUriLauncher.OpenAsync(
            uriResult.Value,
            cancellationToken);
    }

    private static Result<Uri> ResolveUri(
        AboutSupportLinkKind linkKind)
    {
        return linkKind switch
        {
            AboutSupportLinkKind.Website =>
                Result<Uri>.Success(WebsiteUri),

            AboutSupportLinkKind.SourceCode =>
                Result<Uri>.Success(SourceCodeUri),
AboutSupportLinkKind.ReportIssue =>
                Result<Uri>.Success(ReportIssueUri),

            _ => Result<Uri>.Failure(
                new Error(
                    "AboutSupport.UnsupportedLink",
                    "The selected support link is not supported."))
        };
    }
}
