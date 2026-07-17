using System.Net;
using System.Net.Http.Json;
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
public class GetUserStatusTests : InternalApiIntegrationEndpointTestBase
{
    private const string endpoint = "interaction/Users.GetUserStatus";

    public GetUserStatusTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Theory]
    [InlineData(0, AccountStatus.Inactive)]
    [InlineData(1, AccountStatus.Active)]
    public async Task GetUserStatus_ReturnsSuccess_WithUserExistsTrue_WhenUserExistsByEmail(short dbStatus, AccountStatus expectedStatus)
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.Email, (_, _) => "test.user@example.com")
            .RuleFor(x => x.Status, (_, _) => dbStatus)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new GetUserStatusRequest {
            EmailAddress = "test.user@example.com"
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<GetUserStatusResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.True(body.Data.UserExists);
        Assert.Equal(user.Sub, body.Data.UserId);
        Assert.Equal(expectedStatus, body.Data.AccountStatus);
    }

    [Theory]
    [InlineData(0, AccountStatus.Inactive)]
    [InlineData(1, AccountStatus.Active)]
    public async Task GetUserStatus_ReturnsSuccess_WithUserExistsTrue_WhenUserExistsByEntraOid(short dbStatus, AccountStatus expectedStatus)
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var entraOid = Guid.NewGuid();
        var user = EntityFaker.User
            .RuleFor(x => x.EntraOid, (_, _) => entraOid)
            .RuleFor(x => x.Status, (_, _) => dbStatus)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new GetUserStatusRequest {
            EntraUserId = entraOid
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<GetUserStatusResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.True(body.Data.UserExists);
        Assert.Equal(user.Sub, body.Data.UserId);
        Assert.Equal(expectedStatus, body.Data.AccountStatus);
    }

    [Fact]
    public async Task GetUserStatus_ReturnsSuccess_WithUserExistsFalse_WhenUserDoesNotExistByEmail()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var request = new GetUserStatusRequest {
            EmailAddress = "nonexistent@example.com"
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<GetUserStatusResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.False(body.Data.UserExists);
        Assert.Null(body.Data.UserId);
        Assert.Null(body.Data.AccountStatus);
    }

    [Fact]
    public async Task GetUserStatus_ReturnsSuccess_WithUserExistsFalse_WhenUserDoesNotExistByEntraOid()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var request = new GetUserStatusRequest {
            EntraUserId = Guid.NewGuid()
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<GetUserStatusResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.False(body.Data.UserExists);
        Assert.Null(body.Data.UserId);
        Assert.Null(body.Data.AccountStatus);
    }

    [Fact]
    public async Task GetUserStatus_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var request = new GetUserStatusRequest {
            EmailAddress = "test@example.com"
        };

        var response = await anonymousClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserStatus_Returns400_WhenBothEmailAndEntraUserIdProvided()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var request = new GetUserStatusRequest {
            EmailAddress = "test@example.com",
            EntraUserId = Guid.NewGuid()
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUserStatus_Returns400_WhenNeitherEmailNorEntraUserIdProvided()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var request = new GetUserStatusRequest {
            EmailAddress = null,
            EntraUserId = null
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task GetUserStatus_Returns400_WhenEmailIsInvalid()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var request = new GetUserStatusRequest {
            EmailAddress = "invalid-email-format"
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }
}
