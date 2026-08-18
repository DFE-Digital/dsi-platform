using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Features.Users.ChangeEmailAddress;
using Dfe.SignIn.Core.Contracts.Features.Users.Shared;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users.ChangeEmailAddress;

[Trait("Category", "Integration")]
public sealed class GetPendingChangeEmailTests : InternalApiIntegrationEndpointTestBase
{
    private static string GetEndpoint(Guid? userId) => $"/internal/users/{userId}/pending-change-email";

    public GetPendingChangeEmailTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns200OK_WithPendingDetails_WhenValidCodeExists()
    {
        var client = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();

        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.Code, (_, _) => "12345678")
            .RuleFor(x => x.Email, (_, _) => "new.email@example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var response = await client.GetAsync(GetEndpoint(user.Sub));

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var result = await response.Content.ReadFromJsonAsync<GetPendingChangeEmailResponse>();
        Assert.NotNull(result);
        Assert.Equal("new.email@example.com", result.NewEmailAddress);
        Assert.False(result.HasExpired);
        Assert.Equal(DateTime.UtcNow.AddHours(1), result.ExpiryTimeUtc, TimeSpan.FromSeconds(1));
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns200OK_WithHasExpiredTrue_WhenCodeIsExpired()
    {
        var dateTimeNow = new DateTimeOffset(2025, 11, 18, 17, 56, 45, TimeSpan.Zero);
        var mockTimeProvider = new MockTimeProvider(dateTimeNow);

        this.TimestampInterceptor.Setup(mockTimeProvider);

        var authenticatedClient = this
            .CreateClient()
            .WithAuthentication();

        var user = EntityFaker.User.Generate();
        var pendingCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.Code, (_, _) => "12345678")
            .RuleFor(x => x.Email, (_, _) => "expired.email@example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(pendingCode);

        var response = await authenticatedClient.GetAsync(GetEndpoint(user.Sub));

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

        var response = await client.GetAsync(GetEndpoint(user.Sub));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns404NotFound_WhenCodeBelongsToDifferentUser()
    {
        var client = this.CreateClient().WithAuthentication();

        var userA = EntityFaker.User.Generate();
        var userB = EntityFaker.User.Generate();

        var codeForB = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => userB.Sub)
            .RuleFor(x => x.Code, (_, _) => "87654321")
            .RuleFor(x => x.Email, (_, _) => "userb.new@example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(userA);
        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(userB);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(codeForB);

        // Request pending change for User A, but the code belongs to User B
        var response = await client.GetAsync(GetEndpoint(userA.Sub));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns404NotFound_WhenCodeTypeIsNotChangeEmail()
    {
        var client = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User.Generate();

        var passwordResetCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.CodeType, (_, _) => UserCodeType.PasswordReset.Value)
            .RuleFor(x => x.Code, (_, _) => "12345678")
            .RuleFor(x => x.Email, (_, _) => "some.email@example.com")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(passwordResetCode);

        var response = await client.GetAsync(GetEndpoint(user.Sub));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns404NotFound_WhenPendingEmailIsBlank()
    {
        var client = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User.Generate();

        var emptyEmailCode = EntityFaker.UserCode
            .RuleFor(x => x.Uid, (_, _) => user.Sub)
            .RuleFor(x => x.Code, (_, _) => "12345678")
            .RuleFor(x => x.Email, (_, _) => "")
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);
        await this.InsertEntityAsync<DbDirectoriesContext, UserCodeEntity>(emptyEmailCode);

        var response = await client.GetAsync(GetEndpoint(user.Sub));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetPendingChangeEmail_Returns401Unauthorized_WhenUnauthenticated()
    {
        var unauthenticatedClient = this.CreateClient();
        var userId = Guid.NewGuid();

        var response = await unauthenticatedClient.GetAsync(GetEndpoint(userId));

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
