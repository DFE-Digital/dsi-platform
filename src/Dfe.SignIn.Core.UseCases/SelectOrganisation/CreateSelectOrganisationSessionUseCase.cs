using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.SelectOrganisation;
using Dfe.SignIn.Core.Interfaces.SelectOrganisationSessions;
using Dfe.SignIn.Core.Public.SelectOrganisation;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.SelectOrganisation;

/// <summary>
/// Use case for creating a "select organisation" session.
/// </summary>
/// <param name="optionsAccessor">Provides access to "select organisation" options.</param>
/// <param name="sessionRepository">The repository of "select organisation" sessions.</param>
/// <param name="interaction">Service to dispatch interaction requests.</param>
public sealed class CreateSelectOrganisationSessionUseCase(
    IOptions<SelectOrganisationOptions> optionsAccessor,
    ISelectOrganisationSessionRepository sessionRepository,
    IInteractionDispatcher interaction
) : Interactor<CreateSelectOrganisationSessionRequest, CreateSelectOrganisationSessionResponse>
{
    private static string GenerateSessionKey() => Guid.NewGuid().ToString();

    /// <inheritdoc/>
    public override async Task<CreateSelectOrganisationSessionResponse> InvokeAsync(
        InteractionContext<CreateSelectOrganisationSessionRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var options = optionsAccessor.Value;

        var createdUtc = DateTime.UtcNow;

        var filteredOrganisationsResponse = await interaction.DispatchAsync(
            new FilterOrganisationsForUserRequest {
                ClientId = context.Request.ClientId,
                UserId = context.Request.UserId,
                Filter = context.Request.Filter,
            }, cancellationToken
        ).To<FilterOrganisationsForUserResponse>();

        var filteredOptions = filteredOrganisationsResponse.FilteredOrganisations
            .Select(organisation => new SelectOrganisationOption {
                Id = organisation.Id,
                Name = organisation.Name,
            })
            .ToArray();

        Guid requestId = Guid.NewGuid();

        var sessionData = new SelectOrganisationSessionData {
            ClientId = context.Request.ClientId,
            UserId = context.Request.UserId,
            Prompt = context.Request.Prompt,
            OrganisationOptions = filteredOptions,
            AllowCancel = context.Request.AllowCancel,
            CallbackUrl = new Uri(context.Request.CallbackUrl.GetLeftPart(UriPartial.Path) + $"?{CallbackParamNames.RequestId}={requestId}"),
            Created = createdUtc,
            Expires = createdUtc + new TimeSpan(0, options.SessionTimeoutInMinutes, 0),
        };

        string sessionKey = GenerateSessionKey();
        await sessionRepository.StoreAsync(sessionKey, sessionData);

        return new CreateSelectOrganisationSessionResponse {
            RequestId = requestId,
            HasOptions = filteredOptions.Length > 0,
            Url = new Uri(options.SelectOrganisationBaseAddress, $"{context.Request.ClientId}/{sessionKey}"),
        };
    }
}
