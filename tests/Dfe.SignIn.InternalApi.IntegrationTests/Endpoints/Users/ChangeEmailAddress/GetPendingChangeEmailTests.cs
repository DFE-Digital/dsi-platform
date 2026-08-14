using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users.ChangeEmailAddress;

[Trait("Category", "Integration")]
public sealed class GetPendingChangeEmailTests : InternalApiIntegrationEndpointTestBase
{
    private const string Endpoint = "/internal/users/{userId}/pending-change-email";

    private static string GetEndpointForUser(Guid userId) => Endpoint.Replace("{userId}", userId.ToString());

    public GetPendingChangeEmailTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns200OK_WithPendingDetails_WhenValidCodeExists()
    {
        var client = this.CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();
        var createdAt = DateTime.UtcNow;
        var pendingCode = new UserCodeEntity {
            Uid = user.Sub,
            CodeType = "changeemail",
            Code = "12345678",
            Email = "new.email@example.com",
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var response = await client.GetAsync(GetEndpointForUser(user.Sub));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetPendingChangeEmailResponse>();
        Assert.NotNull(result);
        Assert.Equal("new.email@example.com", result.NewEmailAddress);
        Assert.False(result.HasExpired);
        Assert.Equal(createdAt.AddHours(1), result.ExpiryTimeUtc, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns200OK_WithHasExpiredTrue_WhenCodeIsExpired()
    {
        var dateTimeNow = new DateTimeOffset(2025, 11, 18, 17, 56, 45, TimeSpan.Zero).DateTime;

        var client = this.CreateClient(dateTimeNow)
            .WithAuthentication();

        var createdAt = dateTimeNow.AddHours(-2);

        var user = EntityFaker.User.Generate();
        var pendingCode = new UserCodeEntity {
            Uid = user.Sub,
            CodeType = "changeemail",
            Code = "12345678",
            Email = "expired.email@example.com",
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = createdAt,
            UpdatedAt = createdAt,
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var response = await client.GetAsync(GetEndpointForUser(user.Sub));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetPendingChangeEmailResponse>();
        Assert.NotNull(result);
        Assert.Equal("expired.email@example.com", result.NewEmailAddress);
        Assert.True(result.HasExpired);
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns404NotFound_WhenNoPendingCodeExists()
    {
        var client = this.CreateClient().WithAuthentication();
        var user = EntityFaker.User.Generate();
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var response = await client.GetAsync(GetEndpointForUser(user.Sub));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns404NotFound_WhenCodeBelongsToDifferentUser()
    {
        var client = this.CreateClient().WithAuthentication();

        var userA = EntityFaker.User.Generate();
        var userB = EntityFaker.User.Generate();
        var codeForB = new UserCodeEntity {
            Uid = userB.Sub,
            CodeType = "changeemail",
            Code = "87654321",
            Email = "userb.new@example.com",
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(userA);
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(userB);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(codeForB);

        // Request pending change for User A, but the code belongs to User B
        var response = await client.GetAsync(GetEndpointForUser(userA.Sub));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns404NotFound_WhenCodeTypeIsNotChangeEmail()
    {
        var client = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User.Generate();
        var passwordResetCode = new UserCodeEntity {
            Uid = user.Sub,
            CodeType = "passwordreset",
            Code = "12345678",
            Email = "some.email@example.com",
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(passwordResetCode);

        var response = await client.GetAsync(GetEndpointForUser(user.Sub));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns404NotFound_WhenPendingEmailIsBlank()
    {
        var client = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User.Generate();
        var emptyEmailCode = new UserCodeEntity {
            Uid = user.Sub,
            CodeType = "changeemail",
            Code = "12345678",
            Email = "",
            ClientId = "test-client",
            RedirectUri = "n/a",
            ContextData = null,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(emptyEmailCode);

        var response = await client.GetAsync(GetEndpointForUser(user.Sub));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns401Unauthorized_WhenUnauthenticated()
    {
        var unauthenticatedClient = this.CreateClient();
        var userId = Guid.NewGuid();

        var response = await unauthenticatedClient.GetAsync(GetEndpointForUser(userId));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
