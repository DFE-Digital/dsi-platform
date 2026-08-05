using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users.ChangeEmailAddress;

[Trait("Category", "Integration")]
public class ChangeEmailAddressTests : InternalApiIntegrationEndpointTestBase
{
    private const string endpoint = "interaction/Users.InitiateChangeEmail";

    public ChangeEmailAddressTests(InternalApiWebApplicationFactory factory)
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
        Assert.Equal(AuditChangeEmailEventNames.VerificationCodeSent, auditRequest.EventName);
        //todo: update audit mock to support multiple events and assert the second event is the expected one
        //Assert.Equal("Request to change email from john.doe@old.example.com to john.doe@new.example.com", auditRequest.Message);
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
    public async Task InitiateChangeEmail_ReturnsLimiterMappedStatus_WhenRateLimited()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        this.FakeLimiter.ShouldAlwaysReject = true;

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = this.CreateRequest(user.Sub, "john.doe@new.example.com");

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await this.GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.Null(pendingCode);

        Assert.Null(auditMock.CapturedRequest);
    }

    [Fact]
    public async Task InitiateChangeEmail_SendsEmailOrNotification_WhenApplicable()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        this.FakeLimiter.ShouldAlwaysReject = false;
        this.FakeEmailRequestTracker.Clear();

        var existingEmail = "john.doe@old.example.com";
        var newEmail = "john.doe@new.example.com";
        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => existingEmail)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = this.CreateRequest(user.Sub, newEmail);

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await this.GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.NotNull(pendingCode);

        var trackedRequests = this.FakeEmailRequestTracker.Requests.ToArray();
        Assert.Equal(2, trackedRequests.Length);

        var verificationRequest = Assert.Single(trackedRequests, x => x.RecipientEmailAddress == newEmail);
        Assert.Equal("8a6b7625-87d5-41bc-bc58-035343571d81", verificationRequest.TemplateId);
        Assert.Equal(newEmail, verificationRequest.Personalisation["email"]);
        Assert.Equal(pendingCode.Code, verificationRequest.Personalisation["code"]);
        Assert.Equal("FirstName", verificationRequest.Personalisation["firstName"]);
        Assert.Equal("LastName", verificationRequest.Personalisation["lastName"]);

        var migratedEmailRequest = Assert.Single(trackedRequests, x => x.RecipientEmailAddress == existingEmail);
        Assert.Equal("18e0e804-04c6-4f73-9462-ab3cbf8b990f", migratedEmailRequest.TemplateId);
        Assert.Equal(newEmail, migratedEmailRequest.Personalisation["newEmail"]);
        Assert.Equal("FirstName", migratedEmailRequest.Personalisation["firstName"]);
        Assert.Equal("LastName", migratedEmailRequest.Personalisation["lastName"]);

        var auditRequest = auditMock.CapturedRequest;
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeEmail, auditRequest.EventCategory);
        Assert.Equal(AuditChangeEmailEventNames.VerificationCodeSent, auditRequest.EventName);
        Assert.Equal(user.Sub, auditRequest.UserId);
    }

    [Fact]
    public async Task InitiateChangeEmail_DoesNotSendEmailOrNotification_OnValidationOrLimiterFailure_WhenApplicable()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        this.FakeLimiter.ShouldAlwaysReject = true;

        var validationFailureUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "alex.old@example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(validationFailureUser);

        var validationFailureRequest = this.CreateRequest(validationFailureUser.Sub, "invalid-email");

        var validationFailureResponse = await authenticatedClient.PostAsJsonAsync(endpoint, validationFailureRequest);

        Assert.Equal(HttpStatusCode.BadRequest, validationFailureResponse.StatusCode);
        Assert.Empty(this.FakeEmailRequestTracker.Requests);

        await using (var validationAssertionScope = this.WebAppFactory.Services.CreateAsyncScope()) {
            var validationAssertionDbContext = validationAssertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            var validationFailurePendingCode = await this.GetChangeEmailCode(validationAssertionDbContext, validationFailureUser.Sub);
            Assert.Null(validationFailurePendingCode);
        }

        Assert.Null(auditMock.CapturedRequest);

        this.FakeLimiter.ShouldAlwaysReject = true;

        var rateLimitedUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(rateLimitedUser);

        var rateLimitedRequest = this.CreateRequest(rateLimitedUser.Sub, "john.doe@new.example.com");

        var rateLimitedResponse = await authenticatedClient.PostAsJsonAsync(endpoint, rateLimitedRequest);

        Assert.Equal(HttpStatusCode.TooManyRequests, rateLimitedResponse.StatusCode);
        Assert.Empty(this.FakeEmailRequestTracker.Requests);

        await using (var rateLimitedAssertionScope = this.WebAppFactory.Services.CreateAsyncScope()) {
            var rateLimitedAssertionDbContext = rateLimitedAssertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            var rateLimitedPendingCode = await this.GetChangeEmailCode(rateLimitedAssertionDbContext, rateLimitedUser.Sub);
            Assert.Null(rateLimitedPendingCode);
        }

        Assert.Null(auditMock.CapturedRequest);
    }

    private Core.Contracts.Users.InitiateChangeEmailAddressRequest CreateRequest(Guid userId, string newEmailAddress)
    {
        return new Core.Contracts.Users.InitiateChangeEmailAddressRequest {
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
