using System.Security.Cryptography;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.UseCases.PublicApiSigning;

namespace Dfe.SignIn.SelectOrganisation.Web.Configuration.Interactions;

/// <exclude/>
public static class PublicApiSigningExtensions
{
    /// <summary>
    /// Setup "public api signing" interactions.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupPublicApiSigningInteractions(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services, nameof(services));

        SetupDigitalSigning(services);

        services.AddInteractor<CreateDigitalSignatureForPayload_UseCase>();
    }

    private static void SetupDigitalSigning(this IServiceCollection services)
    {
        services.Configure<PublicApiSigningOptions>(options => {
            using var rsa = new RSACryptoServiceProvider(2048);
            options.Algorithm = HashAlgorithmName.SHA256;
            options.PublicKeyId = "3605fbcf-7664-4e9f-aecc-a7d1ae7b175e";
            options.Padding = RSASignaturePadding.Pkcs1;
            options.PrivateKeyPem = rsa.ExportRSAPrivateKeyPem();
        });
    }
}
