using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Core.Contracts.Features.Users.Exceptions;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Features.Users.ChangeName;

/// <summary>
/// An endpoint to change the name of a user.
/// </summary>
public sealed class ChangeNameEndpoint : IEndpoint
{
    /// <summary>
    /// Maps the endpoint to the specified <see cref="IEndpointRouteBuilder"/>.
    /// </summary>
    /// <param name="app">The endpoint route builder to map the endpoint to.</param>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(UsersApiRoutes.ChangeName, Handler)
            .WithName("Change Name")
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidationFilter<ChangeNameRequest>()
            .WithOpenApi();
    }

    /// <summary>
    /// Changes the name of a user.
    /// </summary>
    /// <param name="directoriesDbContext">The database context to use for accessing user data.</param>
    /// <param name="logger">The logger to use for logging information.</param>
    /// <param name="request">The request containing the user ID and new name.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The result of the operation.</returns>
    public static async Task<IResult> Handler(
        DbDirectoriesContext directoriesDbContext,
        ILogger<ChangeNameEndpoint> logger,
        [FromBody] ChangeNameRequest request,
        CancellationToken cancellationToken)
    {
        logger.LogInformation("Changing name for user {UserId}", request.UserId);

        var user = await directoriesDbContext.Users
            .Where(x => x.Sub == request.UserId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw UserNotFoundException.FromUserId(request.UserId);

        if (user.FirstName == request.FirstName && user.LastName == request.LastName) {
            return Results.Ok();
            //return new ChangeNameResponse();
        }

        if (user.FirstName != request.FirstName) {
            user.FirstName = request.FirstName.NormalizeWhitespace();
        }

        if (user.LastName != request.LastName) {
            user.LastName = request.LastName.NormalizeWhitespace();
        }

        await directoriesDbContext.SaveChangesAsync(cancellationToken);

        //TODO: Add new implementation for audit logging here, as the previous implementation was commented out.
        //await interaction.DispatchAsync(
        //    new WriteToAuditRequest {
        //        EventCategory = AuditEventCategoryNames.ChangeName,
        //        Message = $"Successfully changed users name to {user.FirstName} {user.LastName}",
        //        UserId = context.Request.UserId,
        //    }
        //);

        return Results.Ok();
    }
}
