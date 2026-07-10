using Dfe.SignIn.Base.Framework.Internal;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Public;
using Dfe.SignIn.Core.Public.Metadata;
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
        var tagColor = AnnotationHelpers.GetTagColour(o.Status);

        var categoryName = string.Empty;

        if (!string.IsNullOrEmpty(o.CategoryId)) {
            categoryName = EnumHelpers.MapEnum<OrganisationCategory>(o.CategoryId).GetDescription();
        }

        return new UserOrganisationDto {
            Id = o.Id,
            Name = o.Name,
            Category = new CategoryDto {
                Id = o.CategoryId,
                Name = categoryName
            },
            Urn = o.Urn,
            Uid = o.Uid,
            Ukprn = o.Ukprn,
            EstablishmentNumber = o.EstablishmentNumber,
            Status = new StatusWithLabelDto {
                Id = (int)o.Status,
                Name = EnumHelpers.MapEnum<OrganisationStatus>((int)o.Status).GetDescription(),
                TagColor = tagColor?.ToString().ToLower()
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
            ProviderTypeCode = o.ProviderTypeCode,
            GIASProviderType = o.GiasProviderType,
            PIMSStatusName = o.PimsStatusName,
            GIASStatusName = o.GiasStatusName,
            GIASStatus = o.GiasStatus,
            MasterProviderStatusName = o.MasterProviderStatusName,
            MasterProviderStatusCode = o.MasterProviderStatusCode,
            DistrictAdministrativeCode = o.DistrictAdministrativeCode,
            IsOnAPAR = o.IsOnApar,
            LocalAuthority = o.LocalAuthority is not null
                ? new Models.LocalAuthority(o.LocalAuthority.Id, o.LocalAuthority.Name, o.LocalAuthority.Code)
                : null,
            PhaseOfEducation = ConstructPhaseOfEducationData(o.PhaseOfEducation)
        };
    }

    private static PhaseOfEducation ConstructPhaseOfEducationData(int? phaseOfEducation)
    {
        if (phaseOfEducation is null) {
            return new PhaseOfEducation("0", "Not applicable");
        }

        OrganisationCategory name = EnumHelpers.MapEnum<OrganisationCategory>(phaseOfEducation);

        return new PhaseOfEducation(phaseOfEducation.Value.ToString("D3"), name.GetDescription());
    }
}
