using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.SelectOrganisation;
using Dfe.SignIn.Core.Models.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.SelectOrganisation;

/// <summary>
/// Use case for creating a "select organisation" session.
/// </summary>
/// <param name="optionsAccessor">Provides access to "select organisation" options.</param>
/// <param name="sessionRepository">The repository of "select organisation" sessions.</param>
public sealed class CreateSelectOrganisationSession_UseCase(
    IOptions<SelectOrganisationOptions> optionsAccessor,
    ISelectOrganisationSessionRepository sessionRepository
) : IInteractor<CreateSelectOrganisationSessionRequest, CreateSelectOrganisationSessionResponse>
{
    private static string GenerateSessionKey() => Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public async Task<CreateSelectOrganisationSessionResponse> InvokeAsync(
        CreateSelectOrganisationSessionRequest request)
    {
        var options = optionsAccessor.Value;

        var createdUtc = DateTime.UtcNow;

        var sessionData = new SelectOrganisationSessionData {
            CallbackUrl = request.CallbackUrl,
            ClientId = request.ClientId,
            UserId = request.UserId,
            Prompt = request.Prompt,
            OrganisationOptions = [],
            Created = createdUtc,
            Expires = createdUtc + new TimeSpan(0, options.SessionTimeoutInMinutes, 0),
        };

        string sessionKey = GenerateSessionKey();
        await sessionRepository.StoreAsync(sessionKey, sessionData);

        return new CreateSelectOrganisationSessionResponse {
            Url = new Uri(options.SelectOrganisationBaseAddress, $"{request.ClientId}/{sessionKey}"),
        };
    }
}
