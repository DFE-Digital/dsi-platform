
using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Users;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Microsoft.EntityFrameworkCore;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait("Category", "Integration")]
public class AutoLinkEntraToDsiTests : InternalApiIntegrationEndpointTestBase
{
    private static string GetEndpoint() => $"/internal/users/auto-link-entra-to-dsi";
    public AutoLinkEntraToDsiTests(InternalApiWebApplicationFactory factory)
            : base(factory)
    {
    }

    [Fact]
    public async Task UserAlreadyLinkedReturnsGuid()
    {
        var entraId = Guid.NewGuid();

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => "Old Title")
            .RuleFor(x => x.IsEntra, (_, _) => true)
            .RuleFor(x => x.EntraOid, (_, _) => entraId)
            .RuleFor(x => x.Email, (_, _) => "test@Test.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint();
        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new AutoLinkEntraUserToDsiRequest {
            EmailAddress = "test@Test.com",
            EntraUserId = entraId,
            FirstName = user.FirstName,
            LastName = user.LastName
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //get the user and check to see if its not been changed
        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(db =>
        db.Users.SingleAsync(x => x.Sub == user.Sub && x.Email == "test@Test.com"));

        Assert.Equal(1, user.Status);

        var usersWithEmail = await this.ExecuteDbContextAsync<DbDirectoriesContext, int>(db =>
        db.Users.CountAsync(x => x.Email == "test@Test.com"));

        Assert.Equal(1, usersWithEmail);

    }

    [Fact]
    public async Task UserAlreadyLinkedButInActiveCausesException()
    {
        var entraId = Guid.NewGuid();

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => "Old Title")
            .RuleFor(x => x.IsEntra, (_, _) => true)
            .RuleFor(x => x.EntraOid, (_, _) => entraId)
            .RuleFor(x => x.Email, (_, _) => "test@Test.com")
            .RuleFor(x => x.Status, (_, _) => (short)0)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint();

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new AutoLinkEntraUserToDsiRequest {
            EmailAddress = "test@Test.com",
            EntraUserId = entraId,
            FirstName = user.FirstName,
            LastName = user.LastName,
        });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        //get the user and check to see if its not been changed
        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(db =>
        db.Users.SingleAsync(x => x.Sub == user.Sub && x.Email == "test@Test.com"));

        Assert.Equal(0, user.Status);
        Assert.Equal(user.FirstName, updatedUser.FirstName);
        Assert.Equal(user.LastName, updatedUser.LastName);
        Assert.Equal(user.Email, updatedUser.Email);
        Assert.Equal(entraId, updatedUser.EntraOid);
        Assert.True(updatedUser.IsEntra);

        var usersWithEmail = await this.ExecuteDbContextAsync<DbDirectoriesContext, int>(db =>
        db.Users.CountAsync(x => x.Email == "test@Test.com"));

        Assert.Equal(1, usersWithEmail);
    }

    [Fact]
    public async Task SuccessfullyFindsUnlinkedUserAndLinks()
    {
        var entraId = Guid.NewGuid();

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => "Old Title")
            .RuleFor(x => x.IsEntra, (_, _) => false)
            .RuleFor(x => x.Email, (_, _) => "test@Test.com")
            .RuleFor(x => x.Status, (_, _) => (short)1)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint();

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new AutoLinkEntraUserToDsiRequest {
            EmailAddress = "test@Test.com",
            EntraUserId = entraId,
            FirstName = user.FirstName,
            LastName = user.LastName,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //get the user and check to see if its not been changed
        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(db =>
        db.Users.SingleAsync(x => x.Sub == user.Sub && x.Email == "test@Test.com"));

        Assert.Equal(1, user.Status);
        Assert.Equal(user.FirstName, updatedUser.FirstName);
        Assert.Equal(user.LastName, updatedUser.LastName);
        Assert.Equal(user.Email, updatedUser.Email);
        Assert.Equal(entraId, updatedUser.EntraOid);
        Assert.True(updatedUser.IsEntra);
        Assert.NotNull(updatedUser.EntraLinked);

        var usersWithEmail = await this.ExecuteDbContextAsync<DbDirectoriesContext, int>(db =>
        db.Users.CountAsync(x => x.Email == "test@Test.com"));

        Assert.Equal(1, usersWithEmail);
    }

    [Fact]
    public async Task UserExistsButInactiveAndNotAssociatedWithEntraIdThrowsException()
    {
        var entraId = Guid.NewGuid();

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => "Old Title")
            .RuleFor(x => x.IsEntra, (_, _) => false)
            .RuleFor(x => x.Email, (_, _) => "test@Test.com")
            .RuleFor(x => x.Status, (_, _) => (short)0)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint();

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new AutoLinkEntraUserToDsiRequest {
            EmailAddress = "test@Test.com",
            EntraUserId = entraId,
            FirstName = user.FirstName,
            LastName = user.LastName,
        });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        //get the user and check to see if its not been changed
        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(db =>
        db.Users.SingleAsync(x => x.Sub == user.Sub && x.Email == "test@Test.com"));

        Assert.Equal(0, user.Status);
        Assert.Equal(user.FirstName, updatedUser.FirstName);
        Assert.Equal(user.LastName, updatedUser.LastName);
        Assert.Equal(user.Email, updatedUser.Email);
        Assert.Null(updatedUser.EntraOid);
        Assert.False(updatedUser.IsEntra);
        Assert.Null(updatedUser.EntraLinked);

        var usersWithEmail = await this.ExecuteDbContextAsync<DbDirectoriesContext, int>(db =>
        db.Users.CountAsync(x => x.Email == "test@Test.com"));

        Assert.Equal(1, usersWithEmail);
    }

    [Fact]
    public async Task UserExistsButAssociatedWithDifferentEntraIdThrowsException()
    {
        var entraId = Guid.NewGuid();
        var OtherEntraId = Guid.NewGuid();

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => "Old Title")
            .RuleFor(x => x.IsEntra, (_, _) => false)
            .RuleFor(x => x.Email, (_, _) => "test@Test.com")
            .RuleFor(x => x.Status, (_, _) => (short)0)
            .RuleFor(x => x.EntraOid, (_, _) => OtherEntraId)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint();

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new AutoLinkEntraUserToDsiRequest {
            EmailAddress = "test@Test.com",
            EntraUserId = entraId,
            FirstName = user.FirstName,
            LastName = user.LastName,
        });

        Assert.Equal(HttpStatusCode.InternalServerError, response.StatusCode);

        //get the user and check to see if its not been changed
        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(db =>
        db.Users.SingleAsync(x => x.Sub == user.Sub && x.Email == "test@Test.com"));

        Assert.Equal(0, user.Status);
        Assert.Equal(user.FirstName, updatedUser.FirstName);
        Assert.Equal(user.LastName, updatedUser.LastName);
        Assert.Equal(user.Email, updatedUser.Email);

        var usersWithEmail = await this.ExecuteDbContextAsync<DbDirectoriesContext, int>(db =>
        db.Users.CountAsync(x => x.Email == "test@Test.com"));

        Assert.Equal(1, usersWithEmail);
    }

    [Fact]
    public async Task UserUpdatedWithFirstNameWhenExistsButNotLinked()
    {
        var entraId = Guid.NewGuid();

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => "Old Title")
            .RuleFor(x => x.IsEntra, (_, _) => false)
            .RuleFor(x => x.Email, (_, _) => "test@Test.com")
            .RuleFor(x => x.Status, (_, _) => (short)1)
            .RuleFor(x => x.FirstName, (_, _) => "Test123")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint();

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new AutoLinkEntraUserToDsiRequest {
            EmailAddress = "test@Test.com",
            EntraUserId = entraId,
            FirstName = "Test",
            LastName = user.LastName,
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //get the user and check to see if its not been changed
        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(db =>
        db.Users.SingleAsync(x => x.Sub == user.Sub && x.Email == "test@Test.com"));

        Assert.Equal(1, user.Status);
        Assert.Equal("Test", updatedUser.FirstName);
        Assert.Equal(user.LastName, updatedUser.LastName);
        Assert.Equal(user.Email, updatedUser.Email);

        var usersWithEmail = await this.ExecuteDbContextAsync<DbDirectoriesContext, int>(db =>
        db.Users.CountAsync(x => x.Email == "test@Test.com"));

        Assert.Equal(1, usersWithEmail);
    }

    [Fact]
    public async Task UserUpdatedWithLastNameWhenExistsButNotLinked()
    {
        var entraId = Guid.NewGuid();

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => "Old Title")
            .RuleFor(x => x.IsEntra, (_, _) => false)
            .RuleFor(x => x.Email, (_, _) => "test@Test.com")
            .RuleFor(x => x.Status, (_, _) => (short)1)
            .RuleFor(x => x.FirstName, (_, _) => "Test123")
            .RuleFor(x => x.LastName, (_, _) => "LastName123")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint();

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new AutoLinkEntraUserToDsiRequest {
            EmailAddress = "test@Test.com",
            EntraUserId = entraId,
            FirstName = "Test",
            LastName = "LastName"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //get the user and check to see if its not been changed
        var updatedUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(db =>
        db.Users.SingleAsync(x => x.Sub == user.Sub && x.Email == "test@Test.com"));

        Assert.Equal(1, user.Status);
        Assert.Equal("Test", updatedUser.FirstName);
        Assert.Equal("LastName", updatedUser.LastName);
        Assert.Equal(user.Email, updatedUser.Email);

        var usersWithEmail = await this.ExecuteDbContextAsync<DbDirectoriesContext, int>(db =>
        db.Users.CountAsync(x => x.Email == "test@Test.com"));

        Assert.Equal(1, usersWithEmail);
    }

    [Fact]
    public async Task UserUpdatedWithLastNameWhenExistsButNotLinkedGeneratesAudit()
    {
        var entraId = Guid.NewGuid();

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => "Old Title")
            .RuleFor(x => x.IsEntra, (_, _) => false)
            .RuleFor(x => x.Email, (_, _) => "test@Test.com")
            .RuleFor(x => x.Status, (_, _) => (short)1)
            .RuleFor(x => x.FirstName, (_, _) => "Test123")
            .RuleFor(x => x.LastName, (_, _) => "LastName123")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint();

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new AutoLinkEntraUserToDsiRequest {
            EmailAddress = "test@Test.com",
            EntraUserId = entraId,
            FirstName = "Test",
            LastName = "LastName"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Equal(2, this.AuditCapturer.CapturedRequests.Count);

        var auditRequest1 = this.AuditCapturer.CapturedRequests[0];
        Assert.NotNull(auditRequest1);
        Assert.Equal(AuditEventCategoryNames.ChangeName, auditRequest1.EventCategory);
        Assert.Equal($"Successfully changed name to Test LastName", auditRequest1.Message);
        Assert.Equal(user.Sub, auditRequest1.UserId);

        var auditRequest2 = this.AuditCapturer.CapturedRequests[1];

        Assert.NotNull(auditRequest2);
        Assert.Equal(AuditAuthEventNames.LinkToExistingUser, auditRequest2.EventName);
        Assert.Equal(AuditEventCategoryNames.Auth, auditRequest2.EventCategory);
        Assert.Equal($"Linked Entra account with existing DfE Sign-In user {user.Email}.", auditRequest2.Message);
        Assert.Equal(user.Sub, auditRequest2.UserId);
    }

    [Fact]
    public async Task UserWhenExistsButNotLinkedDoesNotGenerateUserChangeAudit()
    {
        var entraId = Guid.NewGuid();

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => "Old Title")
            .RuleFor(x => x.IsEntra, (_, _) => false)
            .RuleFor(x => x.Email, (_, _) => "test@Test.com")
            .RuleFor(x => x.Status, (_, _) => (short)1)
            .RuleFor(x => x.FirstName, (_, _) => "Test")
            .RuleFor(x => x.LastName, (_, _) => "LastName")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var endpoint = GetEndpoint();

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new AutoLinkEntraUserToDsiRequest {
            EmailAddress = "test@Test.com",
            EntraUserId = entraId,
            FirstName = "Test",
            LastName = "LastName"
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        Assert.Single(this.AuditCapturer.CapturedRequests);

        var auditRequest = this.AuditCapturer.CapturedRequests[0];
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditAuthEventNames.LinkToExistingUser, auditRequest.EventName);
        Assert.Equal(AuditEventCategoryNames.Auth, auditRequest.EventCategory);
        Assert.Equal($"Linked Entra account with existing DfE Sign-In user {user.Email}.", auditRequest.Message);
    }

    /// Create DSI User Tests ///
    [Fact]
    public async Task CreateDsiUserWhenNotExists()
    {
        var entraId = Guid.NewGuid();

        var PendingInvite = new InvitationEntity {
            Code = "123",
            FirstName = "firstName",
            LastName = "LastName",
            CreatedAt = DateTime.UtcNow,
            Email = "test@Test.com"
        };

        await this.InsertEntityAsync<DbDirectoriesContext, InvitationEntity>(PendingInvite);

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var endpoint = GetEndpoint();

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new AutoLinkEntraUserToDsiRequest {
            EmailAddress = "test@Test.com",
            EntraUserId = entraId,
            FirstName = "Test",
            LastName = "LastName"
        });

        var result = await response.Content.ReadFromJsonAsync<AutoLinkEntraUserToDsiResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        //get the user and check to see if its been created
        var createdUser = await this.ExecuteDbContextAsync<DbDirectoriesContext, UserEntity>(db =>
        db.Users.SingleAsync(x => x.Email == "test@Test.com"));

        Assert.Equal(createdUser.Sub, result!.UserId);

        Assert.Equal(1, createdUser.Status);
        Assert.Equal("Test", createdUser.FirstName);
        Assert.Equal("LastName", createdUser.LastName);
        Assert.Equal("test@Test.com", createdUser.Email);
    }

    [Fact]
    public async Task StaleInvitationsGetDeletedWhenUserCreated()
    {
        var entraId = Guid.NewGuid();

        var activePendingInvite = new InvitationEntity {
            Id = Guid.NewGuid(),
            Code = "123",
            FirstName = "firstName",
            LastName = "LastName",
            CreatedAt = DateTime.UtcNow,
            Email = "test@Test.com"
        };

        var stalePendingInvite = new InvitationEntity {
            Id = Guid.NewGuid(),
            Code = "123555",
            FirstName = "firstName",
            LastName = "LastName",
            CreatedAt = DateTime.UtcNow.AddHours(-24),
            Email = "test@Test.com"
        };

        await this.InsertEntityAsync<DbDirectoriesContext, InvitationEntity>(stalePendingInvite);
        await this.InsertEntityAsync<DbDirectoriesContext, InvitationEntity>(activePendingInvite);

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var endpoint = GetEndpoint();

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, new AutoLinkEntraUserToDsiRequest {
            EmailAddress = "test@Test.com",
            EntraUserId = entraId,
            FirstName = "Test",
            LastName = "LastName"
        });

        var result = await response.Content.ReadFromJsonAsync<AutoLinkEntraUserToDsiResponse>();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var userInvitations = await this.ExecuteDbContextAsync<DbDirectoriesContext, List<InvitationEntity>>(db =>
        db.Invitations.Where(x => x.Email == "test@Test.com").ToListAsync());

        Assert.Single(userInvitations);
        Assert.Equal("123", userInvitations[0].Code);
    }

    //** Stale invitiations update the Search

    //Generic email sends support email
    //[Fact]
    //public async Task GenericEmailSendsSupportMessage()
    //{
    //    var entraId = Guid.NewGuid();

    //    var activePendingInvite = new InvitationEntity {
    //        Id = Guid.NewGuid(),
    //        Code = "123",
    //        FirstName = "firstName",
    //        LastName = "LastName",
    //        CreatedAt = DateTime.UtcNow,
    //        Email = "admin@Test.com"
    //    };

    //    await this.InsertEntityAsync<DbDirectoriesContext, InvitationEntity>(activePendingInvite);
    //    var authenticatedClient = this
    //        .CreateClient()
    //        .WithAuthentication();

    //    var endpoint = GetEndpoint();

    //    var response = await authenticatedClient.PostAsJsonAsync(endpoint, new AutoLinkEntraUserToDsiRequest {
    //        EmailAddress = "admin@Test.com",
    //        EntraUserId = entraId,
    //        FirstName = "Test",
    //        LastName = "LastName"
    //    });

    //    var result = await response.Content.ReadFromJsonAsync<AutoLinkEntraUserToDsiResponse>();

    //    Assert.Equal(HttpStatusCode.OK, response.StatusCode);

    //    var userInvitations = await this.ExecuteDbContextAsync<DbDirectoriesContext, List<InvitationEntity>>(db =>
    //    db.Invitations.Where(x => x.Email == "test@Test.com").ToListAsync());

    //    Assert.Single(userInvitations);
    //    Assert.Equal("123", userInvitations[0].Code);
    //}
}
