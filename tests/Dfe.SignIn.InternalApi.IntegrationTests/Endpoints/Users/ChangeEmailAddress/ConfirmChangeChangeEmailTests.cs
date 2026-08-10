using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users.ChangeEmailAddress;

[Trait("Category", "Integration")]
public sealed class ConfirmChangeChangeEmailTests : InternalApiIntegrationEndpointTestBase
{
    private const string endpoint = "/internal/users/{userId}/confirm-change-email";

    private static string GetEndpointForUser(Guid userId) => endpoint.Replace("{userId}", userId.ToString());

    public ConfirmChangeChangeEmailTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ConfirmChangeEmail_ReturnsSuccess_UpdatesEmail_DeletesCode_AndWritesAudit_WhenCodeValid()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        var pendingCode = new UserCodeEntity
        {
            Uid = user.Sub,
            CodeType = "changeemail",
            Code = "ABC1234",
            Email = "john.doe@new.example.com",
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var request = CreateConfirmRequest(user.Sub, "ABC1234");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        // Assert user email is updated
        var updatedUser = await assertionDbContext.Users.SingleOrDefaultAsync(x => x.Sub == user.Sub);
        Assert.NotNull(updatedUser);
        Assert.Equal("john.doe@new.example.com", updatedUser.Email);

        // Assert pending code is deleted
        var dbCode = await GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.Null(dbCode);

        // Assert audit event is written
        var auditRequest = auditMock.CapturedRequest;
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeEmail, auditRequest.EventCategory);
        Assert.Equal($"Successfully changed email to john.doe@new.example.com", auditRequest.Message);
        Assert.Equal(user.Sub, auditRequest.UserId);

        var editedFields = Assert.Single(auditRequest.CustomProperties, x => x.Key == "editedFields");
        Assert.NotNull(editedFields.Value);
    }

    [Fact]
    public async Task ConfirmChangeEmail_UpdatesOnlyTargetUsersEmail_WithoutMutatingUnrelatedFields()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var targetUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "target.old@example.com")
            .RuleFor(x => x.FirstName, (_, _) => "TargetFirstName")
            .RuleFor(x => x.LastName, (_, _) => "TargetLastName")
            .Generate();

        var bystanderUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "bystander@example.com")
            .Generate();

        var pendingCode = new UserCodeEntity
        {
            Uid = targetUser.Sub,
            CodeType = "changeemail",
            Code = "XYZ789",
            Email = "target.new@example.com",
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await this.InsertEntitiesAsync<DbDirectoriesContext, UserEntity>([targetUser, bystanderUser]);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var request = CreateConfirmRequest(targetUser.Sub, "XYZ789");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(targetUser.Sub), request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        // Target user email is updated, but names are not
        var updatedTarget = await assertionDbContext.Users.SingleOrDefaultAsync(x => x.Sub == targetUser.Sub);
        Assert.NotNull(updatedTarget);
        Assert.Equal("target.new@example.com", updatedTarget.Email);
        Assert.Equal("TargetFirstName", updatedTarget.FirstName);
        Assert.Equal("TargetLastName", updatedTarget.LastName);

        // Bystander is untouched
        var updatedBystander = await assertionDbContext.Users.SingleOrDefaultAsync(x => x.Sub == bystanderUser.Sub);
        Assert.NotNull(updatedBystander);
        Assert.Equal("bystander@example.com", updatedBystander.Email);
    }

    [Fact]
    public async Task ConfirmChangeEmail_Returns400_WhenVerificationCodeMissing()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = CreateConfirmRequest(user.Sub, string.Empty);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmChangeEmail_Returns400_AndWritesFailureAudit_WhenVerificationCodeInvalid()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        var pendingCode = new UserCodeEntity
        {
            Uid = user.Sub,
            CodeType = "changeemail",
            Code = "CORRECT",
            Email = "john.doe@new.example.com",
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var request = CreateConfirmRequest(user.Sub, "WRONG");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        // Email remains unchanged
        var dbUser = await assertionDbContext.Users.SingleOrDefaultAsync(x => x.Sub == user.Sub);
        Assert.NotNull(dbUser);
        Assert.Equal("john.doe@old.example.com", dbUser.Email);

        // Pending code still exists
        var dbCode = await GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.NotNull(dbCode);

        // Failure audit is written
        var failureAudit = Assert.Single(auditMock.CapturedRequests, x => x.EventName == AuditChangeEmailEventNames.EmailChangeFailed);
        Assert.True(failureAudit.WasFailure);
        Assert.Equal($"Failed changed email to john.doe@new.example.com - invalid code", failureAudit.Message);
        Assert.Equal(user.Sub, failureAudit.UserId);
    }

    [Fact]
    public async Task ConfirmChangeEmail_Returns400_AndWritesExpiredAudit_WhenVerificationCodeExpired()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        var expiredCode = new UserCodeEntity
        {
            Uid = user.Sub,
            CodeType = "changeemail",
            Code = "VALIDCODE",
            Email = "john.doe@new.example.com",
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            UpdatedAt = DateTime.UtcNow.AddHours(-2),
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(expiredCode);

        var request = CreateConfirmRequest(user.Sub, "VALIDCODE");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        // Email remains unchanged
        var dbUser = await assertionDbContext.Users.SingleOrDefaultAsync(x => x.Sub == user.Sub);
        Assert.NotNull(dbUser);
        Assert.Equal("john.doe@old.example.com", dbUser.Email);

        // Pending code still exists
        var dbCode = await GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.NotNull(dbCode);

        // Expired audit is written
        var expiredAudit = Assert.Single(auditMock.CapturedRequests, x => x.EventName == AuditChangeEmailEventNames.EnteredExpiredCode);
        Assert.True(expiredAudit.WasFailure);
        Assert.Contains("expired", expiredAudit.Message);
        Assert.Equal(user.Sub, expiredAudit.UserId);
    }

    [Fact]
    public async Task ConfirmChangeEmail_ReturnsNoPendingMappedStatus_WhenNoPendingChange()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = CreateConfirmRequest(user.Sub, "ANYCODE");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        // Mapped status should be a non-success code, e.g., BadRequest or NotFound.
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact(Skip = "External auth service/interface does not exist yet on .NET platform")]
    public async Task ConfirmChangeEmail_MapsAuthMethodUpdateFailure_AsCurrentBehaviour()
    {
        // Replicating patchUser.js Entra MFA failure logic (retains DB update, returns error)
        await Task.CompletedTask;
    }

    [Fact]
    public async Task ConfirmChangeEmail_WritesFailureAudit_WhenDownstreamPatchFails()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        // Target user with a pending change
        var targetUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "target@example.com")
            .Generate();

        // Conflict user who already owns the new email, causing a DB unique constraint failure
        var conflictUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "conflict@example.com")
            .Generate();

        var pendingCode = new UserCodeEntity
        {
            Uid = targetUser.Sub,
            CodeType = "changeemail",
            Code = "CODE123",
            Email = "conflict@example.com", // causes DB update collision
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await this.InsertEntitiesAsync<DbDirectoriesContext, UserEntity>([targetUser, conflictUser]);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var request = CreateConfirmRequest(targetUser.Sub, "CODE123");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(targetUser.Sub), request);

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        // Email remains unchanged
        var dbUser = await assertionDbContext.Users.SingleOrDefaultAsync(x => x.Sub == targetUser.Sub);
        Assert.NotNull(dbUser);
        Assert.Equal("target@example.com", dbUser.Email);

        // Pending code still exists
        var dbCode = await GetChangeEmailCode(assertionDbContext, targetUser.Sub);
        Assert.NotNull(dbCode);

        // Failure audit is written
        var failureAudit = Assert.Single(auditMock.CapturedRequests, x => x.EventName == AuditChangeEmailEventNames.EmailChangeFailed);
        Assert.True(failureAudit.WasFailure);
        Assert.Contains("Failed changed email", failureAudit.Message);
        Assert.Equal(targetUser.Sub, failureAudit.UserId);
    }

    [Fact]
    public async Task ConfirmChangeEmail_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var userId = Guid.NewGuid();
        var request = CreateConfirmRequest(userId, "ANYCODE");

        var response = await anonymousClient.PostAsJsonAsync(GetEndpointForUser(userId), request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmChangeEmail_DoesNotUpdateEmail_WhenVerificationCodeInvalidOrExpired()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        // 1. Wrong code scenario
        var pendingCode = new UserCodeEntity
        {
            Uid = user.Sub,
            CodeType = "changeemail",
            Code = "CORRECT",
            Email = "john.doe@new.example.com",
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var requestWrong = CreateConfirmRequest(user.Sub, "WRONG");
        var responseWrong = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), requestWrong);
        Assert.Equal(HttpStatusCode.BadRequest, responseWrong.StatusCode);

        await using (var scope = this.WebAppFactory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            var dbUser = await db.Users.SingleAsync(x => x.Sub == user.Sub);
            Assert.Equal("john.doe@old.example.com", dbUser.Email);
        }

        // 2. Expired code scenario
        pendingCode.CreatedAt = DateTime.UtcNow.AddHours(-2);
        pendingCode.UpdatedAt = DateTime.UtcNow.AddHours(-2);
        
        await using (var scope = this.WebAppFactory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            db.UserCodes.Update(pendingCode);
            await db.SaveChangesAsync();
        }

        var requestExpired = CreateConfirmRequest(user.Sub, "CORRECT");
        var responseExpired = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), requestExpired);
        Assert.Equal(HttpStatusCode.BadRequest, responseExpired.StatusCode);

        await using (var scope = this.WebAppFactory.Services.CreateAsyncScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            var dbUser = await db.Users.SingleAsync(x => x.Sub == user.Sub);
            Assert.Equal("john.doe@old.example.com", dbUser.Email);
        }
    }

    [Fact]
    public async Task ConfirmChangeEmail_DoesNotDeletePendingCode_WhenCommitFails()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var targetUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "target@example.com")
            .Generate();

        var conflictUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "conflict@example.com")
            .Generate();

        var pendingCode = new UserCodeEntity
        {
            Uid = targetUser.Sub,
            CodeType = "changeemail",
            Code = "CODE123",
            Email = "conflict@example.com", // causes DB collision on update
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await this.InsertEntitiesAsync<DbDirectoriesContext, UserEntity>([targetUser, conflictUser]);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var request = CreateConfirmRequest(targetUser.Sub, "CODE123");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(targetUser.Sub), request);

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        // Pending code must still exist
        var dbCode = await GetChangeEmailCode(assertionDbContext, targetUser.Sub);
        Assert.NotNull(dbCode);
    }

    [Fact]
    public async Task ConfirmChangeEmail_DoesNotWriteSuccessAudit_OnFailurePaths()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var user = EntityFaker.User.Generate();
        var pendingCode = new UserCodeEntity
        {
            Uid = user.Sub,
            CodeType = "changeemail",
            Code = "CODE123",
            Email = "new@example.com",
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var request = CreateConfirmRequest(user.Sub, "WRONGCODE");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Verify there is no success audit captured
        Assert.DoesNotContain(auditMock.CapturedRequests, x => x.Message.Contains("Successfully changed email"));
    }

    [Fact]
    public async Task ConfirmChangeEmail_DoesNotSendEmailOrNotification_OnFailure_WhenApplicable()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();
        this.FakeEmailRequestTracker.Clear();

        var user = EntityFaker.User.Generate();
        var pendingCode = new UserCodeEntity
        {
            Uid = user.Sub,
            CodeType = "changeemail",
            Code = "CODE123",
            Email = "new@example.com",
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var request = CreateConfirmRequest(user.Sub, "WRONGCODE");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Verification email/notifications should not be sent
        Assert.Empty(this.FakeEmailRequestTracker.Requests);
    }

    private static ConfirmChangeEmailAddressRequest CreateConfirmRequest(Guid userId, string verificationCode)
        => new() { UserId = userId, VerificationCode = verificationCode };

    private static async Task<UserCodeEntity?> GetChangeEmailCode(DbDirectoriesContext dbContext, Guid userId)
        => await dbContext.UserCodes.SingleOrDefaultAsync(x => x.Uid == userId && x.CodeType == "changeemail");
}
