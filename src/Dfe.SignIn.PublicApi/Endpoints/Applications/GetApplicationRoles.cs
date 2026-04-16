using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.PublicApi.Contracts.Applications;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.Applications;

public static partial class ApplicationEndpoints
{
    /// <summary>
    /// Get the roles associated with a service application.
    /// </summary>
    /// <param name="clientId">The unique client identifier of the application.</param>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <returns>The application roles.</returns>
    //[Produces("application/json")]
    //[ProduceResponseType(typeof(IEnumerable<ApplicationRole>), StatusCodes.Status200OK)]
    //[ProduceResponseType(StatusCodes.Status404NotFound)]
    //[ProduceResponseType(StatusCodes.Status400BadRequest)]
    public static async Task<IEnumerable<ApplicationRoleDto>> GetApplicationRoles(
        [FromRoute] string clientId,
        IInteractionDispatcher interaction)
    {
        var applicationResponse = await interaction.DispatchAsync(
            new GetApplicationByClientIdRequest {
                ClientId = clientId
            }
        ).To<GetApplicationByClientIdResponse>();

        var applicationId = applicationResponse.Application.Id;

        var rolesResponse = await interaction.DispatchAsync(
            new GetRolesOfApplicationRequest {
                ApplicationId = applicationId
            }
        ).To<GetRolesOfApplicationResponse>();

        return rolesResponse.Roles
            .Select(r => new ApplicationRoleDto {
                Name = r.Name,
                Code = r.Code,
                Status = r.Status.ToString()
            });
    }
}
