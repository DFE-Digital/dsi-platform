using Microsoft.Extensions.Options;

namespace Dfe.SignIn.WebFramework.Mvc.Configuration;

/// <summary>
/// Application options for integration with the DfE Sign-In identity provider.
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
