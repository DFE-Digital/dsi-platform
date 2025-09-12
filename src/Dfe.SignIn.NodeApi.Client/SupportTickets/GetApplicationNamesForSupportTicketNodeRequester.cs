using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SupportTickets;
using Dfe.SignIn.NodeApi.Client.Applications.Models;
using Dfe.SignIn.NodeApi.Client.AuthenticatedHttpClient;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.SupportTickets;

/// <summary>
/// An interactor to retrieve the list of application names that can be chosen from
/// when raising a support ticket. This implementation makes requests to the Node
/// 'applications' middle-tier API.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Applications)]
public sealed class GetApplicationNamesForSupportTicketNodeRequester(
    [FromKeyedServices(NodeApiName.Applications)] HttpClient applicationsClient
) : Interactor<GetApplicationNamesForSupportTicketRequest, GetApplicationNamesForSupportTicketResponse>
{
    /// <inheritdoc/>
    public override async Task<GetApplicationNamesForSupportTicketResponse> InvokeAsync(
        InteractionContext<GetApplicationNamesForSupportTicketRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = (await applicationsClient.GetFromJsonOrDefaultAsync<ApplicationListingDto>(
            $"services?page=1&pageSize=1000",
            cancellationToken
        ))!;

        var filteredServices = response.Services
            .Where(service => service.RelyingParty.Params?.HelpHidden != "true")
            .OrderBy(service => service.Name);

        return new GetApplicationNamesForSupportTicketResponse {
            Applications = [
                .. filteredServices.Select(service => new ApplicationNameForSupportTicket {
                    Name = service.Name,
                }),
                new() {
                    Name = "Other (please specify)",
                },
                new() {
                    Name = "None",
                },
            ],
        };
    }
}
