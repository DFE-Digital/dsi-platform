using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
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
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        this.FakeUserUpdatedPublisher.Clear();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        var pendingCode = new UserCodeEntity {
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

        var request = CreateConfirmRequest("ABC1234");

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
        var auditRequest = this.AuditCapturer.CapturedRequests[0];
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeEmail, auditRequest.EventCategory);
        Assert.Equal($"Successfully changed email to john.doe@new.example.com", auditRequest.Message);
        Assert.Equal(user.Sub, auditRequest.UserId);

        var editedFields = Assert.Single(auditRequest.CustomProperties, x => x.Key == "editedFields");
        Assert.NotNull(editedFields.Value);

        // Assert user updated publisher published event
        var (UserId, EmailAddress, FirstName, LastName, Status) = Assert.Single(this.FakeUserUpdatedPublisher.PublishedEvents);
        Assert.Equal(user.Sub, UserId);
        Assert.Equal("john.doe@new.example.com", EmailAddress);
        Assert.Equal(user.FirstName, FirstName);
        Assert.Equal(user.LastName, LastName);
        Assert.Equal(user.Status, Status);
    }

    [Fact]
    public async Task ConfirmChangeEmail_UpdatesOnlyTargetUsersEmail_WithoutMutatingUnrelatedFields()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var targetUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "target.old@example.com")
            .RuleFor(x => x.FirstName, (_, _) => "TargetFirstName")
            .RuleFor(x => x.LastName, (_, _) => "TargetLastName")
            .Generate();

        var bystanderUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "bystander@example.com")
            .Generate();

        var pendingCode = new UserCodeEntity {
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

        var request = CreateConfirmRequest("XYZ789");

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
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = CreateConfirmRequest(string.Empty);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmChangeEmail_Returns400_AndWritesFailureAudit_WhenVerificationCodeInvalid()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        var pendingCode = new UserCodeEntity {
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

        var request = CreateConfirmRequest("WRONG");

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
        var failureAudit = Assert.Single(this.AuditCapturer.CapturedRequests, x => x.EventName == AuditChangeEmailEventNames.EmailChangeFailed);
        Assert.True(failureAudit.WasFailure);
        Assert.Equal($"Failed changed email to john.doe@new.example.com - invalid code", failureAudit.Message);
        Assert.Equal(user.Sub, failureAudit.UserId);
    }

    [Fact]
    public async Task ConfirmChangeEmail_Returns400_AndWritesExpiredAudit_WhenVerificationCodeExpired()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        var expiredCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.CodeType, (_, _) => "changeemail")
            .RuleFor(x => x.Code, (_, _) => "VALIDCODE")
            .RuleFor(x => x.Email, (_, _) => "john.doe@new.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(expiredCode);

        // Override CreatedAt/UpdatedAt using a separate DbContext context to bypass TimestampInterceptor State == EntityState.Added overwrite
        await using (var scope = this.WebAppFactory.Services.CreateAsyncScope()) {
            var db = scope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            var codeToExpire = await db.UserCodes.SingleAsync(x => x.Uid == user.Sub && x.CodeType == "changeemail");
            codeToExpire.CreatedAt = DateTime.UtcNow.AddHours(-2);
            codeToExpire.UpdatedAt = DateTime.UtcNow.AddHours(-2);
            await db.SaveChangesAsync();
        }

        var request = CreateConfirmRequest("VALIDCODE");

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
        var expiredAudit = Assert.Single(this.AuditCapturer.CapturedRequests, x => x.EventName == AuditChangeEmailEventNames.EnteredExpiredCode);
        Assert.True(expiredAudit.WasFailure);
        Assert.Contains("expired", expiredAudit.Message);
        Assert.Equal(user.Sub, expiredAudit.UserId);
    }

    [Fact]
    public async Task ConfirmChangeEmail_ReturnsNoPendingMappedStatus_WhenNoPendingChange()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = CreateConfirmRequest("ANYCODE");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        // Mapped status should be a non-success code, e.g., BadRequest or NotFound.
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmChangeEmail_MapsAuthMethodUpdateFailure_AsCurrentBehaviour()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .RuleFor(x => x.IsEntra, (_, _) => true)
            .RuleFor(x => x.EntraOid, (_, _) => Guid.NewGuid())
            .Generate();

        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.CodeType, (_, _) => "changeemail")
            .RuleFor(x => x.Code, (_, _) => "ABC1234")
            .RuleFor(x => x.Email, (_, _) => "john.doe@new.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        // Set up the fake to throw the expected exception
        this.FakeExternalAuthService.OnChangeEmail = (externalUserId, newEmail, ct) =>
            throw new FailedToUpdateAuthenticationMethodException(user.Sub);

        var request = CreateConfirmRequest("ABC1234");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        // Check mapped JSON error matches legacy contract
        var error = await response.Content.ReadFromJsonAsync<ErrorMessageDto>();
        Assert.NotNull(error);
        Assert.Equal("ChangeEmailAddressAuthenticationMethodError", error.Type);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        // Retains DB update (not rolled back)
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        Assert.Equal("john.doe@new.example.com", updatedUser.Email);

        // Failure audit is logged
        var failureAudit = Assert.Single(this.AuditCapturer.CapturedRequests, x => x.EventName == AuditChangeEmailEventNames.EmailChangeFailed);
        Assert.True(failureAudit.WasFailure);
        Assert.Contains("FailedToUpdateAuthenticationMethodException", failureAudit.Message);
    }

    [Fact]
    public async Task ConfirmChangeEmail_WritesFailureAudit_WhenDownstreamPatchFails()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.CodeType, (_, _) => "changeemail")
            .RuleFor(x => x.Code, (_, _) => "CODE123")
            .RuleFor(x => x.Email, (_, _) => "john.doe@new.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        // Configure interceptor to simulate database save failure
        this.TimestampInterceptor.Setup(
            onSavingChangesError: () => new DbUpdateException("Simulated database failure during save.", new Exception("Inner database exception constraint violation"))
        );

        var request = CreateConfirmRequest("CODE123");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);

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
        var failureAudit = Assert.Single(this.AuditCapturer.CapturedRequests, x => x.EventName == AuditChangeEmailEventNames.EmailChangeFailed);
        Assert.True(failureAudit.WasFailure);
        Assert.Contains("Failed changed email", failureAudit.Message);
        Assert.Equal(user.Sub, failureAudit.UserId);
    }

    [Fact]
    public async Task ConfirmChangeEmail_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var userId = Guid.NewGuid();
        var request = CreateConfirmRequest("ANYCODE");

        var response = await anonymousClient.PostAsJsonAsync(GetEndpointForUser(userId), request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmChangeEmail_DoesNotUpdateEmail_WhenVerificationCodeInvalidOrExpired()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        // 1. Wrong code scenario
        var pendingCode = new UserCodeEntity {
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

        var requestWrong = CreateConfirmRequest("WRONG");
        var responseWrong = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), requestWrong);

        Assert.Equal(HttpStatusCode.BadRequest, responseWrong.StatusCode);

        await using (var scope = this.WebAppFactory.Services.CreateAsyncScope()) {
            var db = scope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            var dbUser = await db.Users.SingleAsync(x => x.Sub == user.Sub);
            Assert.Equal("john.doe@old.example.com", dbUser.Email);
        }

        // 2. Expired code scenario
        pendingCode.CreatedAt = DateTime.UtcNow.AddHours(-2);
        pendingCode.UpdatedAt = DateTime.UtcNow.AddHours(-2);

        await using (var scope = this.WebAppFactory.Services.CreateAsyncScope()) {
            var db = scope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            db.UserCodes.Update(pendingCode);
            await db.SaveChangesAsync();
        }

        var requestExpired = CreateConfirmRequest("CORRECT");
        var responseExpired = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), requestExpired);
        Assert.Equal(HttpStatusCode.BadRequest, responseExpired.StatusCode);

        await using (var scope = this.WebAppFactory.Services.CreateAsyncScope()) {
            var db = scope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            var dbUser = await db.Users.SingleAsync(x => x.Sub == user.Sub);
            Assert.Equal("john.doe@old.example.com", dbUser.Email);
        }
    }

    [Fact]
    public async Task ConfirmChangeEmail_DoesNotDeletePendingCode_WhenCommitFails()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.CodeType, (_, _) => "changeemail")
            .RuleFor(x => x.Code, (_, _) => "CODE123")
            .RuleFor(x => x.Email, (_, _) => "john.doe@new.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        // Configure interceptor to simulate database save failure

        this.TimestampInterceptor.Setup(
            onSavingChangesError: () => new DbUpdateException("Simulated database failure during save.", new Exception("Inner database exception constraint violation"))
        );

        var request = CreateConfirmRequest("CODE123");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        // Pending code must still exist
        var dbCode = await GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.NotNull(dbCode);
    }

    [Fact]
    public async Task ConfirmChangeEmail_DoesNotWriteSuccessAudit_OnFailurePaths()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();
        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.CodeType, (_, _) => "changeemail")
            .RuleFor(x => x.Code, (_, _) => "CODE123")
            .RuleFor(x => x.Email, (_, _) => "john.doe@new.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var request = CreateConfirmRequest("WRONGCODE");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Verify there is no success audit captured
        Assert.DoesNotContain(this.AuditCapturer.CapturedRequests, x => x.Message.Contains("Successfully changed email"));
    }

    [Fact]
    public async Task ConfirmChangeEmail_DoesNotSendEmailOrNotification_OnFailure_WhenApplicable()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        this.FakeEmailRequestTracker.Clear();

        var user = EntityFaker.User.Generate();
        var pendingCode = new UserCodeEntity {
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

        var request = CreateConfirmRequest("WRONGCODE");

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpointForUser(user.Sub), request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Verification email/notifications should not be sent
        Assert.Empty(this.FakeEmailRequestTracker.Requests);
    }

    private static ConfirmChangeEmailAddressRequest CreateConfirmRequest(string verificationCode)
        => new() { VerificationCode = verificationCode };

    private static async Task<UserCodeEntity?> GetChangeEmailCode(DbDirectoriesContext dbContext, Guid userId)
        => await dbContext.UserCodes.SingleOrDefaultAsync(x => x.Uid == userId && x.CodeType == "changeemail");

    private record ErrorMessageDto(
        [property: JsonPropertyName("type")] string Type,
        [property: JsonPropertyName("message")] string Message);
}
