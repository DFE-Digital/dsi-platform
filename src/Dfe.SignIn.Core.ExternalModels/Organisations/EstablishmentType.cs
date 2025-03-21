using System.ComponentModel;

namespace Dfe.SignIn.Core.ExternalModels.Organisations;

/// <summary>
/// Specifies the establishment type of an organisation.
/// </summary>
/// <seealso cref="OrganisationCategory"/>
public enum EstablishmentType
{
    /// <summary>
    /// Community school.
    /// </summary>
    [Description("Community School")]
    CommunitySchool = 1,

    /// <summary>
    /// Voluntary aided school.
    /// </summary>
    [Description("Voluntary Aided School")]
    VoluntaryAidedSchool = 2,

    /// <summary>
    /// Voluntary controlled school
    /// </summary>
    [Description("Voluntary Controlled School")]
    VoluntaryControlledSchool = 3,

    /// <summary>
    /// Foundation school.
    /// </summary>
    [Description("Foundation School")]
    FoundationSchool = 5,

    /// <summary>
    /// City technology college.
    /// </summary>
    [Description("City Technology College")]
    CityTechnologyCollege = 6,

    /// <summary>
    /// Community special school.
    /// </summary>
    [Description("Community Special School")]
    CommunitySpecialSchool = 7,

    /// <summary>
    /// Non-Maintained special school.
    /// </summary>
    [Description("Non-Maintained Special School")]
    NonMaintainedSpecialSchool = 8,

    /// <summary>
    /// Other independent special school.
    /// </summary>
    [Description("Other Independent Special School")]
    OtherIndependentSpecialSchool = 10,

    /// <summary>
    /// Other independent school.
    /// </summary>
    [Description("Other Independent School")]
    OtherIndependentSchool = 11,

    /// <summary>
    /// Foundation special school.
    /// </summary>
    [Description("Foundation Special School")]
    FoundationSpecialSchool = 12,

    /// <summary>
    /// Pupil referral unit.
    /// </summary>
    [Description("Pupil Referral Unit")]
    PupilReferralUnit = 14,

    /// <summary>
    /// Local authority nursery school.
    /// </summary>
    [Description("LA Nursery School")]
    LocalAuthorityNurserySchool = 15,

    /// <summary>
    /// Further education.
    /// </summary>
    [Description("Further Education")]
    FurtherEducation = 18,

    /// <summary>
    /// Secure units.
    /// </summary>
    [Description("Secure Units")]
    SecureUnits = 24,

    /// <summary>
    /// Offshore schools.
    /// </summary>
    [Description("Offshore Schools")]
    OffshoreSchools = 25,

    /// <summary>
    /// Service children's education.
    /// </summary>
    [Description("Service Childrens Education")]
    ServiceChildrensEducation = 26,

    /// <summary>
    /// Miscellaneous.
    /// </summary>
    [Description("Miscellaneous")]
    Miscellaneous = 27,

    /// <summary>
    /// Academy sponsor led.
    /// </summary>
    [Description("Academy Sponsor Led")]
    AcademySponserLed = 28,

    /// <summary>
    /// Higher education institution.
    /// </summary>
    [Description("Higher education institution")]
    HigherEducationInstitution = 29,

    /// <summary>
    /// Welsh establishment.
    /// </summary>
    [Description("Welsh Establishment")]
    WelshEstablishment = 30,

    /// <summary>
    /// Sixth form centres.
    /// </summary>
    [Description("Sixth Form Centres")]
    SixthFormCentres = 31,

    /// <summary>
    /// Special post age 16 institution.
    /// </summary>
    [Description("Special Post 16 Institution")]
    SpecialPost16Institution = 32,

    /// <summary>
    /// Academy special sponsor led.
    /// </summary>
    [Description("Academy Special Sponsor Led")]
    AcademySpecialSponserLed = 33,

    /// <summary>
    /// Academy converter.
    /// </summary>
    [Description("Academy Converter")]
    AcademyConverter = 34,

    /// <summary>
    /// Free schools.
    /// </summary>
    [Description("Free Schools")]
    FreeSchools = 35,

    /// <summary>
    /// Free schools special.
    /// </summary>
    [Description("Free Schools Special")]
    FreeSchoolsSpecial = 36,

    /// <summary>
    /// British overseas schools.
    /// </summary>
    [Description("British Overseas Schools")]
    BritishOverseasSchools = 37,

    /// <summary>
    /// Free schools - alternative provision.
    /// </summary>
    [Description("Free Schools - Alternative Provision")]
    FreeSchoolsAlternativeProvision = 38,

    /// <summary>
    /// Free schools ages 16 to 19.
    /// </summary>
    [Description("Free Schools - 16-19")]
    FreeSchools16To19 = 39,

    /// <summary>
    /// University technical college.
    /// </summary>
    [Description("University Technical College")]
    UniversityTechnicalCollege = 40,

    /// <summary>
    /// Studio schools.
    /// </summary>
    [Description("Studio Schools")]
    StudioSchools = 41,

    /// <summary>
    /// Academy alternative provision converter.
    /// </summary>
    [Description("Academy Alternative Provision Converter")]
    AcademyAlternativeProvisionConverter = 42,

    /// <summary>
    /// Academy alternative provision sponsor led.
    /// </summary>
    [Description("Academy Alternative Provision Sponsor Led")]
    AcademyAlternativeProvisionSponserLed = 43,

    /// <summary>
    /// Academy special converter.
    /// </summary>
    [Description("Academy Special Converter")]
    AcademySpecialConverter = 44,

    /// <summary>
    /// Academy ages 16 to 19 converter.
    /// </summary>
    [Description("Academy 16-19 Converter")]
    Academy16To19Converter = 45,

    /// <summary>
    /// Academy ages 16 to 19 sponsor led
    /// </summary>
    [Description("Academy 16-19 Sponsor Led")]
    Academy16To19SponserLed = 46,

    /// <summary>
    /// Children's centre.
    /// </summary>
    [Description("Children's Centre")]
    ChildrensCentre = 47,

    /// <summary>
    /// Children's centre linked site.
    /// </summary>
    [Description("Children's Centre Linked Site")]
    ChildrensCentreLinkedSite = 48,

    /// <summary>
    /// Institution funded by other government department.
    /// </summary>
    [Description("Institution funded by other government department")]
    InstitutionFundedByOtherGovernmentDepartment = 56,

    /// <summary>
    /// Academy secure ages 16 to 19.
    /// </summary>
    [Description("Academy secure 16 to 19")]
    AcademySecure16To19 = 57,
}
