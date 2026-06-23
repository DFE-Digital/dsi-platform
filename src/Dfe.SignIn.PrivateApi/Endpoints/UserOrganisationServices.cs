using System.Diagnostics;
using Dfe.SignIn.PrivateApi.DataModels;
using Dfe.SignIn.PrivateApi.MappingExtensions;
using Dfe.SignIn.PrivateApi.Repository;
using Dfe.SignIn.PrivateApi.Responses;

namespace Dfe.SignIn.PrivateApi.Endpoints;

/// <summary>
/// Endpoint users/{userId}/organisationservices.
/// </summary>
public static partial class UserOrganisationServices
{

    /// <summary>
    /// Gets the list of organisations a user belongs to.
    /// Hidden organisations (status = 0) are excluded.
    /// </summary>
    /// <param name="userId">Unique identifier of the user.</param>
    /// <param name="clientId">The client service.</param>
    /// <param name="organisationRepository">Use to communicate with database.</param>
    /// <param name="loggerFactory">Factory to create loggers for logging request details.</param>
    /// <param name="httpContext">The current HTTP context, used to access headers and request information.</param>
    /// <param name="cancellationToken">The cacellation context</param>
    /// <returns>
    ///   <para>200 with an array of organisations when the user belongs to, including services and roles.</para>
    ///   <para>404 when the user belongs to no organisations.</para>
    /// </returns>
    public static async Task<GetUserOrganisationServicesResponse?> GetUserOrganisationServices(
        Guid userId,
        string clientId,
        IOrganisationRepository organisationRepository,
        ILoggerFactory loggerFactory,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var logger = loggerFactory.CreateLogger(nameof(GetUserOrganisationServices));
        var correlationId = Activity.Current?.TraceId.ToString();
        var clientCorrelationId = httpContext.Request.Headers["x-correlation-id"].FirstOrDefault();

        if (logger.IsEnabled(LogLevel.Information)) {
            logger.LogInformation(
                "{ClientId} is attempting to get organisation services (correlationId: {CorrelationId}, clientCorrelationId: {ClientCorrelationId})",
                clientId,
                correlationId,
                clientCorrelationId
            );
        }

        IEnumerable<UserOrganisationServicesQuery> models = await organisationRepository.SelectOrganisationServicesAndRolesByUserId(clientId, userId, cancellationToken);

        // userId is primary key
        return models?.ToUserDtos().SingleOrDefault();
    }
}
