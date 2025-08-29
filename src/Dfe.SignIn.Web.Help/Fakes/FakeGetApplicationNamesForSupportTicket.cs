using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SupportTickets;

namespace Dfe.SignIn.Web.Help.Fakes;

public sealed class FakeGetApplicationNamesForSupportTicket
    : Interactor<GetApplicationNamesForSupportTicketRequest, GetApplicationNamesForSupportTicketResponse>
{
    public override Task<GetApplicationNamesForSupportTicketResponse> InvokeAsync(
        InteractionContext<GetApplicationNamesForSupportTicketRequest> context,
        CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new GetApplicationNamesForSupportTicketResponse {
            Applications = [
                new() {
                    Name = "Fake Service A",
                },
                new() {
                    Name = "Fake Service B",
                },
                new() {
                    Name = "Other (please specify)",
                },
                new() {
                    Name = "None",
                },
            ],
        });
    }
}
