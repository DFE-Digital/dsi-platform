using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class TokenEntity
{
    public Guid Id { get; set; }

    public Guid GrantId { get; set; }

    public Guid Sid { get; set; }

    public string Jti { get; set; } = null!;

    public string Kind { get; set; } = null!;

    public int? Exp { get; set; }

    public bool? Active { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual GrantEntity Grant { get; set; } = null!;
}
#pragma warning restore CS1591

