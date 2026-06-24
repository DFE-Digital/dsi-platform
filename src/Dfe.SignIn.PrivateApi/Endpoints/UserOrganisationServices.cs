using System.Diagnostics;
using Dfe.SignIn.PrivateApi.MappingExtensions;
using Dfe.SignIn.PrivateApi.Repository;
using Dfe.SignIn.PrivateApi.Responses;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace Dfe.SignIn.PrivateApi.Endpoints;

/// <summary>
/// Endpoint users/{userId}/organisationservices.
/// </summary>
public static partial class UserOrganisationServices
{

    /// <summary>
    /// Gives all organisations and services associated with a user, where the organisation grants the user access to the service passed in on the clientId.
    /// </summary>
    /// <param name="userId">Unique identifier of the user.</param>
    /// <param name="clientId">The client service.</param>
    /// <param name="organisationRepository">Use to communicate with database.</param>
    /// <param name="loggerFactory">Factory to create loggers for logging request details.</param>
    /// <param name="clientCorrelationId">Correclation Id.</param>
    /// <param name="cancellationToken">The cacellation context</param>
    /// <returns>
    ///   <para>200 with an array of organisations when the user belongs to, including services and roles.</para>
    ///   <para>404 when the user belongs to no organisations.</para>
    /// </returns>

    public static async Task<Results<Ok<GetUserOrganisationServicesResponse>, NotFound>>
        GetUserOrganisationServices(
            Guid userId,
            string clientId,
            IOrganisationRepository organisationRepository,
            ILoggerFactory loggerFactory,
            [FromHeader(Name = "x-correlation-id")] string? clientCorrelationId,
            CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger("UserOrganisationServices");

        var correlationId = Activity.Current?.TraceId.ToString();

        logger.LogInformation(
            "{ClientId} requesting organisation services (correlationId: {CorrelationId}, clientCorrelationId: {ClientCorrelationId})",
            clientId,
            correlationId,
            clientCorrelationId
        );

        var models = await organisationRepository
            .SelectOrganisationServicesAndRolesByUserId(clientId, userId, cancellationToken);

        var dto = models?.ToUserDtos().SingleOrDefault();

        return dto is null
            ? TypedResults.NotFound()
            : TypedResults.Ok(dto);
    }
}
