using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Notifications;
using Dfe.SignIn.Core.Interfaces.Notifications;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.Gateways.Entra.ChangeEmail;
using Dfe.SignIn.InternalApi.IntegrationTests.Mocks;
using Dfe.SignIn.TestHelpers.Integration;
using Dfe.SignIn.TestHelpers.Integration.Mocks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Notify.Interfaces;

namespace Dfe.SignIn.InternalApi.IntegrationTests;

public class InternalApiWebApplicationFactory : IntegrationTestFactory<Program>, IAsyncLifetime
{
    protected override IReadOnlyList<DatabaseCatalog> DatabaseCatalogs
    => [
        new("dsi-directories-test", "Directories", typeof(DbDirectoriesContext)),
        new("dsi-organisations-test", "Organisations", typeof(DbOrganisationsContext))
    ];

    public FakeInteractionLimiter FakeLimiter { get; } = new();
    public FakeEmailRequestTracker FakeEmailTracker { get; } = new();
    public FakeExternalAuthService FakeExternalAuth { get; } = new();
    public FakeUserUpdatedPublisher FakeUserUpdatedPublisher { get; } = new();
    public CapturingWriteToAuditInteractor AuditCapturer { get; } = new();
    internal FakeTimestampInterceptor TimestampInterceptor { get; } = new();
    internal FailingDbCommandInterceptor FailingDbCommandInterceptor { get; } = new();

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        base.ConfigureWebHost(builder);

        builder.ConfigureTestServices(services => {
            services.AddAuthentication(options => {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>("TestScheme", _ => { });

            // Audit
            services.RemoveAll<IInteractor<WriteToAuditRequest>>();
            services.AddSingleton<IInteractor<WriteToAuditRequest>>(this.AuditCapturer);
            services.RemoveAll<IAuditWriter>();
            services.AddSingleton<IAuditWriter>(new TestAuditWriter(this.AuditCapturer));

            // Notifications
            services.RemoveAll<IAsyncNotificationClient>();
            services.RemoveAll<INotificationService>();
            services.AddSingleton<INotificationService>(new FakeEmailNotificationService(this.FakeEmailTracker));

            // Rate limiter
            services.RemoveAll<IInteractionLimiter>();
            services.AddSingleton<IInteractionLimiter>(this.FakeLimiter);

            // External auth (Entra)
            services.RemoveAll<IEntraChangeEmailService>();
            services.AddSingleton<IEntraChangeEmailService>(this.FakeExternalAuth);

            // User updated publisher
            services.RemoveAll<IUserUpdatedPublisher>();
            services.AddSingleton<IUserUpdatedPublisher>(this.FakeUserUpdatedPublisher);

            // Timestamp interceptor
            services.RemoveAll<TimestampInterceptor>();
            services.AddSingleton<TimestampInterceptor>(this.TimestampInterceptor);

            // Failing DB command interceptor
            services.AddSingleton<DbCommandInterceptor>(this.FailingDbCommandInterceptor);
        });
    }

    /// <summary>
    /// Resets all shared test fakes to their clean default state.
    /// Called from <see cref="InternalApiIntegrationEndpointTestBase.InitializeAsync"/>.
    /// </summary>
    public void ResetAllFakes()
    {
        this.FakeLimiter.ResetAll();
        this.FakeEmailTracker.Clear();
        this.FakeExternalAuth.OnChangeEmail = null;
        this.FakeUserUpdatedPublisher.Clear();
        this.AuditCapturer.Clear();
        this.TimestampInterceptor.Reset();
        this.FailingDbCommandInterceptor.Reset();
    }

    public async Task InitializeAsync()
    {
        await this.InitialiseDatabasesAsync();
    }

    async Task IAsyncLifetime.DisposeAsync() => await this.DisposeAsync();
}
