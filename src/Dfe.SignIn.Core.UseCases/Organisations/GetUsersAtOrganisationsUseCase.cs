using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;

namespace Dfe.SignIn.Core.UseCases.Organisations;

/// <summary>
/// Use case for getting users an organisation, along with the users roles.
/// sjw
/// </summary>
public sealed class GetUsersAtOrganisationUseCase(
    IInteractionDispatcher interaction
) : Interactor<GetUsersAtOrganisationRequest, GetUsersAtOrganisationResponse>
{
    /// <inheritdoc/>
    public override async Task<GetUsersAtOrganisationResponse> InvokeAsync(
        InteractionContext<GetUsersAtOrganisationRequest> context,
        CancellationToken cancellationToken = default)
    {

        var result = await interaction.DispatchAsync(
            new GetUsersAssociatedWithOrganisationRequest {
                Ukprn = context.Request.Ukprn.ToString(),
            }
        ).To<GetUsersAssociatedWithOrganisationResponse>();

        /*
        var response = await interaction.DispatchAsync(
          organisationClient  new GetUsersAtOrganisationRequest(context.Request.Ukprn)
        ).To<GetUsersAtOrganisationResponse>();
        */

        var response = new GetUsersAtOrganisationResponse(result.Organisations.Count(), []);

        return response;

    }
}
