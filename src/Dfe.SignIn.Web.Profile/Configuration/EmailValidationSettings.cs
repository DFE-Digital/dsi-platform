using Dfe.SignIn.WebFramework.Extensions;

namespace Dfe.SignIn.Web.Profile.Configuration;

/// <summary>
/// Settings for the optional email-address blocklist check in Profile.
/// </summary>
public sealed record EmailValidationSettings : IApplicationSettings
{
    /// <summary>
    /// Gets the name of the configuration section for email validation settings.
    /// </summary>
    static string IApplicationSettings.SectionName => "EmailValidation";

    /// <summary>
    /// Gets or sets whether to check submitted addresses against the blocklist.
    /// </summary>
    public bool Enabled { get; set; }
}
