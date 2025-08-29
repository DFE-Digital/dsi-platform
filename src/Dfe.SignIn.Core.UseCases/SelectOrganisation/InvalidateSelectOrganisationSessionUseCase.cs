using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;

namespace Dfe.SignIn.Core.UseCases.SelectOrganisation;

/// <summary>
/// Use case for invalidating a "select organisation" session.
/// </summary>
/// <param name="sessionRepository">The repository of "select organisation" sessions.</param>
public sealed class InvalidateSelectOrganisationSessionUseCase(
    ISelectOrganisationSessionRepository sessionRepository
) : Interactor<InvalidateSelectOrganisationSessionRequest, InvalidateSelectOrganisationSessionResponse>
{
    /// <inheritdoc/>
    public override async Task<InvalidateSelectOrganisationSessionResponse> InvokeAsync(
        InteractionContext<InvalidateSelectOrganisationSessionRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        await sessionRepository.InvalidateAsync(context.Request.SessionKey);

        return new InvalidateSelectOrganisationSessionResponse();
    }
}
