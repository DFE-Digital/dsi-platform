namespace Dfe.SignIn.Core.Contracts.Users;

public class GetUserOrganisationService
{
    public Guid UserId { get; set; }
    public int UserStatus { get; set; }
    public string Email { get; set; }
    public string FamilyName { get; set; }
    public string GivenName { get; set; }
    public Guid OrganisationId { get; set; }
    public string OrganisationName { get; set; }
    public string? CategoryId { get; set; }
    //public string CategoryName { get; set; }

    public string? Urn { get; set; }
    public string? Uid { get; set; }
    public string? Ukprn { get; set; }
    public string? EstablishmentNumber { get; set; }

    public int StatusId { get; set; }
    //public string StatusName { get; set; }
    public DateOnly? ClosedOn { get; set; }

    public string? Address { get; set; }
    public string? Telephone { get; set; }

    public int? StatutoryLowAge { get; set; }
    public int? StatutoryHighAge { get; set; }

    public long? LegacyId { get; set; }
    public string? CompanyRegistrationNumber { get; set; }
    public string? ProviderProfileID { get; set; }
    public string? UPIN { get; set; }

    public string? PIMSProviderType { get; set; }
    public string? PIMSStatus { get; set; }
    public string? DistrictAdministrativeName { get; set; }

    public string? OpenedOn { get; set; }

    public string? SourceSystem { get; set; }
    public string? ProviderTypeName { get; set; }
    public string? GIASProviderType { get; set; }
    public int? PIMSProviderTypeCode { get; set; }

    public Guid ServiceId { get; set; }

    public string ServiceName { get; set; }
    public string? ServiceDescription { get; set; }
    public string RoleName { get; set; }
    public string RoleCode { get; set; }

    public short OrgRoleId { get; set; }
}
