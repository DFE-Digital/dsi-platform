using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Features.Shared;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait("Category", "Integration")]
public class PendingApprovalCountTests : InternalApiIntegrationEndpointTestBase
{
    private static string GetEndpoint(Guid? userId) => $"/internal/users/{userId}/pending-approval-counter";

    private const short ActiveUserOrganisationStatus = 1;

    public PendingApprovalCountTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetPendingApprovalCount_ReturnsCorrectSum_WhenUserIsApproverWithPendingRequests()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var userId = Guid.NewGuid();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(EntityFaker.User
            .RuleFor(u => u.Sub, _ => userId)
            .Generate());

        var org = EntityFaker.Organisation.Generate();

        // Seed Organisation
        await this.InsertEntityAsync<DbOrganisationsContext, OrganisationEntity>(org);

        // Seed UserOrganisation as Approver
        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationEntity>(EntityFaker.UserOrganisation
            .RuleFor(uo => uo.UserId, _ => userId)
            .RuleFor(uo => uo.OrganisationId, _ => org.Id)
            .RuleFor(uo => uo.Status, _ => UserOrganisationStatus.Approved.Value)
            .RuleFor(uo => uo.RoleId, _ => OrganisationRole.Approver.Value)
            .Generate());

        // Seed 2 active pending service requests & 1 active pending org request
        await this.InsertEntitiesAsync<DbOrganisationsContext, UserServiceRequestEntity>([
            EntityFaker.UserServiceRequest
                .RuleFor(sr => sr.OrganisationId, _ => org.Id)
                .Generate(),

            EntityFaker.UserServiceRequest
                .RuleFor(sr => sr.OrganisationId, _ => org.Id)
                .Generate(),

            EntityFaker.UserServiceRequest
                .RuleFor(sr => sr.OrganisationId, _ => org.Id)
                .RuleFor(sr => sr.Status, _ => ServiceRequestStatus.Approved.Value)
                .RuleFor(sr => sr.ActionedAt, _ => DateTime.UtcNow)
                .Generate(),
        ]);

        await this.InsertEntitiesAsync<DbOrganisationsContext, UserOrganisationRequestEntity>([
            EntityFaker.UserOrganisationRequest
                .RuleFor(ur => ur.OrganisationId, _ => org.Id)
                .Generate(),

            EntityFaker.UserOrganisationRequest
                .RuleFor(ur => ur.OrganisationId, _ => org.Id)
                .RuleFor(ur => ur.ActionedAt, _ => DateTime.UtcNow)
                .RuleFor(ur => ur.Status, _ => OrganisationRequestStatus.Approved.Value)
                .Generate(),
        ]);

        var response = await authenticatedClient.GetAsync(GetEndpoint(userId));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PendingApprovalCountResponse>();
        Assert.NotNull(body);
        Assert.Equal(3, body.Count);
    }

    [Fact]
    public async Task GetPendingApprovalCount_OnlyCountsRequestsForOrgsWhereUserIsApprover()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var userId = Guid.NewGuid();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(EntityFaker.User
            .RuleFor(u => u.Sub, _ => userId)
            .Generate());

        var orgA = EntityFaker.Organisation.Generate(); // User is Approver
        var orgB = EntityFaker.Organisation.Generate(); // User is End User
        var orgC = EntityFaker.Organisation.Generate(); // User is Not Associated

        await this.InsertEntitiesAsync<DbOrganisationsContext, OrganisationEntity>([orgA, orgB, orgC]);

        // Seed UserOrganisation: Approver for Org A, End User for Org B
        await this.InsertEntitiesAsync<DbOrganisationsContext, UserOrganisationEntity>([
            EntityFaker.UserOrganisation
                .RuleFor(uo => uo.UserId, _ => userId)
                .RuleFor(uo => uo.OrganisationId, _ => orgA.Id)
                .RuleFor(uo => uo.RoleId, _ => OrganisationRole.Approver.Value)
                .RuleFor(uo => uo.Status, _ => ActiveUserOrganisationStatus)
                .Generate(),

            EntityFaker.UserOrganisation
                .RuleFor(uo => uo.UserId, _ => userId)
                .RuleFor(uo => uo.OrganisationId, _ => orgB.Id)
                .RuleFor(uo => uo.RoleId, _ => OrganisationRole.EndUser.Value)
                .RuleFor(uo => uo.Status, _ => ActiveUserOrganisationStatus)
                .Generate()
        ]);

        // Org A: 2 pending requests
        await this.InsertEntityAsync<DbOrganisationsContext, UserServiceRequestEntity>(EntityFaker.UserServiceRequest
            .RuleFor(sr => sr.OrganisationId, _ => orgA.Id)
            .Generate());

        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationRequestEntity>(EntityFaker.UserOrganisationRequest
            .RuleFor(ur => ur.OrganisationId, _ => orgA.Id)
            .Generate());

        // Org B (End User): 1 pending request
        await this.InsertEntityAsync<DbOrganisationsContext, UserServiceRequestEntity>(EntityFaker.UserServiceRequest
            .RuleFor(sr => sr.OrganisationId, _ => orgB.Id)
            .Generate());

        // Org C (Unassociated): 1 pending request
        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationRequestEntity>(EntityFaker.UserOrganisationRequest
            .RuleFor(ur => ur.OrganisationId, _ => orgC.Id)
            .Generate());

        var response = await authenticatedClient.GetAsync(GetEndpoint(userId));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PendingApprovalCountResponse>();
        Assert.NotNull(body);
        Assert.Equal(2, body.Count);
    }

    [Fact]
    public async Task GetPendingApprovalCount_ReturnsZero_WhenUserIsEndUserOnly()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var userId = Guid.NewGuid();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(EntityFaker.User
            .RuleFor(u => u.Sub, _ => userId)
            .Generate());

        var org = EntityFaker.Organisation.Generate();

        await this.InsertEntityAsync<DbOrganisationsContext, OrganisationEntity>(org);

        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationEntity>(EntityFaker.UserOrganisation
            .RuleFor(uo => uo.UserId, _ => userId)
            .RuleFor(uo => uo.OrganisationId, _ => org.Id)
            .RuleFor(uo => uo.RoleId, _ => OrganisationRole.EndUser.Value)
            .RuleFor(uo => uo.Status, _ => ActiveUserOrganisationStatus)
            .Generate());

        // Seed pending request for the organisation
        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationRequestEntity>(EntityFaker.UserOrganisationRequest
            .RuleFor(ur => ur.OrganisationId, _ => org.Id)
            .RuleFor(ur => ur.Status, _ => OrganisationRequestStatus.Pending.Value)
            .Generate());

        var response = await authenticatedClient.GetAsync(GetEndpoint(userId));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PendingApprovalCountResponse>();
        Assert.NotNull(body);
        Assert.Equal(0, body.Count);
    }

    [Fact]
    public async Task GetPendingApprovalCount_ReturnsZero_WhenUserHasNoAssociatedOrganisations()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var userId = Guid.NewGuid();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(EntityFaker.User
            .RuleFor(u => u.Sub, _ => userId)
            .Generate());

        var response = await authenticatedClient.GetAsync(GetEndpoint(userId));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<PendingApprovalCountResponse>();
        Assert.NotNull(body);
        Assert.Equal(0, body.Count);
    }

    [Fact]
    public async Task GetPendingApprovalCount_Returns400_WhenUserIdIsEmpty()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var response = await authenticatedClient.GetAsync(GetEndpoint(Guid.Empty));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingApprovalCount_Returns404_WhenUserIdIsInvalid()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var response = await authenticatedClient.GetAsync(GetEndpoint(Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingApprovalCount_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var response = await anonymousClient.GetAsync(GetEndpoint(Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
