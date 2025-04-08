using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;

namespace Dfe.SignIn.Core.UseCases.SelectOrganisation;

/// <summary>
/// Use case for invalidating a "select organisation" session.
/// </summary>
/// <param name="sessionRepository">The repository of "select organisation" sessions.</param>
public sealed class InvalidateSelectOrganisationSession_UseCase(
    ISelectOrganisationSessionRepository sessionRepository
) : IInteractor<InvalidateSelectOrganisationSessionRequest, InvalidateSelectOrganisationSessionResponse>
{
    /// <inheritdoc/>
    public async Task<InvalidateSelectOrganisationSessionResponse> InvokeAsync(
        InvalidateSelectOrganisationSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        await sessionRepository.InvalidateAsync(request.SessionKey);

        return new InvalidateSelectOrganisationSessionResponse();
    }
}
