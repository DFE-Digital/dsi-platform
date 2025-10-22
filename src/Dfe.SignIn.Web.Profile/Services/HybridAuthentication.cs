using System.Diagnostics;
using System.Text.Json;
using Azure.Core;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Identity.Client;

namespace Dfe.SignIn.Web.Profile.Services;

public interface IHybridAuthentication
{
    Task<Uri> GetAuthorizationRequestUrlAsync(string callbackUri, string? returnUri, string? loginHint);

    Task<Uri?> ProcessCallbackAsync();

    AccessToken? GetAccessTokenFromSession();

    void VerifyAccessToken(string callbackUri, string? loginHint, out AccessToken? accessToken, out Task<Uri>? getAuthUrlTask);
}

public sealed class HybridAuthentication(
    IHttpContextAccessor httpContextAccessor,
    IConfidentialClientApplication confidentialClientApp
) : IHybridAuthentication
{
    private const string CodeVerifierSessionKey = "125097a0-2ec3-492d-b0db-6bf390bad109";
    private const string AccessTokenSessionKey = "8e64adac-b146-461a-9d76-bfeaaa43d1e6";
    private const string AccessTokenExpiresOnSessionKey = "9f261193-4b1c-4625-8711-26d2e023e7bb";
    private const string ReturnUriSessionKey = "15500975-23cd-46dd-a3ef-a5106cdeb7b9";

    private static Guid GetCorrelationId()
        => Guid.TryParse(Activity.Current?.TraceId.ToString() ?? "", out var parsedTraceId)
            ? parsedTraceId : Guid.NewGuid();

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

        httpContext.Session.SetString(CodeVerifierSessionKey, codeVerifier);
        httpContext.Session.SetString(ReturnUriSessionKey, returnUri ?? "");

        return url;
    }

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

        string codeVerifier = httpContext.Session.GetString(CodeVerifierSessionKey)
            ?? throw new InvalidOperationException("Missing session state 'code verifier'.");

        string? returnUri = httpContext.Session.GetString(ReturnUriSessionKey);

        var result = await confidentialClientApp
            .AcquireTokenByAuthorizationCode([], httpContext.Request.Query["code"])
            .WithPkceCodeVerifier(codeVerifier)
            .ExecuteAsync();

        httpContext.Session.Remove(CodeVerifierSessionKey);
        httpContext.Session.Remove(ReturnUriSessionKey);

        var accessToken = new AccessToken(result.AccessToken, result.ExpiresOn);
        httpContext.Session.SetString(AccessTokenSessionKey, accessToken.Token);
        httpContext.Session.SetString(AccessTokenExpiresOnSessionKey, JsonSerializer.Serialize(accessToken.ExpiresOn));

        return !string.IsNullOrWhiteSpace(returnUri) ? new Uri(returnUri!) : null;
    }

    public AccessToken? GetAccessTokenFromSession()
    {
        var httpContext = httpContextAccessor.HttpContext
            ?? throw new InvalidOperationException("Invalid context.");

        string? token = httpContext.Session.GetString(AccessTokenSessionKey);
        string? expiresOn = httpContext.Session.GetString(AccessTokenExpiresOnSessionKey);

        if (token is null || expiresOn is null) {
            return null;
        }

        return new AccessToken(
            token,
            expiresOn: JsonSerializer.Deserialize<DateTimeOffset>(expiresOn)
        );
    }

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
