using System.Diagnostics;
using System.Text.Json;
using Azure.Core;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Identity.Client;

namespace Dfe.SignIn.Web.Profile.Services;

/// <summary>
/// .
/// </summary>
public interface IHybridAuthentication
{
    /// <summary>
    /// .
    /// </summary>
    /// <param name="callbackUri"></param>
    /// <param name="returnUri"></param>
    /// <param name="loginHint"></param>
    /// <returns></returns>
    Task<Uri> GetAuthorizationRequestUrlAsync(string callbackUri, string? returnUri, string? loginHint);

    /// <summary>
    /// .
    /// </summary>
    /// <returns></returns>
    Task<Uri?> ProcessCallbackAsync();

    /// <summary>
    /// .
    /// </summary>
    /// <returns></returns>
    AccessToken? GetAccessTokenFromSession();

    /// <summary>
    /// .
    /// </summary>
    /// <param name="callbackUri"></param>
    /// <param name="loginHint"></param>
    /// <param name="accessToken"></param>
    /// <param name="getAuthUrlTask"></param>
    void VerifyAccessToken(string callbackUri, string? loginHint, out AccessToken? accessToken, out Task<Uri>? getAuthUrlTask);
}

/// <summary>
/// A concrete implementation of the <see cref="IHybridAuthentication"/> service.
/// </summary>
public sealed class HybridAuthentication(
    IHttpContextAccessor httpContextAccessor,
    IConfidentialClientApplication confidentialClientApp
) : IHybridAuthentication
{
    private static Guid GetCorrelationId()
        => Guid.TryParse(Activity.Current?.TraceId.ToString() ?? "", out var parsedTraceId)
            ? parsedTraceId : Guid.NewGuid();

    /// <inheritdoc/>
    public async Task<Uri> GetAuthorizationRequestUrlAsync(string callbackUri, string? returnUri, string? loginHint)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Invalid context.");

        var urlBuilder = confidentialClientApp
            .GetAuthorizationRequestUrl([])
            .WithPkce(out string codeVerifier)
            .WithRedirectUri(callbackUri)
            .WithPrompt(Prompt.NoPrompt)
            .WithCorrelationId(GetCorrelationId());

        if (!string.IsNullOrWhiteSpace(loginHint)) {
            urlBuilder.WithLoginHint(loginHint);
            // The following line was previously required to hide the "Sign in with another account"
            // link since that would cause issues with user journey's where a login hint is
            // provided. This no longer seems to be necessary due to recent changes by Microsoft.
            //.WithExtraQueryParameters(new Dictionary<string, string> { ["hsu"] = "1" });
        }

        var url = await urlBuilder.ExecuteAsync();

        httpContext.Session.SetString(SessionKeys.HybridCodeVerifier, codeVerifier);
        httpContext.Session.SetString(SessionKeys.HybridReturnUri, returnUri ?? "");

        return url;
    }

    /// <inheritdoc/>
    public async Task<Uri?> ProcessCallbackAsync()
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Invalid context.");

        string? code = httpContext.Request.Query["code"];
        if (string.IsNullOrEmpty(code)) {
            string errorCode = httpContext.Request.Query["error"].FirstOrDefault() ?? "unknown";
            string errorDescription = httpContext.Request.Query["error_description"].FirstOrDefault() ?? "";
            throw new InvalidOperationException($"Unable to authenticate user. {errorCode}: {errorDescription}");
        }

        string codeVerifier = httpContext.Session.GetString(SessionKeys.HybridCodeVerifier)
            ?? throw new InvalidOperationException("Missing session state 'code verifier'.");

        string? returnUri = httpContext.Session.GetString(SessionKeys.HybridReturnUri);

        var result = await confidentialClientApp
            .AcquireTokenByAuthorizationCode([], httpContext.Request.Query["code"])
            .WithPkceCodeVerifier(codeVerifier)
            .ExecuteAsync();

        httpContext.Session.Remove(SessionKeys.HybridCodeVerifier);
        httpContext.Session.Remove(SessionKeys.HybridReturnUri);

        var accessToken = new AccessToken(result.AccessToken, result.ExpiresOn);
        httpContext.Session.SetString(SessionKeys.HybridAccessToken, accessToken.Token);
        httpContext.Session.SetString(SessionKeys.HybridAccessTokenExpiresOn, JsonSerializer.Serialize(accessToken.ExpiresOn));

        return !string.IsNullOrWhiteSpace(returnUri) ? new Uri(returnUri) : null;
    }

    /// <inheritdoc/>
    public AccessToken? GetAccessTokenFromSession()
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Invalid context.");

        string? token = httpContext.Session.GetString(SessionKeys.HybridAccessToken);
        string? expiresOn = httpContext.Session.GetString(SessionKeys.HybridAccessTokenExpiresOn);

        if (token is null || expiresOn is null) {
            return null;
        }

        return new AccessToken(
            token,
            expiresOn: JsonSerializer.Deserialize<DateTimeOffset>(expiresOn)
        );
    }

    /// <inheritdoc/>
    public void VerifyAccessToken(string callbackUri, string? loginHint, out AccessToken? accessToken, out Task<Uri>? getAuthUrlTask)
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Invalid context.");

        var possibleAccessToken = this.GetAccessTokenFromSession();

        if (possibleAccessToken is null || (possibleAccessToken.Value.ExpiresOn - DateTimeOffset.UtcNow).TotalMinutes < 10) {
            accessToken = null;
            string returnUri = httpContext.Request.GetEncodedUrl();
            getAuthUrlTask = this.GetAuthorizationRequestUrlAsync(callbackUri, returnUri, loginHint);
            return;
        }

        accessToken = possibleAccessToken;
        getAuthUrlTask = null;
        return;
    }
}
