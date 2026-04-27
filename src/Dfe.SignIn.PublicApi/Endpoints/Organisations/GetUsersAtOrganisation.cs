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
    /// Get users and their roles on the service in the bearer token at the organisation.
    /// </summary>
    /// <param name="ukprn">The UKPRN of the organisation.</param>
    /// <param name="clientSession">The client session for the current request.</param>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <returns>User names, email address, status and roles of the service at the organisation(s).</returns>
    public static async Task<Results<Ok<GetUsersAtOrganisationResponse>, NotFound, InternalServerError<ProblemDetails>>> GetUsersAtOrganisation(
        string ukprn,
        IClientSession clientSession,
        IInteractionDispatcher interaction)
    {
        try {

            // get the application id from the session
            var applicationId = await FetchApplicationIdFromNode(interaction, clientSession.ClientId);
            if (applicationId == null) {
                return TypedResults.NotFound();
            }

            // look for organisations matching UKPRN
            var organisationIds = await FetchOrganisationIdsByUkprnFromNode(interaction, ukprn);

            if (organisationIds == null || !organisationIds.Any()) {
                return TypedResults.NotFound();
            }

            // find user ids and the user roles for the organisation and service
            var userRoles = await GetUserIdsAndRoles(interaction, applicationId.Value, organisationIds);

            // get user names and their email address and statuses
            var users = await GetNameandEmailandStatusOfUsers(interaction, userRoles);

            // populate the model
            var responseModel = new GetUsersAtOrganisationResponse(ukprn, users);

            return TypedResults.Ok(responseModel);
        }
        catch (NotFoundInteractionException) {
            return TypedResults.NotFound();
        }
        catch (Exception) {

            return TypedResults.InternalServerError(
                new ProblemDetails {
                    Title = "Internal Server Error",
                    Detail = "Something unexpected happened while processing your request.",
                    Status = StatusCodes.Status500InternalServerError
                });
        }
    }

    /// <summary>
    /// Use the client name, extracted from the bearer token, to lookup the client id
    /// </summary>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="clientName">The application/service/client name.</param>
    /// <returns>The application id of the service.</returns>
    private static async Task<Guid?> FetchApplicationIdFromNode(IInteractionDispatcher interaction, string clientName)
    {
        GetApplicationByClientIdRequest request = new() { ClientId = clientName };
        var response = await interaction.DispatchAsync(request).To<GetApplicationByClientIdResponse>();

        var application = response.Application;

        return application?.Id;
    }

    /// <summary>
    /// Get matching organisation ids from the UKPRN.
    /// Usually, a UKPRN will usually be linked to a single organisation, but not always.
    /// </summary>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="ukprn">The UKPRN of the organisation.</param>
    /// <returns>Organisation ids.</returns>
    private static async Task<IEnumerable<Guid>?> FetchOrganisationIdsByUkprnFromNode(IInteractionDispatcher interaction, string ukprn)
    {
        // look for a matching UKPRN
        GetOrganisationIdsByExternalIdRequest request = new() { LookupKey = "UKPRN-multi", LookupValue = ukprn };
        var response = await interaction.DispatchAsync(request).To<GetOrganisationIdsByExternalIdResponse>();
        IEnumerable<Guid>? organisationIds = response?.OrganisationIds;

        return organisationIds;
    }

    /// <summary>
    /// Get user ids along with the roles of the user.
    /// </summary>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="applicationId">Unique identifier of the service.</param>
    /// <param name="organisationIds">Unique identifiers of the organisations.</param>
    /// <returns>Service roles codes for users.</returns>
    private static async Task<Dictionary<Guid, HashSet<string>>> GetUserIdsAndRoles(IInteractionDispatcher interaction, Guid applicationId, IEnumerable<Guid> organisationIds)
    {
        var userRoles = new Dictionary<Guid, HashSet<string>>();
        foreach (Guid orgId in organisationIds) {

            var userIds = await FetchUserIdsFromNode(interaction, orgId, applicationId);

            if (userIds == null) {
                continue;
            }

            foreach (var userId in userIds) {

                if (!userRoles.TryGetValue(userId, out var value)) {
                    value = [];
                    userRoles[userId] = value;
                }

                var rolesList = await FetchUserRolesFromNode(interaction, orgId, applicationId, userId);

                if (rolesList != null) {
                    foreach (string role in rolesList) {
                        value.Add(role);
                    }
                }
            }
        }

        return userRoles;
    }

    /// <summary>
    /// Get user details, e.g. their names.
    /// </summary>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="userRoles">Service roles codes for users.</param>
    /// <returns>User names, email addresses, and statuses</returns>
    private static async Task<List<UserAtOrganisation>> GetNameandEmailandStatusOfUsers(IInteractionDispatcher interaction, Dictionary<Guid, HashSet<string>> userRoles)
    {
        List<UserAtOrganisation> users = [];
        foreach (var (userId, roles) in userRoles) {

            try {
                var userProfile = await FetchUserProfileFromNode(interaction, userId);

                if (userProfile != null) {
                    UserAtOrganisation user = new(
                        userProfile.EmailAddress,
                        userProfile.FirstName,
                        userProfile.LastName,
                        userProfile.Status,
                        roles != null ? roles.ToList().AsReadOnly() : []
                    );
                    users.Add(user);
                }
            }
            catch (Exception) {
                // overlook the exception that occurs when there are no matches
            }
        }

        return users;
    }

    /// <summary>
    /// Get user ids of service at organisation.
    /// </summary>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="organisationId">Unique identifier of the organisation.</param>
    /// <param name="applicationId">Unique identifier of the service.</param>
    /// <returns>List of unique user ids.</returns>
    private static async Task<IEnumerable<Guid>?> FetchUserIdsFromNode(IInteractionDispatcher interaction, Guid organisationId, Guid applicationId)
    {
        GetServiceUsersAtOrganisationRequest serviceUsersRequest = new() { OrganisationId = organisationId, ApplicationId = applicationId };
        var response = await interaction.DispatchAsync(serviceUsersRequest).To<GetServiceUsersAtOrganisationResponse>();

        return response?.UserIds;
    }

    /// <summary>
    /// Get service roles of a user at organisation.
    /// </summary>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="organisationId">Unique identifier of the organisation.</param>
    /// <param name="applicationId">Unique identifier of the service.</param>
    /// <param name="userId">Unique identifier of the user.</param>
    /// <returns>Service roles of the user.</returns>
    private static async Task<IEnumerable<string>?> FetchUserRolesFromNode(IInteractionDispatcher interaction,
        Guid organisationId,
        Guid applicationId,
        Guid userId)
    {
        GetRolesOfUserRequest request = new() { ApplicationId = applicationId, OrganisationId = organisationId, UserId = userId };

        var response = await interaction.DispatchAsync(request).To<GetRolesOfUserResponse>();

        return response?.Roles;
    }

    /// <summary>
    /// Get user details such as first name and family name.
    /// </summary>
    /// <param name="interaction">Service to dispatch interaction requests.</param>
    /// <param name="userId">Unique identifier of the user.</param>
    /// <returns>Users first name, family name, email, and status.</returns>
    private static async Task<GetUserProfileResponse?> FetchUserProfileFromNode(IInteractionDispatcher interaction, Guid userId)
    {
        GetUserProfileRequest request = new() { UserId = userId };

        var response = await interaction.DispatchAsync(request).To<GetUserProfileResponse>();

        return response;
    }
}
