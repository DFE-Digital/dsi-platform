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

    public static InteractionContext<EncryptApiSecretRequest> CreateEncryptionInteractionContext(string apiSecret)
    {
        return new InteractionContext<EncryptApiSecretRequest>(
            new EncryptApiSecretRequest {
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

    public static InteractionContext<DecryptApiSecretRequest> CreateDecryptInteractionContext(string encrypted)
    {
        return new InteractionContext<DecryptApiSecretRequest>(
            new DecryptApiSecretRequest {
                EncryptedApiSecret = encrypted
            }
        );
    }
}
