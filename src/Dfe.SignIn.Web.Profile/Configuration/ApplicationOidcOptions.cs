using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Web.Profile.Configuration;

/// <summary>
/// Options for the profile application.
/// </summary>
public sealed class ApplicationOidcOptions : IOptions<ApplicationOidcOptions>
{
    /// <summary>
    /// Gets the client ID of the application.
    /// </summary>
    public required string ClientId { get; set; }

    /// <inheritdoc/>
    ApplicationOidcOptions IOptions<ApplicationOidcOptions>.Value => this;
}
