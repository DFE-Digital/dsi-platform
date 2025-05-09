using Dfe.SignIn.Core.ExternalModels.Organisations;
using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
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
            DetailLevel = OrganisationDetailLevel.Basic,
            AllowCancel = true,
            CallbackUrl = new Uri("https://example.localhost/callback"),
        };

        await sessionRepository.StoreAsync("test", session);

        return this.View();
    }
}
