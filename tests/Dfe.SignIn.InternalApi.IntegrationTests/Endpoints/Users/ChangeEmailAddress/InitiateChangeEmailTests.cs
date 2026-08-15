using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users.ChangeEmailAddress;

[Trait( "Category", "Integration" )]
public sealed class InitiateChangeEmailTests : InternalApiIntegrationEndpointTestBase
{
    private const string endpoint = "/internal/users/{userId}/initiate-change-email";

    private static string GetEndpointForUser( Guid userId ) => endpoint.Replace( "{userId}", userId.ToString() );

    public InitiateChangeEmailTests( InternalApiWebApplicationFactory factory )
        : base( factory )
    {
    }

    [Fact]
    public async Task InitiateChangeEmail_ReturnsSuccess_WritesAudit_AndCreatesVerificationCode_WhenEmailAvailable()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .WithAuditMock()
            .Build();

        var authenticatedClient = testContext.Client;
        var auditMock = testContext.AuditMock;

        var user = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => "john.doe@old.example.com" )
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var request = CreateRequest( "john.doe@new.example.com" );

        var response = await authenticatedClient.PostAsJsonAsync( GetEndpointForUser( user.Sub ), request );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await GetChangeEmailCode( assertionDbContext, user.Sub );
        Assert.NotNull( pendingCode );
        Assert.Equal( user.Sub, pendingCode.Uid );
        Assert.Equal( "changeemail", pendingCode.CodeType );
        Assert.Equal( "john.doe@new.example.com", pendingCode.Email );
        Assert.Equal( "test-client", pendingCode.ClientId );
        Assert.Equal( "n/a", pendingCode.RedirectUri );
        Assert.False( string.IsNullOrWhiteSpace( pendingCode.Code ) );

        var auditRequest = auditMock.CapturedRequest;
        Assert.NotNull( auditRequest );
        Assert.Equal( AuditEventCategoryNames.ChangeEmail, auditRequest.EventCategory );
        Assert.Equal( AuditChangeEmailEventNames.VerificationCode, auditRequest.EventName );
        //todo: update audit mock to support multiple events and assert the second event is the expected one
        //Assert.Equal("Request to change email from john.doe@old.example.com to john.doe@new.example.com", auditRequest.Message);
        Assert.Equal( user.Sub, auditRequest.UserId );
    }

    [Fact]
    public async Task InitiateChangeEmail_CreatesOrReplacesPendingCodeRecord_InPersistence()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .WithAuditMock()
            .Build();

        var authenticatedClient = testContext.Client;
        var auditMock = testContext.AuditMock;

        var user = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => "alex.old@example.com" )
            .Generate();

        var existingCode = new UserCodeEntity {
            Uid = user.Sub,
            CodeType = "changeemail",
            Code = "OLD1234",
            Email = "old-target@example.com",
            ClientId = "old-client",
            RedirectUri = "old-uri",
            ContextData = null,
            CreatedAt = DateTime.UtcNow.AddHours( -2 ),
            UpdatedAt = DateTime.UtcNow.AddHours( -2 ),
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>( existingCode );

        var request = CreateRequest( "alex.new@example.com" );

        var response = await authenticatedClient.PostAsJsonAsync( GetEndpointForUser( user.Sub ), request );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var userCodes = await assertionDbContext.UserCodes
            .Where( x => x.Uid == user.Sub && x.CodeType == "changeemail" )
            .ToListAsync();

        Assert.Single( userCodes );

        var pendingCode = userCodes.Single();
        Assert.Equal( "alex.new@example.com", pendingCode.Email );
        Assert.Equal( "test-client", pendingCode.ClientId );
        Assert.Equal( "n/a", pendingCode.RedirectUri );
        Assert.False( string.IsNullOrWhiteSpace( pendingCode.Code ) );
    }

    [Fact]
    public async Task InitiateChangeEmail_EnsuresSingleActivePendingCode_ForUser()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .Build();

        var authenticatedClient = testContext.Client;

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var firstRequest = CreateRequest( "first.new@example.com" );
        var secondRequest = CreateRequest( "second.new@example.com" );

        var firstResponse = await authenticatedClient.PostAsJsonAsync( GetEndpointForUser( user.Sub ), firstRequest );
        Assert.Equal( HttpStatusCode.OK, firstResponse.StatusCode );

        var secondResponse = await authenticatedClient.PostAsJsonAsync( GetEndpointForUser( user.Sub ), secondRequest );
        Assert.Equal( HttpStatusCode.OK, secondResponse.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var userCodes = await assertionDbContext.UserCodes
            .Where( x => x.Uid == user.Sub && x.CodeType == "changeemail" )
            .ToListAsync();

        Assert.Single( userCodes );
        Assert.Equal( "second.new@example.com", userCodes.Single().Email );
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.SetupClient().Build().Client;

        var userId = Guid.NewGuid();
        var request = CreateRequest( "jane.smith@example.com" );

        var response = await anonymousClient.PostAsJsonAsync( GetEndpointForUser( userId ), request );

        Assert.Equal( HttpStatusCode.Unauthorized, response.StatusCode );
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns400_WhenNewEmailMissing()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .Build();

        var authenticatedClient = testContext.Client;

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var request = CreateRequest( string.Empty );

        var response = await authenticatedClient.PostAsJsonAsync( endpoint, request );

        Assert.Equal( HttpStatusCode.BadRequest, response.StatusCode );
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns400_WhenNewEmailInvalid()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .Build();

        var authenticatedClient = testContext.Client;

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var request = CreateRequest( "invalid-email" );

        var response = await authenticatedClient.PostAsJsonAsync( endpoint, request );

        Assert.Equal( HttpStatusCode.BadRequest, response.StatusCode );
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns400_WhenNewEmailMatchesCurrentEmail()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .WithAuditMock()
            .Build();

        var authenticatedClient = testContext.Client;
        var auditMock = testContext.AuditMock;

        var currentEmail = "matching.email@example.com";
        var user = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => currentEmail )
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var request = CreateRequest( currentEmail );

        var response = await authenticatedClient.PostAsJsonAsync( endpoint, request );

        Assert.Equal( HttpStatusCode.BadRequest, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await GetChangeEmailCode( assertionDbContext, user.Sub );
        Assert.Null( pendingCode );

        Assert.Null( auditMock.CapturedRequest );
    }

    [Fact]
    public async Task InitiateChangeEmail_Returns400_AndWritesAudit_WhenNewEmailBelongsToDifferentUser()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .WithAuditMock()
            .Build();

        var authenticatedClient = testContext.Client;
        var auditMock = testContext.AuditMock;

        var requester = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => "requester@example.com" )
            .Generate();

        var existingUser = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => "already.in.use@example.com" )
            .Generate();

        await this.InsertEntitiesAsync<DbDirectoriesContext, UserEntity>( [requester, existingUser] );

        var request = CreateRequest( "already.in.use@example.com" );

        var response = await authenticatedClient.PostAsJsonAsync( GetEndpointForUser( requester.Sub ), request );

        Assert.Equal( HttpStatusCode.BadRequest, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await GetChangeEmailCode( assertionDbContext, requester.Sub );
        Assert.Null( pendingCode );

        var auditRequest = auditMock.CapturedRequest;
        Assert.NotNull( auditRequest );
        Assert.Equal( AuditEventCategoryNames.ChangeEmail, auditRequest.EventCategory );
        Assert.Equal( AuditChangeEmailEventNames.RequestedExistingEmail, auditRequest.EventName );
        Assert.Equal( $"Request to change email from requester@example.com to existing user already.in.use@example.com", auditRequest.Message );
        Assert.Equal( requester.Sub, auditRequest.UserId );
    }

    [Fact]
    public async Task InitiateChangeEmail_DoesNotCreateOrMutatePendingCode_WhenValidationFails()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .Build();

        var authenticatedClient = testContext.Client;

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var request = CreateRequest( "not-an-email" );

        var response = await authenticatedClient.PostAsJsonAsync( endpoint, request );

        Assert.Equal( HttpStatusCode.BadRequest, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await GetChangeEmailCode( assertionDbContext, user.Sub );
        Assert.Null( pendingCode );
    }

    [Fact]
    public async Task InitiateChangeEmail_ReturnsLimiterMappedStatus_WhenRateLimited()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .WithAuditMock()
            .Build();

        var authenticatedClient = testContext.Client;
        var auditMock = testContext.AuditMock;

        this.FakeLimiter.ShouldAlwaysReject = true;

        var user = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => "john.doe@old.example.com" )
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var request = CreateRequest( "john.doe@new.example.com" );

        var response = await authenticatedClient.PostAsJsonAsync( GetEndpointForUser( user.Sub ), request );

        Assert.Equal( HttpStatusCode.TooManyRequests, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await GetChangeEmailCode( assertionDbContext, user.Sub );
        Assert.Null( pendingCode );

        Assert.Null( auditMock.CapturedRequest );
    }

    [Fact]
    public async Task InitiateChangeEmail_SendsEmailOrNotification_WhenApplicable()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .WithAuditMock()
            .Build();

        var authenticatedClient = testContext.Client;
        var auditMock = testContext.AuditMock;

        this.FakeLimiter.ShouldAlwaysReject = false;
        this.FakeEmailRequestTracker.Clear();

        var existingEmail = "john.doe@old.example.com";
        var newEmail = "john.doe@new.example.com";
        var user = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => existingEmail )
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( user );

        var request = CreateRequest( newEmail );

        var response = await authenticatedClient.PostAsJsonAsync( GetEndpointForUser( user.Sub ), request );

        Assert.Equal( HttpStatusCode.OK, response.StatusCode );

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var pendingCode = await GetChangeEmailCode( assertionDbContext, user.Sub );
        Assert.NotNull( pendingCode );

        var trackedRequests = this.FakeEmailRequestTracker.Requests.ToArray();
        Assert.Equal( 2, trackedRequests.Length );

        var verificationRequest = Assert.Single( trackedRequests, x => x.RecipientEmailAddress == newEmail );
        Assert.Equal( "8a6b7625-87d5-41bc-bc58-035343571d81", verificationRequest.TemplateId );
        Assert.Equal( newEmail, verificationRequest.Personalisation["email"] );
        Assert.Equal( pendingCode.Code, verificationRequest.Personalisation["code"] );
        Assert.Equal( user.FirstName, verificationRequest.Personalisation["firstName"] );
        Assert.Equal( user.LastName, verificationRequest.Personalisation["lastName"] );

        var migratedEmailRequest = Assert.Single( trackedRequests, x => x.RecipientEmailAddress == existingEmail );
        Assert.Equal( "18e0e804-04c6-4f73-9462-ab3cbf8b990f", migratedEmailRequest.TemplateId );
        Assert.Equal( newEmail, migratedEmailRequest.Personalisation["newEmail"] );
        Assert.Equal( user.FirstName, migratedEmailRequest.Personalisation["firstName"] );
        Assert.Equal( user.LastName, migratedEmailRequest.Personalisation["lastName"] );

        var auditRequest = auditMock.CapturedRequest;
        Assert.NotNull( auditRequest );
        Assert.Equal( AuditEventCategoryNames.ChangeEmail, auditRequest.EventCategory );
        Assert.Equal( AuditChangeEmailEventNames.VerificationCode, auditRequest.EventName );
        Assert.Equal( user.Sub, auditRequest.UserId );
    }

    [Fact]
    public async Task InitiateChangeEmail_DoesNotSendEmailOrNotification_OnValidationOrLimiterFailure_WhenApplicable()
    {
        var testContext = this.SetupClient()
            .WithAuthentication()
            .WithAuditMock()
            .Build();

        var authenticatedClient = testContext.Client;
        var auditMock = testContext.AuditMock;

        this.FakeLimiter.ShouldAlwaysReject = true;

        var validationFailureUser = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => "alex.old@example.com" )
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( validationFailureUser );

        var validationFailureRequest = CreateRequest( "invalid-email" );

        var validationFailureResponse = await authenticatedClient.PostAsJsonAsync( endpoint, validationFailureRequest );

        Assert.Equal( HttpStatusCode.BadRequest, validationFailureResponse.StatusCode );
        Assert.Empty( this.FakeEmailRequestTracker.Requests );

        await using (var validationAssertionScope = this.WebAppFactory.Services.CreateAsyncScope()) {
            var validationAssertionDbContext = validationAssertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            var validationFailurePendingCode = await GetChangeEmailCode( validationAssertionDbContext, validationFailureUser.Sub );
            Assert.Null( validationFailurePendingCode );
        }

        Assert.Null( auditMock.CapturedRequest );

        this.FakeLimiter.ShouldAlwaysReject = true;

        var rateLimitedUser = EntityFaker.User
            .RuleFor( x => x.Email, ( _, _ ) => "john.doe@old.example.com" )
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>( rateLimitedUser );

        var rateLimitedRequest = CreateRequest( "john.doe@new.example.com" );

        var rateLimitedResponse = await authenticatedClient.PostAsJsonAsync( GetEndpointForUser( rateLimitedUser.Sub ), rateLimitedRequest );

        Assert.Equal( HttpStatusCode.TooManyRequests, rateLimitedResponse.StatusCode );
        Assert.Empty( this.FakeEmailRequestTracker.Requests );

        await using (var rateLimitedAssertionScope = this.WebAppFactory.Services.CreateAsyncScope()) {
            var rateLimitedAssertionDbContext = rateLimitedAssertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
            var rateLimitedPendingCode = await GetChangeEmailCode( rateLimitedAssertionDbContext, rateLimitedUser.Sub );
            Assert.Null( rateLimitedPendingCode );
        }

        Assert.Null( auditMock.CapturedRequest );
    }

    private static InitiateChangeEmailAddressRequest CreateRequest( string newEmailAddress )
        => new( "test-client", newEmailAddress, true );

    private static async Task<UserCodeEntity?> GetChangeEmailCode( DbDirectoriesContext dbContext, Guid userId )
        => await dbContext.UserCodes.SingleOrDefaultAsync( x => x.Uid == userId && x.CodeType == "changeemail" );
}
