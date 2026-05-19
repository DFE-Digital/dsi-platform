using System.Diagnostics;
using Dfe.SignIn.Core.Repository;
using Dfe.SignIn.PublicApi.Authorization;
using Dfe.SignIn.PublicApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

public static partial class UserEndpoints
{

    /// <summary>
    /// Gets the list of organisations a user belongs to.
    /// Hidden organisations (status = 0) are excluded.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="organisationRepository"></param>
    /// <param name="clientSession">The client session for the current request.</param>
    /// <param name="loggerFactory">Factory to create loggers for logging request details.</param>
    /// <param name="httpContext">The current HTTP context, used to access headers and request information.</param>
    /// <param name="cancellationToken">The cacellation context</param>
    /// <returns>
    ///   <para>200 with an array of organisations when the user belongs to, including services and roles.</para>
    ///   <para>404 when the user belongs to no organisations.</para>
    /// </returns>
    public static async Task<Results<Ok<GetUserOrganisationServicesResponse>, NotFound, InternalServerError<ProblemDetails>>> GetUserOrganisationServices(
        Guid userId,
        IOrganisationRepository organisationRepository,
        IClientSession clientSession,
        ILoggerFactory loggerFactory,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(UserEndpoints));
        var correlationId = Activity.Current?.TraceId.ToString();
        var clientCorrelationId = httpContext.Request.Headers["x-correlation-id"].FirstOrDefault();

        if (logger.IsEnabled(LogLevel.Information)) {
            logger.LogInformation(
                "{ClientId} is attempting to get organisation services (correlationId: {CorrelationId}, clientCorrelationId: {ClientCorrelationId})",
                clientSession.ClientId,
                correlationId,
                clientCorrelationId
            );
        }

        try {

            var models = await organisationRepository.SelectOrganisationServicesAndRolesByUserId(clientSession.ClientId, userId, cancellationToken);

            if (!models.Any()) {
                return TypedResults.NotFound();
            }

            // userId is primary key
            return TypedResults.Ok(models.ToUserDtos().SingleOrDefault());
        }
        catch (Exception ex) {
            logger.LogError(ex, "Error getting organisations for user {UserId} (correlationId: {CorrelationId}, clientCorrelationId: {ClientCorrelationId}",
                userId,
                correlationId,
                clientCorrelationId
            );

            return TypedResults.InternalServerError(
                new ProblemDetails {
                    Title = "Internal Server Error",
                    Detail = "Something unexpected happened while processing your request.",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }
}
