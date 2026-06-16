using System.Diagnostics;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.PublicApi.Endpoints.Users;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dfe.SignIn.PublicApi.Endpoints.Services;

public static partial class ServiceEndpoints
{
    /// <summary>
    /// Gets a user's access to a service within an organisation, including their roles,
    /// external identifiers, and legacy IDs used by relying-party systems.
    /// </summary>
    /// <returns>
    ///   <para>200 with the access details when the user has access.</para>
    ///   <para>404 when the user does not have access, or the organisation does not exist.</para>
    /// </returns>
    public static async Task<Results<Ok<GetUserServiceAccessDetailsResponse>, NotFound>> GetUserServiceAccessDetails(
        string serviceId,
        string organisationId,
        string userId,
        // ---
        HttpContext httpContext,
        ILoggerFactory loggerFactory,
        IInteractionDispatcher interaction)
    {
        var logger = loggerFactory.CreateLogger(nameof(UserEndpoints));
        var correlationId = Activity.Current?.TraceId.ToString();
        var clientCorrelationId = httpContext.Request.Headers["x-correlation-id"].FirstOrDefault();

        try {
            var response = await interaction.DispatchAsync(
                new GetUserServiceAccessDetailsRequest {
                    ServiceId = serviceId.ToGuid(),
                    OrganisationId = organisationId.ToGuid(),
                    UserId = userId.ToGuid(),
                }
            ).To<GetUserServiceAccessDetailsResponse>();

            return TypedResults.Ok(response);
        }
        catch (FormatException e) {
            logger.LogWarning(
                "Error getting user {userId}'s access to service {serviceId} within organisation {organisationId} (correlationId: {CorrelationId}, client correlationId: {ClientCorrelationId}) - {ErrorMessage}",
                userId,
                serviceId,
                organisationId,
                correlationId,
                clientCorrelationId,
                e.Message
            );

            throw;
        }
        catch (NotFoundInteractionException) {
            return TypedResults.NotFound();
        }
    }
}
