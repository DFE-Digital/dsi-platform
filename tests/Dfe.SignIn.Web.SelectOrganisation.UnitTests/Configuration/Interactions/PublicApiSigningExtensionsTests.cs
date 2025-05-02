using System.Security.Cryptography;
using Dfe.SignIn.Core.Framework;
using Dfe.SignIn.Core.InternalModels.PublicApiSigning.Interactions;
using Dfe.SignIn.Core.UseCases.PublicApiSigning;
using Dfe.SignIn.Web.SelectOrganisation.Configuration.Interactions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Web.SelectOrganisation.UnitTests.Configuration.Interactions;

[TestClass]
public sealed class PublicApiSigningExtensionsTests
{
    #region GetConfigurationReader(IConfiguration)

    private static IConfiguration CreateMockConfiguration(Dictionary<string, string?>? overrides = null)
    {
        var builder = new ConfigurationBuilder();

        builder.AddInMemoryCollection(new Dictionary<string, string?> {
            { "PrivateKeyPem", "example private key" },
            { "PublicKeyId", "4cd0e0f7-b30d-4969-848e-4295dd83c26b" },
            { "Algorithm", "SHA256" },
            { "Padding", "Pkcs1" },
        });

        if (overrides is not null) {
            builder.AddInMemoryCollection(overrides);
        }

        return builder.Build();
    }

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void GetConfigurationReader_Throws_WhenConfigurationArgumentIsNull()
    {
        PublicApiSigningExtensions.GetConfigurationReader(null!);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetConfigurationReader_Throws_WhenPrivateKeyPemOptionIsMissing()
    {
        var mockedConfiguration = CreateMockConfiguration(new Dictionary<string, string?> {
            { "PrivateKeyPem", "" },
        });

        var reader = PublicApiSigningExtensions.GetConfigurationReader(mockedConfiguration);
        var options = Activator.CreateInstance<PublicApiSigningOptions>();
        reader(options);
    }

    [TestMethod]
    public void GetConfigurationReader_AssignsPrivateKeyPemOption()
    {
        var mockedConfiguration = CreateMockConfiguration();

        var reader = PublicApiSigningExtensions.GetConfigurationReader(mockedConfiguration);
        var options = Activator.CreateInstance<PublicApiSigningOptions>();
        reader(options);

        Assert.AreEqual("example private key", options.PrivateKeyPem);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetConfigurationReader_Throws_WhenPublicKeyIdOptionIsMissing()
    {
        var mockedConfiguration = CreateMockConfiguration(new Dictionary<string, string?> {
            { "PublicKeyId", "" },
        });

        var reader = PublicApiSigningExtensions.GetConfigurationReader(mockedConfiguration);
        var options = Activator.CreateInstance<PublicApiSigningOptions>();
        reader(options);
    }

    [TestMethod]
    public void GetConfigurationReader_AssignsPublicKeyIdOption()
    {
        var mockedConfiguration = CreateMockConfiguration();

        var reader = PublicApiSigningExtensions.GetConfigurationReader(mockedConfiguration);
        var options = Activator.CreateInstance<PublicApiSigningOptions>();
        reader(options);

        Assert.AreEqual("4cd0e0f7-b30d-4969-848e-4295dd83c26b", options.PublicKeyId);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetConfigurationReader_Throws_WhenAlgorithmOptionIsMissing()
    {
        var mockedConfiguration = CreateMockConfiguration(new Dictionary<string, string?> {
            { "Algorithm", "" },
        });

        var reader = PublicApiSigningExtensions.GetConfigurationReader(mockedConfiguration);
        var options = Activator.CreateInstance<PublicApiSigningOptions>();
        reader(options);
    }

    [TestMethod]
    public void GetConfigurationReader_AssignsAlgorithmOption()
    {
        var mockedConfiguration = CreateMockConfiguration();

        var reader = PublicApiSigningExtensions.GetConfigurationReader(mockedConfiguration);
        var options = Activator.CreateInstance<PublicApiSigningOptions>();
        reader(options);

        Assert.AreEqual(HashAlgorithmName.SHA256, options.Algorithm);
    }

    [TestMethod]
    [ExpectedException(typeof(InvalidOperationException))]
    public void GetConfigurationReader_Throws_WhenPaddingModeOptionIsMissing()
    {
        var mockedConfiguration = CreateMockConfiguration(new Dictionary<string, string?> {
            { "Padding", "" },
        });

        var reader = PublicApiSigningExtensions.GetConfigurationReader(mockedConfiguration);
        var options = Activator.CreateInstance<PublicApiSigningOptions>();
        reader(options);
    }

    [TestMethod]
    public void GetConfigurationReader_AssignsPaddingMode_WhenPkcs1()
    {
        var mockedConfiguration = CreateMockConfiguration();

        var reader = PublicApiSigningExtensions.GetConfigurationReader(mockedConfiguration);
        var options = Activator.CreateInstance<PublicApiSigningOptions>();
        reader(options);

        Assert.AreEqual(RSASignaturePadding.Pkcs1, options.Padding);
    }

    [TestMethod]
    public void GetConfigurationReader_AssignsPaddingMode_WhenPss()
    {
        var mockedConfiguration = CreateMockConfiguration(new Dictionary<string, string?> {
            { "Padding", "pss" },
        });

        var reader = PublicApiSigningExtensions.GetConfigurationReader(mockedConfiguration);
        var options = Activator.CreateInstance<PublicApiSigningOptions>();
        reader(options);

        Assert.AreEqual(RSASignaturePadding.Pss, options.Padding);
    }

    #endregion

    #region SetupPublicApiSigningInteractions(IServiceCollection)

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupPublicApiSigningInteractions_Throws_WhenServicesArgumentIsNull()
    {
        PublicApiSigningExtensions.SetupPublicApiSigningInteractions(null!);
    }

    [DataRow(
        typeof(IInteractor<CreateDigitalSignatureForPayloadRequest, CreateDigitalSignatureForPayloadResponse>),
        DisplayName = nameof(CreateDigitalSignatureForPayloadRequest)
    )]
    [DataTestMethod]
    public void SetupPublicApiSigningInteractions_HasExpectedInteractionType(
        Type interactionType)
    {
        var services = new ServiceCollection();

        services.SetupPublicApiSigningInteractions();

        Assert.IsTrue(
            services.Any(descriptor =>
                descriptor.Lifetime == ServiceLifetime.Transient &&
                descriptor.ServiceType == interactionType
            )
        );
    }

    #endregion
}
