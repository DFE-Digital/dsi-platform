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
public class PendingApprovalCountTests : InternalApiIntegrationEndpointTestBase
{
    private const string endpoint = "interaction/Users.GetPendingApprovalCount";
    private const short ActiveUserOrganisationStatus = 1;

    public PendingApprovalCountTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetPendingApprovalCount_ReturnsCorrectSum_WhenUserIsApproverWithPendingRequests()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var userId = Guid.NewGuid();
        var org = EntityFaker.Organisation.Generate();

        // Seed Organisation
        await this.InsertEntityAsync<DbOrganisationsContext, OrganisationEntity>(org);

        // Seed UserOrganisation as Approver
        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationEntity>(new UserOrganisationEntity {
            UserId = userId,
            OrganisationId = org.Id,
            RoleId = OrganisationRoles.Approver.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = ActiveUserOrganisationStatus
        });

        // Seed 2 active pending service requests & 1 active pending org request
        await this.InsertEntitiesAsync<DbOrganisationsContext, UserServiceRequestEntity>([
            new UserServiceRequestEntity {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ServiceId = Guid.NewGuid(),
                OrganisationId = org.Id,
                Status = 0,
                RequestType = "ServiceAccess",
                ActionedAt = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            new UserServiceRequestEntity {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ServiceId = Guid.NewGuid(),
                OrganisationId = org.Id,
                Status = 0,
                RequestType = "ServiceAccess",
                ActionedAt = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // Seed 1 actioned service request (should be ignored)
            new UserServiceRequestEntity {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                ServiceId = Guid.NewGuid(),
                OrganisationId = org.Id,
                Status = 1,
                RequestType = "ServiceAccess",
                ActionedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ]);

        await this.InsertEntitiesAsync<DbOrganisationsContext, UserOrganisationRequestEntity>([
            new UserOrganisationRequestEntity {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                OrganisationId = org.Id,
                Status = 0,
                ActionedAt = null,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            },
            // Seed 1 actioned organisation request (should be ignored)
            new UserOrganisationRequestEntity {
                Id = Guid.NewGuid(),
                UserId = Guid.NewGuid(),
                OrganisationId = org.Id,
                Status = 1,
                ActionedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        ]);

        var request = new GetPendingApprovalCountRequest { UserId = userId };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<PendingApprovalCountResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.Equal(3, body.Data.Count);
    }

    [Fact]
    public async Task GetPendingApprovalCount_OnlyCountsRequestsForOrgsWhereUserIsApprover()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var userId = Guid.NewGuid();
        var orgA = EntityFaker.Organisation.Generate(); // User is Approver
        var orgB = EntityFaker.Organisation.Generate(); // User is End User
        var orgC = EntityFaker.Organisation.Generate(); // User is Not Associated

        await this.InsertEntitiesAsync<DbOrganisationsContext, OrganisationEntity>([orgA, orgB, orgC]);

        // Seed UserOrganisation: Approver for Org A, End User for Org B
        await this.InsertEntitiesAsync<DbOrganisationsContext, UserOrganisationEntity>([
            new UserOrganisationEntity {
                UserId = userId,
                OrganisationId = orgA.Id,
                RoleId = OrganisationRoles.Approver.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = ActiveUserOrganisationStatus
            },
            new UserOrganisationEntity {
                UserId = userId,
                OrganisationId = orgB.Id,
                RoleId = OrganisationRoles.EndUser.Id,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = ActiveUserOrganisationStatus
            }
        ]);

        // Org A: 2 pending requests
        await this.InsertEntityAsync<DbOrganisationsContext, UserServiceRequestEntity>(new UserServiceRequestEntity {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            OrganisationId = orgA.Id,
            Status = 0,
            RequestType = "ServiceAccess",
            ActionedAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationRequestEntity>(new UserOrganisationRequestEntity {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            OrganisationId = orgA.Id,
            Status = 0,
            ActionedAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Org B (End User): 3 pending requests
        await this.InsertEntityAsync<DbOrganisationsContext, UserServiceRequestEntity>(new UserServiceRequestEntity {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            ServiceId = Guid.NewGuid(),
            OrganisationId = orgB.Id,
            Status = 0,
            RequestType = "ServiceAccess",
            ActionedAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        // Org C (Unassociated): 4 pending requests
        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationRequestEntity>(new UserOrganisationRequestEntity {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            OrganisationId = orgC.Id,
            Status = 0,
            ActionedAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var request = new GetPendingApprovalCountRequest { UserId = userId };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<PendingApprovalCountResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.Equal(2, body.Data.Count);
    }

    [Fact]
    public async Task GetPendingApprovalCount_ReturnsZero_WhenUserIsEndUserOnly()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var userId = Guid.NewGuid();
        var org = EntityFaker.Organisation.Generate();

        await this.InsertEntityAsync<DbOrganisationsContext, OrganisationEntity>(org);

        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationEntity>(new UserOrganisationEntity {
            UserId = userId,
            OrganisationId = org.Id,
            RoleId = OrganisationRoles.EndUser.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = ActiveUserOrganisationStatus
        });

        // Seed pending request for the organisation
        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationRequestEntity>(new UserOrganisationRequestEntity {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            OrganisationId = org.Id,
            Status = 0,
            ActionedAt = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });

        var request = new GetPendingApprovalCountRequest { UserId = userId };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<PendingApprovalCountResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.Equal(0, body.Data.Count);
    }

    [Fact]
    public async Task GetPendingApprovalCount_ReturnsZero_WhenUserHasNoAssociatedOrganisations()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var request = new GetPendingApprovalCountRequest { UserId = Guid.NewGuid() };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<PendingApprovalCountResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.Equal(0, body.Data.Count);
    }

    [Fact]
    public async Task GetPendingApprovalCount_Returns400_WhenUserIdIsEmpty()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var request = new GetPendingApprovalCountRequest { UserId = Guid.Empty };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<PendingApprovalCountResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.Equal(0, body.Data.Count);
    }

    [Fact]
    public async Task GetPendingApprovalCount_Returns400_WhenRequestBodyIsInvalid()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, "");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingApprovalCount_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var request = new GetPendingApprovalCountRequest { UserId = Guid.NewGuid() };

        var response = await anonymousClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
