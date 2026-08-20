using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Contracts;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Organisations;

[Trait("Category", "Integration")]
public class GetOrganisationByIdTests : InternalApiIntegrationEndpointTestBase
{
    private const string endpoint = "interaction/Organisations.GetOrganisationById";

    public GetOrganisationByIdTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetOrganisationById_ReturnsOrganisation_WhenExists()
    {
        var authenticatedClient = this.CreateClient()
            .WithAuthentication();

        // Arrange: Seed an organisation
        var orgId = Guid.NewGuid();
        var expectedName = "Test Academy Trust";

        var organisation = EntityFaker.Organisation
            .RuleFor(o => o.Id, f => orgId)
            .RuleFor(o => o.Name, f => expectedName);

        await this.InsertEntityAsync<DbOrganisationsContext, OrganisationEntity>(organisation);

        var request = new GetOrganisationByIdRequest {
            OrganisationId = orgId
        };

        // Act: POST to the endpoint
        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

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
        var httpClient = this.CreateClient()
            .WithAuthentication();

        // Arrange
        var missingOrgId = Guid.NewGuid();
        var request = new GetOrganisationByIdRequest {
            OrganisationId = missingOrgId
        };

        // Act
        var response = await httpClient.PostAsJsonAsync(endpoint, request);

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetOrganisationById_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var request = new GetOrganisationByIdRequest {
            OrganisationId = Guid.NewGuid()
        };

        var response = await anonymousClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
