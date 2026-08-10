using System.Diagnostics.CodeAnalysis;
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.InternalApi.Configuration;

/// <summary>
/// An authentication handler that allows local development bypass of authentication for testing purposes.
/// </summary>
[ExcludeFromCodeCoverage(Justification = "We could come back and test this, but it is not worth the effort for now.")]
public sealed class LocalDevAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    /// <summary>
    /// Handles the authentication process for local development bypass.
    /// This method creates a claims principal with a fixed user ID and name, simulating an authenticated user for testing purposes.
    /// </summary>
    /// <returns>A task that represents the asynchronous authentication operation. The task result contains the authentication result.</returns>
    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "00000000-0000-0000-0000-000000000001"),
            new Claim(ClaimTypes.Name, "Local Developer")
        };

        var identity = new ClaimsIdentity(claims, this.Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, this.Scheme.Name);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
