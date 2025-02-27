using Dfe.SignIn.SelectOrganisation.Data;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.SelectOrganisation.Web.Controllers;

/// <summary>
/// A controller to assist with development.
/// </summary>
public sealed class DeveloperController : Controller
{
    private readonly ISelectOrganisationSessionStorer selectOrganisationStorer;

    public DeveloperController(
        ISelectOrganisationSessionStorer selectOrganisationStorer)
    {
        this.selectOrganisationStorer = selectOrganisationStorer;
    }

    public async Task<IActionResult> Index()
    {
        var session = new SelectOrganisationSessionData {
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow + new TimeSpan(24, 0, 0),
            CallbackUrl = new Uri("https://example.localhost/callback"),
            ClientId = "test-client",
            UserId = Guid.NewGuid(),
            Prompt = new() {
                Heading = "Which organisation would you like to use?",
                Hint = "Select one option.",
            },
            OrganisationOptions = [
                new SelectOrganisationOption {
                    Id = Guid.NewGuid(),
                    Name = "Organisation A",
                },
                new SelectOrganisationOption {
                    Id = Guid.NewGuid(),
                    Name = "Organisation B",
                },
                new SelectOrganisationOption {
                    Id = Guid.NewGuid(),
                    Name = "Organisation C",
                },
            ],
        };

        await this.selectOrganisationStorer.StoreSessionAsync("test", session);

        return this.View();
    }
}
