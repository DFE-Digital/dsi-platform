using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class ServiceEntity
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string? Description { get; set; }

    public string ClientId { get; set; } = null!;

    public string ClientSecret { get; set; } = null!;

    public string? ApiSecret { get; set; }

    public string? TokenEndpointAuthMethod { get; set; }

    public string? ServiceHome { get; set; }

    public string? PostResetUrl { get; set; }

    public bool IsExternalService { get; set; }

    public bool IsMigrated { get; set; }

    public Guid? ParentId { get; set; }

    public bool IsChildService { get; set; }

    public bool IsIdOnlyService { get; set; }

    public bool IsHiddenService { get; set; }

    public virtual ICollection<GrantEntity> Grants { get; set; } = [];

    public virtual ICollection<ServiceEntity> InverseParent { get; set; } = [];

    public virtual ICollection<InvitationServiceEntity> InvitationServices { get; set; } = [];

    public virtual ServiceEntity? Parent { get; set; }

    public virtual ICollection<ServiceAssertionEntity> ServiceAssertions { get; set; } = [];

    public virtual ICollection<ServiceBannerEntity> ServiceBanners { get; set; } = [];

    public virtual ICollection<ServiceGrantTypeEntity> ServiceGrantTypes { get; set; } = [];

    public virtual ICollection<ServiceParamEntity> ServiceParams { get; set; } = [];

    public virtual ICollection<ServicePostLogoutRedirectUriEntity> ServicePostLogoutRedirectUris { get; set; } = [];

    public virtual ICollection<ServiceRedirectUriEntity> ServiceRedirectUris { get; set; } = [];

    public virtual ICollection<ServiceResponseTypeEntity> ServiceResponseTypes { get; set; } = [];

    public virtual ICollection<UserServiceEntity> UserServices { get; set; } = [];
}
#pragma warning restore CS1591

