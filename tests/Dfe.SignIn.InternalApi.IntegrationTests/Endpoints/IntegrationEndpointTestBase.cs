namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints;

[Collection("IntegrationTestsCollection")]
public abstract class IntegrationEndpointTestBase(InternalApiWebApplicationFactory webAppFactory) : IAsyncLifetime
{
    protected InternalApiWebApplicationFactory WebAppFactory { get; } = webAppFactory;

    public async Task InitializeAsync()
    {
        await this.WebAppFactory.ResetDatabasesAsync();
    }

    public Task DisposeAsync()
    {
        return Task.CompletedTask;
    }

    protected HttpClient CreateAuthenticatedClient(string? userId = null, string? userName = null, params string[] roles)
    {
        return this.WebAppFactory.CreateAuthenticatedClient(userId, userName, roles);
    }

    protected HttpClient CreateAnonymousClient()
    {
        return this.WebAppFactory.CreateAnonymousClient();
    }
}
