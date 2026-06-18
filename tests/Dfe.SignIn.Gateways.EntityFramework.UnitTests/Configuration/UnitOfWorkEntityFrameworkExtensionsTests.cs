using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Gateways.EntityFramework.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.Gateways.EntityFramework.UnitTests.Configuration;

[TestClass]
public sealed class EntityFrameworkUnitOfWorkExtensionsTests
{
    private IConfiguration configMock = null!;

    [TestInitialize]
    public void Setup()
    {
        this.configMock = new ConfigurationBuilder()
            .AddInMemoryCollection([
                new("Directories:Host", "localhost"),
                new("Directories:Name", "Dirs"),
                new("Directories:Username", "sa"),
                new("Directories:Password", "password"),

                new("Organisations:Host", "localhost"),
                new("Organisations:Name", "Orgs"),
                new("Organisations:Username", "sa"),
                new("Organisations:Password", "password"),
            ])
            .Build();
    }

    [TestMethod]
    public void AddUnitOfWorkEntityFrameworkServices_Throws_WhenServicesNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => {
            ServiceCollection services = null!;
            services.AddUnitOfWorkEntityFrameworkServices(
                this.configMock,
                addDirectoriesUnitOfWork: true,
                addOrganisationsUnitOfWork: true,
                addAuditUnitOfWork: true
            );
        });
    }

    [TestMethod]
    public void AddUnitOfWorkEntityFrameworkServices_Throws_WhenSectionNull()
    {
        Assert.ThrowsExactly<ArgumentNullException>(() => {
            var services = new ServiceCollection();
            IConfiguration config = null!;
            services.AddUnitOfWorkEntityFrameworkServices(
                config,
                addDirectoriesUnitOfWork: true,
                addOrganisationsUnitOfWork: true,
                addAuditUnitOfWork: true
            );
        });
    }

    [TestMethod]
    [DataRow("Host")]
    [DataRow("Name")]
    [DataRow("Username")]
    [DataRow("Password")]
    public void AddUnitOfWorkEntityFrameworkServices_Throws_WhenMissingRequiredConfigValue(string missingKey)
    {
        var configData = new Dictionary<string, string?>([
            new("Directories:Host", "localhost"),
            new("Directories:Name", "Dirs"),
            new("Directories:Username", "sa"),
            new("Directories:Password", "password"),
        ]);

        configData.Remove($"Directories:{missingKey}");

        var brokenConfiguration = new ConfigurationBuilder()
            .AddInMemoryCollection(configData)
            .Build();

        var services = new ServiceCollection();
        services.AddTransient<IInteractionDispatcher, FakeDispatcher>();

        var ex = Assert.ThrowsExactly<InvalidOperationException>(() =>
            services.AddUnitOfWorkEntityFrameworkServices(
                brokenConfiguration,
                addDirectoriesUnitOfWork: true,
                addOrganisationsUnitOfWork: false,
                addAuditUnitOfWork: false
            ));

        Assert.AreEqual($"Section 'Directories:{missingKey}' not found in configuration.", ex.Message);
    }

    [TestMethod]
    public void AddUnitOfWorkEntityFrameworkServices_RegistersExpectedServices()
    {
        var services = new ServiceCollection();
        services.AddTransient<IInteractionDispatcher, FakeDispatcher>();

        services.AddUnitOfWorkEntityFrameworkServices(
            this.configMock,
            addDirectoriesUnitOfWork: true,
            addOrganisationsUnitOfWork: false,
            addAuditUnitOfWork: false);

        var provider = services.BuildServiceProvider();

        var timeProvider = provider.GetRequiredService<TimeProvider>();
        Assert.AreEqual(TimeProvider.System, timeProvider);

        var interceptor = provider.GetRequiredService<TimestampInterceptor>();
        Assert.IsNotNull(interceptor);
    }

    private sealed class FakeDispatcher : IInteractionDispatcher
    {
        public InteractionTask DispatchAsync<TRequest>(InteractionContext<TRequest> context) where TRequest : class => throw new NotImplementedException();
    }
}
