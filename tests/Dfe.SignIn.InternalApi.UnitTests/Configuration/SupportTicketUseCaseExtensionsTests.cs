using Dfe.SignIn.Core.Contracts.SupportTickets;
using Dfe.SignIn.Core.UseCases.SupportTickets;
using Dfe.SignIn.InternalApi.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.UnitTests.Configuration;

[TestClass]
public sealed class SupportTicketUseCaseExtensionsTests
{
    [TestMethod]
    public void Throws_WhenServicesArgumentIsNull()
    {
        var configuration = new ConfigurationBuilder().Build();

        Assert.Throws<ArgumentNullException>(()
            => SupportTicketUseCaseExtensions.AddSupportTicketUseCases(null!, configuration));
    }

    [TestMethod]
    public void Throws_WhenConfigurationArgumentIsNull()
    {
        var services = new ServiceCollection();

        Assert.Throws<ArgumentNullException>(()
            => SupportTicketUseCaseExtensions.AddSupportTicketUseCases(services, null!));
    }

    [TestMethod]
    public void RegistersExpectedUseCases()
    {
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        SupportTicketUseCaseExtensions.AddSupportTicketUseCases(services, configuration);

        Assert.IsTrue(
            services.HasInteractor<GetApplicationNamesForSupportTicketRequest, GetApplicationNamesForSupportTicketUseCase>()
        );
    }
}
