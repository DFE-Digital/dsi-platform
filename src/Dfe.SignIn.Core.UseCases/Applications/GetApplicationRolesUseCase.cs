using System.Data;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Core.Interfaces.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Applications;

/// <summary>
/// Use case responsible for obtaining information about an application.
/// </summary>
public sealed class GetApplicationRolesUseCase(
    IUnitOfWorkOrganisations uowOrganisations
) : Interactor<GetApplicationRolesRequest, GetApplicationRolesResponse>
{
    /// <inheritdoc/>
    public override async Task<GetApplicationRolesResponse> InvokeAsync(
        InteractionContext<GetApplicationRolesRequest> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var roles = await uowOrganisations.Repository<RoleEntity>()
            .Include(r => r.Parent)
            .Where(r => r.ApplicationId == context.Request.ApplicationId)
            .OrderBy(r => r.Name)
            .Select(x => new {
                x.Id,
                x.Name,
                x.Code,
                x.NumericId,
                x.Status,
                x.Parent
            })
            .ToListAsync(cancellationToken);

        return new GetApplicationRolesResponse {
            Roles = roles?.Select(r => new ApplicationRole {
                Id = r.Id,
                Name = r.Name,
                Code = r.Code,
                NumericId = r.NumericId,
                Status = (ApplicationRoleStatus)r.Status,
                Parent = r.Parent != null ? new ApplicationRole {
                    Id = r.Parent.Id,
                    Name = r.Parent.Name,
                    Code = r.Parent.Code,
                    NumericId = r.Parent.NumericId,
                    Status = (ApplicationRoleStatus)r.Parent.Status
                } : null
            }) ?? []
        };
    }
}
