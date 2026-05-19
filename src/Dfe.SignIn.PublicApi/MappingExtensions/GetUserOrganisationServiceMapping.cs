using Dfe.SignIn.Base.Framework.Internal;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.PublicApi.Models;

namespace Dfe.SignIn.PublicApi.MappingExtensions;

/// <summary>
/// Map GetUserOrganisationService to data transfer objects.
/// </summary>
public static class GetUserOrganisationServiceMapping
{
    /// <summary>
    /// Trasform to users.
    /// </summary>
    /// <param name="models"></param>
    /// <returns></returns>
    public static IEnumerable<GetUserOrganisationServicesResponse> ToUserDtos(this IEnumerable<GetUserOrganisationService> models)
    {
        return models
            .GroupBy(x => x.UserId)
            .Select(userGroup => {
                GetUserOrganisationService u = userGroup.First();

                return new GetUserOrganisationServicesResponse {
                    UserId = u.UserId,
                    UserStatus = u.UserStatus,
                    Email = u.Email,
                    FamilyName = u.FamilyName,
                    GivenName = u.GivenName,

                    Organisations = userGroup.ToOrganisationDtos()

                };
            });
    }

    /// <summary>
    /// Trasform to organisations.
    /// </summary>
    /// <param name="userGroup"></param>
    /// <returns></returns>
    public static IEnumerable<OrganisationDto> ToOrganisationDtos(
    this IGrouping<Guid, GetUserOrganisationService> userGroup)
    {
        return userGroup
            .GroupBy(x => x.OrganisationId)
            .Select(static orgGroup => {
                var o = orgGroup.First();

                return new OrganisationDto {
                    Id = o.OrganisationId,
                    Name = o.OrganisationName,

                    Category = new CategoryDto {
                        Id = o.CategoryId,
                        Name = EnumHelpers.MapEnum<OrganisationCategory>(o.CategoryId).GetDescription()
                    },

                    Urn = o.Urn,
                    Uid = o.Uid,
                    Ukprn = o.Ukprn,
                    EstablishmentNumber = o.EstablishmentNumber,

                    Status = new StatusDto {
                        Id = o.StatusId,
                        Name = EnumHelpers.MapEnum<OrganisationStatus>(o.StatusId).GetDescription()
                    },

                    ClosedOn = o.ClosedOn,
                    Address = o.Address,
                    Telephone = o.Telephone,
                    StatutoryLowAge = o.StatutoryLowAge,
                    StatutoryHighAge = o.StatutoryHighAge,
                    LegacyId = o.LegacyId?.ToString(),
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

                    Services = orgGroup.ToServiceDtos(),

                    OrgRoleId = o.OrgRoleId,
                    OrgRoleName = OrganisationRoles.FromId(o.OrgRoleId)?.Name
                };
            });

    }

    /// <summary>
    /// Trasform to services.
    /// </summary>
    /// <param name="orgGroup"></param>
    /// <returns></returns>
    public static IEnumerable<ServiceDto> ToServiceDtos(
    this IGrouping<Guid, GetUserOrganisationService> orgGroup)
    {
        return orgGroup
            .GroupBy(x => x.ServiceName)
            .OrderBy(x => x.Key)
            .Select(serviceGroup => {
                var s = serviceGroup.First();

                return new ServiceDto {
                    Name = s.ServiceName,
                    Description = s.ServiceDescription,
                    Roles = serviceGroup.ToRoleDtos()
                };

            });
    }

    /// <summary>
    /// Transform in roles;
    /// </summary>
    /// <param name="serviceGroup"></param>
    /// <returns></returns>
    public static IEnumerable<RoleDto> ToRoleDtos(
        this IGrouping<string, GetUserOrganisationService> serviceGroup)
    {
        return serviceGroup
            .Where(r => r.RoleName != null)
            .Select(r => new RoleDto {
                Name = r.RoleName,
                Code = r.RoleCode
            })
            .DistinctBy(r => new { r.Name, r.Code });
    }
}

