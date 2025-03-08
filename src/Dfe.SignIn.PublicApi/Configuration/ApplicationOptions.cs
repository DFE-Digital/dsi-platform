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
#pragma warning disable JSON002 // Probable JSON string detected
    public string PublicKeysJson { get; set; } = """
        { "keys": [] }
    """;
#pragma warning restore JSON002 // Probable JSON string detected

    /// <inheritdoc/>
    ApplicationOptions IOptions<ApplicationOptions>.Value => this;
}
