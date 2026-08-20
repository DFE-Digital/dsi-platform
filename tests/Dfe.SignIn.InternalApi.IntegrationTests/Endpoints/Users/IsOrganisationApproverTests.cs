using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait("Category", "Integration")]
public class IsOrganisationApproverTests : InternalApiIntegrationEndpointTestBase
{
    private static string GetEndpoint(Guid? userId) => $"/internal/users/{userId}/is-approver";

    public IsOrganisationApproverTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task IsOrganisationApprover_ReturnsTrue_WhenUserIsApprover()
    {
        var userId = Guid.NewGuid();

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication(userId.ToString());

        var org = EntityFaker.Organisation.Generate();
        var orgUser = EntityFaker.UserOrganisation
            .RuleFor(u => u.UserId, userId)
            .RuleFor(u => u.OrganisationId, org.Id)
            .RuleFor(u => u.Status, UserOrganisationStatus.Approved.Value)
            .RuleFor(u => u.RoleId, OrganisationRole.Approver.Value)
            .Generate();

        await this.InsertEntityAsync<DbOrganisationsContext, OrganisationEntity>(org);
        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationEntity>(orgUser);

        var endpoint = GetEndpoint(userId);
        var response = await authenticatedClient.GetAsync(endpoint);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<IsOrganisationApproverResponse>();
        Assert.NotNull(body);
        Assert.True(body.IsApprover);
    }

    [Fact]
    public async Task IsOrganisationApprover_ReturnsTrue_WhenUserHasMultipleOrgsAndOneIsApprover()
    {
        var userId = Guid.NewGuid();

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication(userId.ToString());

        var organisations = EntityFaker.Organisation.Generate(2);
        var org1 = organisations[0];
        var org2 = organisations[1];

        var orgUser1 = EntityFaker.UserOrganisation
            .RuleFor(u => u.UserId, userId)
            .RuleFor(u => u.OrganisationId, org1.Id)
            .RuleFor(u => u.Status, UserOrganisationStatus.Approved.Value)
            .RuleFor(u => u.RoleId, OrganisationRole.EndUser.Value)
            .Generate();

        var orgUser2 = EntityFaker.UserOrganisation
            .RuleFor(u => u.UserId, userId)
            .RuleFor(u => u.OrganisationId, org2.Id)
            .RuleFor(u => u.Status, UserOrganisationStatus.Approved.Value)
            .RuleFor(u => u.RoleId, OrganisationRole.Approver.Value)
            .Generate();

        await this.InsertEntitiesAsync<DbOrganisationsContext, OrganisationEntity>([org1, org2]);
        await this.InsertEntitiesAsync<DbOrganisationsContext, UserOrganisationEntity>([orgUser1, orgUser2]);

        var endpoint = GetEndpoint(userId);
        var response = await authenticatedClient.GetAsync(endpoint);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<IsOrganisationApproverResponse>();
        Assert.NotNull(body);
        Assert.True(body.IsApprover);
    }

    [Fact]
    public async Task IsOrganisationApprover_ReturnsFalse_WhenUserIsEndUser()
    {
        var userId = Guid.NewGuid();

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication(userId.ToString());

        var org = EntityFaker.Organisation.Generate();

        var orgUser = EntityFaker.UserOrganisation
            .RuleFor(u => u.UserId, userId)
            .RuleFor(u => u.OrganisationId, org.Id)
            .RuleFor(u => u.Status, UserOrganisationStatus.Approved.Value)
            .RuleFor(u => u.RoleId, OrganisationRole.EndUser.Value)
            .Generate();

        await this.InsertEntityAsync<DbOrganisationsContext, OrganisationEntity>(org);
        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationEntity>(orgUser);

        var endpoint = GetEndpoint(userId);
        var response = await authenticatedClient.GetAsync(endpoint);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<IsOrganisationApproverResponse>();
        Assert.NotNull(body);
        Assert.False(body.IsApprover);
    }

    [Fact]
    public async Task IsOrganisationApprover_ReturnsFalse_WhenUserHasNoAssociatedOrganisations()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var endpoint = GetEndpoint(Guid.NewGuid());
        var response = await authenticatedClient.GetAsync(endpoint);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<IsOrganisationApproverResponse>();
        Assert.NotNull(body);
        Assert.False(body.IsApprover);
    }

    [Fact]
    public async Task IsOrganisationApprover_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var endpoint = GetEndpoint(Guid.NewGuid());
        var response = await anonymousClient.GetAsync(endpoint);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
