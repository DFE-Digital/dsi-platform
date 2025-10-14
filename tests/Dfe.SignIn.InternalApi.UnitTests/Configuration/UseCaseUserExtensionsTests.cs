using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.UseCases.Users;
using Dfe.SignIn.InternalApi.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.UnitTests.Configuration;

[TestClass]
public sealed class UseCaseUserExtensionsTests
{
    [TestMethod]
    public void RegistersExpectedUseCases()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        UseCaseUserExtensions.AddUseCasesUser(services, configuration);

        Assert.IsTrue(
            services.HasInteractor<AutoLinkEntraUserToDsiRequest, AutoLinkEntraUserToDsiUseCase>()
        );
        Assert.IsTrue(
            services.HasInteractor<CheckIsBlockedEmailAddressRequest, CheckIsBlockedEmailAddressUseCase>()
        );
    }
}
