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
public class ChangeNameTests : InternalApiIntegrationEndpointTestBase
{
    private const string endpoint = "interaction/Users.ChangeName";

    public ChangeNameTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ChangeName_ReturnsSuccess_UpdatesDb_AndWritesAudit_WhenUserExists()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var expectedFirstName = "Jane";
        var expectedLastName = "Smith";
        var user = EntityFaker.User
            .RuleFor(x => x.FirstName, (_, _) => "John")
            .RuleFor(x => x.LastName, (_, _) => "Doe")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = expectedFirstName,
            LastName = expectedLastName
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<ChangeNameResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        Assert.Equal(expectedFirstName, updatedUser.FirstName);
        Assert.Equal(expectedLastName, updatedUser.LastName);

        var auditRequest = auditMock.CapturedRequest;
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeName, auditRequest.EventCategory);
        Assert.Equal($"Successfully changed users name to {expectedFirstName} {expectedLastName}", auditRequest.Message);
        Assert.Equal(user.Sub, auditRequest.UserId);
    }

    [Fact]
    public async Task ChangeName_Returns404_WhenUserDoesNotExist()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var request = new ChangeNameRequest {
            UserId = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith"
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var request = new ChangeNameRequest {
            UserId = Guid.NewGuid(),
            FirstName = "Jane",
            LastName = "Smith"
        };

        var response = await anonymousClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_DoesNotWriteAuditEvent_WhenNameIsUnchanged()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var firstName = "John";
        var lastName = "Doe";
        var user = EntityFaker.User
            .RuleFor(x => x.FirstName, (_, _) => firstName)
            .RuleFor(x => x.LastName, (_, _) => lastName)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = firstName,
            LastName = lastName
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<ChangeNameResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        Assert.Equal(firstName, updatedUser.FirstName);
        Assert.Equal(lastName, updatedUser.LastName);

        var auditRequest = auditMock.CapturedRequest;
        Assert.Null(auditRequest);
    }

    [Fact]
    public async Task ChangeName_NormalisesWhitespaceBeforeSaving()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var newFirstName = "Jane    Mary";  // Intentional extra spaces
        var expectedFirstName = "Jane Mary";
        var newLastName = "Smith   Jones"; // Intentional extra spaces
        var expectedLastName = "Smith Jones";
        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = newFirstName,
            LastName = newLastName
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<ChangeNameResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        Assert.Equal(expectedFirstName, updatedUser.FirstName);
        Assert.Equal(expectedLastName, updatedUser.LastName);
    }

    [Fact]
    public async Task ChangeName_LeavesOtherUsersUnchanged()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var newFirstName = "Jane";
        var newLastName = "Smith";
        var users = EntityFaker.User.Generate(3);

        await this.InsertEntitiesAsync<DbDirectoriesContext, UserEntity>(users);

        var userToUpdate = users[1];

        var request = new ChangeNameRequest {
            UserId = userToUpdate.Sub,
            FirstName = newFirstName,
            LastName = newLastName
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<ChangeNameResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();

        var userSubs = users.Select(u => u.Sub).ToList();
        var assertionUsers = await assertionDbContext.Users
            .Where(x => userSubs.Contains(x.Sub))
            .ToListAsync();

        var updatedUser = assertionUsers.Single(x => x.Sub == userToUpdate.Sub);
        Assert.Equal(newFirstName, updatedUser.FirstName);
        Assert.Equal(newLastName, updatedUser.LastName);

        var unchangedUsers = assertionUsers.Where(x => x.Sub != userToUpdate.Sub).ToList();
        foreach (var unchangedUser in unchangedUsers) {
            Assert.NotEqual(newFirstName, unchangedUser.FirstName);
            Assert.NotEqual(newLastName, unchangedUser.LastName);
        }
    }

    [Theory]
    [InlineData("")]
    [InlineData("Jane!!!")]
    public async Task ChangeName_Returns400_WhenFirstNameIsInvalid(string invalidFirstName)
    {
        var (authenticatedClient, _) = this.CreateClientWithAuditMock();
        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = invalidFirstName,
            LastName = "Smith"
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Smith!!!")]
    public async Task ChangeName_Returns400_WhenLastNameIsInvalid(string invalidLastName)
    {
        var (authenticatedClient, _) = this.CreateClientWithAuditMock();
        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = "Jane",
            LastName = invalidLastName
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_Returns400_WhenFirstNameExceedsMaxLength()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var longFirstName = new string('A', 61);
        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = longFirstName,
            LastName = "Smith"
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_Returns400_WhenLastNameExceedsMaxLength()
    {
        var (authenticatedClient, auditMock) = this.CreateClientWithAuditMock();

        var longLastName = new string('A', 61);
        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = "Jane",
            LastName = longLastName
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
