using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait( "Category", "Integration" )]
public class ChangeJobTitleTests : InternalApiIntegrationEndpointTestBase
{
    public ChangeJobTitleTests( InternalApiWebApplicationFactory factory )
        : base( factory )
    {
    }

    [Fact]
    public async Task ChangeJobTitle_ReturnsSuccess_UpdatesDb_AndWritesAudit_WhenUserExists()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .WithAuditMock()
            .Build();

        var authenticatedClient = testContext.Client;
        var auditMock = testContext.AuditMock;

        var expectedJobTitle = "Senior Software Developer";
        var user = EntityFaker.User
            .RuleFor( x => x.JobTitle, ( _, _ ) => "Old Title" )
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var request = new ChangeJobTitleRequest {
            NewJobTitle = expectedJobTitle
        };

        var url = UsersApiRoutes.ChangeJobTitle
    .Replace( "{userId}", user.Sub.ToString() );

        var response = await authenticatedClient.PostAsJsonAsync( url, request );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync( x => x.Sub == user.Sub );
        Assert.Equal( expectedJobTitle, updatedUser.JobTitle );

        var auditRequest = auditMock.CapturedRequest;
        Assert.NotNull( auditRequest );
        Assert.Equal( AuditEventCategoryNames.ChangeJobTitle, auditRequest.EventCategory );
        Assert.Equal( $"Successfully changed job title to {expectedJobTitle}", auditRequest.Message );
        Assert.Equal( user.Sub, auditRequest.UserId );
    }

    [Fact]
    public async Task ChangeJobTitle_Returns404_WhenUserDoesNotExist()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .Build();

        var authenticatedClient = testContext.Client;
        var userId = Guid.NewGuid();

        var request = new ChangeJobTitleRequest {
            NewJobTitle = "Senior Software Developer"
        };

        var url = UsersApiRoutes.ChangeJobTitle
            .Replace( "{userId}", userId.ToString() );

        var response = await authenticatedClient.PostAsJsonAsync( url, request );

        Assert.Equal( HttpStatusCode.NotFound, response.StatusCode );
    }

    [Fact]
    public async Task ChangeJobTitle_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.SetupClient()
            .Build()
            .Client;

        var userId = Guid.NewGuid();

        var request = new ChangeJobTitleRequest {
            NewJobTitle = "Senior Software Developer"
        };

        var url = UsersApiRoutes.ChangeJobTitle
            .Replace( "{userId}", userId.ToString() );

        var response = await anonymousClient.PostAsJsonAsync( url, request );

        Assert.Equal( HttpStatusCode.Unauthorized, response.StatusCode );
    }

    [Fact]
    public async Task ChangeJobTitle_DoesNotWriteAuditEvent_WhenTitleIsUnchanged()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .WithAuditMock()
            .Build();

        var authenticatedClient = testContext.Client;
        var auditMock = testContext.AuditMock;

        var jobTitle = "Senior Software Developer";
        var user = EntityFaker.User
            .RuleFor( x => x.JobTitle, ( _, _ ) => jobTitle )
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var request = new ChangeJobTitleRequest {
            NewJobTitle = jobTitle
        };

        var url = UsersApiRoutes.ChangeJobTitle
            .Replace( "{userId}", user.Sub.ToString() );

        var response = await authenticatedClient.PostAsJsonAsync( url, request );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync( x => x.Sub == user.Sub );
        Assert.Equal( jobTitle, updatedUser.JobTitle );

        var auditRequest = auditMock.CapturedRequest;
        Assert.Null( auditRequest );
    }

    [Fact]
    public async Task ChangeJobTitle_NormalisesWhitespaceBeforeSaving()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .WithAuditMock()
            .Build();

        var authenticatedClient = testContext.Client;
        var auditMock = testContext.AuditMock;

        var newjobTitle = "Senior Software      Developer"; // Intentionally includes multiple spaces
        var expectedJobTitle = "Senior Software Developer";
        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var request = new ChangeJobTitleRequest {
            NewJobTitle = newjobTitle
        };

        var url = UsersApiRoutes.ChangeJobTitle
            .Replace( "{userId}", user.Sub.ToString() );

        var response = await authenticatedClient.PostAsJsonAsync( url, request );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync( x => x.Sub == user.Sub );
        Assert.Equal( expectedJobTitle, updatedUser.JobTitle );
    }

    [Fact]
    public async Task ChangeJobTitle_LeavesOtherUsersUnchanged()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .Build();

        var authenticatedClient = testContext.Client;

        var newJobTitle = "Senior Software Developer";
        var users = EntityFaker.User.Generate( 3 );

        await this.InsertEntitiesAsync<DbDirectoriesContext, UserEntity>( users );

        var userToUpdate = users[1];

        var request = new ChangeJobTitleRequest {
            NewJobTitle = newJobTitle
        };

        var url = UsersApiRoutes.ChangeJobTitle
            .Replace( "{userId}", userToUpdate.Sub.ToString() );

        var response = await authenticatedClient.PostAsJsonAsync( url, request );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var userSubs = users.Select( u => u.Sub ).ToList();
        var assertionUsers = await assertionDbContext.Users
            .Where( x => userSubs.Contains( x.Sub ) )
            .ToListAsync();

        var updatedUser = assertionUsers.Single( x => x.Sub == userToUpdate.Sub );
        Assert.Equal( newJobTitle, updatedUser.JobTitle );

        var unchangedUsers = assertionUsers.Where( x => x.Sub != userToUpdate.Sub ).ToList();
        foreach (var unchangedUser in unchangedUsers) {
            Assert.NotEqual( newJobTitle, unchangedUser.JobTitle );
        }
    }

    [Fact]
    public async Task ChangeJobTitle_Returns400_WhenNewJobTitleIsInvalid()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .Build();

        var authenticatedClient = testContext.Client;

        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var request = new ChangeJobTitleRequest {
            NewJobTitle = "Senior Software Engineer!!!" // Invalid characters in job title
        };

        var url = UsersApiRoutes.ChangeJobTitle
            .Replace( "{userId}", user.Sub.ToString() );

        var response = await authenticatedClient.PostAsJsonAsync( url, request );

        Assert.Equal( HttpStatusCode.BadRequest, response.StatusCode );
    }

    [Fact]
    public async Task ChangeJobTitle_Returns400_WhenNewJobTitleExceedsMaxLength()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .Build();

        var authenticatedClient = testContext.Client;

        var longJobTitle = "Vice President of Global Human Capital Management and Organizational Culture Development";
        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var request = new ChangeJobTitleRequest {
            NewJobTitle = longJobTitle
        };
        var url = UsersApiRoutes.ChangeJobTitle
            .Replace( "{userId}", user.Sub.ToString() );

        var response = await authenticatedClient.PostAsJsonAsync( url, request );

        Assert.Equal( HttpStatusCode.BadRequest, response.StatusCode );
    }
}
