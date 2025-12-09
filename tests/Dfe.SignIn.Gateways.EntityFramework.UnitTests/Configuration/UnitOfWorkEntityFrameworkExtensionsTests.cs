using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Gateways.EntityFramework.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests.Configuration;

[TestClass]
public class EntityFrameworkUnitOfWorkExtensionsTests
{
    private Mock<IConfiguration> configMock = null!;
    private Mock<IConfigurationSection> directoriesSectionMock = null!;
    private Mock<IConfigurationSection> organisationsSectionMock = null!;

    [TestInitialize]
    public void Setup()
    {
        this.configMock = new Mock<IConfiguration>();

        this.directoriesSectionMock = new Mock<IConfigurationSection>();
        this.directoriesSectionMock.Setup(s => s.Value).Returns("Server=.;Database=Dirs;Trusted_Connection=True;");

        this.organisationsSectionMock = new Mock<IConfigurationSection>();
        this.organisationsSectionMock.Setup(s => s.Value).Returns("Server=.;Database=Orgs;Trusted_Connection=True;");

        this.configMock.Setup(c => c.GetSection("Directories"))
        .Returns(this.directoriesSectionMock.Object);

        this.configMock.Setup(c => c.GetSection("Organisations"))
            .Returns(this.organisationsSectionMock.Object);
    }

    [TestMethod]
    public void AddEntityFrameworkUnitOfWork_Throws_WhenServicesNull()
    {

        Assert.ThrowsExactly<ArgumentNullException>(() => {
            ServiceCollection services = null!;
            services.AddUnitOfWorkEntityFrameworkServices(this.configMock.Object, true, true);
        });
    }

    [TestMethod]
    public void AddEntityFrameworkUnitOfWork_Throws_WhenSectionNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => {
            var services = new ServiceCollection();
            IConfiguration config = null!;
            services.AddUnitOfWorkEntityFrameworkServices(config, true, true);
        });
    }

    [TestMethod]
    public void AddEntityFrameworkUnitOfWorkServices_Registers_TransactionDecorator_And_TransactionContext()
    {
        var services = new ServiceCollection();

        services.AddTransient<IInteractionDispatcher, FakeDispatcher>();

        UnitOfWorkEntityFrameworkExtensions.AddUnitOfWorkEntityFrameworkServices(
            services,
            this.configMock.Object,
            addDirectoriesUnitOfWork: true,
            addOrganisationsUnitOfWork: false);

        var provider = services.BuildServiceProvider();

        var dispatcher = provider.GetRequiredService<IInteractionDispatcher>();
        Assert.IsInstanceOfType(dispatcher, typeof(ProtectTransactionInteractionDispatcher));

        var transactionContext = provider.GetRequiredService<IEntityFrameworkTransactionContext>();
        Assert.IsInstanceOfType(transactionContext, typeof(EntityFrameworkTransactionContext));
    }

    [TestMethod]
    public void AddEntityFrameworkUnitOfWorkServices_DoesNotRegister_Decorator_IfNoUoWEnabled()
    {
        var services = new ServiceCollection();
        services.AddTransient<IInteractionDispatcher, FakeDispatcher>();

        UnitOfWorkEntityFrameworkExtensions.AddUnitOfWorkEntityFrameworkServices(
            services,
            this.configMock.Object,
            addDirectoriesUnitOfWork: false,
            addOrganisationsUnitOfWork: false);

        var provider = services.BuildServiceProvider();
        var dispatcher = provider.GetRequiredService<IInteractionDispatcher>();

        Assert.IsInstanceOfType(dispatcher, typeof(FakeDispatcher));
        Assert.IsNull(provider.GetService<IEntityFrameworkTransactionContext>());
    }

    // [TestMethod]
    // public void AddEntityFrameworkUnitOfWorkServices_Registers_Directories_UoW_And_Context()
    // {
    //     var services = new ServiceCollection();
    //     services.AddTransient<IInteractionDispatcher, FakeDispatcher>();

    //     EntityFrameworkUnitOfWorkServiceExtensions.AddEntityFrameworkUnitOfWorkServices(
    //         services,
    //         this.configMock.Object,
    //         addDirectoriesUnitOfWork: true,
    //         addOrganisationsUnitOfWork: false);

    //     var provider = services.BuildServiceProvider();

    //     Assert.IsNotNull(provider.GetService<DbDirectoriesContext>());

    //     Assert.IsInstanceOfType(
    //         provider.GetRequiredService<IDirectoriesUnitOfWork>(),
    //         typeof(DirectoriesUnitOfWork));
    // }

    // [TestMethod]
    // public void AddEntityFrameworkUnitOfWorkServices_Registers_Organisations_UoW_And_Context()
    // {
    //     var services = new ServiceCollection();
    //     services.AddTransient<IInteractionDispatcher, FakeDispatcher>();

    //     EntityFrameworkUnitOfWorkServiceExtensions.AddEntityFrameworkUnitOfWorkServices(
    //         services,
    //         this.configMock.Object,
    //         addDirectoriesUnitOfWork: false,
    //         addOrganisationsUnitOfWork: true);

    //     var provider = services.BuildServiceProvider();

    //     Assert.IsNotNull(provider.GetService<DbOrganisationsContext>());

    //     Assert.IsInstanceOfType(
    //         provider.GetRequiredService<IOrganisationsUnitOfWork>(),
    //         typeof(OrganisationsUnitOfWork));
    // }

    private sealed class FakeDispatcher : IInteractionDispatcher
    {
        public InteractionTask DispatchAsync<TRequest>(InteractionContext<TRequest> context) where TRequest : class => throw new NotImplementedException();
    }
}
