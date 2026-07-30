using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.InternalApi.Features.Users;

/// <summary>
/// Provides extension methods for mapping user-related endpoints to an <see cref="IEndpointRouteBuilder"/>.
/// </summary>
[ExcludeFromCodeCoverage]
public static class UsersEndpoints
{
    /// <inheritdoc/>
    public static void MapUsersEndpoints(this IEndpointRouteBuilder app)
    {
        ChangeName.ChangeNameEndpoint.Map(app);
    }
}
