using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Options for the application.
/// </summary>
public sealed class ApplicationOptions : IOptions<ApplicationOptions>
{
    /// <inheritdoc/>
    ApplicationOptions IOptions<ApplicationOptions>.Value => this;
}
