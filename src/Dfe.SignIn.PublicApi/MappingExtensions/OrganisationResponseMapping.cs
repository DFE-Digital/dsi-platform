using Dfe.SignIn.Base.Framework.Internal;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.PublicApi.Models;

namespace Dfe.SignIn.PublicApi.MappingExtensions;

/// <summary>
/// User related organiosation reponse mapper to DTO
/// </summary>
public static class OrganisationResponseMapping
{
    /// <summary>
    /// MAps the Organisation to The user organisation Dto
    /// </summary>
    /// <param name="o"></param>
    /// <returns></returns>
    public static UserOrganisationDto ToDto(this Organisation o)
    {
        return new UserOrganisationDto {
            Id = o.Id,
            Name = o.Name,
            Category = new CategoryDto {
                Id = ((int)o.Category).ToString(),
                Name = EnumHelpers.MapEnum<OrganisationCategory>((int)o.Category).GetDescription()
            },
            Urn = o.Urn,
            Uid = o.Uid,
            Ukprn = o.Ukprn,
            EstablishmentNumber = o.EstablishmentNumber,
            Status = new StatusDto {
                Id = (int)o.Status,
                Name = EnumHelpers.MapEnum<OrganisationStatus>((int)o.Status).GetDescription()
            },
            PIMSProviderType = o.PimsProviderType,
            PIMSProviderTypeCode = o.PimsProviderTypeCode,
            PIMSStatus = o.PimsStatus?.ToString(),
            UPIN = o.Upin,
            ClosedOn = o.ClosedOn,
            Address = o.Address,
            Telephone = o.Telephone,
            StatutoryLowAge = o.StatutoryLowAge,
            StatutoryHighAge = o.StatutoryHighAge,
            LegacyId = o.LegacyId?.ToString(),
            CompanyRegistrationNumber = o.CompanyRegistrationNumber,
            DistrictAdministrativeName = o.DistrictAdministrativeName,
            OpenedOn = o.OpenedOn,
            SourceSystem = o.SourceSystem,
            ProviderTypeName = o.ProviderTypeName,
            LegalName = o.LegalName,
            ProviderTypeCode = o.ProviderTypeName,
            GIASProviderType = o.GiasProviderType,
            PIMSStatusName = o.PimsStatusName,
            GIASStatusName = o.GiasStatusName,
            GIASStatus = o.GiasStatus,
            MasterProviderStatusName = o.MasterProviderStatusName,
            MasterProviderStatusCode = o.MasterProviderStatusCode,
            DistrictAdministrativeCode = o.DistrictAdministrativeCode,
            DistrictAdministrative_code = o.DistrictAdministrative_code,
            IsOnAPAR = o.IsOnApar
        };
    }
}
