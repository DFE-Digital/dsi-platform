using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.InternalApi.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.UnitTests.Configuration;

[TestClass]
public sealed class UserUseCaseExtensionsTests
{
    [TestMethod]
    public void Throws_WhenServicesArgumentIsNull()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentNullException>(()
            => UserUseCaseExtensions.AddUserUseCases(null!, configuration));
    }

    [TestMethod]
    public void Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(()
            => UserUseCaseExtensions.AddUserUseCases(services, null!));
    }

    [TestMethod]
    public void RegistersExpectedUseCases()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        UserUseCaseExtensions.AddUserUseCases(services, configuration);

        Assert.IsTrue(
            services.HasInteractor<AutoLinkEntraUserToDsiRequest, AutoLinkEntraUserToDsiUseCase>()
        );
        Assert.IsTrue(
            services.HasInteractor<CheckIsBlockedEmailAddressRequest, CheckIsBlockedEmailAddressUseCase>()
        );
        Assert.IsTrue(
            services.HasInteractor<GetUserProfileRequest, GetUserProfileUseCase>()
        );
    }
}
