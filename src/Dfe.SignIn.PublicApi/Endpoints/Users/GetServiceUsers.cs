using System.Diagnostics;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.PublicApi.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

/// <summary>
/// Endpoints for service users, migrated from Node.js getServiceUsers.
/// </summary>
public static partial class UserEndpoints
{
    /// <summary>
    /// Get the service users for a given service (client).
    /// </summary>
    /// <param name="page"></param>
    /// <param name="pageSize"></param>
    /// <param name="clientSession">The client session for the current request.</param>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="loggerFactory">Factory to create loggers for logging request details.</param>
    /// <param name="httpContext">The current HTTP context, used to access headers and request information.</param>
    /// <returns>The service users for the given service.</returns>
    public static async Task<IResult> GetServiceUsers(
        IClientSession clientSession,
        IInteractionDispatcher interaction,
        ILoggerFactory loggerFactory,
        HttpContext httpContext,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 25)
    {
        var logger = loggerFactory.CreateLogger(nameof(UserEndpoints));
        var correlationId = Activity.Current?.TraceId.ToString();
        var clientCorrelationId = httpContext.Request.Headers["x-correlation-id"].FirstOrDefault();

        logger.LogInformation(
            "{ClientId} is attempting to get service users (correlationId: {CorrelationId}, clientCorrelationId: {ClientCorrelationId})",
            clientSession.ClientId,
            correlationId,
            clientCorrelationId
        );

        var clientId = clientSession.ClientId;

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
            return Results.Problem("An unexpected error occurred while retrieving application roles.");
        }

        if (applicationResponse?.Application == null) {
            return Results.NotFound();
        }

        var response = await interaction.DispatchAsync(
            new GetServiceUsersRequest {
                ApplicationId = applicationResponse.Application.Id,
                PageNumber = page,
                PageSize = pageSize,
            }
        ).To<GetServiceUsersResponse>();

        return Results.Ok(response);
    }
}
