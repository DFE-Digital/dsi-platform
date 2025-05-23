using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Dfe.SignIn.Web.SelectOrganisation.Controllers;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Web.SelectOrganisation.UnitTests.Controllers;

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
                    param.CallbackUrl == new Uri($"https://example.localhost/callback?{CallbackParamNames.RequestId}=00000000-0000-0000-0000-000000000000") &&
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
