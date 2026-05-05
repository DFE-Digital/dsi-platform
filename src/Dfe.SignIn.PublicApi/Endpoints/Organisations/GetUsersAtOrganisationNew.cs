using System.Diagnostics;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.PublicApi.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PublicApi.Endpoints.Organisations;

public static partial class OrganisationEndpoints
{
    /// <summary>
    /// Get users and their roles on the current service at an organisation or organisations.
    /// </summary>
    /// <param name="externalId">The UKPRN or UPIN of the organisation.</param>
    /// <param name="clientSession">The client session for the current request.</param>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="loggerFactory">Factory to create loggers for logging request details.</param>
    /// <param name="httpContext">The current HTTP context, used to access headers and request information.</param>
    /// <returns>User names, email address, status and roles of the service at the organisation(s).</returns>
    public static async Task<Results<Ok<GetUsersAtOrganisationResponseNew>, NotFound, InternalServerError<ProblemDetails>>> GetUsersAtOrganisationV2(
        string externalId,
        IClientSession clientSession,
        IInteractionDispatcher interaction,
        ILoggerFactory loggerFactory,
        HttpContext httpContext)
    {
        var logger = loggerFactory.CreateLogger(nameof(OrganisationEndpoints));

        var correlationId = Activity.Current?.TraceId.ToString();
        var clientCorrelationId = httpContext.Request.Headers["x-correlation-id"].FirstOrDefault();

        logger.LogInformation(
            "{ClientId} is attempting to get users at organisation for ukprn/upin: {RequestedExternalId} (correlationId: {CorrelationId}, clientCorrelationId: {ClientCorrelationId})",
            clientSession.ClientId,
            externalId,
            correlationId,
            clientCorrelationId
        );

        try {

            GetUsersAtOrganisationRequestNew request = new(clientSession.ClientId, externalId);

            GetUsersAtOrganisationResponseNew model = await interaction.DispatchAsync(request).To<GetUsersAtOrganisationResponseNew>();

            // populate the model
            var responseModel = new GetUsersAtOrganisationResponseNew {
                IsUkprn = true,
                ExternalId = externalId,
                Users = model.Users
            };

            return TypedResults.Ok(responseModel);
        }
        catch (NotFoundInteractionException ex) {
            logger.LogWarning(ex, "Organisation missing");
            return TypedResults.NotFound();
        }
        catch (Exception ex) {

            logger.LogError(ex, "Unexpected error while retrieving organisation users for ukprn/upin {RequestedExternalId}", externalId);

            return TypedResults.InternalServerError(
                new ProblemDetails {
                    Title = "Internal Server Error",
                    Detail = "Something unexpected happened while processing your request.",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }
}
