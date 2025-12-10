using Dfe.SignIn.Core.Entities.Directories;
using Microsoft.EntityFrameworkCore;

namespace Dfe.SignIn.Gateways.EntityFramework;

#pragma warning disable CS1591
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
        modelBuilder.Entity<InvitationEntity>(entity => {
            entity.HasKey(e => e.Id).HasName("PK_Invitation");

            entity.ToTable("invitation");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.ApproverEmail)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("approverEmail");
            entity.Property(e => e.Code)
                .HasMaxLength(15)
                .IsUnicode(false)
                .HasColumnName("code");
            entity.Property(e => e.CodeMetaData)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("codeMetaData");
            entity.Property(e => e.Completed).HasColumnName("completed");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Deactivated).HasColumnName("deactivated");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.FirstName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("firstName");
            entity.Property(e => e.IsApprover).HasColumnName("isApprover");
            entity.Property(e => e.IsMigrated).HasColumnName("isMigrated");
            entity.Property(e => e.LastName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("lastName");
            entity.Property(e => e.OrgName)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("orgName");
            entity.Property(e => e.OriginClientId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("originClientId");
            entity.Property(e => e.OriginRedirectUri)
                .HasMaxLength(1024)
                .IsUnicode(false)
                .HasColumnName("originRedirectUri");
            entity.Property(e => e.OverrideBody).HasColumnName("overrideBody");
            entity.Property(e => e.OverrideSubject)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("overrideSubject");
            entity.Property(e => e.PreviousPassword)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("previousPassword");
            entity.Property(e => e.PreviousSalt)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("previousSalt");
            entity.Property(e => e.PreviousUsername)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("previousUsername");
            entity.Property(e => e.Reason)
                .IsUnicode(false)
                .HasColumnName("reason");
            entity.Property(e => e.SelfStarted).HasColumnName("selfStarted");
            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });

        modelBuilder.Entity<InvitationCallbackEntity>(entity => {
            entity.HasKey(e => new { e.InvitationId, e.SourceId }).HasName("PK_InvitationCallback");

            entity.ToTable("invitation_callback");

            entity.Property(e => e.InvitationId).HasColumnName("invitationId");
            entity.Property(e => e.SourceId)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("sourceId");
            entity.Property(e => e.CallbackUrl)
                .HasMaxLength(1024)
                .IsUnicode(false)
                .HasColumnName("callbackUrl");
            entity.Property(e => e.ClientId)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("clientId");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

            entity.HasOne(d => d.Invitation).WithMany(p => p.InvitationCallbacks)
                .HasForeignKey(d => d.InvitationId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_InvitationCallback_Invitation");
        });

        modelBuilder.Entity<PasswordHistoryEntity>(entity => {
            entity.HasKey(e => e.Id).HasName("PK_user_password_history");

            entity.ToTable("password_history");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Password)
                .HasMaxLength(5000)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.Salt)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("salt");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });

        modelBuilder.Entity<UserEntity>(entity => {
            entity.HasKey(e => e.Sub).HasName("PK__user__DDDF3AD9CA56D5BF");

            entity.ToTable("user");

            entity.HasIndex(e => e.EntraOid, "IDX__user__entra_oid__unique")
                .IsUnique()
                .HasFilter("([entra_oid] IS NOT NULL)");

            entity.Property(e => e.Sub)
                .ValueGeneratedNever()
                .HasColumnName("sub");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .HasColumnName("email");
            entity.Property(e => e.EntraDeferUntil)
                .HasColumnType("datetime")
                .HasColumnName("entra_defer_until");
            entity.Property(e => e.EntraLinked)
                .HasColumnType("datetime")
                .HasColumnName("entra_linked");
            entity.Property(e => e.EntraOid).HasColumnName("entra_oid");
            entity.Property(e => e.FamilyName)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("family_name");
            entity.Property(e => e.GivenName)
                .HasMaxLength(255)
                .HasColumnName("given_name");
            entity.Property(e => e.IsEntra).HasColumnName("is_entra");
            entity.Property(e => e.IsInternalUser).HasColumnName("is_internal_user");
            entity.Property(e => e.IsMigrated).HasColumnName("isMigrated");
            entity.Property(e => e.IsTestUser).HasColumnName("is_test_user");
            entity.Property(e => e.JobTitle)
                .HasMaxLength(255)
                .HasColumnName("job_title");
            entity.Property(e => e.LastLogin)
                .HasColumnType("datetime")
                .HasColumnName("last_login");
            entity.Property(e => e.Password)
                .HasMaxLength(5000)
                .IsUnicode(false)
                .HasColumnName("password");
            entity.Property(e => e.PasswordResetRequired).HasColumnName("password_reset_required");
            entity.Property(e => e.PhoneNumber)
                .HasMaxLength(50)
                .IsUnicode(false)
                .HasColumnName("phone_number");
            entity.Property(e => e.PrevLogin)
                .HasColumnType("datetime")
                .HasColumnName("prev_login");
            entity.Property(e => e.Salt)
                .HasMaxLength(500)
                .IsUnicode(false)
                .HasColumnName("salt");
            entity.Property(e => e.Status).HasColumnName("status");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });

        modelBuilder.Entity<UserCodeEntity>(entity => {
            entity.HasKey(e => new { e.Uid, e.CodeType }).HasName("PK__user_cod__E4DCEC89DE598738");

            entity.ToTable("user_code");

            entity.HasIndex(e => e.Email, "idx_user_code_email");

            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.CodeType)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasDefaultValue("PasswordReset")
                .HasColumnName("codeType");
            entity.Property(e => e.ClientId)
                .HasMaxLength(255)
                .HasColumnName("clientId");
            entity.Property(e => e.Code)
                .HasMaxLength(255)
                .HasColumnName("code");
            entity.Property(e => e.ContextData)
                .HasMaxLength(5000)
                .IsUnicode(false)
                .HasColumnName("contextData");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.Email)
                .HasMaxLength(255)
                .IsUnicode(false)
                .HasColumnName("email");
            entity.Property(e => e.RedirectUri)
                .HasMaxLength(255)
                .HasColumnName("redirectUri");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });

        modelBuilder.Entity<UserPasswordHistoryEntity>(entity => {
            entity.HasKey(e => new { e.PasswordHistoryId, e.UserSub }).HasName("ck_user_password_history");

            entity.ToTable("user_password_history");

            entity.Property(e => e.PasswordHistoryId).HasColumnName("passwordHistoryId");
            entity.Property(e => e.UserSub).HasColumnName("userSub");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");
        });

        modelBuilder.Entity<UserPasswordPolicyEntity>(entity => {
            entity.HasKey(e => e.Id).HasName("PK_UserPasswordPolicy");

            entity.ToTable("user_password_policy");

            entity.Property(e => e.Id)
                .ValueGeneratedNever()
                .HasColumnName("id");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.PasswordHistoryLimit)
                .HasDefaultValue((short)3)
                .HasColumnName("password_history_limit");
            entity.Property(e => e.PolicyCode)
                .HasMaxLength(255)
                .HasColumnName("policyCode");
            entity.Property(e => e.Uid).HasColumnName("uid");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

            entity.HasOne(d => d.UidNavigation).WithMany(p => p.UserPasswordPolicies)
                .HasForeignKey(d => d.Uid)
                .HasConstraintName("FK__user_passwo__uid__72E607DB");
        });

        modelBuilder.Entity<UserStatusChangeReasonEntity>(entity => {
            entity.HasKey(e => new { e.Id, e.UserId }).HasName("PK_UserStatusChangeReasons");

            entity.ToTable("user_status_change_reasons");

            entity.Property(e => e.Id).HasColumnName("id");
            entity.Property(e => e.UserId).HasColumnName("user_id");
            entity.Property(e => e.CreatedAt).HasColumnName("createdAt");
            entity.Property(e => e.NewStatus).HasColumnName("new_status");
            entity.Property(e => e.OldStatus).HasColumnName("old_status");
            entity.Property(e => e.Reason)
                .HasMaxLength(5000)
                .IsUnicode(false)
                .HasColumnName("reason");
            entity.Property(e => e.UpdatedAt).HasColumnName("updatedAt");

            entity.HasOne(d => d.User).WithMany(p => p.UserStatusChangeReasons)
                .HasForeignKey(d => d.UserId)
                .OnDelete(DeleteBehavior.ClientSetNull)
                .HasConstraintName("FK_UserStatusChangeReasons_User");
        });

        this.OnModelCreatingPartial(modelBuilder);
    }

    partial void OnModelCreatingPartial(ModelBuilder modelBuilder);
}
#pragma warning restore CS1591

