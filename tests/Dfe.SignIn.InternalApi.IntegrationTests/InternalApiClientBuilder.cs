//namespace Dfe.SignIn.InternalApi.IntegrationTests;

//public class InternalApiClientBuilder
//{
//    private readonly InternalApiWebApplicationFactory factory;
//    private readonly List<Action<HttpClient>> clientConfigurators = [];

//    internal InternalApiClientBuilder(InternalApiWebApplicationFactory factory)
//    {
//        this.factory = factory;
//    }

//    /// <summary>
//    /// Builds the WebApplicationFactory scope and returns the configured test context.
//    /// </summary>
//    public IntegrationTestContext Build()
//    {
//        var client = this.factory.CreateClient();

//        foreach (var configAction in this.clientConfigurators) {
//            configAction(client);
//        }

//        return new IntegrationTestContext {
//            Client = client,
//            Services = this.factory.Services,
//            AuditMock = this.factory.AuditCapturer,
//            EmailTracker = this.factory.FakeEmailTracker
//        };
//    }
//}
