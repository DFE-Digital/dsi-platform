using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.TestHelpers.Integration;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string EnableAuthHeaderName = "X-Test-Auth";
    public const string UserIdHeaderName = "X-Test-UserId";
    public const string UserNameHeaderName = "X-Test-UserName";
    public const string RoleHeaderName = "X-Test-Role";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    private const string DefaultTestUserId = "00000000-0000-0000-0000-000000000001";
    private const string DefaultTestUserName = "TestUser";

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!this.Request.Headers.TryGetValue(EnableAuthHeaderName, out var authValues)) {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!bool.TryParse(authValues.ToString(), out var shouldAuthenticate) || !shouldAuthenticate) {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var userId = this.Request.Headers.TryGetValue(UserIdHeaderName, out var userIdValues)
            ? userIdValues.ToString()
            : DefaultTestUserId;

        var userName = this.Request.Headers.TryGetValue(UserNameHeaderName, out var userNameValues)
            ? userNameValues.ToString()
            : DefaultTestUserName;

        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, userName),
            new(ClaimTypes.NameIdentifier, userId)
        };

        if (this.Request.Headers.TryGetValue(RoleHeaderName, out var roleValues)) {
            foreach (var roleValue in roleValues) {
                if (!string.IsNullOrWhiteSpace(roleValue)) {
                    claims.Add(new Claim(ClaimTypes.Role, roleValue));
                }
            }
        }

        var identity = new ClaimsIdentity(claims, "TestScheme");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
