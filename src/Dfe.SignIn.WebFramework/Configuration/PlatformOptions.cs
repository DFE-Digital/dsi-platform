using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Configuration;

/// <summary>
/// Options for the application.
/// </summary>
public sealed class PlatformOptions : IOptions<PlatformOptions>
{
    /// <summary>
    /// Gets URL of the survey that the user can use to provide feedback.
    /// </summary>
    public Uri SurveyUrl { get; set; } = new Uri("https://survey.localhost");

    /// <summary>
    /// Gets URL of the "Help" frontend component.
    /// </summary>
    public Uri HelpUrl { get; set; } = new Uri("https://help.localhost");

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

    /// <inheritdoc/>
    PlatformOptions IOptions<PlatformOptions>.Value => this;
}
