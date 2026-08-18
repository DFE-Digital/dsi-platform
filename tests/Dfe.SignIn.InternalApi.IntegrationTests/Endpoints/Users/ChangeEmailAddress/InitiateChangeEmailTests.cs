using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users.ChangeEmailAddress;

[Trait("Category", "Integration")]
public sealed class InitiateChangeEmailTests : InternalApiIntegrationEndpointTestBase
{
    private static string GetEndpoint(Guid? userId) => $"/internal/users/{userId}/initiate-change-email";

    public InitiateChangeEmailTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task InitiateChangeEmail_ReturnsSuccess_WritesAudit_AndCreatesVerificationCode_WhenEmailAvailable()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateRequest("john.doe@new.example.com"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.NotNull(pendingCode);
        Assert.Equal(user.Sub, pendingCode.Uid);
        Assert.Equal("changeemail", pendingCode.CodeType);
        Assert.Equal("john.doe@new.example.com", pendingCode.Email);
        Assert.Equal("test-client", pendingCode.ClientId);
        Assert.Equal("n/a", pendingCode.RedirectUri);
        Assert.False(string.IsNullOrWhiteSpace(pendingCode.Code));

        var auditRequest = this.AuditCapturer.CapturedRequests[0];
        Assert.NotNull(auditRequest);

        Assert.NotEmpty(this.AuditCapturer.CapturedRequests);
        Assert.Equal(2, this.AuditCapturer.CapturedRequests.Count);

        var firstAuditLog = this.AuditCapturer.CapturedRequests[1];
        Assert.NotNull(firstAuditLog);
        Assert.Equal(AuditEventCategoryNames.ChangeEmail, firstAuditLog.EventCategory);
        Assert.Equal(AuditChangeEmailEventNames.VerificationCode, firstAuditLog.EventName);

        Assert.Equal(user.Sub, auditRequest.UserId);
    }

    [Fact]
    public async Task InitiateChangeEmail_CreatesOrReplacesPendingCodeRecord_InPersistence()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "alex.old@example.com")
            .Generate();

        var existingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.Code, (_, _) => "OLD1234")
            .RuleFor(x => x.Email, (_, _) => "old-target@example.com")
            .RuleFor(x => x.ClientId, (_, _) => "old-client")
            .RuleFor(x => x.RedirectUri, (_, _) => "old-uri")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(existingCode);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateRequest("alex.new@example.com"));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var userCodes = await assertionDbContext.UserCodes
            .Where(x => x.Uid == user.Sub && x.CodeType == UserCodeType.ChangeEmail.Value)
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
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var firstResponse = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateRequest("first.new@example.com"));

        Assert.Equal(HttpStatusCode.OK, firstResponse.StatusCode);

        var secondResponse = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateRequest("second.new@example.com"));

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

        var response = await anonymousClient.PostAsJsonAsync(
            GetEndpoint(Guid.NewGuid()),
            CreateRequest("jane.smith@example.com"));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns400_WhenNewEmailMissing()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateRequest(string.Empty));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns400_WhenNewEmailInvalid()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateRequest("invalid-email"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns400_WhenNewEmailMatchesCurrentEmail()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var currentEmail = "matching.email@example.com";
        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => currentEmail)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateRequest(currentEmail));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.Null(pendingCode);

        Assert.Empty(this.AuditCapturer.CapturedRequests);
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns400_AndWritesAudit_WhenNewEmailBelongsToDifferentUser()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var requester = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "requester@example.com")
            .Generate();

        var existingUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "already.in.use@example.com")
            .Generate();

        await this.InsertEntitiesAsync<DbDirectoriesContext, UserEntity>([requester, existingUser]);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(requester.Sub),
            CreateRequest("already.in.use@example.com"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await GetChangeEmailCode(assertionDbContext, requester.Sub);
        Assert.Null(pendingCode);

        //var auditRequest = this.AuditCapturer.CapturedRequest;
        var auditRequest = this.AuditCapturer.CapturedRequests[0];
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeEmail, auditRequest.EventCategory);
        Assert.Equal(AuditChangeEmailEventNames.RequestedExistingEmail, auditRequest.EventName);
        Assert.Equal($"Request to change email from requester@example.com to existing user already.in.use@example.com", auditRequest.Message);
        Assert.Equal(requester.Sub, auditRequest.UserId);
    }

    [Fact]
    public async Task InitiateChangeEmail_DoesNotCreateOrMutatePendingCode_WhenValidationFails()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateRequest("not-an-email"));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.Null(pendingCode);
    }

    [Fact]
    public async Task InitiateChangeEmail_ReturnsLimiterMappedStatus_WhenRateLimited()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        this.FakeLimiter.ShouldAlwaysReject = true;

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateRequest("john.doe@new.example.com"));

        Assert.Equal(HttpStatusCode.TooManyRequests, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.Null(pendingCode);

        Assert.Empty(this.AuditCapturer.CapturedRequests);
    }

    [Fact]
    public async Task InitiateChangeEmail_SendsEmailOrNotification_WhenApplicable()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        this.FakeLimiter.ShouldAlwaysReject = false;
        this.FakeEmailRequestTracker.Clear();

        var existingEmail = "john.doe@old.example.com";
        var newEmail = "john.doe@new.example.com";
        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => existingEmail)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(user.Sub),
            CreateRequest(newEmail));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await GetChangeEmailCode(assertionDbContext, user.Sub);
        Assert.NotNull(pendingCode);

        var trackedRequests = this.FakeEmailRequestTracker.Requests.ToArray();
        Assert.Equal(2, trackedRequests.Length);

        var verificationRequest = Assert.Single(trackedRequests, x => x.RecipientEmailAddress == newEmail);
        Assert.Equal("8a6b7625-87d5-41bc-bc58-035343571d81", verificationRequest.TemplateId);
        Assert.Equal(newEmail, verificationRequest.Personalisation["email"]);
        Assert.Equal(pendingCode.Code, verificationRequest.Personalisation["code"]);
        Assert.Equal(user.FirstName, verificationRequest.Personalisation["firstName"]);
        Assert.Equal(user.LastName, verificationRequest.Personalisation["lastName"]);

        var migratedEmailRequest = Assert.Single(trackedRequests, x => x.RecipientEmailAddress == existingEmail);
        Assert.Equal("18e0e804-04c6-4f73-9462-ab3cbf8b990f", migratedEmailRequest.TemplateId);
        Assert.Equal(newEmail, migratedEmailRequest.Personalisation["newEmail"]);
        Assert.Equal(user.FirstName, migratedEmailRequest.Personalisation["firstName"]);
        Assert.Equal(user.LastName, migratedEmailRequest.Personalisation["lastName"]);

        Assert.NotEmpty(this.AuditCapturer.CapturedRequests);
        Assert.Equal(2, this.AuditCapturer.CapturedRequests.Count);

        var auditRequest = this.AuditCapturer.CapturedRequests[1];
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeEmail, auditRequest.EventCategory);
        Assert.Equal(AuditChangeEmailEventNames.VerificationCode, auditRequest.EventName);
        Assert.Equal(user.Sub, auditRequest.UserId);
    }

    [Fact]
    public async Task InitiateChangeEmail_DoesNotSendEmailOrNotification_OnValidationOrLimiterFailure_WhenApplicable()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        this.FakeLimiter.ShouldAlwaysReject = true;

        var validationFailureUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "alex.old@example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(validationFailureUser);

        var validationFailureResponse = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(validationFailureUser.Sub),
            CreateRequest("invalid-email"));

        Assert.Equal(HttpStatusCode.BadRequest, validationFailureResponse.StatusCode);
        Assert.Empty(this.FakeEmailRequestTracker.Requests);

        await using (var validationAssertionScope = this.WebAppFactory.Services.CreateAsyncScope()) {
            var validationAssertionDbContext = validationAssertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            var validationFailurePendingCode = await GetChangeEmailCode(validationAssertionDbContext, validationFailureUser.Sub);
            Assert.Null(validationFailurePendingCode);
        }

        Assert.Empty(this.AuditCapturer.CapturedRequests);

        this.FakeLimiter.ShouldAlwaysReject = true;

        var rateLimitedUser = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "john.doe@old.example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(rateLimitedUser);

        var rateLimitedResponse = await authenticatedClient.PostAsJsonAsync(
            GetEndpoint(rateLimitedUser.Sub),
            CreateRequest("john.doe@new.example.com"));

        Assert.Equal(HttpStatusCode.TooManyRequests, rateLimitedResponse.StatusCode);
        Assert.Empty(this.FakeEmailRequestTracker.Requests);

        await using (var rateLimitedAssertionScope = this.WebAppFactory.Services.CreateAsyncScope()) {
            var rateLimitedAssertionDbContext = rateLimitedAssertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            var rateLimitedPendingCode = await GetChangeEmailCode(rateLimitedAssertionDbContext, rateLimitedUser.Sub);
            Assert.Null(rateLimitedPendingCode);
        }

        Assert.Empty(this.AuditCapturer.CapturedRequests);
    }

    private static InitiateChangeEmailAddressRequest CreateRequest(string newEmailAddress)
        => new("test-client", newEmailAddress, true);

    private static async Task<UserCodeEntity?> GetChangeEmailCode(DbDirectoriesContext dbContext, Guid userId)
        => await dbContext.UserCodes.SingleOrDefaultAsync(x => x.Uid == userId && x.CodeType == UserCodeType.ChangeEmail.Value);
}
