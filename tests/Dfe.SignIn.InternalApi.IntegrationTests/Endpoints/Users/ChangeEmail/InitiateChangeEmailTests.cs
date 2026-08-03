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

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users.ChangeEmail;

[Trait("Category", "Integration")]
public class InitiateChangeEmailTests : InternalApiIntegrationEndpointTestBase
{
    private const string endpoint = "interaction/Users.InitiateChangeEmail";

    public InitiateChangeEmailTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task InitiateChangeEmail_ReturnsSuccess_WritesAudit_AndCreatesVerificationCode_WhenEmailAvailable()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = this.CreateRequest(user.Sub, "john.doe@new.example.com");

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await this.GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.NotNull(pendingCode);
        Assert.Equal(user.Sub, pendingCode.Uid);
        Assert.Equal("changeemail", pendingCode.CodeType);
        Assert.Equal("john.doe@new.example.com", pendingCode.Email);
        Assert.Equal("test-client", pendingCode.ClientId);
        Assert.Equal("n/a", pendingCode.RedirectUri);
        Assert.False(string.IsNullOrWhiteSpace(pendingCode.Code));

        var auditRequest = auditMock.CapturedRequest;
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeEmail, auditRequest.EventCategory);
        Assert.Equal(AuditChangeEmailEventNames.RequestToChangeEmail, auditRequest.EventName);
        Assert.Equal("Request to change email from john.doe@old.example.com to john.doe@new.example.com", auditRequest.Message);
        Assert.Equal(user.Sub, auditRequest.UserId);
    }

    [Fact]
    public async Task InitiateChangeEmail_CreatesOrReplacesPendingCodeRecord_InPersistence()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "alex.old@example.com")
            .Generate();

        var existingCode = new UserCodeEntity {
            Uid = user.Sub,
            CodeType = "changeemail",
            Code = "OLD1234",
            Email = "old-target@example.com",
            ClientId = "old-client",
            RedirectUri = "old-uri",
            ContextData = null,
            CreatedAt = DateTime.UtcNow.AddHours(-2),
            UpdatedAt = DateTime.UtcNow.AddHours(-2),
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(existingCode);

        var request = this.CreateRequest(user.Sub, "alex.new@example.com");

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var userCodes = await assertionDbContext.UserCodes
            .Where(x => x.Uid == user.Sub && x.CodeType == "changeemail")
            .ToListAsync();

        Assert.Single(userCodes);

        var pendingCode = userCodes.Single();
        Assert.Equal("alex.new@example.com", pendingCode.Email);
        Assert.Equal("test-client", pendingCode.ClientId);
        Assert.Equal("n/a", pendingCode.RedirectUri);
        Assert.False(string.IsNullOrWhiteSpace(pendingCode.Code));
    }

    [Fact]
    public async Task InitiateChangeEmail_EnsuresSingleActivePendingCode_ForUser()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var firstRequest = this.CreateRequest(user.Sub, "first.new@example.com");
        var secondRequest = this.CreateRequest(user.Sub, "second.new@example.com");

        var firstResponse = await authenticatedClient.PostAsJsonAsync(endpoint, firstRequest);
        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await authenticatedClient.PostAsJsonAsync(endpoint, secondRequest);
        Assert.Equal(HttpStatusCode.OK, secondResponse.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var userCodes = await assertionDbContext.UserCodes
            .Where(x => x.Uid == user.Sub && x.CodeType == "changeemail")
            .ToListAsync();

        Assert.Single(userCodes);
        Assert.Equal("second.new@example.com", userCodes.Single().Email);
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var request = this.CreateRequest(Guid.NewGuid(), "jane.smith@example.com");

        var response = await anonymousClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns400_WhenNewEmailMissing()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = this.CreateRequest(user.Sub, string.Empty);

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns400_WhenNewEmailInvalid()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = this.CreateRequest(user.Sub, "invalid-email");

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns400_WhenNewEmailMatchesCurrentEmail()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var currentEmail = "matching.email@example.com";
        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => currentEmail)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = this.CreateRequest(user.Sub, currentEmail);

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await this.GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.Null(pendingCode);

        Assert.Null(auditMock.CapturedRequest);
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns400_AndWritesAudit_WhenNewEmailBelongsToDifferentUser()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var requester = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "requester@example.com")
            .Generate();

        var existingUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "already.in.use@example.com")
            .Generate();

        await this.InsertEntitiesAsync<DbDirectoriesContext, UserEntity>([requester, existingUser]);

        var request = this.CreateRequest(requester.Sub, "already.in.use@example.com");

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await this.GetChangeEmailCode(assertionDbContext, requester.Sub);
        Assert.Null(pendingCode);

        var auditRequest = auditMock.CapturedRequest;
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeEmail, auditRequest.EventCategory);
        Assert.Equal(AuditChangeEmailEventNames.RequestedExistingEmail, auditRequest.EventName);
        Assert.Equal($"Request to change email from requester@example.com to existing user already.in.use@example.com", auditRequest.Message);
        Assert.Equal(requester.Sub, auditRequest.UserId);
    }

    [Fact]
    public async Task InitiateChangeEmail_DoesNotCreateOrMutatePendingCode_WhenValidationFails()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = this.CreateRequest(user.Sub, "not-an-email");

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await this.GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.Null(pendingCode);
    }

    [Fact]
    public Task InitiateChangeEmail_ReturnsLimiterMappedStatus_WhenRateLimited()
    {
        // Steps to implement:
        // 1) Arrange:
        //    - Create a customised integration test host for this test.
        //    - Override IInteractionLimiter so LimitActionAsync(...) returns WasRejected = true
        //      for InitiateChangeEmailAddressRequest.
        //    - Seed a valid user in DbDirectoriesContext.
        //    - Build a valid InitiateChangeEmailAddressRequest.
        // 2) Act:
        //    - POST to interaction/Users.InitiateChangeEmail with authentication enabled.
        // 3) Assert:
        //    - Assert mapped HTTP status for limiter rejection (parity with current behaviour).
        //    - Assert no pending changeemail code was created/updated in UserCodes.
        //    - Assert success audit event was not written.
        Assert.Fail(
            "TODO: Wire IInteractionLimiter mock/fake to reject request, then assert mapped status + no DB mutation + no success audit.");
        return Task.CompletedTask;
    }

    [Fact]
    public Task InitiateChangeEmail_SendsEmailOrNotificationViaMockedDependency_WhenApplicable()
    {
        // Steps to implement:
        // 1) Arrange:
        //    - Confirm that the Internal API initiate endpoint owns email/notification sending.
        //    - Create a customised integration test host and replace the dependency with a mock/fake
        //      (for example an email sender, notification publisher, or outbox publisher).
        //    - Seed a valid user and create a valid request.
        // 2) Act:
        //    - POST to interaction/Users.InitiateChangeEmail with authentication enabled.
        // 3) Assert:
        //    - Assert HTTP 200 OK.
        //    - Assert exactly one send/publish call.
        //    - Assert payload content is correct (target email, user identifier, code/context).
        //    - Assert pending changeemail record exists and audit success event is written.
        // Note:
        //    - If endpoint does not own this responsibility, keep this as not applicable and cover
        //      the boundary side effect instead (for example persisted pending code/outbox record).
        Assert.Fail(
            "TODO: Add mocked dependency and assert one successful send/publish with correct payload, if endpoint owns this responsibility.");
        return Task.CompletedTask;
    }

    [Fact]
    public Task InitiateChangeEmail_DoesNotSendEmailOrNotification_OnValidationOrLimiterFailure_WhenApplicable()
    {
        // Steps to implement:
        // 1) Arrange:
        //    - Use the same mocked/faked email/notification dependency as the positive-path test.
        //    - Create two sub-scenarios:
        //      a) validation failure (for example invalid email format)
        //      b) limiter rejection (override IInteractionLimiter to reject)
        // 2) Act:
        //    - Execute initiate request for each failure scenario.
        // 3) Assert:
        //    - Assert failure HTTP status for each scenario.
        //    - Assert zero send/publish calls for each scenario.
        //    - Assert no pending code mutation for each scenario.
        //    - Assert success audit event is not written.
        Assert.Fail(
            "TODO: Add failure-path assertions showing zero send/publish calls and no success side effects.");
        return Task.CompletedTask;
    }

    private InitiateChangeEmailAddressRequest CreateRequest(Guid userId, string newEmailAddress)
    {
        return new InitiateChangeEmailAddressRequest {
            UserId = userId,
            ClientId = "test-client",
            IsSelfInvoked = true,
            NewEmailAddress = newEmailAddress,
        };
    }

    private async Task<UserCodeEntity?> GetChangeEmailCode(DbDirectoriesContext dbContext, Guid userId)
    {
        return await dbContext.UserCodes
            .SingleOrDefaultAsync(x => x.Uid == userId && x.CodeType == "changeemail");
    }
}
