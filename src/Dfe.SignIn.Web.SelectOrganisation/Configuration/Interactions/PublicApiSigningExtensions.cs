using System.Security.Cryptography;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.UseCases.PublicApiSigning;

namespace Dfe.SignIn.Web.SelectOrganisation.Configuration.Interactions;

/// <exclude/>
public static class PublicApiSigningExtensions
{
    /// <summary>
    /// Gets a delegate for reading the configuration section.
    /// </summary>
    /// <param name="configuration">The configuration section.</param>
    /// <returns>
    ///   <para>A configuration reader delegate.</para>
    /// </returns>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="configuration"/> is null.</para>
    /// </exception>
    /// <exception cref="InvalidOperationException">
    ///   <para>If one or more required options are missing.</para>
    ///   <para>- or -</para>
    ///   <para>If one or more options are invalid.</para>
    /// </exception>
    public static Action<PublicApiSigningOptions> GetConfigurationReader(IConfiguration configuration)
    {
        ExceptionHelpers.ThrowIfArgumentNull(configuration, nameof(configuration));

        return (options) => {
            string? privateKeyPem = configuration.GetValue<string>("PrivateKeyPem");
            if (string.IsNullOrWhiteSpace(privateKeyPem)) {
                throw new InvalidOperationException("Missing required option 'PrivateKeyPem'.");
            }
            options.PrivateKeyPem = privateKeyPem;

            string? publicKeyId = configuration.GetValue<string>("PublicKeyId");
            if (string.IsNullOrWhiteSpace(publicKeyId)) {
                throw new InvalidOperationException("Missing required option 'PublicKeyId'.");
            }
            options.PublicKeyId = publicKeyId;

            string? algorithm = configuration.GetValue<string>("Algorithm");
            if (string.IsNullOrWhiteSpace(algorithm)) {
                throw new InvalidOperationException("Missing required option 'Algorithm'.");
            }
            options.Algorithm = new HashAlgorithmName(algorithm);

            string? paddingMode = configuration.GetValue<string>("Padding")?.ToLowerInvariant();
            if (paddingMode == "pkcs1") {
                options.Padding = RSASignaturePadding.Pkcs1;
            }
            else if (paddingMode == "pss") {
                options.Padding = RSASignaturePadding.Pss;
            }
            else {
                throw new InvalidOperationException("Invalid padding mode.");
            }
        };
    }

    /// <summary>
    /// Setup "public api signing" interactions.
    /// </summary>
    /// <param name="services">The collection to add services to.</param>
    /// <exception cref="ArgumentNullException">
    ///   <para>If <paramref name="services"/> is null.</para>
    /// </exception>
    public static void SetupPublicApiSigningInteractions(this IServiceCollection services)
    {
        ExceptionHelpers.ThrowIfArgumentNull(services, nameof(services));

        services.AddInteractor<CreateDigitalSignatureForPayload_UseCase>();
    }
}
