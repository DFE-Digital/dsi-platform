using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class CognitiveSearchEntityConfiguration : IEntityTypeConfiguration<CognitiveSearchEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<CognitiveSearchEntity> builder)
    {
        builder.ToTable("cognitive_search");

        builder.HasKey(e => e.Id).HasName("PK__cognitiv__3213E83F039ADE89");

        builder.Property(e => e.Id)
            .HasMaxLength(44)
            .IsUnicode(false)
            .HasColumnName("id");

        builder.Property(e => e.Email)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("email");

        builder.Property(e => e.FirstName)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("firstName");

        builder.Property(e => e.LastLogin)
            .HasColumnType("datetime")
            .HasColumnName("lastLogin");

        builder.Property(e => e.LastName)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("lastName");

        builder.Property(e => e.LegacyUsernames)
            .HasMaxLength(8000)
            .IsUnicode(false)
            .HasColumnName("legacyUsernames");

        builder.Property(e => e.NumberOfSuccessfulLoginsInPast12Months).HasColumnName("numberOfSuccessfulLoginsInPast12Months");

        builder.Property(e => e.OrganisationCategories).HasColumnName("organisationCategories");

        builder.Property(e => e.OrganisationIdentifiers).HasColumnName("organisationIdentifiers");

        builder.Property(e => e.Organisations).HasColumnName("organisations");

        builder.Property(e => e.OrganisationsJson).HasColumnName("organisationsJson");

        builder.Property(e => e.PendingEmail)
            .HasMaxLength(255)
            .IsUnicode(false)
            .HasColumnName("pendingEmail");

        builder.Property(e => e.PrimaryOrganisation).HasColumnName("primaryOrganisation");

        builder.Property(e => e.SearchableEmail)
            .HasMaxLength(8000)
            .IsUnicode(false)
            .HasColumnName("searchableEmail");

        builder.Property(e => e.SearchableName)
            .HasMaxLength(8000)
            .IsUnicode(false)
            .HasColumnName("searchableName");

        builder.Property(e => e.SearchableOrganisations).HasColumnName("searchableOrganisations");

        builder.Property(e => e.Services).HasColumnName("services");

        builder.Property(e => e.StatusId).HasColumnName("statusId");

        builder.Property(e => e.StatusLastChangedOn).HasColumnName("statusLastChangedOn");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
    }
}
