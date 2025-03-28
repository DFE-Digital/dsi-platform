using Dfe.SignIn.Core.ExternalModels.SelectOrganisation;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation;
using Dfe.SignIn.Core.InternalModels.SelectOrganisation.Interactions;
using Dfe.SignIn.Core.UseCases.Gateways.SelectOrganisationSessions;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.SelectOrganisation;

/// <summary>
/// Use case for creating a "select organisation" session.
/// </summary>
/// <param name="optionsAccessor">Provides access to "select organisation" options.</param>
/// <param name="sessionRepository">The repository of "select organisation" sessions.</param>
/// <param name="filterOrganisationsForUser">Interaction to filter organisations for a user.</param>
public sealed class CreateSelectOrganisationSession_UseCase(
    IOptions<SelectOrganisationOptions> optionsAccessor,
    ISelectOrganisationSessionRepository sessionRepository,
    IInteractor<FilterOrganisationsForUserRequest, FilterOrganisationsForUserResponse> filterOrganisationsForUser
) : IInteractor<CreateSelectOrganisationSessionRequest, CreateSelectOrganisationSessionResponse>
{
    private static string GenerateSessionKey() => Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public async Task<CreateSelectOrganisationSessionResponse> InvokeAsync(
        CreateSelectOrganisationSessionRequest request,
        CancellationToken cancellationToken = default)
    {
        var options = optionsAccessor.Value;

        var createdUtc = DateTime.UtcNow;

        var filteredOrganisationsResponse = await filterOrganisationsForUser.InvokeAsync(new() {
            ClientId = request.ClientId,
            UserId = request.UserId,
            Filter = request.Filter,
        }, cancellationToken);

        var filteredOptions = filteredOrganisationsResponse.FilteredOrganisations
            .Select(organisation => new SelectOrganisationOption {
                Id = organisation.Id,
                Name = organisation.Name,
            })
            .ToArray();

        var sessionData = new SelectOrganisationSessionData {
            ClientId = request.ClientId,
            UserId = request.UserId,
            Prompt = request.Prompt,
            OrganisationOptions = filteredOptions,
            CallbackUrl = request.CallbackUrl,
            DetailLevel = request.DetailLevel,
            Created = createdUtc,
            Expires = createdUtc + new TimeSpan(0, options.SessionTimeoutInMinutes, 0),
        };

        string sessionKey = GenerateSessionKey();
        await sessionRepository.StoreAsync(sessionKey, sessionData);

        return new CreateSelectOrganisationSessionResponse {
            HasOptions = filteredOptions.Length > 0,
            Url = new Uri(options.SelectOrganisationBaseAddress, $"{request.ClientId}/{sessionKey}"),
        };
    }
}
