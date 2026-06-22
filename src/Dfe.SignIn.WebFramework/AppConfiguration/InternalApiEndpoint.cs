using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.WebFramework.AppConfiguration;

/// <summary>
/// Internal Api configuration with attribute validation
/// </summary>
public class InternalApiEndpoint
{
    /// <summary>
    /// Base resource address
    /// </summary>
    [Required, Url]
    public string? BaseAddress { get; set; }
}
