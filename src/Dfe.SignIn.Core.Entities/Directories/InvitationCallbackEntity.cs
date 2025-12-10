namespace Dfe.SignIn.Core.Entities.Directories;

#pragma warning disable CS1591
public partial class InvitationCallbackEntity
{
    public Guid InvitationId { get; set; }

    public string SourceId { get; set; } = null!;

    public string CallbackUrl { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? ClientId { get; set; }

    public virtual InvitationEntity Invitation { get; set; } = null!;
}
#pragma warning restore CS1591

