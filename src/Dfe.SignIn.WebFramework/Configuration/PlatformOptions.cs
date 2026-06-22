using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Configuration;

/// <summary>
/// Options for the application.
/// </summary>
[SuppressMessage("csharpsquid", "S1075",
    Justification = "Default URLs configuration for running locally on a development machine."
)]
public sealed class PlatformOptions : IOptions<PlatformOptions>
{
    /// <summary>
    /// Gets URL of the survey that the user can use to provide feedback.
    /// </summary>
    /// 
    [Required, Url]
    public Uri? SurveyUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Help" frontend component.
    /// </summary>
    [Required, Url]
    public Uri? HelpUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Manage" frontend component.
    /// </summary>
    public Uri? ManageUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Profile" frontend component.
    /// </summary>
    [Required, Url]
    public Uri? ProfileUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Services" frontend component.
    /// </summary>
    [Required, Url]
    public Uri? ServicesUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Support" frontend component.
    /// </summary>
    public Uri? SupportUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Cookies" page.
    /// </summary>
    [Required, Url]
    public Uri? CookiesUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Terms and conditions" page.
    /// </summary>
    [Required, Url]
    public Uri? TermsUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Privacy notice" page.
    /// </summary>
    [Required, Url]
    public Uri? PrivacyUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Accessibility statement" page.
    /// </summary>
    [Required, Url]
    public Uri? AccessibilityUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Contact us" page.
    /// </summary>
    [Required, Url]
    public Uri? ContactUrl { get; set; }

    /// <summary>
    /// Gets the name of the current application environment.
    /// </summary>
    public EnvironmentName EnvironmentName { get; set; } = EnvironmentName.Local;

    /// <inheritdoc/>
    PlatformOptions IOptions<PlatformOptions>.Value => this;
}
