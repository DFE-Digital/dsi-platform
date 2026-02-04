using Dfe.SignIn.Core.Contracts.Applications;
using Dfe.SignIn.Core.UseCases.Applications;
using Dfe.SignIn.InternalApi.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.UnitTests.Configuration;

[TestClass]
public sealed class ApplicationUseCaseExtensionsTests
{
    [TestMethod]
    public void Throws_WhenServicesArgumentIsNull()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentNullException>(()
            => ApplicationUseCaseExtensions.AddApplicationUseCases(null!, configuration));
    }

    [TestMethod]
    public void Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(()
            => ApplicationUseCaseExtensions.AddApplicationUseCases(services, null!));
    }

    [TestMethod]
    public void RegistersExpectedUseCases()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        ApplicationUseCaseExtensions.AddApplicationUseCases(services, configuration);

        Assert.IsTrue(
            services.HasInteractor<GetApplicationApiConfigurationRequest, GetApplicationApiConfigurationUseCase>()
        );
    }
}
