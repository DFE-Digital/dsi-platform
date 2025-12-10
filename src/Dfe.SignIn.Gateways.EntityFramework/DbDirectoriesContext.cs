using System.Diagnostics.CodeAnalysis;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework.Configuration.Directories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class DbDirectoriesContext : DbContext
{
    public DbDirectoriesContext(DbContextOptions<DbDirectoriesContext> options)
        : base(options)
    {
    }

    public virtual DbSet<InvitationEntity> Invitations { get; set; }

    public virtual DbSet<InvitationCallbackEntity> InvitationCallbacks { get; set; }

    public virtual DbSet<PasswordHistoryEntity> PasswordHistories { get; set; }

    public virtual DbSet<UserEntity> Users { get; set; }

    public virtual DbSet<UserCodeEntity> UserCodes { get; set; }

    public virtual DbSet<UserPasswordHistoryEntity> UserPasswordHistories { get; set; }

    public virtual DbSet<UserPasswordPolicyEntity> UserPasswordPolicies { get; set; }

    public virtual DbSet<UserStatusChangeReasonEntity> UserStatusChangeReasons { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfiguration(new UserEntityConfiguration());
        modelBuilder.ApplyConfiguration(new InvitationEntityConfiguration());
        modelBuilder.ApplyConfiguration(new InvitationCallbackEntityConfiguration());
        modelBuilder.ApplyConfiguration(new PasswordHistoryEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserCodeEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserPasswordHistoryEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserPasswordPolicyEntityConfiguration());
        modelBuilder.ApplyConfiguration(new UserStatusChangeReasonEntityConfiguration());
    }
}
#pragma warning restore CS1591

