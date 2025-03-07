using System.Security.Cryptography;
using System.Text;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.Models.PublicApiSigning.Interactions;
using Dfe.SignIn.Core.PublicModels.PublicApiSigning;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.PublicApiSigning;

/// <summary>
/// Use case for creating a digital signature for a given payload.
/// </summary>
/// <param name="optionsAccessor">Provides access to digital signing options.</param>
public sealed class CreateDigitalSignatureForPayload_UseCase(
    IOptions<PublicApiSigningOptions> optionsAccessor
) : IInteractor<CreateDigitalSignatureForPayloadRequest, CreateDigitalSignatureForPayloadResponse>
{
    /// <inheritdoc/>
    public Task<CreateDigitalSignatureForPayloadResponse> InvokeAsync(
        CreateDigitalSignatureForPayloadRequest request)
    {
        var options = optionsAccessor.Value;

        using var key = RSA.Create();
        key.ImportFromPem(options.PrivateKeyPem);

        byte[] payloadBytes = Encoding.UTF8.GetBytes(request.Payload);
        byte[] signatureBytes = key.SignData(payloadBytes, options.Algorithm, options.Padding);
        string signatureBase64 = Convert.ToBase64String(signatureBytes);

        return Task.FromResult<CreateDigitalSignatureForPayloadResponse>(new() {
            Signature = new PayloadDigitalSignature {
                KeyId = options.PublicKeyId,
                Signature = signatureBase64,
            },
        });
    }
}
