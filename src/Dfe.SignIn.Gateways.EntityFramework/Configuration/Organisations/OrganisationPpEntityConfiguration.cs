using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class OrganisationPpEntityConfiguration : IEntityTypeConfiguration<OrganisationPpEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<OrganisationPpEntity> builder)
    {
        builder.ToTable("organisation_pp");

        builder.HasKey(e => e.Id).HasName("PK__organisa__3213E83F1B1668BC");

        builder.Property(e => e.Id)
            .ValueGeneratedNever()
            .HasColumnName("id");

        builder.Property(e => e.Category)
            .HasMaxLength(25)
            .IsUnicode(false);

        builder.Property(e => e.CompanyRegistrationNumber)
            .HasMaxLength(50)
            .IsUnicode(false)
            .HasColumnName("companyRegistrationNumber");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.DistrictAdministrativeCode)
            .HasMaxLength(500)
            .HasColumnName("DistrictAdministrative_code");

        builder.Property(e => e.DistrictAdministrativeCode1)
            .HasMaxLength(100)
            .HasColumnName("DistrictAdministrativeCode");

        builder.Property(e => e.DistrictAdministrativeName).HasMaxLength(500);

        builder.Property(e => e.EstablishmentNumber)
            .HasMaxLength(25)
            .IsUnicode(false);

        builder.Property(e => e.GiasproviderType)
            .HasMaxLength(100)
            .HasColumnName("GIASProviderType");

        builder.Property(e => e.Giasstatus).HasColumnName("GIASStatus");

        builder.Property(e => e.GiasstatusName)
            .HasMaxLength(50)
            .HasColumnName("GIASStatusName");

        builder.Property(e => e.LegacyId).HasColumnName("legacyId");

        builder.Property(e => e.LocalAuthorityCode)
            .HasMaxLength(25)
            .IsUnicode(false)
            .HasColumnName("localAuthorityCode");

        builder.Property(e => e.LocalAuthorityId).HasColumnName("localAuthorityId");

        builder.Property(e => e.LocalAuthorityName)
            .IsUnicode(false)
            .HasColumnName("localAuthorityName");

        builder.Property(e => e.MasterProviderStatusName).HasMaxLength(50);

        builder.Property(e => e.MasteringCode)
            .HasMaxLength(50)
            .HasColumnName("masteringCode");

        builder.Property(e => e.Name).HasColumnName("name");

        builder.Property(e => e.OpenedOn).HasMaxLength(100);

        builder.Property(e => e.PhaseOfEducation).HasColumnName("phaseOfEducation");

        builder.Property(e => e.PimsproviderType)
            .HasMaxLength(100)
            .HasColumnName("PIMSProviderType");

        builder.Property(e => e.PimsproviderTypeCode).HasColumnName("PIMSProviderTypeCode");

        builder.Property(e => e.Pimsstatus).HasColumnName("PIMSStatus");

        builder.Property(e => e.PimsstatusName)
            .HasMaxLength(50)
            .HasColumnName("PIMSStatusName");

        builder.Property(e => e.ProviderProfileId)
            .HasMaxLength(100)
            .HasColumnName("ProviderProfileID");

        builder.Property(e => e.ProviderTypeName).HasMaxLength(500);

        builder.Property(e => e.RegionCode)
            .HasMaxLength(1)
            .IsUnicode(false)
            .IsFixedLength()
            .HasColumnName("regionCode");

        builder.Property(e => e.SourceSystem).HasMaxLength(100);

        builder.Property(e => e.Status).HasDefaultValue(1);

        builder.Property(e => e.StatutoryHighAge).HasColumnName("statutoryHighAge");

        builder.Property(e => e.StatutoryLowAge).HasColumnName("statutoryLowAge");

        builder.Property(e => e.Telephone)
            .HasMaxLength(25)
            .IsUnicode(false)
            .HasColumnName("telephone");

        builder.Property(e => e.Type)
            .HasMaxLength(25)
            .IsUnicode(false);

        builder.Property(e => e.Uid)
            .HasMaxLength(25)
            .IsUnicode(false)
            .HasColumnName("UID");

        builder.Property(e => e.Ukprn)
            .HasMaxLength(25)
            .IsUnicode(false)
            .HasColumnName("UKPRN");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.Property(e => e.Upin)
            .HasMaxLength(100)
            .HasColumnName("UPIN");

        builder.Property(e => e.Urn)
            .HasMaxLength(25)
            .IsUnicode(false)
            .HasColumnName("URN");
    }
}
