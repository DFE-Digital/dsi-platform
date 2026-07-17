using System.Data;
using System.Linq.Expressions;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Organisations;

using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.UseCases.Organisations;

/// <summary>
/// Query returns users and their roles for a service and organisation.
/// </summary>
/// <param name="organisationsDbContext"></param>
public sealed class GetUsersAtOrganisationUseCase(DbOrganisationsContext organisationsDbContext) : Interactor<GetUsersAtOrganisationRequestRaw, GetUsersAtOrganisationResponseRaw>
{
    /// <inheritdoc/>
    public override async Task<GetUsersAtOrganisationResponseRaw> InvokeAsync(
        InteractionContext<GetUsersAtOrganisationRequestRaw> context,
        CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        string clientId = context.Request.ClientId;
        string externalId = context.Request.ExternalId;

        // Try UKPRN first
        var results = await this.BuildQuery(clientId, o => o.Ukprn == externalId)
            .ToListAsync(cancellationToken);

        bool isUkprn = true;

        // If no results, fallback to UPIN
        if (results.Count == 0) {
            isUkprn = false;

            results = await this.BuildQuery(clientId, o => o.Upin == externalId)
                .ToListAsync(cancellationToken);
        }

        return new GetUsersAtOrganisationResponseRaw {
            IsUkprn = isUkprn,
            ExternalId = externalId,
            Users = results
        };
    }

    private IQueryable<UserAtOrganisationRaw> BuildQuery(
        string clientId,
        Expression<Func<OrganisationEntity, bool>> organisationFilter)
    {
        var organisations = organisationsDbContext
            .Organisations
            .Where(organisationFilter);

        return
            from us in organisationsDbContext.UserServices
            join o in organisations
                on us.OrganisationId equals o.Id
            join u in organisationsDbContext.Users
                on us.UserId equals u.Sub
            join s in organisationsDbContext.Services
                on us.ServiceId equals s.Id
            join usr in organisationsDbContext.UserServiceRoles
                on new {
                    oid = us.OrganisationId,
                    sid = us.ServiceId,
                    uid = us.UserId
                }
                equals new {
                    oid = (Guid?)usr.OrganisationId,
                    sid = (Guid?)usr.ServiceId,
                    uid = usr.UserId
                }
                into usrGroup
            from usr in usrGroup.DefaultIfEmpty()
            join r in organisationsDbContext.Roles
                on usr.RoleId equals r.Id
                into roleGroup
            from r in roleGroup.DefaultIfEmpty()
            where s.ClientId == clientId
            select new UserAtOrganisationRaw(
                u.Sub,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Status,
                r.Code);
    }
}
