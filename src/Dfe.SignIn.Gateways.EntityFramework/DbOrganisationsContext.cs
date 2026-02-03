using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Gateways.EntityFramework.Configuration.Organisations;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class DbOrganisationsContext : DbContext
{
    public DbOrganisationsContext(DbContextOptions<DbOrganisationsContext> options)
        : base(options)
    {
    }

    public virtual DbSet<CognitiveSearchEntity> CognitiveSearches { get; set; }

    public virtual DbSet<GrantEntity> Grants { get; set; }

    public virtual DbSet<InvitationOrganisationEntity> InvitationOrganisations { get; set; }

    public virtual DbSet<InvitationServiceEntity> InvitationServices { get; set; }

    public virtual DbSet<InvitationServiceIdentifierEntity> InvitationServiceIdentifiers { get; set; }

    public virtual DbSet<InvitationServiceRoleEntity> InvitationServiceRoles { get; set; }

    public virtual DbSet<OrganisationEntity> Organisations { get; set; }

    public virtual DbSet<OrganisationAnnouncementEntity> OrganisationAnnouncements { get; set; }

    public virtual DbSet<OrganisationAssociationEntity> OrganisationAssociations { get; set; }

    public virtual DbSet<OrganisationPpEntity> OrganisationPps { get; set; }

    public virtual DbSet<PolicyEntity> Policies { get; set; }

    public virtual DbSet<PolicyConditionEntity> PolicyConditions { get; set; }

    public virtual DbSet<PolicyRoleEntity> PolicyRoles { get; set; }

    public virtual DbSet<PpAuditEntity> PpAudits { get; set; }

    public virtual DbSet<PpCacheEntity> PpCaches { get; set; }

    public virtual DbSet<PpOrgAssoCacheEntity> PpOrgAssoCaches { get; set; }

    public virtual DbSet<RoleEntity> Roles { get; set; }

    public virtual DbSet<ServiceEntity> Services { get; set; }

    public virtual DbSet<ServiceAssertionEntity> ServiceAssertions { get; set; }

    public virtual DbSet<ServiceBannerEntity> ServiceBanners { get; set; }

    public virtual DbSet<ServiceGrantTypeEntity> ServiceGrantTypes { get; set; }

    public virtual DbSet<ServiceParamEntity> ServiceParams { get; set; }

    public virtual DbSet<ServicePostLogoutRedirectUriEntity> ServicePostLogoutRedirectUris { get; set; }

    public virtual DbSet<ServiceRedirectUriEntity> ServiceRedirectUris { get; set; }

    public virtual DbSet<ServiceResponseTypeEntity> ServiceResponseTypes { get; set; }

    public virtual DbSet<ToggleFlagEntity> ToggleFlags { get; set; }

    public virtual DbSet<TokenEntity> Tokens { get; set; }

    public virtual DbSet<UserBannerEntity> UserBanners { get; set; }

    public virtual DbSet<UserOrganisationEntity> UserOrganisations { get; set; }

    public virtual DbSet<UserOrganisationRequestEntity> UserOrganisationRequests { get; set; }

    public virtual DbSet<UserServiceEntity> UserServices { get; set; }

    public virtual DbSet<UserServiceIdentifierEntity> UserServiceIdentifiers { get; set; }

    public virtual DbSet<UserServiceRequestEntity> UserServiceRequests { get; set; }

    public virtual DbSet<UserServiceRoleEntity> UserServiceRoles { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new CognitiveSearchEntityConfiguration());
        modelBuilder.ApplyConfiguration(new GrantEntityConfiguration());
        modelBuilder.ApplyConfiguration(new InvitationOrganisationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new InvitationServiceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new InvitationServiceIdentifierEntityConfiguration());
        modelBuilder.ApplyConfiguration(new InvitationServiceRoleEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OrganisationAnnouncementEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OrganisationAssociationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OrganisationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new OrganisationPpEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PolicyConditionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PolicyEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PolicyRoleEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PpAuditEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PpCacheEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PpOrgAssoCacheEntityConfiguration());
        modelBuilder.ApplyConfiguration(new RoleEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceAssertionEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceBannerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceGrantTypeEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceParamEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ServicePostLogoutRedirectUriEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceRedirectUriEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ServiceResponseTypeEntityConfiguration());
        modelBuilder.ApplyConfiguration(new ToggleFlagEntityConfiguration());
        modelBuilder.ApplyConfiguration(new TokenEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserBannerEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserOrganisationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserOrganisationRequestEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserServiceEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserServiceIdentifierEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserServiceRequestEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserServiceRoleEntityConfiguration());

        modelBuilder.HasSequence("numeric_id_sequence")
            .StartsAt(50000L)
            .HasMin(50000L);

        modelBuilder.HasSequence("org_legacy_id_sequence")
            .StartsAt(4000000L)
            .HasMin(4000000L);

        modelBuilder.HasSequence("role_numeric_id_sequence")
            .StartsAt(20000L)
            .HasMin(20000L);
    }
}
#pragma warning restore CS1591

