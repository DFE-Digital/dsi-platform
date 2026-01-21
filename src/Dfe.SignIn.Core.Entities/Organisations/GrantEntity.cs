using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class GrantEntity
{
    public Guid Id { get; set; }

    public string UserId { get; set; } = null!;

    public string Email { get; set; } = null!;

    public string Jti { get; set; } = null!;

    public Guid ServiceId { get; set; }

    public string Scope { get; set; } = null!;

    public Guid? OrganisationId { get; set; }

    public string? OrganisationName { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual ServiceEntity Service { get; set; } = null!;

    public virtual ICollection<TokenEntity> Tokens { get; set; } = [];
}
#pragma warning restore CS1591

