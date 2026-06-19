using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Contracts;
using Microsoft.Extensions.DependencyInjection;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints;

[TestClass]
public class GetOrganisationByIdTests
{
    private static InternalApiWebApplicationFactory _factory = null!;
    private HttpClient _client = null!;

    [ClassInitialize]
    public static async Task ClassInitialize(TestContext context)
    {
        _factory = new InternalApiWebApplicationFactory();
        await _factory.InitializeContainerAsync();
    }

    [ClassCleanup]
    public static async Task ClassCleanup()
    {
        await _factory.DisposeAsync();
    }

    [TestInitialize]
    public async Task TestInitialize()
    {
        // Clear database state between tests
        await _factory.ResetDatabasesAsync();
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task GetOrganisationById_ReturnsOrganisation_WhenExists()
    {
        // Arrange: Seed an organisation
        var orgId = Guid.NewGuid();
        var expectedName = "Test Academy Trust";
        
        using (var scope = _factory.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<DbOrganisationsContext>();
            dbContext.Organisations.Add(new OrganisationEntity
            {
                Id = orgId,
                Name = expectedName,
                Status = 1,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
            await dbContext.SaveChangesAsync();
        }

        var request = new GetOrganisationByIdRequest
        {
            OrganisationId = orgId
        };

        // Act: POST to the endpoint
        var response = await _client.PostAsJsonAsync("interaction/Organisations.GetOrganisationById", request);

        // Assert
        Assert.AreEqual(HttpStatusCode.OK, response.StatusCode);
        
        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<GetOrganisationByIdResponse>>();
        Assert.IsNotNull(body);
        Assert.IsNotNull(body.Data);
        Assert.AreEqual(orgId, body.Data.Organisation.Id);
        Assert.AreEqual(expectedName, body.Data.Organisation.Name);
    }

    [TestMethod]
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
        Assert.AreEqual(HttpStatusCode.NotFound, response.StatusCode);
    }
}
