using System.Net.Http.Json;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Organisations;
using Dfe.SignIn.Core.Models.Users.Interactions;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApiClient.Organisations;

/// <summary>
/// ApiRequester for obtaining organisations associated with a user.
/// </summary>
/// <param name="httpClient"></param>
[ApiRequester, NodeApi(NodeApiName.Organisations)]
public sealed class GetOrganisationsAssociatedWithUser_ApiRequester(
    [FromKeyedServices(NodeApiName.Organisations)] HttpClient httpClient)
    : IInteractor<GetOrganisationsAssociatedWithUserRequest, GetOrganisationsAssociatedWithUserResponse>
{

    /// <inheritdoc/>
    public async Task<GetOrganisationsAssociatedWithUserResponse> InvokeAsync(GetOrganisationsAssociatedWithUserRequest request)
    {
        var response = await httpClient.GetFromJsonAsync<Models.OrganisationsAssociatedWithUserDto[]>($"/organisations/v2/associated-with-user/{request.UserId}");

        var organisations = response?.Select(org => new OrganisationModel {
            Id = org.Organisation.Id,
            Name = org.Organisation.Name,
            LegalName = org.Organisation.LegalName ?? string.Empty
        }) ?? [];

        return new GetOrganisationsAssociatedWithUserResponse {
            Organisations = organisations
        };
    }
}