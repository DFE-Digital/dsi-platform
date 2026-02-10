using System.Text;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.PublicApi;

/// <summary>
/// Use case responsible for encrypting Public API secrets using the
/// platform’s standard encryption scheme.
/// </summary>
/// <remarks>
/// Secrets are encrypted using the current encryption version
/// (<c>ENC:0</c>, AES-256-GCM V1) and returned in a version-prefixed
/// format of <c>ENC:{version}:{base64Data}</c>.
/// </remarks>
/// <param name="optionsAccessor">
/// Provides access to <see cref="ApiSecretEncryptionOptions"/>, including
/// the encryption key used to encrypt API secrets.
/// </param>
public sealed class EncryptApiSecretUseCase(
    IOptions<ApiSecretEncryptionOptions> optionsAccessor
) : Interactor<EncryptApiSecretRequest, EncryptApiSecretResponse>
{
    /// <inheritdoc/>
    public override Task<EncryptApiSecretResponse> InvokeAsync(
            InteractionContext<EncryptApiSecretRequest> context,
            CancellationToken cancellationToken = default)
    {
        context.ThrowIfHasValidationErrors();

        var encrypted = AesGcmV1EncryptionProvider.Encrypt(
            optionsAccessor.Value.EncryptionKeyBytes,
            Encoding.UTF8.GetBytes(context.Request.ApiSecret));

        var base64Encrypted = Convert.ToBase64String(encrypted);

        return Task.FromResult(new EncryptApiSecretResponse {
            EncryptedApiSecret = $"ENC:0:{base64Encrypted}"
        });
    }
}
