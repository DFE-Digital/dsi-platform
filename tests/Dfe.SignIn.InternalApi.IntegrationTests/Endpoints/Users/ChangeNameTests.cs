using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Audit;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeName;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait("Category", "Integration")]
public class ChangeNameTests : InternalApiIntegrationEndpointTestBase
{
    private static string GetEndpoint(Guid? userId) => $"internal/Users.ChangeName?userId={userId}";

    public ChangeNameTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task ChangeName_ReturnsSuccess_UpdatesDb_AndWritesAudit_WhenUserExists()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var expectedFirstName = "Jane";
        var expectedLastName = "Smith";
        var user = EntityFaker.User
            .RuleFor(x => x.FirstName, (_, _) => "John")
            .RuleFor(x => x.LastName, (_, _) => "Doe")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = expectedFirstName,
            LastName = expectedLastName
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        Assert.Equal(expectedFirstName, updatedUser.FirstName);
        Assert.Equal(expectedLastName, updatedUser.LastName);

        var auditRequest = this.AuditCapturer.CapturedRequests[0];
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeName, auditRequest.EventCategory);
        Assert.Equal($"Successfully changed users name to {expectedFirstName} {expectedLastName}", auditRequest.Message);
        Assert.Equal(user.Sub, auditRequest.UserId);
    }

    [Fact]
    public async Task ChangeName_Returns404_WhenUserDoesNotExist()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var userId = Guid.NewGuid();

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(userId), new ChangeNameRequest {
            UserId = userId,
            FirstName = "Jane",
            LastName = "Smith"
        });

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var userId = Guid.NewGuid();

        var response = await anonymousClient.PostAsJsonAsync(GetEndpoint(userId), new ChangeNameRequest {
            UserId = userId,
            FirstName = "Jane",
            LastName = "Smith"
        });

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_DoesNotWriteAuditEvent_WhenNameIsUnchanged()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var firstName = "John";
        var lastName = "Doe";
        var user = EntityFaker.User
            .RuleFor(x => x.FirstName, (_, _) => firstName)
            .RuleFor(x => x.LastName, (_, _) => lastName)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = firstName,
            LastName = lastName
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        Assert.Equal(firstName, updatedUser.FirstName);
        Assert.Equal(lastName, updatedUser.LastName);

        Assert.Empty(this.AuditCapturer.CapturedRequests);
    }

    [Fact]
    public async Task ChangeName_NormalisesWhitespaceBeforeSaving()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var newFirstName = "Jane    Mary";  // Intentional extra spaces
        var expectedFirstName = "Jane Mary";
        var newLastName = "Smith   Jones"; // Intentional extra spaces
        var expectedLastName = "Smith Jones";
        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = newFirstName,
            LastName = newLastName
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        Assert.Equal(expectedFirstName, updatedUser.FirstName);
        Assert.Equal(expectedLastName, updatedUser.LastName);
    }

    [Fact]
    public async Task ChangeName_LeavesOtherUsersUnchanged()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var newFirstName = "Jane";
        var newLastName = "Smith";
        var users = EntityFaker.User.Generate(3);

        await this.InsertEntitiesAsync<DbDirectoriesContext, UserEntity>(users);

        var userToUpdate = users[1];

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(userToUpdate.Sub), new ChangeNameRequest {
            UserId = userToUpdate.Sub,
            FirstName = newFirstName,
            LastName = newLastName
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

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
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = invalidFirstName,
            LastName = "Smith"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData("")]
    [InlineData("Smith!!!")]
    public async Task ChangeName_Returns400_WhenLastNameIsInvalid(string invalidLastName)
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = "Jane",
            LastName = invalidLastName
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_Returns400_WhenFirstNameExceedsMaxLength()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var longFirstName = new string('A', 61);
        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = longFirstName,
            LastName = "Smith"
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_Returns400_WhenLastNameExceedsMaxLength()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var longLastName = new string('A', 61);
        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = "Jane",
            LastName = longLastName
        });

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task ChangeName_UpdatesOnlyFirstName_WhenLastNameIsUnchanged()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var initialFirstName = "Alex";
        var initialLastName = "Johnson";
        var newFirstName = "Bob";

        var user = EntityFaker.User
            .RuleFor(x => x.FirstName, (_, _) => initialFirstName)
            .RuleFor(x => x.LastName, (_, _) => initialLastName)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = newFirstName,
            LastName = initialLastName
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        Assert.Equal(newFirstName, updatedUser.FirstName);
        Assert.Equal(initialLastName, updatedUser.LastName);

        var auditRequest = this.AuditCapturer.CapturedRequests[0];
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeName, auditRequest.EventCategory);
        Assert.Equal($"Successfully changed users name to {newFirstName} {initialLastName}", auditRequest.Message);
    }

    [Fact]
    public async Task ChangeName_UpdatesOnlyLastName_WhenFirstNameIsUnchanged()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var initialFirstName = "Alex";
        var initialLastName = "Johnson";
        var newLastName = "Smith";

        var user = EntityFaker.User
            .RuleFor(x => x.FirstName, (_, _) => initialFirstName)
            .RuleFor(x => x.LastName, (_, _) => initialLastName)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = initialFirstName,
            LastName = newLastName
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        Assert.Equal(initialFirstName, updatedUser.FirstName);
        Assert.Equal(newLastName, updatedUser.LastName);

        var auditRequest = this.AuditCapturer.CapturedRequests[0];
        Assert.NotNull(auditRequest);
        Assert.Equal(AuditEventCategoryNames.ChangeName, auditRequest.EventCategory);
        Assert.Equal($"Successfully changed users name to {initialFirstName} {newLastName}", auditRequest.Message);
    }

    [Fact]
    public async Task ChangeName_NormalisesTrailingWhitespace()
    {
        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var newFirstName = "Jane   ";
        var expectedFirstName = "Jane";
        var newLastName = "Smith   ";
        var expectedLastName = "Smith";
        var user = EntityFaker.User.Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await authenticatedClient.PostAsJsonAsync(GetEndpoint(user.Sub), new ChangeNameRequest {
            UserId = user.Sub,
            FirstName = newFirstName,
            LastName = newLastName
        });

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var assertionScope = this.WebAppFactory.Services.CreateAsyncScope();
        var assertionDbContext = assertionScope.ServiceProvider.GetRequiredService<DbDirectoriesContext>();
        var updatedUser = await assertionDbContext.Users.SingleAsync(x => x.Sub == user.Sub);
        Assert.Equal(expectedFirstName, updatedUser.FirstName);
        Assert.Equal(expectedLastName, updatedUser.LastName);
    }
}
