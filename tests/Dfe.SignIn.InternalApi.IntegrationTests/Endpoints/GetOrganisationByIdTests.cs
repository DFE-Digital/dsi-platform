using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints;

[Collection("IntegrationTestsCollection")]
[Trait("Category", "Integration")]
public class GetOrganisationByIdTests : IAsyncLifetime
{
    private readonly InternalApiWebApplicationFactory webAppfactory;
    private readonly HttpClient httpClient;

    public GetOrganisationByIdTests(InternalApiWebApplicationFactory factory)
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
    public async Task GetOrganisationById_ReturnsOrganisation_WhenExists()
    {
        // Arrange: Seed an organisation
        var orgId = Guid.NewGuid();
        var expectedName = "Test Academy Trust";

        await using var scope = this.webAppfactory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DbOrganisationsContext>();
        dbContext.Organisations.Add(new OrganisationEntity {
            Id = orgId,
            Name = expectedName,
            Category = "001",
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var request = new GetOrganisationByIdRequest {
            OrganisationId = orgId
        };

        // Act: POST to the endpoint
        var response = await this.httpClient.PostAsJsonAsync("interaction/Organisations.GetOrganisationById", request);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK) {
            var errorContent = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Request failed with status {response.StatusCode}. Response: {errorContent}");
        }

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<GetOrganisationByIdResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.Equal(orgId, body.Data.Organisation.Id);
        Assert.Equal(expectedName, body.Data.Organisation.Name);
    }

    [Fact]
    public async Task GetOrganisationById_Returns404_WhenDoesNotExist()
    {
        // Arrange
        var missingOrgId = Guid.NewGuid();
        var request = new GetOrganisationByIdRequest {
            OrganisationId = missingOrgId
        };

        // Act
        var response = await this.httpClient.PostAsJsonAsync("interaction/Organisations.GetOrganisationById", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
