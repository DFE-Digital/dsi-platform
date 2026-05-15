using Dfe.SignIn.Base.Framework.Internal;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Core.Repository;
using Dfe.SignIn.PublicApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;

namespace Dfe.SignIn.PublicApi.Endpoints.Users;

public static partial class UserEndpoints
{
    /// <summary>
    /// Gets the list of organisations a user belongs to.
    /// Hidden organisations (status = 0) are excluded.
    /// </summary>
    /// <returns>
    ///   <para>200 with an array of organisations when the user belongs to, including services and roles.</para>
    ///   <para>404 when the user belongs to no organisations.</para>
    /// </returns>
    public static async Task<Results<Ok<GetUserOrganisationServicesResponse>, NotFound>> GetUserOrganisationServices(
        Guid userId,
        IOrganisationRepository organisationRepository,
        CancellationToken cancellationToken)
    {
        IEnumerable<GetUserOrganisationService> models = await organisationRepository.SelectOrganisationServicesByUserId(userId, cancellationToken);

        // suppress Sonarquble, ToList() is fine
#pragma warning disable IDE0305 // Simplify collection initialization

        List<GetUserOrganisationServicesResponse> response =
            models
                .GroupBy(x => x.UserId)
                .Select(userGroup => {
                    var u = userGroup.First();

                    return new GetUserOrganisationServicesResponse {
                        UserId = u.UserId,
                        UserStatus = u.UserStatus,
                        Email = u.Email,
                        FamilyName = u.FamilyName,
                        GivenName = u.GivenName,

                        Organisations = userGroup
                            .GroupBy(x => x.OrganisationId)
                            .Select(orgGroup => {
                                var o = orgGroup.First();

                                return new OrganisationDto {
                                    Id = o.OrganisationId,
                                    Name = o.OrganisationName,

                                    Category = new CategoryDto {
                                        Id = o.CategoryId,
                                        Name = EnumHelpers.MapEnum<OrganisationCategory>(o.CategoryId)
                                    },

                                    Status = new StatusDto {
                                        Id = o.StatusId,
                                        Name = EnumHelpers.MapEnum<OrganisationStatus>(o.StatusId)
                                    },

                                    Urn = o.Urn,
                                    Uid = o.Uid,
                                    Ukprn = o.Ukprn,
                                    EstablishmentNumber = o.EstablishmentNumber,
                                    ClosedOn = o.ClosedOn,
                                    Address = o.Address,
                                    Telephone = o.Telephone,
                                    StatutoryLowAge = o.StatutoryLowAge,
                                    StatutoryHighAge = o.StatutoryHighAge,
                                    LegacyId = o.LegacyId,
                                    CompanyRegistrationNumber = o.CompanyRegistrationNumber,
                                    ProviderProfileID = o.ProviderProfileID,
                                    UPIN = o.UPIN,
                                    PIMSProviderType = o.PIMSProviderType,
                                    PIMSStatus = o.PIMSStatus,
                                    DistrictAdministrativeName = o.DistrictAdministrativeName,
                                    OpenedOn = o.OpenedOn,
                                    SourceSystem = o.SourceSystem,
                                    ProviderTypeName = o.ProviderTypeName,
                                    GIASProviderType = o.GIASProviderType,
                                    PIMSProviderTypeCode = o.PIMSProviderTypeCode,

                                    Services = orgGroup
                                        .GroupBy(x => x.ServiceName)
                                        .Select(serviceGroup => {
                                            var s = serviceGroup.First();

                                            return new ServiceDto {
                                                Name = s.ServiceName,
                                                Description = s.ServiceDescription,

                                                Roles = serviceGroup
                                                    .Where(r => r.RoleName != null)
                                                    .Select(r => new RoleDto {
                                                        Name = r.RoleName,
                                                        Code = r.RoleCode
                                                    })
                                                    .DistinctBy(r => new { r.Name, r.Code })
                                                    .ToList()
                                            };

                                        })
                                        .ToList(),

                                    OrgRoleId = o.OrgRoleId,
                                    OrgRoleName = o.OrgRoleName
                                };
                            })
                            .ToList()
                    };
                })
                .ToList();

#pragma warning restore IDE0305 // Simplify collection initialization

        return TypedResults.Ok(response.FirstOrDefault());

    }
}
