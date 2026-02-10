using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class OrganisationEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Category { get; set; }

    public string? Type { get; set; }

    public string? Urn { get; set; }

    public string? Uid { get; set; }

    public string? Ukprn { get; set; }

    public string? EstablishmentNumber { get; set; }

    public int Status { get; set; }

    public DateOnly? ClosedOn { get; set; }

    public string? Address { get; set; }

    public int? PhaseOfEducation { get; set; }

    public int? StatutoryLowAge { get; set; }

    public int? StatutoryHighAge { get; set; }

    public string? Telephone { get; set; }

    public string? RegionCode { get; set; }

    public long? LegacyId { get; set; }

    public string? CompanyRegistrationNumber { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? DistrictAdministrativeName { get; set; }

    public string? MasteringCode { get; set; }

    public string? ProviderProfileId { get; set; }

    public string? SourceSystem { get; set; }

    public string? Upin { get; set; }

    public string? ProviderTypeName { get; set; }

    public string? GiasProviderType { get; set; }

    public string? PimsProviderType { get; set; }

    public int? ProviderTypeCode { get; set; }

    public int? PimsProviderTypeCode { get; set; }

    public string? PimsStatus { get; set; }

    public string? OpenedOn { get; set; }

    public string? DistrictAdministrativeName1 { get; set; }

    public string? DistrictAdministrativeCode { get; set; }

    public string? DistrictAdministrativeCode1 { get; set; }

    public string? PimsStatusName { get; set; }

    public int? GiasStatus { get; set; }

    public string? GiasStatusName { get; set; }

    public int? MasterProviderStatusCode { get; set; }

    public string? MasterProviderStatusName { get; set; }

    public string? LegalName { get; set; }

    public string? IsOnApar { get; set; }

    public Guid? LocalAuthorityId { get; set; }

    public string? LocalAuthorityCode { get; set; }

    public string? LocalAuthorityName { get; set; }

    public virtual ICollection<InvitationOrganisationEntity> InvitationOrganisations { get; set; } = [];

    public virtual ICollection<InvitationServiceEntity> InvitationServices { get; set; } = [];

    public virtual ICollection<OrganisationAnnouncementEntity> OrganisationAnnouncements { get; set; } = [];

    public virtual ICollection<UserOrganisationRequestEntity> UserOrganisationRequests { get; set; } = [];

    public virtual ICollection<UserOrganisationEntity> UserOrganisations { get; set; } = [];

    public virtual ICollection<UserServiceRequestEntity> UserServiceRequests { get; set; } = [];

    public virtual ICollection<UserServiceEntity> UserServices { get; set; } = [];
}
#pragma warning restore CS1591

