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
    private readonly List<Action<HttpClient>> clientConfigurators = [];

    internal InternalApiClientBuilder(InternalApiWebApplicationFactory factory)
    {
        this.factory = factory;
    }

    /// <summary>
    /// Configures the client to authenticate using test header conventions.
    /// </summary>
    public InternalApiClientBuilder WithAuthentication(string? userId = null, string? userName = null, params string[] roles)
    {
        this.clientConfigurators.Add(client => {
            client.DefaultRequestHeaders.Add(TestAuthHandler.EnableAuthHeaderName, bool.TrueString);

            if (!string.IsNullOrWhiteSpace(userId)) {
                client.DefaultRequestHeaders.Add(TestAuthHandler.UserIdHeaderName, userId);
            }

            if (!string.IsNullOrWhiteSpace(userName)) {
                client.DefaultRequestHeaders.Add(TestAuthHandler.UserNameHeaderName, userName);
            }

            foreach (var role in roles.Where(static r => !string.IsNullOrWhiteSpace(r))) {
                client.DefaultRequestHeaders.Add(TestAuthHandler.RoleHeaderName, role);
            }
        });
        return this;
    }

    /// <summary>
    /// Retained for backward compatibility. The audit capturer is always active
    /// in the root factory, so this is effectively a no-op.
    /// </summary>
    public InternalApiClientBuilder WithAuditMock() => this;

    /// <summary>
    /// Overrides the timestamp interceptor with a specific time provider.
    /// </summary>
    public InternalApiClientBuilder WithTimeProvider(TimeProvider timeProvider, bool shouldSkipTimestamps = false, bool shouldFail = false)
    {
        this.factory.TimestampInterceptor.TimeProvider = timeProvider;
        this.factory.TimestampInterceptor.ShouldSkipTimestamps = shouldSkipTimestamps;
        this.factory.TimestampInterceptor.ShouldFail = shouldFail;
        return this;
    }

    /// <summary>
    /// Builds the WebApplicationFactory scope and returns the configured test context.
    /// </summary>
    public IntegrationTestContext Build()
    {
        var client = this.factory.CreateClient();

        foreach (var configAction in this.clientConfigurators) {
            configAction(client);
        }

        return new IntegrationTestContext {
            Client = client,
            Services = this.factory.Services,
            AuditMock = this.factory.AuditCapturer,
            EmailTracker = this.factory.FakeEmailTracker
        };
    }
}
