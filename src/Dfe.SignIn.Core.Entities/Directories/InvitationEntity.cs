using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Directories;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class InvitationEntity
{
    public Guid Id { get; set; }

    public string Email { get; set; } = null!;

    public string Code { get; set; } = null!;

    public string FirstName { get; set; } = null!;

    public string LastName { get; set; } = null!;

    public string? OriginClientId { get; set; }

    public string? OriginRedirectUri { get; set; }

    public bool SelfStarted { get; set; }

    public string? OverrideSubject { get; set; }

    public string? OverrideBody { get; set; }

    public string? PreviousUsername { get; set; }

    public string? PreviousPassword { get; set; }

    public string? PreviousSalt { get; set; }

    public bool Deactivated { get; set; }

    public string? Reason { get; set; }

    public bool Completed { get; set; }

    public Guid? Uid { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public bool IsMigrated { get; set; }

    public bool IsApprover { get; set; }

    public string? ApproverEmail { get; set; }

    public string? OrganisationName { get; set; }

    public string? CodeMetaData { get; set; }

    public virtual ICollection<InvitationCallbackEntity> InvitationCallbacks { get; set; } = [];
}
#pragma warning restore CS1591

