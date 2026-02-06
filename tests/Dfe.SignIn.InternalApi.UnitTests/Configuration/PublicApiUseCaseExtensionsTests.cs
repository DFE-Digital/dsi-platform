using Dfe.SignIn.Core.Contracts.PublicApi;
using Dfe.SignIn.Core.UseCases.PublicApi;
using Dfe.SignIn.InternalApi.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.UnitTests.Configuration;

[TestClass]
public sealed class PublicApiUseCaseExtensionsTests
{
    [TestMethod]
    public void Throws_WhenServicesArgumentIsNull()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentNullException>(()
            => PublicApiUseCaseExtensions.AddPublicApiUseCases(null!, configuration));
    }

    [TestMethod]
    public void Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(()
            => PublicApiUseCaseExtensions.AddPublicApiUseCases(services, null!));
    }

    [TestMethod]
    public void RegistersExpectedUseCases()
    {
        var services = new ServiceCollection();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("PublicApiSecretEncryption:Key", "fake-key")
            ])
            .Build();

        PublicApiUseCaseExtensions.AddPublicApiUseCases(services, configuration);

        Assert.IsTrue(
            services.HasInteractor<EncryptPublicApiSecretRequest, EncryptApiSecretUseCase>()
        );
        Assert.IsTrue(
            services.HasInteractor<DecryptApiSecretRequest, DecryptApiSecretUseCase>()
        );
    }
}
