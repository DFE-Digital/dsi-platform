using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

public static partial class UserEndpoints
{
    public static void UseUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("v2/users/{userId}/organisations/{organisationId}/query", PostQueryUserOrganisation);

        app.MapGet("/users", GetServiceUsers)
            .WithName("GetServiceUsersRequest")
            .WithTags("Users")
            .Produces<GetServiceUsersResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithOpenApi();
    }
}
