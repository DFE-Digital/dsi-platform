using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Contracts;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait("Category", "Integration")]
public class ChangeJobTitleTests : InternalApiIntegrationEndpointTestBase
{
    public ChangeJobTitleTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ChangeJobTitle_ReturnsSuccess_UpdatesDb_AndWritesAudit_WhenUserExists()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var expectedJobTitle = "Senior Software Developer";
        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => "Old Title")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new ChangeJobTitleRequest {
            UserId = user.Sub,
            NewJobTitle = expectedJobTitle
        };

        var response = await authenticatedClient.PostAsJsonAsync("interaction/Users.ChangeJobTitle", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<ChangeJobTitleResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        Assert.Equal(expectedJobTitle, updatedUser.JobTitle);

        var auditRequest = auditMock.CapturedRequest;
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeJobTitle, auditRequest.EventCategory);
        Assert.Equal($"Successfully changed job title to {expectedJobTitle}", auditRequest.Message);
        Assert.Equal(user.Sub, auditRequest.UserId);
    }

    [Fact]
    public async Task ChangeJobTitle_Returns404_WhenUserDoesNotExist()
    {
        var authenticatedClient = this.CreateClient().Authenticate();

        var request = new ChangeJobTitleRequest {
            UserId = Guid.NewGuid(),
            NewJobTitle = "Senior Software Developer"
        };

        var response = await authenticatedClient.PostAsJsonAsync("interaction/Users.ChangeJobTitle", request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangeJobTitle_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var request = new ChangeJobTitleRequest {
            UserId = Guid.NewGuid(),
            NewJobTitle = "Senior Software Developer"
        };

        var response = await anonymousClient.PostAsJsonAsync("interaction/Users.ChangeJobTitle", request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeJobTitle_DoesNotWriteAuditEvent_WhenTitleIsUnchanged()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var jobTitle = "Senior Software Developer";
        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => jobTitle)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new ChangeJobTitleRequest {
            UserId = user.Sub,
            NewJobTitle = jobTitle
        };

        var response = await authenticatedClient.PostAsJsonAsync("interaction/Users.ChangeJobTitle", request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<ChangeJobTitleResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        Assert.Equal(jobTitle, updatedUser.JobTitle);

        var auditRequest = auditMock.CapturedRequest;
        Assert.Null(auditRequest);
    }

    [Fact(Skip = "TODO: verify whitespace normalisation")]
    public async Task ChangeJobTitle_NormalisesWhitespaceBeforeSaving()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "TODO: verify other users are untouched")]
    public async Task ChangeJobTitle_LeavesOtherUsersUnchanged()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "TODO: verify validation failure")]
    public async Task ChangeJobTitle_Returns400_WhenNewJobTitleIsInvalid()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "TODO: verify max-length validation failure")]
    public async Task ChangeJobTitle_Returns400_WhenNewJobTitleExceedsMaxLength()
    {
        await Task.CompletedTask;
    }
}
