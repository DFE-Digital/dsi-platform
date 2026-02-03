using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.PublicApi;
using Dfe.SignIn.Core.UseCases.PublicApi;
using Microsoft.Extensions.Options;

namespace Dfe.SignIn.Core.UseCases.UnitTests.PublicApi;

public static class PublicApiHelpers
{
    public static EncryptApiSecretUseCase CreateEncryptionUseCase(string key)
    {
        var options = Options.Create(new ApiSecretEncryptionOptions {
            Key = key
        });

        return new EncryptApiSecretUseCase(options);
    }

    public static InteractionContext<EncryptPublicApiSecretRequest> CreateEncryptionInteractionContext(string apiSecret)
    {
        return new InteractionContext<EncryptPublicApiSecretRequest>(
            new EncryptPublicApiSecretRequest {
                ApiSecret = apiSecret
            }
        );
    }

    public static DecryptApiSecretUseCase CreateDecryptionUseCase(string key)
    {
        var options = Options.Create(new ApiSecretEncryptionOptions {
            Key = key
        });

        return new DecryptApiSecretUseCase(options);
    }

    public static InteractionContext<DecryptPublicApiSecretRequest> CreateDecryptInteractionContext(string encrypted)
    {
        return new InteractionContext<DecryptPublicApiSecretRequest>(
            new DecryptPublicApiSecretRequest {
                EncryptedApiSecret = encrypted
            }
        );
    }
}
