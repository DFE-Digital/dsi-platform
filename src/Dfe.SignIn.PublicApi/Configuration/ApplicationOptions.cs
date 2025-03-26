using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Configuration;

/// <summary>
/// Options for the application.
/// </summary>
public sealed class ApplicationOptions : IOptions<ApplicationOptions>
{
    /// <summary>
    /// Gets or sets the JSON encoded list of public keys.
    /// </summary>
    public string PublicKeysJson { get; set; } = /*lang=json,strict*/ """
        { "keys": [] }
    """;

    /// <inheritdoc/>
    ApplicationOptions IOptions<ApplicationOptions>.Value => this;
}
