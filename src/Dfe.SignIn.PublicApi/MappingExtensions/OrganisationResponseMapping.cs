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
    private static readonly Dictionary<string, string> EstablishmentTypes = new() {
        {  "01","Community School" },
        {  "02", "Voluntary Aided School" },
        {  "03", "Voluntary Controlled School" },
        {  "05", "Foundation School" },
        {  "06", "City Technology College" },
        {  "07", "Community Special School" },
        {  "08", "Non-Maintained Special School" },
        {  "10", "Other Independent Special School" },
        {  "11", "Other Independent School" },
        {  "12", "Foundation Special School" },
        {  "14", "Pupil Referral Unit" },
        {  "15", "LA Nursery School" },
        {  "18", "Further Education" },
        {  "24", "Secure Units" },
        {  "25", "Offshore Schools" },
        {  "26", "Service Childrens Education" },
        {  "27", "Miscellaneous" },
        {  "28", "Academy Sponsor Led" },
        {  "29", "Higher education institution" },
        {  "30", "Welsh Establishment" },
        {  "31", "Sixth Form Centres" },
        {  "32", "Special Post 16 Institution" },
        {  "33", "Academy Special Sponsor Led" },
        {  "34", "Academy Converter" },
        {  "35", "Free Schools" },
        {  "36", "Free Schools Special" },
        {  "37", "British Overseas Schools" },
        {  "38", "Free Schools - Alternative Provision" },
        {  "39", "Free Schools - 16-19" },
        {  "40", "University Technical College" },
        {  "41", "Studio Schools" },
        {  "42", "Academy Alternative Provision Converter" },
        {  "43", "Academy Alternative Provision Sponsor Led" },
        {  "44", "Academy Special Converter" },
        {  "45", "Academy 16-19 Converter" },
        {  "46", "Academy 16-19 Sponsor Led" },
        {  "47", "Children's Centre" },
        {  "48", "Children's Centre Linked Site" },
        {  "49", "Online Provider" },
        {  "56", "Institution funded by other government department" },
        {  "57", "Academy secure 16 to 19" }
    };

    private static readonly Dictionary<string, string> RegionMappings = new() {
        {  "A","North East" },
        {  "B", "North West" },
        {  "D", "Yorkshire and the Humber" },
        {  "E", "East Midlands" },
        {  "F", "West Midlands" },
        {  "G", "East of England" },
        {  "H", "London" },
        {  "J", "South East" },
        {  "K", "South West" },
        {  "W", "Wales (pseudo)" },
        {  "Z", "Not Applicable" }
    };

    private static readonly Dictionary<int, string> PhaseOfEducationMappings = new() {
        {  0,  "Not applicable" },
        {  1,  "Nursery" },
        {  2,  "Primary" },
        {  3,  "Middle deemed primary" },
        {  4,  "Secondary" },
        {  5,  "Middle deemed secondary" },
        {  6,  "16 plus" },
        {  7,  "All through" }
    };

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
            DistrictAdministrative_Code = o.DistrictAdministrative_code,
            IsOnAPAR = o.IsOnApar,
            LocalAuthority = o.LocalAuthority is not null
                ? new Models.LocalAuthority(o.LocalAuthority.Id, o.LocalAuthority.Name, o.LocalAuthority.Code)
                : null,
            PhaseOfEducation = ConstructPhaseOfEducationData(o.PhaseOfEducation),
            Region = ConstructRegion(o.RegionCode),
            EstablishmentType = o.EstablishmentType is not null ? ConstructEstablishmentType((int)o.EstablishmentType) : null
        };
    }

    private static Region? ConstructRegion(string? regionCode)
    {
        if (string.IsNullOrEmpty(regionCode)) {
            return null;
        }

        if (!RegionMappings.ContainsKey(regionCode)) {
            return null;
        }
        return new Region(regionCode, RegionMappings[regionCode]);
    }

    private static Models.EstablishmentType? ConstructEstablishmentType(int establishment)
    {
        string establishmentKey = establishment.ToString("D2");

        if (!EstablishmentTypes.ContainsKey(establishmentKey)) {
            return null;
        }

        return new Models.EstablishmentType(establishmentKey, EstablishmentTypes[establishmentKey]);
    }

    private static PhaseOfEducation? ConstructPhaseOfEducationData(int? phaseOfEducation)
    {
        if (phaseOfEducation is null) {
            return new PhaseOfEducation("0", "Not applicable");
        }

        if (!PhaseOfEducationMappings.ContainsKey(phaseOfEducation.Value)) {
            return null;
        }

        return new PhaseOfEducation(phaseOfEducation.Value.ToString(), PhaseOfEducationMappings[phaseOfEducation.Value]);
    }
}
