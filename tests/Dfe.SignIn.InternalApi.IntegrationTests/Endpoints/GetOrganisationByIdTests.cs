namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints;

using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Contracts;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

[Collection("IntegrationTestsCollection")]
[Trait("Category", "Integration")]
public class GetOrganisationByIdTests : IAsyncLifetime
{
    private readonly InternalApiWebApplicationFactory _factory;
    private readonly HttpClient _client;

    public GetOrganisationByIdTests(InternalApiWebApplicationFactory factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    public async Task InitializeAsync()
    {
        // Clear database state between tests
        await _factory.ResetDatabasesAsync();
    }

    public Task DisposeAsync() => Task.CompletedTask;

    [Fact]
    public async Task GetOrganisationById_ReturnsOrganisation_WhenExists()
    {
        // Arrange: Seed an organisation
        var orgId = Guid.NewGuid();
        var expectedName = "Test Academy Trust";

        await using var scope = _factory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DbOrganisationsContext>();
        dbContext.Organisations.Add(new OrganisationEntity
        {
            Id = orgId,
            Name = expectedName,
            Category = "001",
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var request = new GetOrganisationByIdRequest
        {
            OrganisationId = orgId
        };

        // Act: POST to the endpoint
        var response = await _client.PostAsJsonAsync("interaction/Organisations.GetOrganisationById", request);

        // Assert
        if (response.StatusCode != HttpStatusCode.OK)
        {
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
        var request = new GetOrganisationByIdRequest
        {
            OrganisationId = missingOrgId
        };

        // Act
        var response = await _client.PostAsJsonAsync("interaction/Organisations.GetOrganisationById", request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }
}
