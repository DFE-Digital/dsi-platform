using Dfe.SignIn.SelectOrganisation.Data;
using Dfe.SignIn.SelectOrganisation.Web.Controllers;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.SelectOrganisation.Web.Tests.Controllers;

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

        var selectOrganisationStorerMock = mocker.GetMock<ISelectOrganisationSessionStorer>();
        selectOrganisationStorerMock.Verify(
            x => x.StoreSessionAsync(
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
