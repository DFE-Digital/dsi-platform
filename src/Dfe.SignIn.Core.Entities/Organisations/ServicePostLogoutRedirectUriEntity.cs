namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
public partial class ServicePostLogoutRedirectUriEntity
{
    public Guid ServiceId { get; set; }

    public string RedirectUrl { get; set; } = null!;

    public virtual ServiceEntity Service { get; set; } = null!;
}
#pragma warning restore CS1591

