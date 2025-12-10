namespace Dfe.SignIn.Core.Entities.Directories;

#pragma warning disable CS1591
public partial class UserEntity
{
    public Guid Sub { get; set; }

    public string Email { get; set; } = null!;

    public string GivenName { get; set; } = null!;

    public string FamilyName { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public short Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? PhoneNumber { get; set; }

    public DateTime? LastLogin { get; set; }

    public bool IsMigrated { get; set; }

    public string? JobTitle { get; set; }

    public bool PasswordResetRequired { get; set; }

    public DateTime? PrevLogin { get; set; }

    public bool IsEntra { get; set; }

    public Guid? EntraOid { get; set; }

    public DateTime? EntraLinked { get; set; }

    public bool IsInternalUser { get; set; }

    public DateTime? EntraDeferUntil { get; set; }

    public bool IsTestUser { get; set; }

    public virtual ICollection<UserPasswordPolicyEntity> UserPasswordPolicies { get; set; } = [];

    public virtual ICollection<UserStatusChangeReasonEntity> UserStatusChangeReasons { get; set; } = [];
}
#pragma warning restore CS1591

