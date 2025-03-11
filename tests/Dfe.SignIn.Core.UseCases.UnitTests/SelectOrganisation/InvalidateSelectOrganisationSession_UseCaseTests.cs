using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Dfe.SignIn.Core.UseCases.SelectOrganisation;
using Moq;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.SelectOrganisation;

[TestClass]
public sealed class InvalidateSelectOrganisationSession_UseCaseTests
{
    #region InvokeAsync(InvalidateSelectOrganisationSessionRequest)

    [TestMethod]
    public async Task InvokeAsync_InvalidatesSessionWithCorrectSessionKey()
    {
        var autoMocker = new AutoMocker();
        var useCase = autoMocker.CreateInstance<InvalidateSelectOrganisationSession_UseCase>();

        var response = await useCase.InvokeAsync(new() {
            SessionKey = "cd66b69c-144c-4365-96f6-4302b754c18b",
        });

        autoMocker.Verify<ISelectOrganisationSessionRepository>(
            x => x.InvalidateAsync(
                It.Is<string>(sessionKey =>
                    sessionKey == "cd66b69c-144c-4365-96f6-4302b754c18b"
                )
            )
        );
    }

    #endregion
}
