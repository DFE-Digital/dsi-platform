using System.Linq.Expressions;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.Repository;

/// <summary>
/// Get organisation details.
/// </summary>
public class OrganisationRepository : IOrganisationRepository
{
    private readonly DbOrganisationsContext _dbContext;

    public OrganisationRepository(DbOrganisationsContext dbContext)
    {
        this._dbContext = dbContext;
    }

    public async Task<GetUsersAtOrganisationResponseRaw> SelectByExternalId(string clientName, string externalId, CancellationToken cancellationToken)
    {
        // Try UKPRN first
        IEnumerable<UserAtOrganisationRaw> results =
            await this.BuildQuery(clientName, o => o.Ukprn == externalId)
                .ToListAsync(cancellationToken);

        bool isUkprn = true;

        // If no results, fallback to UPIN
        if (!results.Any()) {
            isUkprn = false;

            results = await this.BuildQuery(clientName, o => o.Upin == externalId)
                .ToListAsync(cancellationToken);
        }

        return new GetUsersAtOrganisationResponseRaw {
            IsUkprn = isUkprn,
            ExternalId = externalId,
            Users = results
        };
    }

    public async Task<IEnumerable<GetUserOrganisationService>> SelectOrganisationServicesByUserId(Guid userId, CancellationToken cancellationToken)
    {
        IQueryable<GetUserOrganisationService> query =
             from u in this._dbContext.Users
             join uo in this._dbContext.UserOrganisations
                on u.Sub equals uo.UserId
             join o in this._dbContext.Organisations
                on uo.OrganisationId equals o.Id
             join us in this._dbContext.UserServices
                on o.Id equals us.OrganisationId
             join s in this._dbContext.Services
                 on us.ServiceId equals s.Id
             join usr in this._dbContext.UserServiceRoles
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
             join r in this._dbContext.Roles
                 on usr.RoleId equals r.Id
                 into roleGroup
             from r in roleGroup.DefaultIfEmpty()
             where u.Sub == userId
             where uo.UserId == userId
             where us.UserId == userId
             select new GetUserOrganisationService {
                 UserId = u.Sub,
                 UserStatus = u.Status,
                 Email = u.Email,
                 FamilyName = u.LastName,
                 GivenName = u.FirstName,
                 OrganisationId = o.Id,
                 OrganisationName = o.Name,
                 CategoryId = o.Category,
                 CategoryName = (o.Category == "001") ? "Establishment" : "Something else",
                 Urn = o.Urn,
                 Uid = o.Uid,
                 Ukprn = o.Ukprn,
                 EstablishmentNumber = o.EstablishmentNumber,
                 StatusId = o.Status,
                 StatusName = (o.Status == 1) ? "Open" : "Something different",
                 ClosedOn = o.ClosedOn,
                 Address = o.Address,
                 Telephone = o.Telephone,
                 StatutoryLowAge = o.StatutoryLowAge,
                 StatutoryHighAge = o.StatutoryHighAge,
                 LegacyId = o.LegacyId,
                 CompanyRegistrationNumber = o.CompanyRegistrationNumber,
                 ProviderProfileID = o.ProviderProfileId,
                 UPIN = o.Upin,
                 PIMSProviderType = o.PimsProviderType,
                 PIMSStatus = o.PimsStatus,
                 DistrictAdministrativeName = o.DistrictAdministrativeName1, // matches NodeJs
                 OpenedOn = o.OpenedOn,
                 SourceSystem = o.SourceSystem,
                 ProviderTypeName = o.ProviderTypeName,
                 GIASProviderType = o.GiasProviderType,
                 PIMSProviderTypeCode = o.PimsProviderTypeCode,
                 ServiceName = s.Name,
                 ServiceDescription = s.Description,
                 RoleName = r.Name,
                 RoleCode = r.Code,
                 OrgRoleId = uo.RoleId,
                 OrgRoleName = "Approver"
             };

        var results = await query.ToListAsync(cancellationToken);

        return results;
    }

    private IQueryable<UserAtOrganisationRaw> BuildQuery(string clientName,
        Expression<Func<OrganisationEntity, bool>> organisationFilter)
    {
        var organisations = this._dbContext.Organisations
            .Where(organisationFilter);

        return
            from us in this._dbContext.UserServices
            join o in organisations
                on us.OrganisationId equals o.Id
            join u in this._dbContext.Users
                on us.UserId equals u.Sub
            join s in this._dbContext.Services
                on us.ServiceId equals s.Id
            join usr in this._dbContext.UserServiceRoles
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
            join r in this._dbContext.Roles
                on usr.RoleId equals r.Id
                into roleGroup
            from r in roleGroup.DefaultIfEmpty()
            where s.ClientId == clientName
            select new UserAtOrganisationRaw(
                u.Sub,
                u.Email,
                u.FirstName,
                u.LastName,
                u.Status,
                r.Code);
    }
}
