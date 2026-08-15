using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Organisations;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Organisations;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait( "Category", "Integration" )]
public class IsOrganisationApproverTests : InternalApiIntegrationEndpointTestBase
{
    private const short ActiveUserOrganisationStatus = 1;

    public IsOrganisationApproverTests( InternalApiWebApplicationFactory factory )
        : base( factory )
    {
    }

    [Fact]
    public async Task IsOrganisationApprover_ReturnsTrue_WhenUserIsApprover()
    {
        var userId = Guid.NewGuid();

        var url = UsersApiRoutes.IsApprover
            .Replace( "{userId}", userId.ToString() );

        var authenticatedClient = this.CreateClient()
            .WithAuthentication( userId.ToString() );

        var org = EntityFaker.Organisation.Generate();

        // Seed Organisation
        await this.InsertEntityAsync<DbOrganisationsContext, OrganisationEntity>( org );

        // Seed UserOrganisation as Approver (RoleId = 10000)
        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationEntity>( new UserOrganisationEntity {
            UserId = userId,
            OrganisationId = org.Id,
            RoleId = OrganisationRole.Approver.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = ActiveUserOrganisationStatus
        } );

        var response = await authenticatedClient.GetAsync( url );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        var body = await response.Content.ReadFromJsonAsync<IsOrganisationApproverResponse>();
        Assert.NotNull( body );
        Assert.True( body.IsApprover );
    }

    [Fact]
    public async Task IsOrganisationApprover_ReturnsTrue_WhenUserHasMultipleOrgsAndOneIsApprover()
    {
        var userId = Guid.NewGuid();

        var url = UsersApiRoutes.IsApprover
            .Replace( "{userId}", userId.ToString() );

        var authenticatedClient = this.CreateClient()
            .WithAuthentication( userId.ToString() );

        var organisations = EntityFaker.Organisation.Generate( 2 );
        var org1 = organisations[0];
        var org2 = organisations[1];

        // Seed Organisations
        await this.InsertEntitiesAsync<DbOrganisationsContext, OrganisationEntity>( [org1, org2] );

        // Seed UserOrganisations
        await this.InsertEntitiesAsync<DbOrganisationsContext, UserOrganisationEntity>( [
            new UserOrganisationEntity {
                UserId = userId,
                OrganisationId = org1.Id,
                RoleId = OrganisationRole.EndUser.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = ActiveUserOrganisationStatus
            },
            new UserOrganisationEntity {
                UserId = userId,
                OrganisationId = org2.Id,
                RoleId = OrganisationRole.Approver.Value,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                Status = ActiveUserOrganisationStatus
            }
        ] );

        var response = await authenticatedClient.GetAsync( url );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        var body = await response.Content.ReadFromJsonAsync<IsOrganisationApproverResponse>();
        Assert.NotNull( body );
        Assert.True( body.IsApprover );
    }

    [Fact]
    public async Task IsOrganisationApprover_ReturnsFalse_WhenUserIsEndUser()
    {
        var userId = Guid.NewGuid();

        var url = UsersApiRoutes.IsApprover
            .Replace( "{userId}", userId.ToString() );

        var authenticatedClient = this.CreateClient()
            .WithAuthentication( userId.ToString() );

        var org = EntityFaker.Organisation.Generate();

        // Seed Organisation
        await this.InsertEntityAsync<DbOrganisationsContext, OrganisationEntity>( org );

        // Seed UserOrganisation as End User (RoleId = 0)
        await this.InsertEntityAsync<DbOrganisationsContext, UserOrganisationEntity>( new UserOrganisationEntity {
            UserId = userId,
            OrganisationId = org.Id,
            RoleId = OrganisationRole.EndUser.Value,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            Status = ActiveUserOrganisationStatus
        } );

        var response = await authenticatedClient.GetAsync( url );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        var body = await response.Content.ReadFromJsonAsync<IsOrganisationApproverResponse>();
        Assert.NotNull( body );
        Assert.False( body.IsApprover );
    }

    [Fact]
    public async Task IsOrganisationApprover_ReturnsFalse_WhenUserHasNoAssociatedOrganisations()
    {
        var authenticatedClient = this.CreateClient()
            .WithAuthentication();

        var url = UsersApiRoutes.IsApprover
            .Replace( "{userId}", Guid.NewGuid().ToString() );

        var response = await authenticatedClient.GetAsync( url );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        var body = await response.Content.ReadFromJsonAsync<IsOrganisationApproverResponse>();
        Assert.NotNull( body );
        Assert.False( body.IsApprover );
    }

    [Fact]
    public async Task IsOrganisationApprover_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var url = UsersApiRoutes.IsApprover
           .Replace( "{userId}", Guid.NewGuid().ToString() );

        var response = await anonymousClient.GetAsync( url );

        Assert.Equal( HttpStatusCode.Unauthorized, response.StatusCode );
    }
}
