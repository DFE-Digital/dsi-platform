using Dfe.SignIn.Core.Models.SelectOrganisation;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Dfe.SignIn.SelectOrganisation.Web.Controllers;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.SelectOrganisation.Web.UnitTests.Controllers;

[TestClass]
public sealed class DeveloperControllerTests
{
    #region Index()

    [TestMethod]
    public async Task Index_CreatesTestSession()
    {
        var mocker = new AutoMocker();
        var controller = mocker.CreateInstance<DeveloperController>();

        var result = await controller.Index();

        var selectOrganisationRepositoryMock = mocker.GetMock<ISelectOrganisationSessionRepository>();
        selectOrganisationRepositoryMock.Verify(
            x => x.StoreAsync(
                It.Is<string>(param => param == "test"),
                It.Is<SelectOrganisationSessionData>(param =>
                    param.CallbackUrl == new Uri("https://example.localhost/callback") &&
                    param.ClientId == "test-client" &&
                    param.UserId != Guid.Empty &&
                    !string.IsNullOrEmpty(param.Prompt.Heading) &&
                    !string.IsNullOrEmpty(param.Prompt.Hint) &&
                    param.OrganisationOptions.Count() == 3
                )
            ),
            Times.Once
        );
    }

    #endregion
}
