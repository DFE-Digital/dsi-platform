using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.Applications;
using Dfe.SignIn.Core.Models.Applications.Interactions;
using Dfe.SignIn.Core.Models.Organisations;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.Models.Users.Interactions;
using Dfe.SignIn.Core.PublicModels.SelectOrganisation;

/// <summary>
/// Use case for filtering organisations for a user.
/// </summary>
public sealed class FilterOrganisationsForUser_UseCase(
    IInteractor<GetOrganisationsAssociatedWithUserRequest, GetOrganisationsAssociatedWithUserResponse> getOrganisationsAssociatedWithUser,
    IInteractor<GetApplicationByClientIdRequest, GetApplicationByClientIdResponse> getApplicationByClientId,
    IInteractor<GetApplicationsAssociatedWithUserRequest, GetApplicationsAssociatedWithUserResponse> getApplicationsAssociatedWithUser
) : IInteractor<FilterOrganisationsForUserRequest, FilterOrganisationsForUserResponse>
{
    /// <inheritdoc/>
    public async Task<FilterOrganisationsForUserResponse> InvokeAsync(FilterOrganisationsForUserRequest request)
    {
        if (!Enum.IsDefined(request.Filter.Type)) {
            throw new InvalidOperationException($"Unexpected filter type '{request.Filter.Type}'.");
        }

        if (request.Filter.Type == OrganisationFilterType.AnyOf) {
            return new() {
                FilteredOrganisations = await this.RetrieveOrganisationsById(request.Filter.OrganisationIds),
            };
        }

        var predicate = this.GetPredicateForAssociationFiltering(request.Filter);

        return new() {
            FilteredOrganisations = await this.RetrieveOrganisationsAssociatedWithUser(request, predicate),
        };
    }

    private Task<IEnumerable<OrganisationModel>> RetrieveOrganisationsById(IEnumerable<Guid> organisationIds)
    {
        // This will require a mechanism to retrieve many organisations by ID.
        throw new NotImplementedException();
    }

    private Func<OrganisationModel, bool> GetPredicateForAssociationFiltering(OrganisationFilter filter)
    {
        if (filter.Type == OrganisationFilterType.Associated) {
            return (_) => true;
        }
        else if (filter.Type == OrganisationFilterType.AssociatedInclude) {
            var organisationIdSet = filter.OrganisationIds.ToHashSet();
            return (organisation) => organisationIdSet.Contains(organisation.Id);
        }
        else if (filter.Type == OrganisationFilterType.AssociatedExclude) {
            var organisationIdSet = filter.OrganisationIds.ToHashSet();
            return (organisation) => !organisationIdSet.Contains(organisation.Id);
        }
        else {
            throw new InvalidOperationException($"Unexpected association filter type '{filter.Type}'.");
        }
    }

    private async Task<IEnumerable<OrganisationModel>> RetrieveOrganisationsAssociatedWithUser(
        FilterOrganisationsForUserRequest request,
        Func<OrganisationModel, bool> organisationPredicate)
    {
        if (!Enum.IsDefined(request.Filter.Association)) {
            throw new InvalidOperationException($"Unexpected association type '{request.Filter.Association}'.");
        }

        var userOrganisations = (await this.GetOrganisationsAssociatedWithUserAsync(request.UserId))
            .Where(organisationPredicate)
            .ToArray();
        if (userOrganisations.Length == 0) {
            // There are no organisations associated with the user.
            return [];
        }

        if (request.Filter.Association == OrganisationFilterAssociation.AssignedToUser) {
            // Return all organisations that the user is associated with.

            // The list of organisations is not filtered for the 'AFORMS' application when
            // presenting the "select organisation" prompt. When 'AFORMS' uses the new
            // "select organisation" feature it will need to explicitly specify
            // `OrganisationFilterAssociation.AssignedToUser`.

            return userOrganisations;
        }

        var application = await this.GetClientApplicationAsync(request.ClientId);

        // Automatically apply filtering based upon whether the service is ID-only or role based.
        if (application.IsIdOnlyService) {
            if (request.Filter.Association == OrganisationFilterAssociation.Auto) {
                // Do not apply filtering to the list of organisations for an ID-only service.
                return userOrganisations;
            }
        }

        var userAssociationWithApplication = await this.GetUserAssociationWithApplicationAsync(request.UserId, application.Id);
        if (userAssociationWithApplication.Count() == 0) {
            // User is not associated with the application at all.
            if (request.Filter.Association == OrganisationFilterAssociation.Auto) {
                // let them choose any org?
                // Return all organisations that the user is associated with.
                return userOrganisations;
            }
            else {
                // There are no organisations associated with the user for the application.
                return [];
            }
        }

        var associatedOrganisationIds = userAssociationWithApplication
            .Select(association => association.OrganisationId)
            .ToHashSet();

        return userOrganisations
            .Where(organisation => associatedOrganisationIds.Contains(organisation.Id));
    }

    private async Task<IEnumerable<OrganisationModel>> GetOrganisationsAssociatedWithUserAsync(Guid userId)
    {
        var userOrganisationsResponse = await getOrganisationsAssociatedWithUser.InvokeAsync(new() {
            UserId = userId,
        });
        return userOrganisationsResponse.Organisations;
    }

    private async Task<ApplicationModel> GetClientApplicationAsync(string clientId)
    {
        var applicationResponse = await getApplicationByClientId.InvokeAsync(new() {
            ClientId = clientId,
        });
        if (applicationResponse.Application is null) {
            throw new InvalidOperationException("Application was not found.");
        }
        return applicationResponse.Application;
    }

    private async Task<IEnumerable<UserApplicationMappingModel>> GetUserAssociationWithApplicationAsync(Guid userId, Guid applicationId)
    {
        var associatedApplicationsResponse = await getApplicationsAssociatedWithUser.InvokeAsync(new() {
            UserId = userId,
        });
        return associatedApplicationsResponse.UserApplicationMappings
            .Where(mapping => mapping.ApplicationId == applicationId);
    }
}
