using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;

namespace Dfe.SignIn.Core.UseCases.SelectOrganisation;

/// <summary>
/// Use case for getting a "select organisation" session by its unique key.
/// </summary>
/// <param name="sessionRepository">The repository of "select organisation" sessions.</param>
public sealed class GetSelectOrganisationSessionByKey_UseCase(
    ISelectOrganisationSessionRepository sessionRepository
) : IInteractor<GetSelectOrganisationSessionByKeyRequest, GetSelectOrganisationSessionByKeyResponse>
{
    /// <inheritdoc/>
    public async Task<GetSelectOrganisationSessionByKeyResponse> InvokeAsync(
        GetSelectOrganisationSessionByKeyRequest request,
        CancellationToken cancellationToken = default)
    {
        return new GetSelectOrganisationSessionByKeyResponse {
            SessionData = await sessionRepository.RetrieveAsync(request.SessionKey),
        };
    }
}
