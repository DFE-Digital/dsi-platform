using Microsoft.Extensions.Options;

namespace Dfe.SignIn.PublicApi.Client;

/// <summary>
/// Options for the DfE Sign-in Public API.
/// </summary>
public sealed class DfePublicApiOptions : IOptions<DfePublicApiOptions>
{
    /// <summary>
    /// Gets or sets the base address of the DfE Sign-in Public API.
    /// </summary>
    public Uri BaseAddress { get; set; } = new Uri("https://api.signin.education.gov.uk");

    /// <summary>
    /// Gets or sets the service audience.
    /// </summary>
    /// <remarks>
    ///   <para>This is required to authenticate with the DfE Sign-in public API.</para>
    /// </remarks>
    public string Audience { get; set; } = "signin.education.gov.uk";

    /// <summary>
    /// Gets or sets the client ID.
    /// </summary>
    /// <remarks>
    ///   <para>This is required to authenticate with the DfE Sign-in public API.</para>
    /// </remarks>
    public required string ClientId { get; set; }

    /// <summary>
    /// Gets or sets the API secret.
    /// </summary>
    /// <remarks>
    ///   <para>This is required to authenticate with the DfE Sign-in public API.</para>
    /// </remarks>
    public required string ApiSecret { get; set; }

    /// <inheritdoc/>
    DfePublicApiOptions IOptions<DfePublicApiOptions>.Value => this;
}
