using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.PrivateApi.DataModels;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.PrivateApi.Repository;

/// <summary>
/// Get organisation details.
/// </summary>
[ExcludeFromCodeCoverage]
public class OrganisationRepository : IOrganisationRepository
{
    private readonly DbOrganisationsContext _dbContext;

    /// <summary>
    /// Organisation table queries.
    /// </summary>
    /// <param name="dbContext"></param>
    public OrganisationRepository(DbOrganisationsContext dbContext)
    {
        this._dbContext = dbContext;
    }

    ///<inheritdoc/>
    public async Task<IEnumerable<UserOrganisationServicesQuery>> SelectOrganisationServicesAndRolesByUserId(string clientName, Guid userId, CancellationToken cancellationToken)
    {

        var results = await this._dbContext.Database
            .SqlQuery<UserOrganisationServicesQuery>(
                $"""
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
                FROM dbo.user_organisation uo
                JOIN dbo.[user] u 
                    ON u.sub = uo.user_id
                JOIN dbo.organisation o 
                    ON o.Id = uo.organisation_id
                JOIN dbo.user_services us 
                    ON us.organisation_id = uo.organisation_id 
                    AND us.user_id = uo.user_id
                LEFT JOIN dbo.[service] s 
                    ON s.id = us.service_id
                LEFT JOIN dbo.user_service_roles usr 
                    ON usr.organisation_id = us.organisation_id
                    AND usr.service_id = us.service_id
                    AND usr.user_id = us.user_id
                LEFT JOIN dbo.[Role] r 
                    ON r.Id = usr.role_id
                WHERE
                    uo.user_id = {userId}
                    AND 
                    o.[status] <> 0
                    AND EXISTS (
                        SELECT 1
                        FROM dbo.user_services us2
                        JOIN dbo.[service] s2 
                            ON s2.id = us2.service_id
                        WHERE 
                            us2.organisation_id = uo.organisation_id
                            AND us2.user_id = uo.user_id
                            AND s2.clientId = {clientName}
                    );
                """)
            .ToListAsync(cancellationToken);

        return results;
    }
}
