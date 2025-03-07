using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;

namespace Dfe.SignIn.Core.UseCases.SelectOrganisation;

public sealed class GetSelectOrganisationSessionByKey_UseCase(
    ISelectOrganisationSessionRepository sessionRepository
) : IInteractor<GetSelectOrganisationSessionByKeyRequest, GetSelectOrganisationSessionByKeyResponse>
{
    /// <inheritdoc/>
    public async Task<GetSelectOrganisationSessionByKeyResponse> InvokeAsync(
        GetSelectOrganisationSessionByKeyRequest request)
    {
        return new GetSelectOrganisationSessionByKeyResponse {
            SessionData = await sessionRepository.RetrieveAsync(request.SessionKey),
        };
    }
}
