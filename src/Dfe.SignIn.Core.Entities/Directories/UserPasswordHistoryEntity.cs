namespace Dfe.SignIn.Core.Entities.Directories;

#pragma warning disable CS1591
public partial class UserPasswordHistoryEntity
{
    public Guid PasswordHistoryId { get; set; }

    public Guid UserSub { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
#pragma warning restore CS1591

