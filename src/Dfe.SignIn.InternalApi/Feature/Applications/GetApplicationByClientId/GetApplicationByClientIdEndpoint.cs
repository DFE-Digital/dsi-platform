using Dfe.SignIn.Core.Contracts.Features.Applications;
using Dfe.SignIn.Core.Contracts.Features.Applications.GetApplicationByClientId;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.InternalApi.Feature.Applications.GetApplicationByClientId;

/// <summary>
/// An endpoint to get the profile of a user.
/// </summary>
public sealed class GetApplicationByClientIdEndpoint
{
    /// <inheritdoc/>
    public static void Map(IEndpointRouteBuilder app)
    {
        app.MapPost(ApiRoutes.GetApplicationByClientId, Handler)
            .WithName("Get Application By Client Id")
            .WithTags("Applications")
            .Produces<GetApplicationByClientIdResponse>(StatusCodes.Status200OK)
            .Produces(StatusCodes.Status400BadRequest)
            .WithValidationFilter<GetApplicationByClientIdRequest>()
            .WithOpenApi();
    }

    /// <inheritdoc/>
    public static async Task<IResult> Handler(
        DbDirectoriesContext dbDirectoriesContext,
        DbOrganisationsContext dbOrganisationsContext,
        ILoggerFactory loggerFactory,
        HttpContext httpContext,
        [FromBody] GetApplicationByClientIdRequest request,
        CancellationToken cancellationToken)
    {

        var serviceEntity = await dbOrganisationsContext.Services
    .Select(x => new {
        x.Id,
        x.ClientId,
        x.Description,
        x.Name,
        x.ServiceHome,
        x.IsExternalService,
        x.IsHiddenService,
        x.IsIdOnlyService,
        x.ParentId,
        ParentClientId = x.Parent != null ? x.Parent.ClientId : null,
    })
    .SingleOrDefaultAsync(
        x => x.ClientId == request.ClientId,
        cancellationToken
    ) ?? throw new ApplicationNotFoundException(null, request.ClientId);

        Uri? serviceHomeUrl = !string.IsNullOrWhiteSpace(serviceEntity.ServiceHome)
            ? new Uri(serviceEntity.ServiceHome)
            : null;

        var response = new GetApplicationByClientIdResponse {
            Application = new() {
                Id = serviceEntity.Id,
                ClientId = serviceEntity.ClientId,
                Description = serviceEntity.Description,
                Name = serviceEntity.Name,
                ServiceHomeUrl = serviceHomeUrl,
                IsExternalService = serviceEntity.IsExternalService,
                IsHiddenService = serviceEntity.IsHiddenService,
                IsIdOnlyService = serviceEntity.IsIdOnlyService,
                ParentId = serviceEntity.ParentId,
                ParentClientId = serviceEntity.ParentClientId,
            }
        };

        return Results.Ok(response);
    }
}
