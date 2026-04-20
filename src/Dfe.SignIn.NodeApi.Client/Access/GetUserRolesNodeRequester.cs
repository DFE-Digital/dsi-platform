using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.NodeApi.Client.Access;

/// <summary>
/// ApiRequester for obtaining roles of a specific application.
/// </summary>
[ApiRequester, NodeApi(NodeApiName.Access)]
public sealed class GetServiceRolesNodeRequester(
    [FromKeyedServices(NodeApiName.Access)] HttpClient accessClient
) : Interactor<GetRolesOfUserRequest, GetRolesOfUserResponse>
{
    /// <inheritdoc/>
    public override async Task<GetRolesOfUserResponse> InvokeAsync(
        InteractionContext<GetRolesOfUserRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var response = await accessClient.GetFromJsonOrDefaultAsync<Models.ApplicationUserRoleDto>(
            $"services/{context.Request.ApplicationId}/organisations/{context.Request.OrganisationId}/users/{context.Request.UserId}",
            cancellationToken
        );

        string path = $"users/{context.Request.UserId}/services/{context.Request.ApplicationId}/organisations/{context.Request.OrganisationId}";
        //path = $"users/{userId}/services/{serviceId}/organisations/{orgId}"
        var myRequest = new HttpRequestMessage(HttpMethod.Get, path);

        var myResponse = await accessClient.SendAsync(myRequest);
        var myTextPlease = await myResponse.Content.ReadAsStringAsync();
        Console.WriteLine($"GetRolesOfUserResponse {path}");
        Console.WriteLine(myTextPlease);


        List<string> roles = [];
        return new GetRolesOfUserResponse { Roles = roles };
    }
}
