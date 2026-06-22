using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.WebFramework.AppConfiguration;

/// <summary>
/// Options for referencing the location of the azure app configurations service
/// </summary>
public class AzureAppConfig
{
    /// <summary>
    /// Uri resource for pulling configuration.
    /// </summary>
    [Required]
    public string? Endpoint { get; set; }
}
