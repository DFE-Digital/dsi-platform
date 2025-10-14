using Dfe.SignIn.Core.Contracts.SupportTickets;
using Dfe.SignIn.Core.UseCases.SupportTickets;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.SupportTickets;

[TestClass]
public sealed class GetSubjectOptionsForSupportTicketUseCaseTests
{
    [TestMethod]
    public Task Throws_WhenRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetSubjectOptionsForSupportTicketRequest,
            GetSubjectOptionsForSupportTicketUseCase
        >();
    }

    [TestMethod]
    public async Task ReturnsExpectedSubjectOptionCodes()
    {
        var autoMocker = new AutoMocker();
        var interactor = autoMocker.CreateInstance<GetSubjectOptionsForSupportTicketUseCase>();

        var response = await interactor.InvokeAsync(new GetSubjectOptionsForSupportTicketRequest());

        var expectedCodes = new string[] {
            "create-account",
            "service-access",
            "email-password",
            "deactivate-account",
            "approver",
            "add-org",
            "other"
        };
        var actualCodes = response.SubjectOptions.Select(x => x.Code).ToArray();
        CollectionAssert.AreEquivalent(expectedCodes, actualCodes);
    }
}
