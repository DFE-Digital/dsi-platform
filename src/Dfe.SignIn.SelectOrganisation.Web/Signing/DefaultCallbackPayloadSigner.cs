using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.SelectOrganisation.Web.Signing;

/// <summary>
/// A service that can create a digital signature from callback payload data.
/// </summary>
public sealed class DefaultCallbackPayloadSigner(
    IOptions<DefaultCallbackPayloadSignerOptions> optionsAccessor
) : ICallbackPayloadSigner
{
    /// <inheritdoc/>
    public CallbackPayloadDigitalSignature CreateDigitalSignature(string data)
    {
        var options = optionsAccessor.Value;

        using var key = RSA.Create();
        key.ImportFromPem(options.PrivateKeyPem);

        var signatureBytes = key.SignData(Encoding.UTF8.GetBytes(data), options.Algorithm, options.Padding);
        return new CallbackPayloadDigitalSignature {
            KeyId = options.KeyId,
            Signature = Convert.ToBase64String(signatureBytes),
        };
    }
}
