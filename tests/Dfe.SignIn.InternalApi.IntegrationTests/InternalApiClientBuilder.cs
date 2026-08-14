using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Notifications;
using Dfe.SignIn.Core.Interfaces.ExternalAuth;
using Dfe.SignIn.Core.Interfaces.Notifications;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.IntegrationTests.Mocks;
using Dfe.SignIn.TestHelpers.Integration;
using Dfe.SignIn.TestHelpers.Integration.Mocks;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Notify.Interfaces;

namespace Dfe.SignIn.InternalApi.IntegrationTests;

public class InternalApiClientBuilder
{
    private readonly InternalApiWebApplicationFactory factory;
    private readonly List<IAsyncDisposable> createdFactories;
    private readonly List<Action<IServiceCollection>> serviceOverrides = [];
    private readonly List<Action<HttpClient>> clientConfigurators = [];

    // Base default fakes passed from the test base
    private readonly FakeInteractionLimiter fakeLimiter;
    private readonly FakeEmailRequestTracker fakeEmailRequestTracker;
    private readonly FakeExternalAuthService fakeExternalAuthService;
    private readonly FakeUserUpdatedPublisher fakeUserUpdatedPublisher;
    private readonly TestTimestampInterceptor testTimestampInterceptor;

    // Optional test-specific captures
    private CapturingWriteToAuditInteractor? auditMock;

    internal InternalApiClientBuilder(
        InternalApiWebApplicationFactory factory,
        List<IAsyncDisposable> createdFactories,
        FakeInteractionLimiter fakeLimiter,
        FakeEmailRequestTracker fakeEmailRequestTracker,
        FakeExternalAuthService fakeExternalAuthService,
        FakeUserUpdatedPublisher fakeUserUpdatedPublisher,
        TestTimestampInterceptor testTimestampInterceptor )
    {
        this.factory = factory;
        this.createdFactories = createdFactories;
        this.fakeLimiter = fakeLimiter;
        this.fakeEmailRequestTracker = fakeEmailRequestTracker;
        this.fakeExternalAuthService = fakeExternalAuthService;
        this.fakeUserUpdatedPublisher = fakeUserUpdatedPublisher;
        this.testTimestampInterceptor = testTimestampInterceptor;
    }

    /// <summary>
    /// Configures the client to authenticate using test header conventions.
    /// </summary>
    public InternalApiClientBuilder WithAuthentication( string? userId = null, string? userName = null, params string[] roles )
    {
        this.clientConfigurators.Add( client => {
            client.DefaultRequestHeaders.Add( TestAuthHandler.EnableAuthHeaderName, bool.TrueString );

            if (!string.IsNullOrWhiteSpace( userId )) {
                client.DefaultRequestHeaders.Add( TestAuthHandler.UserIdHeaderName, userId );
            }

            if (!string.IsNullOrWhiteSpace( userName )) {
                client.DefaultRequestHeaders.Add( TestAuthHandler.UserNameHeaderName, userName );
            }

            foreach (var role in roles.Where( static r => !string.IsNullOrWhiteSpace( r ) )) {
                client.DefaultRequestHeaders.Add( TestAuthHandler.RoleHeaderName, role );
            }
        } );
        return this;
    }

    /// <summary>
    /// Injects an audit interceptor mock to capture and assert on audit logs.
    /// </summary>
    public InternalApiClientBuilder WithAuditMock()
    {
        this.auditMock = new CapturingWriteToAuditInteractor();
        var auditWriterMock = new TestAuditWriter( this.auditMock );

        this.serviceOverrides.Add( services => {
            services.RemoveAll<IInteractor<WriteToAuditRequest>>();
            services.AddSingleton<IInteractor<WriteToAuditRequest>>( this.auditMock );

            services.RemoveAll<IAuditWriter>();
            services.AddSingleton<IAuditWriter>( auditWriterMock );
        } );

        return this;
    }

    /// <summary>
    /// Allows generic service registration overrides for custom test implementations.
    /// </summary>
    public InternalApiClientBuilder WithService<TInterface, TImplementation>()
        where TInterface : class
        where TImplementation : class, TInterface
    {
        this.serviceOverrides.Add( services => {
            services.RemoveAll<TInterface>();
            services.AddSingleton<TInterface, TImplementation>();
        } );
        return this;
    }

    /// <summary>
    /// Allows injecting a pre-configured instance for a service interface.
    /// </summary>
    public InternalApiClientBuilder WithService<TInterface>( TInterface instance )
        where TInterface : class
    {
        this.serviceOverrides.Add( services => {
            services.RemoveAll<TInterface>();
            services.AddSingleton( instance );
        } );
        return this;
    }

    /// <summary>
    /// Overrides the timestamp interceptor with a specific time provider.
    /// </summary>
    public InternalApiClientBuilder WithTimeProvider( TimeProvider timeProvider )
    {
        this.serviceOverrides.Add( services => {
            services.RemoveAll<TimestampInterceptor>();
            services.AddSingleton<TimestampInterceptor>( new TestTimestampInterceptor( timeProvider ) );
        } );
        return this;
    }

    /// <summary>
    /// Builds the WebApplicationFactory scope and returns the configured test context.
    /// </summary>
    public IntegrationTestContext Build()
    {
        var fakeEmailNotificationService = new FakeEmailNotificationService( this.fakeEmailRequestTracker );

        var customisedFactory = this.factory.WithWebHostBuilder( builder => {
            builder.ConfigureTestServices( services => {

                // Default fallback mock configurations
                services.RemoveAll<IInteractor<WriteToAuditRequest>>();
                services.AddNullInteractor<WriteToAuditRequest, WriteToAuditResponse>();

                services.RemoveAll<IAuditWriter>();
                services.AddSingleton<IAuditWriter, TestAuditWriter>();

                services.RemoveAll<IAsyncNotificationClient>();
                services.RemoveAll<INotificationService>();
                services.AddSingleton<INotificationService>( fakeEmailNotificationService );

                services.RemoveAll<IInteractionLimiter>();
                services.AddSingleton<IInteractionLimiter>( this.fakeLimiter );

                services.RemoveAll<IExternalAuthService>();
                services.AddSingleton<IExternalAuthService>( this.fakeExternalAuthService );

                services.RemoveAll<IUserUpdatedPublisher>();
                services.AddSingleton<IUserUpdatedPublisher>( this.fakeUserUpdatedPublisher );

                services.RemoveAll<TimestampInterceptor>();
                services.AddSingleton<TimestampInterceptor>( this.testTimestampInterceptor );

                // Apply any custom overrides requested via the builder
                foreach (var overrideAction in this.serviceOverrides) {
                    overrideAction( services );
                }
            } );
        } );

        this.createdFactories.Add( customisedFactory );

        var client = customisedFactory.CreateClient();

        foreach (var configAction in this.clientConfigurators) {
            configAction( client );
        }

        return new IntegrationTestContext {
            Client = client,
            AuditMock = this.auditMock,
            EmailTracker = this.fakeEmailRequestTracker
        };
    }
}
