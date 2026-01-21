using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class OrganisationAnnouncementEntity
{
    public Guid AnnouncementId { get; set; }

    public string OriginId { get; set; } = null!;

    public Guid OrganisationId { get; set; }

    public int Type { get; set; }

    public string Title { get; set; } = null!;

    public string Summary { get; set; } = null!;

    public string Body { get; set; } = null!;

    public DateTime PublishedAt { get; set; }

    public DateTime? ExpiresAt { get; set; }

    public bool Published { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public virtual OrganisationEntity Organisation { get; set; } = null!;
}
#pragma warning restore CS1591

