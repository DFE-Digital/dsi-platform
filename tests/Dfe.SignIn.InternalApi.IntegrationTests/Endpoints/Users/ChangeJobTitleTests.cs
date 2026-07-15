using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Base.Framework;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.InternalApi.Contracts;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Dfe.SignIn.TestHelpers.Integration;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait("Category", "Integration")]
public class ChangeJobTitleTests : IntegrationEndpointTestBase
{
    public ChangeJobTitleTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ChangeJobTitle_ReturnsSuccess_WhenUserExists()
    {
        var capturedAudit = new CapturingWriteToAuditInteractor();
        using var customisedFactory = this.WebAppFactory.WithWebHostBuilder(builder => {
            builder.ConfigureTestServices(services => {
                services.RemoveAll<IInteractor<WriteToAuditRequest>>();
                services.AddSingleton<IInteractor<WriteToAuditRequest>>(capturedAudit);
            });
        });

        var authenticatedClient = customisedFactory.CreateClient();
        authenticatedClient.DefaultRequestHeaders.Add(TestAuthHandler.EnableAuthHeaderName, bool.TrueString);

        var userId = Guid.NewGuid();
        var expectedJobTitle = "Senior Software Developer";

        await using var scope = this.WebAppFactory.Services.CreateAsyncScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        dbContext.Users.Add(new UserEntity {
            Sub = userId,
            Email = "alex.johnson@example.com",
            FirstName = "Alex",
            LastName = "Johnson",
            Password = "",
            Salt = "",
            Status = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            JobTitle = "Software Developer"
        });
        await dbContext.SaveChangesAsync();

        var request = new ChangeJobTitleRequest {
            UserId = userId,
            NewJobTitle = expectedJobTitle
        };

        var response = await authenticatedClient.PostAsJsonAsync("interaction/Users.ChangeJobTitle", request);

        if (response.StatusCode != HttpStatusCode.OK) {
            var errorContent = await response.Content.ReadAsStringAsync();
            Assert.Fail($"Request failed with status {response.StatusCode}. Response: {errorContent}");
        }

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<ChangeJobTitleResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);

        await using var assertionScope = customisedFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == userId);
        Assert.Equal(expectedJobTitle, updatedUser.JobTitle);

        var auditRequest = capturedAudit.CapturedRequest;
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeJobTitle, auditRequest.EventCategory);
        Assert.Equal($"Successfully changed job title to {expectedJobTitle}", auditRequest.Message);
        Assert.Equal(userId, auditRequest.UserId);
    }

    [Fact(Skip = "TODO: implement missing-user assertion")]
    public async Task ChangeJobTitle_Returns404_WhenUserDoesNotExist()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "TODO: implement auth assertion")]
    public async Task ChangeJobTitle_Returns401_WhenUnauthenticated()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "TODO: verify database update")]
    public async Task ChangeJobTitle_UpdatesJobTitleInDatabase()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "TODO: verify audit event")]
    public async Task ChangeJobTitle_WritesAuditEvent_OnSuccessfulUpdate()
    {
        await Task.CompletedTask;
    }

    [Fact(Skip = "TODO: verify unchanged-title branch")]
    public async Task ChangeJobTitle_DoesNotWriteAuditEvent_WhenTitleIsUnchanged()
    {
        await Task.CompletedTask;
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

    private sealed class CapturingWriteToAuditInteractor : IInteractor<WriteToAuditRequest>
    {
        public WriteToAuditRequest? CapturedRequest { get; private set; }

        public Task<object> InvokeAsync(InteractionContext<WriteToAuditRequest> context, CancellationToken cancellationToken = default)
        {
            this.CapturedRequest = context.Request;
            return Task.FromResult<object>(new WriteToAuditResponse());
        }
    }
}
