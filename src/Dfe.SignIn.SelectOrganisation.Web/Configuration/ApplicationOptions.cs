using Microsoft.Extensions.Options;

namespace Dfe.SignIn.SelectOrganisation.Web.Configuration;

/// <summary>
/// Options for the application.
/// </summary>
public sealed class ApplicationOptions : IOptions<ApplicationOptions>
{
    /// <summary>
    /// Gets URL of the survey that the user can use to provide feedback.
    /// </summary>
    public Uri SurveyUrl { get; set; } = new Uri("https://survey.localhost");

    /// <summary>
    /// Gets URL of the "Services" frontend component.
    /// </summary>
    public Uri ServicesUrl { get; set; } = new Uri("https://services.localhost");

    /// <inheritdoc/>
    ApplicationOptions IOptions<ApplicationOptions>.Value => this;
}
