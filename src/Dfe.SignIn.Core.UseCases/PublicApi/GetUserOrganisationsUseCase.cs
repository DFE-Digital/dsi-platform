using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Public;

namespace Dfe.SignIn.Core.UseCases.PublicApi;

/// <summary>
/// Use case for the public API "Get organisations for user" endpoint.
/// Fetches all organisations associated with the user, filters out hidden ones
/// (status = 0), and returns 404 if none remain.
/// </summary>
/// <remarks>
///   <para>Data source: Organisations Node API, via
///   <see cref="GetOrganisationsAssociatedWithUserRequest"/>.</para>
///   <para>An organisation with <see cref="OrganisationStatus.Hidden"/> (id = 0)
///   is a hidden id-only org and must be excluded from the response.</para>
/// </remarks>
public sealed class GetUserOrganisationsUseCase(
    IInteractionDispatcher interaction
) : Interactor<GetUserOrganisationsRequest, GetUserOrganisationsResponse>
{
    /// <inheritdoc/>
    public override async Task<GetUserOrganisationsResponse> InvokeAsync(
        InteractionContext<GetUserOrganisationsRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var result = await interaction.DispatchAsync(
            new GetOrganisationsAssociatedWithUserRequest {
                UserId = context.Request.UserId,
            }
        ).To<GetOrganisationsAssociatedWithUserResponse>();

        // Filter out hidden orgs (status.id = 0) — these are id-only orgs not
        // visible to external API consumers.
        var visible = result.Organisations
            .Where(org => org.Status != OrganisationStatus.Hidden)
            .ToList();

        if (visible.Count == 0) {
            throw UserNotFoundException.FromUserId(context.Request.UserId);
        }

        return new GetUserOrganisationsResponse {
            Organisations = visible,
        };
    }
}
