using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Core.Interfaces.SelectOrganisationSessions;

namespace Dfe.SignIn.Core.UseCases.SelectOrganisation;

/// <summary>
/// Use case for getting a "select organisation" session by its unique key.
/// </summary>
/// <param name="sessionRepository">The repository of "select organisation" sessions.</param>
public sealed class GetSelectOrganisationSessionByKeyUseCase(
    ISelectOrganisationSessionRepository sessionRepository
) : Interactor<GetSelectOrganisationSessionByKeyRequest, GetSelectOrganisationSessionByKeyResponse>
{
    /// <inheritdoc/>
    public override async Task<GetSelectOrganisationSessionByKeyResponse> InvokeAsync(
        InteractionContext<GetSelectOrganisationSessionByKeyRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        return new GetSelectOrganisationSessionByKeyResponse {
            SessionData = await sessionRepository.RetrieveAsync(context.Request.SessionKey),
        };
    }
}
