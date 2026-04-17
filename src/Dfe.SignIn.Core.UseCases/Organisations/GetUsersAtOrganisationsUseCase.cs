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
                OrganisationId = new Guid("E6714479-04FA-4037-8E3E-EA7B37DFD50B"),
            }
        ).To<GetUsersAssociatedWithOrganisationResponse>();

        /*
        var response = await interaction.DispatchAsync(
            new GetUsersAtOrganisationRequest(context.Request.Ukprn)
        ).To<GetUsersAtOrganisationResponse>();
        */

        var response = new GetUsersAtOrganisationResponse(context.Request.Ukprn, []);

        return response;

    }
}
