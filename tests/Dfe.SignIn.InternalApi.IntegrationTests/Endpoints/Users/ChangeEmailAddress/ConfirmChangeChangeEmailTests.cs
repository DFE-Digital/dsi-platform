using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Base.Framework.Results;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.Gateways.Entra.ChangeEmail;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Microsoft.EntityFrameworkCore;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users.ChangeEmailAddress;

[Trait("Category", "Integration")]
public sealed class ConfirmChangeChangeEmailTests : InternalApiIntegrationEndpointTestBase
{
    private static string GetEndpoint(Guid? userId) => $"/internal/users/{userId}/confirm-change-email";

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

        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.Code, (_, _) => "ABC1234")
            .RuleFor(x => x.Email, (_, _) => "john.doe@new.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("ABC1234"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ConfirmChangeEmailAddressResponse>();
        Assert.NotNull(content);
        Assert.Equal("john.doe@new.example.com", content.NewEmailAddress);
        Assert.Empty(content.Warnings);

        // Assert user email is updated
        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(
            db => db.Users.SingleAsync(x => x.Sub == user.Sub));

        Assert.NotNull(updatedUser);
        Assert.Equal("john.doe@new.example.com", updatedUser.Email);

        // Assert pending code is deleted
        var dbCode = await this.GetChangeEmailCode(user.Sub);
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

        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => targetUser.Sub)
            .RuleFor(x => x.Code, (_, _) => "XYZ789")
            .RuleFor(x => x.Email, (_, _) => "target.new@example.com")
            .Generate();

        await this.InsertEntitiesAsync<DbDirectoriesContext, UserEntity>([targetUser, bystanderUser]);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(targetUser.Sub),
            CreateConfirmRequest("XYZ789"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Target user email is updated, but names are not
        var updatedTarget = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity?>(db => db.Users.SingleOrDefaultAsync(x => x.Sub == targetUser.Sub));
        Assert.NotNull(updatedTarget);
        Assert.Equal("target.new@example.com", updatedTarget.Email);
        Assert.Equal("TargetFirstName", updatedTarget.FirstName);
        Assert.Equal("TargetLastName", updatedTarget.LastName);

        // Bystander is untouched
        var updatedBystander = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity?>(db => db.Users.SingleOrDefaultAsync(x => x.Sub == bystanderUser.Sub));
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

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest(string.Empty));

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

        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.Code, (_, _) => "CORRECT")
            .RuleFor(x => x.Email, (_, _) => "john.doe@new.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("WRONG"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Email remains unchanged
        var dbUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity?>(db => db.Users.SingleOrDefaultAsync(x => x.Sub == user.Sub));
        Assert.NotNull(dbUser);
        Assert.Equal("john.doe@old.example.com", dbUser.Email);

        // Pending code still exists
        var dbCode = await this.GetChangeEmailCode(user.Sub);
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
        var pastTime = DateTimeOffset.UtcNow.AddHours(-2);
        this.TimestampInterceptor.Setup(timeProvider: new MockTimeProvider(pastTime));

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

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("VALIDCODE"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Email remains unchanged
        var dbUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity?>(db => db.Users.SingleOrDefaultAsync(x => x.Sub == user.Sub));
        Assert.NotNull(dbUser);
        Assert.Equal("john.doe@old.example.com", dbUser.Email);

        // Pending code still exists
        var dbCode = await this.GetChangeEmailCode(user.Sub);
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

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("ANYCODE"));

        // Mapped status should be a non-success code, e.g., BadRequest or NotFound.
        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmChangeEmail_ReturnsOkWithWarning_DeletesCode_AndDoesNotPublishUserUpdated_WhenAuthMethodUpdateFails()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        this.FakeUserUpdatedPublisher.Clear();

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

        // Set up the fake to return the expected MFA failure Result
        this.FakeEntraChangeEmailService.OnChangeEmail = (externalUserId, newEmail, ct) =>
            Task.FromResult(Result.Failure(EntraEmailErrors.MfaAuthenticationMethodFailed("FailedToUpdateAuthenticationMethodException")));

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("ABC1234"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        // Check mapped JSON warning matches Warning Pattern
        var content = await response.Content.ReadFromJsonAsync<ConfirmChangeEmailAddressResponse>();
        Assert.NotNull(content);
        Assert.Equal("john.doe@new.example.com", content.NewEmailAddress);
        Assert.True(content.HasWarning(ChangeEmailWarnings.EntraMfaSyncFailed));

        // Retains DB update (not rolled back)
        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(db => db.Users.SingleAsync(x => x.Sub == user.Sub));
        Assert.Equal("john.doe@new.example.com", updatedUser.Email);

        // Pending code is deleted
        var dbCode = await this.GetChangeEmailCode(user.Sub);
        Assert.Null(dbCode);

        // User updated publisher should NOT publish event (parity with legacy Node)
        Assert.Empty(this.FakeUserUpdatedPublisher.PublishedEvents);

        // Failure audit is logged
        var failureAudit = Assert.Single(this.AuditCapturer.CapturedRequests, x => x.EventName == AuditChangeEmailEventNames.EmailChangeFailed);
        Assert.True(failureAudit.WasFailure);
        Assert.Contains("FailedToUpdateAuthenticationMethodException", failureAudit.Message);
        Assert.DoesNotContain(this.AuditCapturer.CapturedRequests, x => x.Message.Contains("Successfully changed email"));
    }

    [Fact]
    public async Task ConfirmChangeEmail_Returns500_RollsBackEmail_RetainsCode_AndDoesNotPublishUserUpdated_WhenEntraPrimaryUpdateFails()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        this.FakeUserUpdatedPublisher.Clear();

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

        this.FakeEntraChangeEmailService.OnChangeEmail = (_, _, _) =>
            Task.FromResult(Result.Failure(EntraEmailErrors.UserUpdateFailed("primary patch failed")));

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("ABC1234"));

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(
            db => db.Users.SingleAsync(x => x.Sub == user.Sub));
        Assert.Equal("john.doe@old.example.com", updatedUser.Email);

        var dbCode = await this.GetChangeEmailCode(user.Sub);
        Assert.NotNull(dbCode);

        Assert.Empty(this.FakeUserUpdatedPublisher.PublishedEvents);

        var failureAudit = Assert.Single(this.AuditCapturer.CapturedRequests, x => x.EventName == AuditChangeEmailEventNames.EmailChangeFailed);
        Assert.True(failureAudit.WasFailure);
        Assert.Contains("primary patch failed", failureAudit.Message);
        Assert.DoesNotContain(this.AuditCapturer.CapturedRequests, x => x.Message.Contains("Successfully changed email"));
    }

    [Fact]
    public async Task ConfirmChangeEmail_ReturnsSuccess_UpdatesEmail_DeletesCode_PublishesUserUpdated_AndWritesAudit_WhenEntraSyncSucceeds()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        this.FakeUserUpdatedPublisher.Clear();

        var entraOid = Guid.NewGuid();
        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .RuleFor(x => x.IsEntra, (_, _) => true)
            .RuleFor(x => x.EntraOid, (_, _) => entraOid)
            .Generate();

        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.Code, (_, _) => "ABC1234")
            .RuleFor(x => x.Email, (_, _) => "john.doe@new.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        Guid? capturedEntraOid = null;
        this.FakeEntraChangeEmailService.OnChangeEmail = (externalUserId, newEmail, _) => {
            capturedEntraOid = externalUserId;
            return Task.FromResult(Result.Success());
        };

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("ABC1234"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var content = await response.Content.ReadFromJsonAsync<ConfirmChangeEmailAddressResponse>();
        Assert.NotNull(content);
        Assert.Equal("john.doe@new.example.com", content.NewEmailAddress);
        Assert.Empty(content.Warnings);
        Assert.Equal(entraOid, capturedEntraOid);

        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(
            db => db.Users.SingleAsync(x => x.Sub == user.Sub));
        Assert.Equal("john.doe@new.example.com", updatedUser.Email);

        var dbCode = await this.GetChangeEmailCode(user.Sub);
        Assert.Null(dbCode);

        var auditRequest = Assert.Single(this.AuditCapturer.CapturedRequests, x => x.Message.Contains("Successfully changed email"));
        Assert.Equal(AuditEventCategoryNames.ChangeEmail, auditRequest.EventCategory);
        Assert.Equal(user.Sub, auditRequest.UserId);

        var (UserId, EmailAddress, FirstName, LastName, Status) = Assert.Single(this.FakeUserUpdatedPublisher.PublishedEvents);
        Assert.Equal(user.Sub, UserId);
        Assert.Equal("john.doe@new.example.com", EmailAddress);
        Assert.Equal(user.FirstName, FirstName);
        Assert.Equal(user.LastName, LastName);
        Assert.Equal(user.Status, Status);
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

        // This simulates a failure during the SaveChangesAsync call, which would occur after the email is updated but before the transaction is committed.
        this.TimestampInterceptor.Setup(
            onSavingChangesError: () => new DbUpdateException("Simulated database failure during save.", new Exception("Inner database exception constraint violation"))
        );

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("CODE123"));

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);

        // Email remains unchanged
        var dbUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity?>(db => db.Users.SingleOrDefaultAsync(x => x.Sub == user.Sub));
        Assert.NotNull(dbUser);
        Assert.Equal("john.doe@old.example.com", dbUser.Email);

        // Pending code still exists
        var dbCode = await this.GetChangeEmailCode(user.Sub);
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
        var response = await anonymousClient.PostAsJsonAsync(
            GetEndpoint(userId),
            CreateConfirmRequest("ANYCODE"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ConfirmChangeEmail_ReturnsBadRequestWithProblemDetails_WhenNoPendingCode()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("ANYCODE"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        var problemDetails = await response.Content.ReadFromJsonAsync<Microsoft.AspNetCore.Mvc.ProblemDetails>();
        Assert.NotNull(problemDetails);
        Assert.Equal(ChangeEmailErrors.NoPendingRequest.Code, problemDetails.Type);
    }

    [Fact]
    public async Task ConfirmChangeEmail_DoesNotUpdateEmail_WhenVerificationCodeInvalid()
    {
        var pastTime = DateTimeOffset.UtcNow.AddHours(-2);
        this.TimestampInterceptor.Setup(timeProvider: new MockTimeProvider(pastTime));

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        // 1. Wrong code scenario
        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.Code, (_, _) => "CORRECT")
            .RuleFor(x => x.Email, (_, _) => "john.doe@new.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var responseWrong = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("WRONG"));

        Assert.Equal(HttpStatusCode.BadRequest, responseWrong.StatusCode);

        var dbUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(db => db.Users.SingleAsync(x => x.Sub == user.Sub));
        Assert.Equal("john.doe@old.example.com", dbUser.Email);
    }

    [Fact]
    public async Task ConfirmChangeEmail_DoesNotUpdateEmail_WhenVerificationCodeExpired()
    {
        var pastTime = DateTimeOffset.UtcNow.AddHours(-2);
        this.TimestampInterceptor.Setup(timeProvider: new MockTimeProvider(pastTime));

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        // 1. Wrong code scenario
        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.Code, (_, _) => "CORRECT")
            .RuleFor(x => x.Email, (_, _) => "john.doe@new.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var requestExpired = CreateConfirmRequest("CORRECT");
        var responseExpired = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), requestExpired);
        Assert.Equal(HttpStatusCode.BadRequest, responseExpired.StatusCode);

        var updatedDbUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(db => db.Users.SingleAsync(x => x.Sub == user.Sub));
        Assert.Equal("john.doe@old.example.com", updatedDbUser.Email);
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
            .RuleFor(x => x.Code, (_, _) => "CODE123")
            .RuleFor(x => x.Email, (_, _) => "john.doe@new.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        // Configure interceptor to simulate database save failure
        this.TimestampInterceptor.Setup(
            onSavingChangesError: () => new DbUpdateException("Simulated database failure during save.", new Exception("Inner database exception constraint violation"))
        );

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("CODE123"));

        Assert.NotEqual(HttpStatusCode.OK, response.StatusCode);

        // Pending code must still exist
        var dbCode = await this.GetChangeEmailCode(user.Sub);
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

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("WRONGCODE"));

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

        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.Code, (_, _) => "CODE123")
            .RuleFor(x => x.Email, (_, _) => "new@example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateConfirmRequest("WRONGCODE"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        // Verification email/notifications should not be sent
        Assert.Empty(this.FakeEmailRequestTracker.Requests);
    }

    private static ConfirmChangeEmailAddressRequest CreateConfirmRequest(string verificationCode)
        => new() { VerificationCode = verificationCode };

    private async Task<UserCodeEntity?> GetChangeEmailCode(Guid userId)
    {
        return await this.ExecuteDbContextAsync<DbDirectoriesContext, UserCodeEntity?>(
            db => db.UserCodes.SingleOrDefaultAsync(x => x.Uid == userId && x.CodeType == UserCodeType.ChangeEmail.Value));
    }
}
