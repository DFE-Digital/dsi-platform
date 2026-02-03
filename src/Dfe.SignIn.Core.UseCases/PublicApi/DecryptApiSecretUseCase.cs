using System.Text;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.PublicApi;

/// <summary>
/// Use case responsible for decrypting ApiSecrets that were previously
/// encrypted using the platform encryption scheme.
/// </summary>
/// <remarks>
/// This use case supports versioned encryption payloads prefixed with
/// <c>ENC:{version}:</c>. Currently, only version <c>0</c>
/// (AES-256-GCM V1) is supported.
///
/// If the provided value is null, empty, or does not appear
/// to be an encrypted value i.e. missing the expected prefix, the input is
/// returned unchanged.
/// </remarks>
/// <param name="optionsAccessor">
/// Provides access to <see cref="ApiSecretEncryptionOptions"/>, including
/// the encryption key required to decrypt API secrets.
/// </param>
public sealed class DecryptApiSecretUseCase(
    IOptions<ApiSecretEncryptionOptions> optionsAccessor
) : Interactor<DecryptPublicApiSecretRequest, DecryptedPublicApiSecretResponse>
{
    /// <inheritdoc/>
    public override Task<DecryptedPublicApiSecretResponse> InvokeAsync(
            InteractionContext<DecryptPublicApiSecretRequest> context,
            CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        if (!context.Request.EncryptedApiSecret.StartsWith("ENC:0:")) {
            return Task.FromResult(new DecryptedPublicApiSecretResponse {
                ApiSecret = context.Request.EncryptedApiSecret ?? string.Empty
            });
        }
        byte[] encryptedBase64Bytes = Convert.FromBase64String(context.Request.EncryptedApiSecret[6..]);
        var decrypted = AesGcmV1EncryptionProvider.Decrypt(optionsAccessor.Value.EncryptionKeyBytes, encryptedBase64Bytes);

        return Task.FromResult(new DecryptedPublicApiSecretResponse {
            ApiSecret = Encoding.UTF8.GetString(decrypted)
        });
    }
}
