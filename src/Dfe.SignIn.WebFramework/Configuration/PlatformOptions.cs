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
    public Uri SurveyUrl { get; set; } = new Uri("https://survey.localhost");

    /// <summary>
    /// Gets URL of the "Help" frontend component.
    /// </summary>
    public Uri HelpUrl { get; set; } = new Uri("http://localhost:5012");

    /// <summary>
    /// Gets URL of the "Manage" frontend component.
    /// </summary>
    public Uri ManageUrl { get; set; } = new Uri("https://manage.localhost");

    /// <summary>
    /// Gets URL of the "Profile" frontend component.
    /// </summary>
    public Uri ProfileUrl { get; set; } = new Uri("https://profile.localhost");

    /// <summary>
    /// Gets URL of the "Services" frontend component.
    /// </summary>
    public Uri ServicesUrl { get; set; } = new Uri("https://services.localhost");

    /// <summary>
    /// Gets URL of the "Support" frontend component.
    /// </summary>
    public Uri SupportUrl { get; set; } = new Uri("https://support.localhost");

    /// <summary>
    /// Gets URL of the "Cookies" page.
    /// </summary>
    public Uri CookiesUrl { get; set; } = new Uri("http://localhost:5012/cookies");

    /// <summary>
    /// Gets URL of the "Terms and conditions" page.
    /// </summary>
    public Uri TermsUrl { get; set; } = new Uri("http://localhost:5012/terms");

    /// <summary>
    /// Gets URL of the "Privacy notice" page.
    /// </summary>
    public Uri PrivacyUrl { get; set; } = new Uri("http://localhost:5012/privacy");

    /// <summary>
    /// Gets URL of the "Accessibility statement" page.
    /// </summary>
    public Uri AccessibilityUrl { get; set; } = new Uri("https://help.localhost/accessibility-statement");

    /// <summary>
    /// Gets URL of the "Contact us" page.
    /// </summary>
    public Uri ContactUrl { get; set; } = new Uri("http://localhost:5012/contact-us");

    /// <summary>
    /// Gets the name of the current application environment.
    /// </summary>
    public EnvironmentName EnvironmentName { get; set; } = EnvironmentName.Local;

    /// <inheritdoc/>
    PlatformOptions IOptions<PlatformOptions>.Value => this;
}
