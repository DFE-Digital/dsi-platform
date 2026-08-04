using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.InternalApi.Features.Users.ChangeName;
using Dfe.SignIn.InternalApi.Features.Users.GetUserProfile;
using Dfe.SignIn.InternalApi.Features.Users.UserCode;

namespace Dfe.SignIn.InternalApi.Features.Users;

/// <summary>
/// Provides extension methods for mapping user-related endpoints to an <see cref="IEndpointRouteBuilder"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public static class UsersFeature
{
    /// <summary>
    /// Maps the user-related endpoints to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoints to.</param>
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        ChangeNameEndpoint.Map(app);
        GetUserProfileEndpoint.Map(app);
    }

    /// <summary>
    /// Adds the <see cref="IUserLookupService"/> to the service collection.
    /// </summary>
    /// <param name="services">The service collection to add the service to.</param>
    /// <returns>The updated service collection.</returns>
    public static IServiceCollection AddUserServices(this IServiceCollection services)
    {
        services.AddScoped<IUserLookupService, UserLookupService>();
        services.AddScoped<IUserCodeService, UserCodeService>();
        return services;
    }
}
