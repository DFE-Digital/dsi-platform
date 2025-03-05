using System.Security.Cryptography;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.SelectOrganisation.Web.Signing;

/// <summary>
/// Options for <see cref="DefaultCallbackPayloadSigner"/.
/// </summary>
public sealed class DefaultCallbackPayloadSignerOptions : IOptions<DefaultCallbackPayloadSignerOptions>
{
    /// <summary>
    /// Gets or sets the private key.
    /// </summary>
    public required string PrivateKeyPem { get; set; }

    /// <summary>
    /// Gets or sets the unique key identifier.
    /// </summary>
    public required string KeyId { get; set; }

    /// <summary>
    /// Gets or sets the hash algorithm name.
    /// </summary>
    public required HashAlgorithmName Algorithm { get; set; }

    /// <summary>
    /// Gets or sets the RSA padding mode.
    /// </summary>
    public required RSASignaturePadding Padding { get; set; }

    /// <inheritdoc/>
    DefaultCallbackPayloadSignerOptions IOptions<DefaultCallbackPayloadSignerOptions>.Value => this;
}
