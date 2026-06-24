using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.GetUserProfile;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Feature.Users.GetUserProfile;

/// <summary>
/// An endpoint to get the profile of a user.
/// </summary>
public sealed class GetUserProfileEndpoint
{
    /// <inheritdoc/>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.GetUserProfile, Handler)
            .WithName("Get User Profile")
            .WithTags("Users")
            .Produces<GetUserProfileResponseA>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidationFilter<GetUserProfileRequestA>()
            .WithOpenApi();
    }

    /// <inheritdoc/>
    public static async Task<IResult> Handler(
        DbDirectoriesContext dbDirectoriesContext,
        ILoggerFactory loggerFactory,
        HttpContext httpContext,
        [FromBody] GetUserProfileRequestA query,
        CancellationToken cancellationToken)
    {
        var user = await dbDirectoriesContext
            .Users
            .Where(x => x.Sub == query.UserId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw Core.Contracts.Users.UserNotFoundException.FromUserId(query.UserId);

        var response = new GetUserProfileResponseA {
            IsEntra = user.IsEntra,
            IsInternalUser = user.IsInternalUser,
            FirstName = user.FirstName,
            LastName = user.LastName,
            JobTitle = !string.IsNullOrWhiteSpace(user.JobTitle) ? user.JobTitle : null,
            EmailAddress = user.Email,
            Status = user.Status,
        };

        return Results.Ok(response);
    }
}
