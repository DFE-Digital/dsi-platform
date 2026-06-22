using System.ComponentModel.DataAnnotations;

namespace Dfe.SignIn.WebFramework.AppConfiguration;

/// <summary>
/// Options for referencing frontend assets; for example, the design system .css and .js files.
/// </summary>
public class AssetsConfiguration
{
    /// <summary>
    /// Gets the assets base URL.
    /// </summary>
    /// <remarks>
    ///   <para>Defaults to a URL that is suitable for a local development environment
    ///   when using the <c>login.dfe.ui-toolkit</c> package.</para>
    /// </remarks>
    [Required, Url]
    public string? BaseAddress { get; set; }

    /// <summary>
    /// Gets the semantic version number of the frontend assets; eg. "1.2.3".
    /// </summary>
    [Required]
    public string? FrontendVersion { get; set; }
}
