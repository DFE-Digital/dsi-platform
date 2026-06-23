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
