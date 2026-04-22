using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.PublicApi.Authorization;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dfe.SignIn.PublicApi.Endpoints.Organisations;

public static partial class OrganisationEndpoints
{
    /// <summary>
    /// Get users and their roles on the service in the bearer token at the organisation.
    /// </summary>
    /// <param name="ukprn"></param>
    /// <param name="clientSession"></param>
    /// <param name="interaction"></param>
    /// <returns></returns>
    public static async Task<Results<Ok<GetUsersAtOrganisationResponse>, NotFound>> GetUsersAtOrganisation(
        int ukprn,
        IClientSession clientSession,
        IInteractionDispatcher interaction)
    {
        try {

            var applicationId = await FetchApplicationIdFromNode(interaction, clientSession.ClientId);
            if (applicationId == null) {
                return TypedResults.NotFound();
            }

            var organisationIds = await FetchOrganisationIdsFromNode(interaction, ukprn.ToString());
            if (organisationIds == null) {
                return TypedResults.NotFound();
            }

            var userRoles = await GetUserIdsAndRoles(interaction, applicationId.Value, organisationIds);

            var users = await GetUserDetails(interaction, userRoles);

            var responseModel = new GetUsersAtOrganisationResponse(ukprn.ToString(), users);

            return TypedResults.Ok(responseModel);
        }
        catch (NotFoundInteractionException) {
            return TypedResults.NotFound();
        }
        catch (Exception ex) {
            Console.WriteLine(ex.GetBaseException().Message);
            return TypedResults.NotFound();
        }
    }

    /// <summary>
    /// Use the client name, extracted from the bearer token, to lookup the client id
    /// </summary>
    /// <param name="interaction"></param>
    /// <param name="clientName"></param>
    /// <returns></returns>
    private static async Task<Guid?> FetchApplicationIdFromNode(IInteractionDispatcher interaction, string clientName)
    {
        GetApplicationByClientIdRequest request = new() { ClientId = clientName };
        var response = await interaction.DispatchAsync(request).To<GetApplicationByClientIdResponse>();

        var application = response.Application;
        if (application != null) {
            return application?.Id;
        }

        return null;
    }

    /// <summary>
    /// Get matching organisation ids from the UKPRN.
    /// </summary>
    /// <param name="interaction"></param>
    /// <param name="ukprn"></param>
    /// <returns></returns>
    private static async Task<IEnumerable<Guid>?> FetchOrganisationIdsFromNode(IInteractionDispatcher interaction, string ukprn)
    {
        GetOrganisationIdsByExternalIdRequest request = new() { LookupKey = "UKPRN-multi", LookupValue = ukprn.ToString() };
        var response = await interaction.DispatchAsync(request).To<GetOrganisationIdsByExternalIdResponse>();

        IEnumerable<Guid>? organisationIds = response?.OrganisationIds;

        return organisationIds;
    }

    /// <summary>
    /// Get user ids along with the roles of the user.
    /// </summary>
    /// <param name="interaction"></param>
    /// <param name="applicationId"></param>
    /// <param name="organisationIds"></param>
    /// <returns></returns>
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
    /// <param name="interaction"></param>
    /// <param name="userRoles"></param>
    /// <returns></returns>
    private static async Task<List<UserAtOrganisation>> GetUserDetails(IInteractionDispatcher interaction, Dictionary<Guid, HashSet<string>> userRoles)
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
            catch (Exception ex) {
                Console.WriteLine("************** Broken *************");
                Console.WriteLine(ex.GetBaseException().Message);
            }
        }

        return users;
    }

    /// <summary>
    /// Get user ids of service at organisation.
    /// </summary>
    /// <param name="interaction"></param>
    /// <param name="organisationId"></param>
    /// <param name="applicationId"></param>
    /// <returns></returns>
    private static async Task<IEnumerable<Guid>?> FetchUserIdsFromNode(IInteractionDispatcher interaction, Guid organisationId, Guid applicationId)
    {
        GetServiceUsersAtOrganisationRequest serviceUsersRequest = new() { OrganisationId = organisationId, ApplicationId = applicationId };
        var response = await interaction.DispatchAsync(serviceUsersRequest).To<GetServiceUsersAtOrganisationResponse>();

        IEnumerable<Guid>? userIds = response?.UserIds;

        return userIds;
    }

    /// <summary>
    /// Get user service roles for user at organisation.
    /// </summary>
    /// <param name="interaction"></param>
    /// <param name="organisationId"></param>
    /// <param name="applicationId"></param>
    /// <param name="userId"></param>
    /// <returns></returns>
    private static async Task<IEnumerable<string>?> FetchUserRolesFromNode(IInteractionDispatcher interaction,
        Guid organisationId,
        Guid applicationId,
        Guid userId)
    {
        GetRolesOfUserRequest request = new() { ApplicationId = applicationId, OrganisationId = organisationId, UserId = userId };

        var response = await interaction.DispatchAsync(request).To<GetRolesOfUserResponse>();

        return response?.Roles;
    }

    private static async Task<GetUserProfileResponse?> FetchUserProfileFromNode(IInteractionDispatcher interaction, Guid userId)
    {
        GetUserProfileRequest request = new() { UserId = userId };

        var response = await interaction.DispatchAsync(request).To<GetUserProfileResponse>();

        return response;
    }
}
