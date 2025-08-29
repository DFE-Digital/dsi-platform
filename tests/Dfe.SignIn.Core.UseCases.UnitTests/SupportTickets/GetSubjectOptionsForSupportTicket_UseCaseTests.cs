using Dfe.SignIn.Core.InternalModels.SupportTickets;
using Dfe.SignIn.Core.UseCases.SupportTickets;
using Moq.AutoMock;

namespace Dfe.SignIn.Core.UseCases.UnitTests.SupportTickets;

[TestClass]
public sealed class GetSubjectOptionsForSupportTicket_UseCaseTests
{
    [TestMethod]
    public Task InvokeAsync_ThrowsIfRequestIsInvalid()
    {
        return InteractionAssert.ThrowsWhenRequestIsInvalid<
            GetSubjectOptionsForSupportTicketRequest,
            GetSubjectOptionsForSupportTicket_UseCase
        >();
    }

    [TestMethod]
    public async Task InvokeAsync_ReturnsExpectedSubjectOptionCodes()
    {
        var autoMocker = new AutoMocker();
        var interactor = autoMocker.CreateInstance<GetSubjectOptionsForSupportTicket_UseCase>();

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
