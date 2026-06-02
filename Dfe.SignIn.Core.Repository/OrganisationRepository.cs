using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Core.Repository;

/// <summary>
/// Get organisation details.
/// </summary>
[ExcludeFromCodeCoverage]
public class OrganisationRepository : IOrganisationRepository
{
    private readonly DbOrganisationsContext _dbContext;

    public OrganisationRepository(DbOrganisationsContext dbContext)
    {
        this._dbContext = dbContext;
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<GetUserOrganisationService>> SelectOrganisationServicesAndRolesByUserId(string clientName, Guid userId, CancellationToken cancellationToken)
    {
        // COMMENTS below extracted from NodJs.

        // Get the details for the user (name, email, etc)

        // Call to get data about user of this service (limited by clientId). Returns their organisation
        // role for this service (end user/approver), and the organisations this user is part of for
        // this service (can be part of a service for multiple organisations)

        // Need to do 2 calls so we can translate the organisation category and status
        // ids into their human readable names

        // Filter out orgs with status of 0.  This is mostly to remove the hidden id-only org, if present.

        // Get list of ALL services for the user.  We need this because it has all the the service
        // specific roles for the user against each service for each organisationId.
        // We need this because that role information isn't provided in the getFilteredServiceUsersRaw call.

        // A user can have multiple organisations for the same service, so we loop over them all.

        // Find all the services the user has for this organisation so we can put it in the response

        // For all the roles in the service, loop over them so we have a list of names instead
        // of a list of just ids.

        var sql = FormattableStringFactory.Create(
            """
            
            SELECT           
                u.sub AS UserId
                ,CAST(u.[status] AS INT) AS UserStatus
                ,u.email
                ,u.family_name AS FamilyName
                ,u.given_name AS GivenName
                ,o.Id AS OrganisationId
                ,o.[name] AS OrganisationName
                ,o.Category AS CategoryId
                ,o.URN
                ,o.[uid]
                ,o.UKPRN
                ,o.EstablishmentNumber
                ,o.[Status] AS StatusId
                ,o.ClosedOn
                ,o.[Address]
                ,o.telephone
                ,o.statutoryLowAge
                ,o.statutoryHighAge
                ,o.legacyId
                ,o.companyRegistrationNumber
                ,o.ProviderProfileID
                ,o.UPIN
                ,o.PIMSProviderType
                ,o.PIMSStatus
                ,o.DistrictAdministrativeName
                ,o.OpenedOn
                ,o.SourceSystem
                ,o.ProviderTypeName
                ,o.GIASProviderType
                ,o.PIMSProviderTypeCode
                ,s.id AS ServiceId
                ,s.[name] AS ServiceName
                ,s.[description] AS ServiceDescription
                ,r.[Name] AS RoleName
                ,r.Code AS RoleCode
                ,uo.role_id AS OrgRoleId
            FROM
            (
                SELECT
                    uo.[user_id]
                    ,uo.organisation_id
                FROM
                    dbo.user_organisation uo
                INNER JOIN
                    dbo.user_services us ON us.organisation_id = uo.organisation_id AND us.user_id = uo.user_id
                INNER JOIN
                    dbo.[service] s ON s.id = us.service_id
                INNER JOIN
                    dbo.organisation o ON o.Id = uo.organisation_id
                WHERE
                    uo.[user_id] = {0}
                    AND s.clientId = {1}
                    AND o.[status] <> 0
                ) orgUsers
                INNER JOIN
                    dbo.[user] u ON u.sub = orgUsers.user_id
                INNER JOIN
                    dbo.user_organisation uo ON uo.[user_id] = orgUsers.user_id AND uo.organisation_id = orgUsers.organisation_id
                INNER JOIN
                    dbo.organisation o ON o.id = uo.organisation_id
                INNER JOIN
                    dbo.user_services us ON us.organisation_id = o.Id AND us.[user_id] = u.sub
                LEFT OUTER JOIN
                    dbo.[service] s ON s.id = us.service_id
                LEFT OUTER JOIN
                    dbo.user_service_roles usr ON usr.organisation_id = us.organisation_id
                        AND usr.service_id = us.service_id
                            AND usr.[user_id] = us.[user_id]
                LEFT OUTER JOIN
                    dbo.[Role] r ON r.Id = usr.role_id
                ;
            
            """, userId, clientName);

        var results = await this._dbContext.Database.SqlQuery<GetUserOrganisationService>(sql).ToListAsync(cancellationToken);

        return results;
    }
}
