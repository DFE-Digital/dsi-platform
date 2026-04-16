using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;

namespace Dfe.SignIn.Core.UseCases.Organisations;

/// <summary>
/// Use case for getting information about an organisation.
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
        /*
        var response = await interaction.DispatchAsync(
            new GetUsersAtOrganisationRequest(context.Request.Ukprn)
        ).To<GetUsersAtOrganisationResponse>();
        */

        var response = new GetUsersAtOrganisationResponse(context.Request.Ukprn, []);

        return response;

    }
}
