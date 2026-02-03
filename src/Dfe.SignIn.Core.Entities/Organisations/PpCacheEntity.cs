using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class PpCacheEntity
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

    public string? MasteringCode { get; set; }

    public string? ProviderProfileId { get; set; }

    public string? SourceSystem { get; set; }

    public string? Upin { get; set; }

    public string? ProviderTypeName { get; set; }

    public string? GiasproviderType { get; set; }

    public string? PimsproviderType { get; set; }

    public int? ProviderTypeCode { get; set; }

    public int? PimsproviderTypeCode { get; set; }

    public string? Pimsstatus { get; set; }

    public string? OpenedOn { get; set; }

    public string? DistrictAdministrativeName { get; set; }

    public string? DistrictAdministrativeCode { get; set; }

    public string? DistrictAdministrativeCode1 { get; set; }

    public DateTime? RefreshedAt { get; set; }

    public string? PimsstatusName { get; set; }

    public int? Giasstatus { get; set; }

    public string? GiasstatusName { get; set; }

    public int? MasterProviderStatusCode { get; set; }

    public string? MasterProviderStatusName { get; set; }

    public string? LegalName { get; set; }

    public string? IsOnApar { get; set; }
}
#pragma warning restore CS1591

