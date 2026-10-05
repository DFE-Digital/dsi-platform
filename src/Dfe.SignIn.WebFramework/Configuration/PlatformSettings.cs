using Dfe.SignIn.WebFramework.Extensions;

namespace Dfe.SignIn.WebFramework.Configuration;

/// <summary>
/// Options for the application.
/// </summary>
public sealed record PlatformSettings : IApplicationSettings
{
    /// <summary>
    /// Gets the name of the configuration section for platform settings.
    /// </summary>
    static string IApplicationSettings.SectionName => "Platform";

    /// <summary>
    /// Gets URL of the survey that the user can use to provide feedback.
    /// </summary>
    public required Uri SurveyUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Help" frontend component.
    /// </summary>
    public required Uri HelpUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Manage" frontend component.
    /// </summary>
    public required Uri ManageUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Profile" frontend component.
    /// </summary>
    public required Uri ProfileUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Services" frontend component.
    /// </summary>
    public required Uri ServicesUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Support" frontend component.
    /// </summary>
    public required Uri SupportUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Cookies" page.
    /// </summary>
    public required Uri CookiesUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Terms and conditions" page.
    /// </summary>
    public required Uri TermsUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Privacy notice" page.
    /// </summary>
    public required Uri PrivacyUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Accessibility statement" page.
    /// </summary>
    public required Uri AccessibilityUrl { get; set; }

    /// <summary>
    /// Gets URL of the "Contact us" page.
    /// </summary>
    public required Uri ContactUrl { get; set; }

    /// <summary>
    /// Gets the name of the current application environment.
    /// </summary>
    public EnvironmentName EnvironmentName { get; set; } = EnvironmentName.Local;
}
