using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.PublicApi.UnitTests.ScopedSessionProvider;

[TestClass]
public class ScopedSessionExtensionsTests
{
    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void SetupScopedSession_Throws_WhenServicesArgumentIsNull()
    {
        ScopedSessionExtensions.SetupScopedSession(
            services: null!
        );
    }

    [TestMethod]
    public void SetupScopedSession_ShouldAddScopedReaderInstance()
    {
        var services = new ServiceCollection();

        ScopedSessionExtensions.SetupScopedSession(services);
        Assert.IsTrue(
           services.Any(descriptor =>
               descriptor.Lifetime == ServiceLifetime.Scoped &&
               descriptor.ServiceType == typeof(ScopedSession.IScopedSessionReader) &&
               descriptor.ImplementationType == typeof(ScopedSession.ScopedSessionProvider)
           )
       );
    }

    [TestMethod]
    public void SetupScopedSession_ShouldAddScopedWriterInstance()
    {
        var services = new ServiceCollection();

        ScopedSessionExtensions.SetupScopedSession(services);
        Assert.IsTrue(
           services.Any(descriptor =>
               descriptor.Lifetime == ServiceLifetime.Scoped &&
               descriptor.ServiceType == typeof(ScopedSession.IScopedSessionWriter)
           )
       );
    }

    [TestMethod]
    public void SetupScopedSession_ReaderWriterMatchReferenceSameInstance()
    {
        var mockApplication = new Core.Models.Applications.ApplicationModel {
            ClientId = "mock-client-id",
            Id = Guid.Parse("9980e12f-dfca-4631-ae41-1cccd12d231b"),
            IsExternalService = true,
            IsHiddenService = true,
            IsIdOnlyService = true,
            Name = "mock-name",
            ServiceHomeUrl = new Uri("https://mock.com")
        };

        var services = new ServiceCollection();

        ScopedSessionExtensions.SetupScopedSession(services);

        var builder = services.BuildServiceProvider();

        var writer = builder.GetService<ScopedSession.IScopedSessionWriter>();

        Assert.IsNotNull(writer);

        writer.Application = mockApplication;

        var reader = builder.GetService<ScopedSession.IScopedSessionReader>();

        Assert.IsNotNull(reader);

        Assert.AreEqual(reader.Application, mockApplication);
    }
}
