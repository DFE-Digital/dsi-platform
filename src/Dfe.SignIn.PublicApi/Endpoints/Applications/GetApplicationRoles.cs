using System.Diagnostics;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Contracts.Applications;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.Applications;

public static partial class ApplicationEndpoints
{
    /// <summary>
    /// Get the roles associated with a service application.
    /// </summary>
    /// <param name="clientId">The unique client identifier of the application.</param>
    /// <param name="clientSession">The client session for the current request.</param>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="loggerFactory">Factory to create loggers for logging request details.</param>
    /// <param name="httpContext">The current HTTP context, used to access headers and request information.</param>
    /// <returns>The application roles.</returns>
    public static async Task<IResult> GetApplicationRoles(
        [FromRoute] string clientId,
        IClientSession clientSession,
        IInteractionDispatcher interaction,
        ILoggerFactory loggerFactory,
        HttpContext httpContext)
    {
        var logger = loggerFactory.CreateLogger(nameof(ApplicationEndpoints));

        var correlationId = Activity.Current?.TraceId.ToString();
        var clientCorrelationId = httpContext.Request.Headers["x-correlation-id"].FirstOrDefault();

        logger.LogInformation(
            "{ClientId} is attempting to get service roles for: {RequestedClientId} (correlationId: {CorrelationId}, clientCorrelationId: {ClientCorrelationId})",
            clientSession.ClientId,
            clientId,
            correlationId,
            clientCorrelationId
        );

        GetApplicationByClientIdResponse applicationResponse;

        try {
            applicationResponse = await interaction.DispatchAsync(
                new GetApplicationByClientIdRequest {
                    ClientId = clientId
                }
            ).To<GetApplicationByClientIdResponse>();
        }
        catch (ApplicationNotFoundException) {
            return Results.NotFound($"Application with clientId '{clientId}' not found.");
        }
        catch (Exception ex) {
            logger.LogError(ex, "Unexpected error while retrieving roles for clientId {ClientId}", clientId);
            throw;
        }

        var application = applicationResponse.Application;

        // TODO:
        // the old node version checks application parent id too, is this still needed??
        // if so we will need to include parent application id in the GetApplicationByClientIdResponse and check it here as well
        // also the client session should also inclide the service/application id to be able to validate the parent
        if (application.ClientId != clientSession.ClientId) {
            return Results.Forbid();
        }

        var rolesResponse = await interaction.DispatchAsync(
            new GetRolesOfApplicationRequest {
                ApplicationId = application.Id
            }
        ).To<GetRolesOfApplicationResponse>();

        var roles = rolesResponse.Roles
            .Select(r => new ApplicationRoleDto {
                Name = r.Name,
                Code = r.Code,
                Status = r.Status.ToString()
            });

        return Results.Ok(roles);
    }
}
