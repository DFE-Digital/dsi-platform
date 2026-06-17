using Dfe.SignIn.Core.Entities.Directories;

namespace Dfe.SignIn.Core.Entities.Organisations;

/// <summary>
/// Represents the relationship between a user and a service, including status,
/// organisational context, and audit timestamps.
/// </summary>
public partial class UserServiceEntity
{
    /// <summary>
    /// Gets or sets the unique identifier for this user–service relationship.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Gets or sets the status of the user’s access to the service.
    /// Typically mapped to an enum in the domain layer.
    /// </summary>
    public short Status { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the user associated with this record.
    /// </summary>
    public Guid UserId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the organisation the user belongs to,
    /// if applicable.
    /// </summary>
    public Guid? OrganisationId { get; set; }

    /// <summary>
    /// Gets or sets the identifier of the service the user is linked to.
    /// </summary>
    public Guid? ServiceId { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this record was created.
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when this record was last updated.
    /// </summary>
    public DateTime UpdatedAt { get; set; }

    /// <summary>
    /// Gets or sets the timestamp when the user last accessed the service,
    /// if known.
    /// </summary>
    public DateTime? LastAccessed { get; set; }

    /// <summary>
    /// Gets or sets the organisation associated with this record.
    /// </summary>
    public virtual OrganisationEntity? Organisation { get; set; }

    /// <summary>
    /// Gets or sets the service associated with this record.
    /// </summary>
    public virtual ServiceEntity? Service { get; set; }

    /// <summary>
    /// Gets or sets the user associated with this record.
    /// </summary>
    public virtual UserEntity? User { get; set; }
}
