using System.Security.Cryptography;
using System.Text;
using Dfe.SignIn.Core.ExternalModels.PublicApiSigning;

namespace Dfe.SignIn.PublicApi.Client.PublicApiSigning;

internal sealed class DefaultPayloadVerifier(
    IPublicKeyCache publicKeyCache
) : IPayloadVerifier
{
    /// <inheritdoc/>
    public async Task<bool> VerifyPayload(string data, PayloadDigitalSignature signature)
    {
        ArgumentNullException.ThrowIfNull(data, nameof(data));
        ArgumentNullException.ThrowIfNull(signature, nameof(signature));

        var cachedKey = await publicKeyCache.GetPublicKeyAsync(signature.KeyId);
        if (cachedKey is null)
        {
            return false;
        }

        var hashAlgorithmName = ResolveHashAlgorithmName(cachedKey.Key.Alg);

        byte[] dataBytes = Encoding.UTF8.GetBytes(data);
        byte[] signatureBytes = Convert.FromBase64String(signature.Signature);

        return cachedKey.RSA.VerifyData(
            dataBytes,
            signatureBytes,
            hashAlgorithmName,
            RSASignaturePadding.Pkcs1
        );
    }

    private static HashAlgorithmName ResolveHashAlgorithmName(string name)
    {
        return name.ToUpper() switch
        {
            "RS256" => HashAlgorithmName.SHA256,
            "RS384" => HashAlgorithmName.SHA384,
            "RS512" => HashAlgorithmName.SHA512,
            _ => throw new InvalidOperationException($"Unexpected hash algorithm '{name}'."),
        };
    }
}
