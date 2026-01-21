using System.Diagnostics.CodeAnalysis;

namespace Dfe.SignIn.Core.Entities.Organisations;

#pragma warning disable CS1591
[ExcludeFromCodeCoverage]
public partial class UserBannerEntity
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public int BannerId { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public string? BannerData { get; set; }
}
#pragma warning restore CS1591

