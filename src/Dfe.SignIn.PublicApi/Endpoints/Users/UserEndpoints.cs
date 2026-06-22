using Dfe.SignIn.PublicApi.Endpoints.Users.GetServiceUsers;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

/// <summary>
/// Provides extension methods for configuring user-related API endpoints in an ASP.NET Core application.
/// </summary>
/// <remarks>This class contains methods to register user endpoints with the application's routing system. It is
/// intended to be used during application startup to enable user-related API routes. The methods are designed to be
/// called on an instance of IEndpointRouteBuilder, typically within the Program.cs or Startup.cs file.</remarks>
public static partial class UserEndpoints
{
    /// <inheritdoc/>
    public static void UseUserEndpoints(this IEndpointRouteBuilder app)
    {
        app.MapPost("v2/users/{userId}/organisations/{organisationId}/query", PostQueryUserOrganisation);

        GetServiceUsersEndpoint.Map(app);
        app.MapGet("users/{userId}/organisations", GetUserOrganisations);

        app.MapGet("users/{userId}/organisationservices", GetUserOrganisationServices);
    }
}
