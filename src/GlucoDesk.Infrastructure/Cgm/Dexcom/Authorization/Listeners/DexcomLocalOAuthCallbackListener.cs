using System.Net;
using System.Text;
using GlucoDesk.Application.Common.Errors;
using GlucoDesk.Application.Common.Results;
using GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Callbacks;

namespace GlucoDesk.Infrastructure.Cgm.Dexcom.Authorization.Listeners;

/// <summary>
/// Provides a local HTTP listener for Dexcom OAuth callback redirects.
/// </summary>
public sealed class DexcomLocalOAuthCallbackListener : IDexcomLocalOAuthCallbackListener
{
    private readonly IDexcomOAuthCallbackParser _callbackParser;
    private readonly DexcomLocalOAuthCallbackOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="DexcomLocalOAuthCallbackListener"/> class.
    /// </summary>
    /// <param name="callbackParser">The Dexcom OAuth callback parser.</param>
    /// <param name="options">The local OAuth callback listener options.</param>
    public DexcomLocalOAuthCallbackListener(
        IDexcomOAuthCallbackParser callbackParser,
        DexcomLocalOAuthCallbackOptions options)
    {
        ArgumentNullException.ThrowIfNull(callbackParser);
        ArgumentNullException.ThrowIfNull(options);

        _callbackParser = callbackParser;
        _options = options;
    }

    /// <inheritdoc />
    public async Task<Result<DexcomLocalOAuthCallbackListenResult>> ListenForCallbackAsync(
        DexcomLocalOAuthCallbackListenRequest request,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(request);

        using var listener = new HttpListener();

        try
        {
            listener.Prefixes.Add(BuildListenerPrefix(request.RedirectUri));
            listener.Start();

            var context = await listener
                .GetContextAsync()
                .WaitAsync(request.Timeout ?? _options.DefaultTimeout, cancellationToken)
                .ConfigureAwait(false);

            return await HandleCallbackAsync(context, request, cancellationToken)
                .ConfigureAwait(false);
        }
        catch (TimeoutException)
        {
            return Result<DexcomLocalOAuthCallbackListenResult>.Failure(
                new Error("Dexcom.OAuthCallbackTimeout", "Dexcom OAuth callback timed out."));
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            return Result<DexcomLocalOAuthCallbackListenResult>.Failure(
                new Error("Dexcom.OAuthCallbackCancelled", "Dexcom OAuth callback listening was cancelled."));
        }
        catch (HttpListenerException)
        {
            return Result<DexcomLocalOAuthCallbackListenResult>.Failure(
                new Error("Dexcom.OAuthCallbackListenerFailed", "Unable to start or use the local OAuth callback listener."));
        }
        finally
        {
            if (listener.IsListening)
            {
                listener.Stop();
            }
        }
    }

    #region Helpers

    /// <summary>
    /// Handles a received local OAuth callback HTTP request.
    /// </summary>
    /// <param name="context">The HTTP listener context.</param>
    /// <param name="request">The local OAuth callback listen request.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The received and validated OAuth callback result.</returns>
    private async Task<Result<DexcomLocalOAuthCallbackListenResult>> HandleCallbackAsync(
        HttpListenerContext context,
        DexcomLocalOAuthCallbackListenRequest request,
        CancellationToken cancellationToken)
    {
        var callbackUri = context.Request.Url;

        if (callbackUri is null)
        {
            var error = new Error("Dexcom.OAuthInvalidCallback", "Dexcom OAuth callback URI is invalid.");

            await WriteBrowserResponseAsync(context, success: false, error.Message, cancellationToken)
                .ConfigureAwait(false);

            return Result<DexcomLocalOAuthCallbackListenResult>.Failure(error);
        }

        if (!IsExpectedCallbackPath(callbackUri, request.RedirectUri))
        {
            var error = new Error(
                "Dexcom.OAuthUnexpectedCallbackPath",
                "Dexcom OAuth callback was received on an unexpected path.");

            await WriteBrowserResponseAsync(context, success: false, error.Message, cancellationToken)
                .ConfigureAwait(false);

            return Result<DexcomLocalOAuthCallbackListenResult>.Failure(error);
        }

        var parsedCallback = _callbackParser.ParseCallback(callbackUri, request.ExpectedState);

        if (parsedCallback.IsFailure)
        {
            await WriteBrowserResponseAsync(context, success: false, parsedCallback.Error.Message, cancellationToken)
                .ConfigureAwait(false);

            return Result<DexcomLocalOAuthCallbackListenResult>.Failure(parsedCallback.Error);
        }

        await WriteBrowserResponseAsync(
                context,
                success: true,
                "Dexcom authorization completed. You can return to GlucoDesk.",
                cancellationToken)
            .ConfigureAwait(false);

        return Result<DexcomLocalOAuthCallbackListenResult>.Success(
            new DexcomLocalOAuthCallbackListenResult(callbackUri, parsedCallback.Value));
    }

    /// <summary>
    /// Builds the HTTP listener prefix for the loopback redirect URI.
    /// </summary>
    /// <param name="redirectUri">The redirect URI.</param>
    /// <returns>The listener prefix.</returns>
    private static string BuildListenerPrefix(Uri redirectUri)
    {
        var uriBuilder = new UriBuilder(
            redirectUri.Scheme,
            redirectUri.Host,
            redirectUri.Port,
            "/");

        return uriBuilder.Uri.ToString();
    }

    /// <summary>
    /// Checks whether the received callback path matches the configured redirect URI path.
    /// </summary>
    /// <param name="callbackUri">The received callback URI.</param>
    /// <param name="redirectUri">The configured redirect URI.</param>
    /// <returns>True when the callback path matches the redirect URI path; otherwise, false.</returns>
    private static bool IsExpectedCallbackPath(
        Uri callbackUri,
        Uri redirectUri)
    {
        return string.Equals(
            callbackUri.AbsolutePath.TrimEnd('/'),
            redirectUri.AbsolutePath.TrimEnd('/'),
            StringComparison.Ordinal);
    }

    /// <summary>
    /// Writes a simple browser response for the OAuth callback result.
    /// </summary>
    /// <param name="context">The HTTP listener context.</param>
    /// <param name="success">Whether the OAuth callback was successful.</param>
    /// <param name="message">The browser message.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    private static async Task WriteBrowserResponseAsync(
        HttpListenerContext context,
        bool success,
        string message,
        CancellationToken cancellationToken)
    {
        var title = success
            ? "GlucoDesk Dexcom authorization completed"
            : "GlucoDesk Dexcom authorization failed";

        var html = BuildHtmlResponse(title, message);
        var buffer = Encoding.UTF8.GetBytes(html);

        context.Response.StatusCode = success
            ? (int)HttpStatusCode.OK
            : (int)HttpStatusCode.BadRequest;

        context.Response.ContentType = "text/html; charset=utf-8";
        context.Response.ContentLength64 = buffer.Length;

        await context.Response.OutputStream
            .WriteAsync(buffer, cancellationToken)
            .ConfigureAwait(false);

        context.Response.Close();
    }

    /// <summary>
    /// Builds a simple HTML response.
    /// </summary>
    /// <param name="title">The response title.</param>
    /// <param name="message">The response message.</param>
    /// <returns>The HTML response.</returns>
    private static string BuildHtmlResponse(
        string title,
        string message)
    {
        return $$"""
            <!doctype html>
            <html lang="en">
            <head>
              <meta charset="utf-8">
              <title>{{WebUtility.HtmlEncode(title)}}</title>
              <style>
                body {
                  font-family: -apple-system, BlinkMacSystemFont, "Segoe UI", sans-serif;
                  background: #0f172a;
                  color: #e5e7eb;
                  display: flex;
                  min-height: 100vh;
                  align-items: center;
                  justify-content: center;
                  margin: 0;
                }

                main {
                  max-width: 520px;
                  padding: 32px;
                  border-radius: 20px;
                  background: #111827;
                  box-shadow: 0 24px 80px rgba(0, 0, 0, 0.35);
                }

                h1 {
                  margin-top: 0;
                  font-size: 24px;
                }

                p {
                  line-height: 1.6;
                  color: #cbd5e1;
                }
              </style>
            </head>
            <body>
              <main>
                <h1>{{WebUtility.HtmlEncode(title)}}</h1>
                <p>{{WebUtility.HtmlEncode(message)}}</p>
              </main>
            </body>
            </html>
            """;
    }

    #endregion
}