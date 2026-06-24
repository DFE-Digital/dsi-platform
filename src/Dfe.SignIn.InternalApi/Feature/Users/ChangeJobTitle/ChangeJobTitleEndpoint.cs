using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Feature.Users.ChangeJobTitle;

/// <summary>
/// An endpoint to change the job title of a user.
/// </summary>
public sealed class ChangeJobTitleEndpoint
{
    /// <inheritdoc/>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.ChangeJobTitle, Handler)
            .WithName("Change Job Title")
            .WithTags("Users")
            .Produces(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidationFilter<ChangeJobTitleRequest>()
            .WithOpenApi();
    }

    /// <inheritdoc/>
    public static async Task<IResult> Handler(
        DbDirectoriesContext dbDirectoriesContext,
        ILoggerFactory loggerFactory,
        HttpContext httpContext,
        [FromBody] ChangeJobTitleRequest query,
        CancellationToken cancellationToken)
    {
        var user = await dbDirectoriesContext
            .Users
            .Where(x => x.Sub == query.UserId)
            .FirstOrDefaultAsync(cancellationToken) ?? throw UserNotFoundException.FromUserId(query.UserId);

        if (user.JobTitle == query.NewJobTitle) {
            return Results.Ok(); //TODO: Consider returning a different status code or message indicating that the job title is already set to the desired value.
        }

        var normalisedJobTitle = query.NewJobTitle.NormalizeWhitespace();

        user.JobTitle = normalisedJobTitle;

        await dbDirectoriesContext.SaveChangesAsync(cancellationToken);

        //TODO: Add audit logging for job title change
        //await interaction.DispatchAsync(
        //    new WriteToAuditRequest {
        //        EventCategory = AuditEventCategoryNames.ChangeJobTitle,
        //        Message = $"Successfully changed job title to {normalisedJobTitle}",
        //        UserId = context.Request.UserId,
        //    }
        //);

        return Results.Ok();
    }
}
