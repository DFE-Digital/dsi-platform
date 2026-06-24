using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Collection("IntegrationTestsCollection")]
[Trait("Category", "Integration")]
public class ChangeJobTitleEndpointTests : IAsyncLifetime
{
    private readonly InternalApiWebApplicationFactory webAppfactory;
    private readonly HttpClient httpClient;

    public ChangeJobTitleEndpointTests(InternalApiWebApplicationFactory factory)
    {
        this.webAppfactory = factory;
        this.httpClient = this.webAppfactory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Clear database state between tests
        await this.webAppfactory.ResetDatabasesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task ChangeJobTitle_ReturnsSuccess_WhenValidRequest()
    {
        // Arrange: Seed an organisation
        var sub = Guid.NewGuid();
        var expectedJobTitle = "New Job Title";

        await using var scope = this.webAppfactory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DbOrganisationsContext>();
        dbContext.Users.Add(new UserEntity {
            Salt = "salt",
            Email = "dummy@test.com",
            Sub = sub,
            JobTitle = "Old Job Title",
            FirstName = "John",
            LastName = "Doe",
            Password = "hashedpassword",
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var request = new ChangeJobTitleRequest {
            UserId = sub,
            NewJobTitle = expectedJobTitle
        };

        // Act: POST to the endpoint
        var response = await this.httpClient.PostAsJsonAsync("/users/ChangeJobTitle", request);

        // Assert
        Xunit.Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }
}
