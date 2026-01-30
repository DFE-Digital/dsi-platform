using System.Text;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.PublicApi;

/// <summary>
/// Configuration options for encrypting/decrypting ApiSecrets.
/// </summary>
public sealed class ApiSecretEncryptionOptions : IOptions<ApiSecretEncryptionOptions>
{
    /// <summary>
    /// The encryption key used for encrypting and decrypting ApiSecrets.
    /// </summary>
    public required string EncryptionKey { get; set; }

    /// <summary>
    /// The encryption key as a byte array.
    /// </summary>
    public byte[] EncryptionKeyBytes => Encoding.UTF8.GetBytes(this.EncryptionKey);

    ApiSecretEncryptionOptions IOptions<ApiSecretEncryptionOptions>.Value => this;
}
