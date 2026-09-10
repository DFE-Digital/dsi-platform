using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.Gateways.Entra;

/// <summary>
/// Configuration options for connecting to Microsoft Entra ID with application credentials.
/// See README.md in this project for required settings and Azure AD permissions.
/// </summary>
public sealed class EntraApplicationSettings
{
    /// <summary>
    /// The name of the configuration section in appsettings.json for Entra application settings.
    /// </summary>
    public const string SectionName = "Entra";

    /// <summary>
    /// Gets or sets the tenant ID for the Entra application. This is used to identify the Azure AD tenant.
    /// </summary>
    [Required]
    public string TenantId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client ID for the Entra application. This is used to identify the application in Azure AD.
    /// </summary>
    [Required]
    public string ClientId { get; set; } = string.Empty;

    /// <summary>
    /// Gets or sets the client secret for the Entra application. This is used for authenticating the application with Azure AD.
    /// </summary>
    [Required]
    public string ClientSecret { get; set; } = string.Empty;
}
