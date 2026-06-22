using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.WebFramework.AppConfiguration;

/// <summary>
/// Options for pulling in the ExternalIdConfiguration
/// </summary>
public class ExternalIdConfiguration
{
    /// <summary>
    /// ClientId for the external configuration
    /// </summary>
    [Required]
    public string? ClientId { get; set; }

    /// <summary>
    /// Secret for the external configuration
    /// </summary>
    [Required]
    public string? ClientSecret { get; set; }

    /// <summary>
    /// Instance name for the external configuration
    /// </summary>
    [Required]
    public string? Instance { get; set; }

    /// <summary>
    /// Hosting tenant for the external configuration
    /// </summary>
    [Required]
    public string? TenantId { get; set; }

    /// <summary>
    /// Authority for the external configuration
    /// </summary>
    [Required]
    public string? Authority { get; set; }
}
