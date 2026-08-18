using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeJobTitle;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Microsoft.EntityFrameworkCore;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait("Category", "Integration")]
public class ChangeJobTitleTests : InternalApiIntegrationEndpointTestBase
{
    private static string GetEndpoint(Guid? userId) => $"/internal/users/{userId}/change-job-title";

    public ChangeJobTitleTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ChangeJobTitle_ReturnsSuccess_UpdatesDb_AndWritesAudit_WhenUserExists()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var expectedJobTitle = "Senior Software Developer";
        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => "Old Title")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint(user.Sub);
        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new ChangeJobTitleRequest {
            NewJobTitle = expectedJobTitle
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(async dbContext => {
            return await dbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        });

        Assert.Equal(expectedJobTitle, updatedUser.JobTitle);

        var auditRequest = this.AuditCapturer.CapturedRequests[0];
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeJobTitle, auditRequest.EventCategory);
        Assert.Equal($"Successfully changed job title to {expectedJobTitle}", auditRequest.Message);
        Assert.Equal(user.Sub, auditRequest.UserId);
    }

    [Fact]
    public async Task ChangeJobTitle_Returns404_WhenUserDoesNotExist()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var userId = Guid.NewGuid();

        var endpoint = GetEndpoint(userId);
        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new ChangeJobTitleRequest {
            NewJobTitle = "Senior Software Developer"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangeJobTitle_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var userId = Guid.NewGuid();

        var endpoint = GetEndpoint(userId);
        var response = await anonymousClient.PostAsJsonAsync(endpoint, new ChangeJobTitleRequest {
            NewJobTitle = "Senior Software Developer"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeJobTitle_DoesNotWriteAuditEvent_WhenTitleIsUnchanged()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var jobTitle = "Senior Software Developer";
        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => jobTitle)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint(user.Sub);
        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new ChangeJobTitleRequest {
            NewJobTitle = jobTitle
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Empty(this.AuditCapturer.CapturedRequests);

        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(async dbContext => {
            return await dbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        });
        Assert.Equal(jobTitle, updatedUser.JobTitle);
    }

    [Fact]
    public async Task ChangeJobTitle_NormalisesWhitespaceBeforeSaving()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var newjobTitle = "Senior Software      Developer"; // Intentionally includes multiple spaces
        var expectedJobTitle = "Senior Software Developer";
        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint(user.Sub);
        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new ChangeJobTitleRequest {
            NewJobTitle = newjobTitle
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(async dbContext => {
            return await dbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        });
        Assert.Equal(expectedJobTitle, updatedUser.JobTitle);
    }

    [Fact]
    public async Task ChangeJobTitle_LeavesOtherUsersUnchanged()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var newJobTitle = "Senior Software Developer";
        var users = EntityFaker.User.Generate(3);

        await this.InsertEntitiesAsync<DbDirectoriesContext, UserEntity>(users);

        var userToUpdate = users[1];

        var endpoint = GetEndpoint(userToUpdate.Sub);

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new ChangeJobTitleRequest {
            NewJobTitle = newJobTitle
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var userSubs = users.Select(u => u.Sub).ToList();
        var assertionUsers = await this.ExecuteDbContextAsync<DbDirectoriesContext, List<UserEntity>>(db =>
            db.Users.Where(x => userSubs.Contains(x.Sub)).ToListAsync());

        var updatedUser = assertionUsers.Single(x => x.Sub == userToUpdate.Sub);
        Assert.Equal(newJobTitle, updatedUser.JobTitle);

        var unchangedUsers = assertionUsers.Where(x => x.Sub != userToUpdate.Sub).ToList();
        foreach (var unchangedUser in unchangedUsers) {
            Assert.NotEqual(newJobTitle, unchangedUser.JobTitle);
        }
    }

    [Fact]
    public async Task ChangeJobTitle_Returns400_WhenNewJobTitleIsInvalid()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint(user.Sub);
        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new ChangeJobTitleRequest {
            NewJobTitle = "Senior Software Engineer!!!" // Invalid characters in job title
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangeJobTitle_Returns400_WhenNewJobTitleExceedsMaxLength()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var longJobTitle = "Vice President of Global Human Capital Management and Organizational Culture Development";
        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint(user.Sub);
        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new ChangeJobTitleRequest {
            NewJobTitle = longJobTitle
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
