using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Public.SelectOrganisation;

namespace Dfe.SignIn.Core.UseCases.SelectOrganisation;

/// <summary>
/// Use case for filtering organisations for a user.
/// </summary>
/// <param name="interaction">Service to dispatch interaction requests.</param>
public sealed class FilterOrganisationsForUserUseCase(
    IInteractionDispatcher interaction
) : Interactor<FilterOrganisationsForUserRequest, FilterOrganisationsForUserResponse>
{
    /// <inheritdoc/>
    public override async Task<FilterOrganisationsForUserResponse> InvokeAsync(
        InteractionContext<FilterOrganisationsForUserRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        if (context.Request.Filter.Type == OrganisationFilterType.AnyOf) {
            return new() {
                FilteredOrganisations = await RetrieveOrganisationsById(
                    context.Request.Filter.OrganisationIds, cancellationToken
                ),
            };
        }

        var predicate = GetPredicateForAssociationFiltering(context.Request.Filter);

        return new() {
            FilteredOrganisations = await this.RetrieveOrganisationsAssociatedWithUser(
                context.Request, predicate, cancellationToken),
        };
    }

    private static Task<IEnumerable<Organisation>> RetrieveOrganisationsById(
        IEnumerable<Guid> organisationIds,
        CancellationToken cancellationToken = default)
    {
        // This will require a mechanism to retrieve many organisations by ID.
        throw new NotImplementedException();
    }

    private static Func<Organisation, bool> GetPredicateForAssociationFiltering(OrganisationFilter filter)
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

    private async Task<IEnumerable<Organisation>> RetrieveOrganisationsAssociatedWithUser(
        FilterOrganisationsForUserRequest request,
        Func<Organisation, bool> organisationPredicate,
        CancellationToken cancellationToken = default)
    {
        var userOrganisations = (await this.GetOrganisationsAssociatedWithUserAsync(
            request.UserId, cancellationToken
        )).Where(organisationPredicate).ToArray();

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

        var application = await this.GetClientApplicationAsync(request.ClientId, cancellationToken);

        // Automatically apply filtering based upon whether the service is ID-only or role based.
        if (application.IsIdOnlyService) {
            if (request.Filter.Association == OrganisationFilterAssociation.Auto) {
                // Do not apply filtering to the list of organisations for an ID-only service.
                return userOrganisations;
            }
        }

        var userAssociationWithApplication = await this.GetUserAssociationWithApplicationAsync(
            request.UserId, application.Id, cancellationToken);
        if (!userAssociationWithApplication.Any()) {
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

    private async Task<IEnumerable<Organisation>> GetOrganisationsAssociatedWithUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var response = await interaction.DispatchAsync(
            new GetOrganisationsAssociatedWithUserRequest {
                UserId = userId,
            }, cancellationToken
        ).To<GetOrganisationsAssociatedWithUserResponse>();

        return response.Organisations;
    }

    private async Task<Application> GetClientApplicationAsync(
        string clientId,
        CancellationToken cancellationToken = default)
    {
        var response = await interaction.DispatchAsync(
            new GetApplicationByClientIdRequest {
                ClientId = clientId,
            }, cancellationToken
        ).To<GetApplicationByClientIdResponse>();

        return response.Application;
    }

    private async Task<IEnumerable<UserApplicationMapping>> GetUserAssociationWithApplicationAsync(
        Guid userId,
        Guid applicationId,
        CancellationToken cancellationToken = default)
    {
        var response = await interaction.DispatchAsync(
            new GetApplicationsAssociatedWithUserRequest {
                UserId = userId,
            }, cancellationToken
        ).To<GetApplicationsAssociatedWithUserResponse>();

        return response.UserApplicationMappings
            .Where(mapping => mapping.ApplicationId == applicationId);
    }
}
