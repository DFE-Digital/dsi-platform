using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Core.Interfaces.SelectOrganisationSessions;
using Dfe.SignIn.Core.Public.SelectOrganisation;
using Dfe.SignIn.Web.SelectOrganisation.Mocks;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.Web.SelectOrganisation.Controllers;

/// <summary>
/// A controller to assist with development.
/// </summary>
public sealed class DeveloperController(
    ISelectOrganisationSessionRepository sessionRepository
) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var session = new SelectOrganisationSessionData {
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow + new TimeSpan(24, 0, 0),
            ClientId = "test-client",
            UserId = Guid.NewGuid(),
            Prompt = new() {
                Heading = "Which organisation would you like to use?",
                Hint = "Select one option.",
            },
            OrganisationOptions = MockOrganisations.Models.Values
                .Select(organisation => new SelectOrganisationOption {
                    Id = organisation.Id,
                    Name = organisation.Name,
                }),
            AllowCancel = true,
            CallbackUrl = new Uri($"https://example.localhost/callback?{CallbackParamNames.RequestId}=00000000-0000-0000-0000-000000000000"),
        };

        await sessionRepository.StoreAsync("test", session);

        return this.View();
    }
}
