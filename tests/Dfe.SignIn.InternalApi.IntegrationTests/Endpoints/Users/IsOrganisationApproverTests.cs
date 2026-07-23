using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Contracts;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait("Category", "Integration")]
public class IsOrganisationApproverTests : InternalApiIntegrationEndpointTestBase
{
    private const string endpoint = "interaction/Users.IsOrganisationApprover";
    private const short ActiveUserOrganisationStatus = 1;

    public IsOrganisationApproverTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task IsOrganisationApprover_ReturnsTrue_WhenUserIsApprover()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var userId = Guid.NewGuid();
        var org = EntityFaker.Organisation.Generate();

        // Seed Organisation
        await this.InsertEntityAsync<DbOrganisationsContext, OrganisationEntity>(org);

        // Seed UserOrganisation as Approver (RoleId = 10000)
        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationEntity>(new UserOrganisationEntity {
            UserId = userId,
            OrganisationId = org.Id,
            RoleId = OrganisationRoles.Approver.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = ActiveUserOrganisationStatus
        });

        var request = new IsOrganisationApproverRequest(userId);

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<IsOrganisationApproverResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.True(body.Data.IsApprover);
    }

    [Fact]
    public async Task IsOrganisationApprover_ReturnsTrue_WhenUserHasMultipleOrgsAndOneIsApprover()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var userId = Guid.NewGuid();
        var organisations = EntityFaker.Organisation.Generate(2);
        var org1 = organisations[0];
        var org2 = organisations[1];

        // Seed Organisations
        await this.InsertEntitiesAsync<DbOrganisationsContext, OrganisationEntity>(new[] { org1, org2 });

        // Seed UserOrganisations
        await this.InsertEntitiesAsync<DbOrganisationsContext, UserOrganisationEntity>(new[] {
            new UserOrganisationEntity {
                UserId = userId,
                OrganisationId = org1.Id,
                RoleId = OrganisationRoles.EndUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = ActiveUserOrganisationStatus
            },
            new UserOrganisationEntity {
                UserId = userId,
                OrganisationId = org2.Id,
                RoleId = OrganisationRoles.Approver.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = ActiveUserOrganisationStatus
            }
        });

        var request = new IsOrganisationApproverRequest(userId);

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<IsOrganisationApproverResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.True(body.Data.IsApprover);
    }

    [Fact]
    public async Task IsOrganisationApprover_ReturnsFalse_WhenUserIsEndUser()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var userId = Guid.NewGuid();
        var org = EntityFaker.Organisation.Generate();

        // Seed Organisation
        await this.InsertEntityAsync<DbOrganisationsContext, OrganisationEntity>(org);

        // Seed UserOrganisation as End User (RoleId = 0)
        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationEntity>(new UserOrganisationEntity {
            UserId = userId,
            OrganisationId = org.Id,
            RoleId = OrganisationRoles.EndUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = ActiveUserOrganisationStatus
        });

        var request = new IsOrganisationApproverRequest(userId);

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<IsOrganisationApproverResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.False(body.Data.IsApprover);
    }

    [Fact]
    public async Task IsOrganisationApprover_ReturnsFalse_WhenUserHasNoAssociatedOrganisations()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var request = new IsOrganisationApproverRequest(Guid.NewGuid());

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<IsOrganisationApproverResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.False(body.Data.IsApprover);
    }

    [Fact]
    public async Task IsOrganisationApprover_Returns400_WhenRequestHasInvalidBody()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, "");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task IsOrganisationApprover_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var request = new IsOrganisationApproverRequest(Guid.NewGuid());

        var response = await anonymousClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
