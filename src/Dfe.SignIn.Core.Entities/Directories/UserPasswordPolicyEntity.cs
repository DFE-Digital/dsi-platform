namespace Dfe.SignIn.Core.Entities.Directories;

#pragma warning disable CS1591
public partial class UserPasswordPolicyEntity
{
    public Guid Id { get; set; }

    public Guid Uid { get; set; }

    public string PolicyCode { get; set; } = null!;

    public DateTimeOffset CreatedAt { get; set; }

    public DateTimeOffset UpdatedAt { get; set; }

    public short? PasswordHistoryLimit { get; set; }

    public virtual UserEntity UidNavigation { get; set; } = null!;
}
#pragma warning restore CS1591

