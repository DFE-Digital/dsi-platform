namespace Dfe.SignIn.Core.Entities.Directories;

#pragma warning disable CS1591
public partial class UserCodeEntity
{
    public Guid Uid { get; set; }

    public string Code { get; set; } = null!;

    public string RedirectUri { get; set; } = null!;

    public string ClientId { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? Email { get; set; }

    public string CodeType { get; set; } = null!;

    public string? ContextData { get; set; }
}
#pragma warning restore CS1591

