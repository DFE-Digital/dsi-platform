using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Dfe.SignIn.Core.UseCases.SelectOrganisation;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.SelectOrganisation;

[TestClass]
public sealed class GetSelectOrganisationSessionByKey_UseCaseTests
{
    #region InvokeAsync(GetSelectOrganisationSessionByKeyRequest)

    [TestMethod]
    public Task InvokeAsync_ThrowsIfRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetSelectOrganisationSessionByKeyRequest,
            GetSelectOrganisationSessionByKey_UseCase
        >();
    }

    [TestMethod]
    public async Task InvokeAsync_FetchesSessionWithCorrectSessionKey()
    {
        var autoMocker = new AutoMocker();
        var useCase = autoMocker.CreateInstance<GetSelectOrganisationSessionByKey_UseCase>();

        await useCase.InvokeAsync(new GetSelectOrganisationSessionByKeyRequest {
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
            ClientId = "test-client",
            UserId = Guid.NewGuid(),
            Prompt = new SelectOrganisationPrompt {
                Heading = "Which organisation?",
                Hint = "Select one option."
            },
            OrganisationOptions = [],
            AllowCancel = true,
            CallbackUrl = new Uri("https://example.localhost/callback"),
        };

        autoMocker.GetMock<ISelectOrganisationSessionRepository>()
            .Setup(x => x.RetrieveAsync(
                It.IsAny<string>()
            ))
            .ReturnsAsync(fakeSession);

        var useCase = autoMocker.CreateInstance<GetSelectOrganisationSessionByKey_UseCase>();

        var response = await useCase.InvokeAsync(new GetSelectOrganisationSessionByKeyRequest {
            SessionKey = "cd66b69c-144c-4365-96f6-4302b754c18b",
        });

        Assert.AreEqual(fakeSession, response.SessionData);
    }

    #endregion
}
