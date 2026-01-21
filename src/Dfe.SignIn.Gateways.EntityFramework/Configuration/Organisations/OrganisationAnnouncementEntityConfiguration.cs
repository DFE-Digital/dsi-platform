using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;

internal sealed class OrganisationAnnouncementEntityConfiguration : IEntityTypeConfiguration<OrganisationAnnouncementEntity>
{
    [ExcludeFromCodeCoverage]
    public void Configure(EntityTypeBuilder<OrganisationAnnouncementEntity> builder)
    {
        builder.ToTable("organisation_announcement");

        builder.HasKey(e => e.AnnouncementId).HasName("PK_OrganisationAnnoucement");

        builder.Property(e => e.AnnouncementId)
            .ValueGeneratedNever()
            .HasColumnName("announcement_id");

        builder.Property(e => e.Body).HasColumnName("body");

        builder.Property(e => e.CreatedAt).HasColumnName("createdAt");

        builder.Property(e => e.ExpiresAt).HasColumnName("expiresAt");

        builder.Property(e => e.OrganisationId).HasColumnName("organisation_id");

        builder.Property(e => e.OriginId)
            .HasMaxLength(125)
            .HasColumnName("origin_id");

        builder.Property(e => e.Published).HasColumnName("published");

        builder.Property(e => e.PublishedAt).HasColumnName("publishedAt");

        builder.Property(e => e.Summary)
            .HasMaxLength(340)
            .HasColumnName("summary");

        builder.Property(e => e.Title)
            .HasMaxLength(255)
            .HasColumnName("title");

        builder.Property(e => e.Type).HasColumnName("type");

        builder.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

        builder.HasOne(d => d.Organisation).WithMany(p => p.OrganisationAnnouncements)
            .HasForeignKey(d => d.OrganisationId)
            .OnDelete(DeleteBehavior.ClientSetNull)
            .HasConstraintName("FK_OrganisationAnnoucement_Organisation");
    }
}
