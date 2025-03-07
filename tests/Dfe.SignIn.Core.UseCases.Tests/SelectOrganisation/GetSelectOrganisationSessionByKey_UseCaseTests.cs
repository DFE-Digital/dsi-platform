using Dfe.SignIn.Core.Models.SelectOrganisation;
using Dfe.SignIn.Core.PublicModels.SelectOrganisation;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Dfe.SignIn.Core.UseCases.SelectOrganisation;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.Tests.SelectOrganisation;

[TestClass]
public sealed class GetSelectOrganisationSessionByKey_UseCaseTests
{
    #region InvokeAsync(GetSelectOrganisationSessionByKeyRequest)

    [TestMethod]
    public async Task InvokeAsync_FetchesSessionWithCorrectSessionKey()
    {
        var autoMocker = new AutoMocker();
        var useCase = autoMocker.CreateInstance<GetSelectOrganisationSessionByKey_UseCase>();

        var response = await useCase.InvokeAsync(new() {
            SessionKey = "cd66b69c-144c-4365-96f6-4302b754c18b",
        });

        autoMocker.Verify<ISelectOrganisationSessionRepository>(
            x => x.RetrieveAsync(
                It.Is<string>(sessionKey =>
                    sessionKey == "cd66b69c-144c-4365-96f6-4302b754c18b"
                )
            )
        );
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedSessionData()
    {
        var autoMocker = new AutoMocker();

        var fakeSession = new SelectOrganisationSessionData {
            Created = DateTime.UtcNow,
            Expires = DateTime.UtcNow + new TimeSpan(0, 10, 0),
            CallbackUrl = new Uri("https://example.localhost/callback"),
            ClientId = "test-client",
            UserId = Guid.NewGuid(),
            Prompt = new SelectOrganisationPrompt {
                Heading = "Which organisation?",
                Hint = "Select one option."
            },
            OrganisationOptions = [],
        };

        autoMocker.GetMock<ISelectOrganisationSessionRepository>()
            .Setup(x => x.RetrieveAsync(
                It.IsAny<string>()
            ))
            .ReturnsAsync(fakeSession);

        var useCase = autoMocker.CreateInstance<GetSelectOrganisationSessionByKey_UseCase>();

        var response = await useCase.InvokeAsync(new() {
            SessionKey = "cd66b69c-144c-4365-96f6-4302b754c18b",
        });

        Assert.AreEqual(fakeSession, response.SessionData);
    }

    #endregion
}
