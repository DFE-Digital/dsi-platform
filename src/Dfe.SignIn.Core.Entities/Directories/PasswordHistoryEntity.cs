namespace Dfe.SignIn.Core.Entities.Directories;

#pragma warning disable CS1591
public partial class PasswordHistoryEntity
{
    public Guid Id { get; set; }

    public string Password { get; set; } = null!;

    public string Salt { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }
}
#pragma warning restore CS1591

