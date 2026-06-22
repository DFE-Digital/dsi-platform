using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.WebFramework.AppConfiguration;

/// <summary>
/// Internal Api Client Configuration object with property validation
/// </summary>
public class InternalApiClientConfiguration
{
    /// <summary>
    /// Gets the Internal Api Client base URL.
    /// </summary>
    [Required, Url]
    public string? BaseAddress { get; set; }

    /// <summary>
    /// Access resource details
    /// </summary>
    [Required]
    public InternalApiEndpoint? Access { get; set; }

    /// <summary>
    /// Applications resource details
    /// </summary>
    [Required]
    public InternalApiEndpoint? Applications { get; set; }

    /// <summary>
    /// Directories resource details
    /// </summary>
    [Required]
    public InternalApiEndpoint? Directories { get; set; }

    /// <summary>
    /// Organisations resource details
    /// </summary>
    [Required]
    public InternalApiEndpoint? Organisations { get; set; }

    /// <summary>
    /// Search resource details
    /// </summary>
    [Required]
    public InternalApiEndpoint? Search { get; set; }
}
