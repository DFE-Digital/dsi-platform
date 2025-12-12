using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Directories;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class UserStatusChangeReasonEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public short OldStatus { get; set; }

    public short NewStatus { get; set; }

    public string Reason { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual UserEntity User { get; set; } = null!;
}
#pragma warning restore CS1591

