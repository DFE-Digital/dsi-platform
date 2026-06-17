using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Access;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Users;

/// <summary>
/// Use case for the public API "Get user access to service" endpoint.
/// Fetches the user's roles, identifiers, and legacy IDs for a given
/// service + organisation combination.
/// </summary>
/// <remarks>
///   <para>Data sources:</para>
///   <list type="bullet">
///     <item>Access Node API — user's roles and service identifiers
///     (via <see cref="GetUserServiceAccessRequest"/>)</item>
///     <item>InternalApi / Organisations DB — user-organisation legacy numeric and text identifiers
///     (via <see cref="GetUserOrganisationIdentifiersRequest"/>)</item>
///     <item>InternalApi / Organisations DB — organisation legacy ID and APAR flag
///     (via <see cref="GetOrganisationByIdRequest"/>)</item>
///   </list>
/// </remarks>
public sealed class GetUserServiceAccessDetailsUseCase(
    DbOrganisationsContext uowOrganisations,
    IInteractionDispatcher interaction
) : Interactor<GetUserServiceAccessDetailsRequest, GetUserServiceAccessDetailsResponse>
{
    /// <inheritdoc/>
    public override async Task<GetUserServiceAccessDetailsResponse> InvokeAsync(
        InteractionContext<GetUserServiceAccessDetailsRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        // 1. Confirm the user has access to this service in this organisation.
        //    Returns null access when no record exists → 404 from the endpoint.
        var accessResponse = await this.GetUserService(
            context.Request.UserId,
            context.Request.ServiceId,
            context.Request.OrganisationId
        );

        if (accessResponse.Access is null) {
            throw UserServiceAccessNotFoundException.From(
                context.Request.UserId,
                context.Request.ServiceId,
                context.Request.OrganisationId);
        }

        // 2. Fetch user-org legacy identifiers and organisation details in parallel.
        var userOrgIdentifiersTask = interaction.DispatchAsync(
            new GetUserOrganisationIdentifiersRequest {
                UserId = context.Request.UserId,
                OrganisationId = context.Request.OrganisationId,
            }
        ).To<GetUserOrganisationIdentifiersResponse>();

        var organisationTask = interaction.DispatchAsync(
            new GetOrganisationByIdRequest {
                OrganisationId = context.Request.OrganisationId,
            }
        ).To<GetOrganisationByIdResponse>();

        await Task.WhenAll(userOrgIdentifiersTask, organisationTask);

        var userOrgIdentifiers = await userOrgIdentifiersTask;
        var organisation = (await organisationTask).Organisation;

        return new GetUserServiceAccessDetailsResponse {
            UserId = accessResponse.Access.UserId,
            UserLegacyNumericId = userOrgIdentifiers.NumericIdentifier.ToString(),
            UserLegacyTextId = userOrgIdentifiers.TextIdentifier,
            ServiceId = accessResponse.Access.ServiceId,
            OrganisationId = accessResponse.Access.OrganisationId,
            OrganisationLegacyId = organisation.LegacyId.ToString(),
            OrganisationIsOnApar = organisation.IsOnApar,
            Roles = accessResponse.Access.Roles,
            Identifiers = accessResponse.Access.Identifiers,
        };
    }

    private async Task<GetUserServiceAccessResponse> GetUserService(Guid userId, Guid serviceId, Guid organisationId)
    {
        var userService = await uowOrganisations.UserServices
            .AsNoTracking()
            .Where(x => x.ServiceId == serviceId)
            .Where(x => x.UserId == userId)
            .Where(x => x.OrganisationId == organisationId)
            .FirstOrDefaultAsync();

        if (userService is null) {
            return new GetUserServiceAccessResponse { Access = null };
        }

        var roles = await uowOrganisations.UserServiceRoles
            .AsNoTracking()
            .Include(x => x.Role)
            .Where(x => x.UserId == userId)
            .Where(x => x.ServiceId == serviceId)
            .Where(x => x.OrganisationId == organisationId)
            .ToListAsync();

        var identifiers = await uowOrganisations.UserServiceIdentifiers
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Where(x => x.ServiceId == serviceId)
            .Where(x => x.OrganisationId == organisationId)
            .ToListAsync();

        return new GetUserServiceAccessResponse {
            Access = new UserServiceAccess {
                UserId = userService.UserId,
                ServiceId = userService.ServiceId!.Value,
                OrganisationId = userService.OrganisationId!.Value,
                Roles = roles.Select(r => new UserServiceRole {
                    Id = r.Role.Id,
                    Name = r.Role.Name,
                    Code = r.Role.Code,
                    NumericId = r.Role.NumericId.ToString(),
                    Status = UserServiceRoleStatus.FromId(r.Role.Status),
                }),
                Identifiers = identifiers.Select(i => new UserServiceIdentifier {
                    Key = i.IdentifierKey,
                    Value = i.IdentifierValue ?? string.Empty,
                }),
            }
        };
    }
}
