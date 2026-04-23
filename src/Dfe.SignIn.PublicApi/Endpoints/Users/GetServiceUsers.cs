using System.Diagnostics;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.PublicApi.Authorization;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

/// <summary>
/// Endpoints for service users, migrated from Node.js getServiceUsers.
/// </summary>
public static partial class UserEndpoints
{
    /// <summary>
    /// Get the service users for a given service (client).
    /// </summary>
    /// <param name="clientSession">The client session for the current request.</param>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="loggerFactory">Factory to create loggers for logging request details.</param>
    /// <param name="httpContext">The current HTTP context, used to access headers and request information.</param>
    /// <returns>The service users for the given service.</returns>
    public static async Task<IResult> GetServiceUsers(
        IClientSession clientSession,
        IInteractionDispatcher interaction,
        ILoggerFactory loggerFactory,
        HttpContext httpContext)
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

        // Extract pagination parameters from query string
        var page = 1;
        var pageSize = 25;
        if (httpContext.Request.Query.TryGetValue("page", out var pageValues) && int.TryParse(pageValues, out var parsedPage)) {
            page = parsedPage;
        }
        if (httpContext.Request.Query.TryGetValue("pageSize", out var pageSizeValues) && int.TryParse(pageSizeValues, out var parsedPageSize)) {
            pageSize = parsedPageSize;
        }

        // Call the use case (Interactor)
        var response = await interaction.DispatchAsync(
            new Core.Contracts.Users.GetServiceUsersRequest {
                PageNumber = page,
                PageSize = pageSize
            }
        ).To<Core.Contracts.Users.GetServiceUsersResponse>();

        return Results.Ok(response);
    }
}
