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
public class GetUserProfileTests : InternalApiIntegrationEndpointTestBase
{
    private const string endpoint = "interaction/Users.GetUserProfile";

    public GetUserProfileTests(InternalApiWebApplicationFactory factory)
        : base(factory)
    {
    }

    [Fact]
    public async Task GetUserProfile_ReturnsSuccess_WhenUserExists()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.IsEntra, (_, _) => true)
            .RuleFor(x => x.IsInternalUser, (_, _) => false)
            .RuleFor(x => x.FirstName, (_, _) => "Jane")
            .RuleFor(x => x.LastName, (_, _) => "Smith")
            .RuleFor(x => x.JobTitle, (_, _) => "Software Developer")
            .RuleFor(x => x.Email, (_, _) => "jane.smith@example.com")
            .RuleFor(x => x.Status, (_, _) => (short)1)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new GetUserProfileRequest {
            UserId = user.Sub
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<GetUserProfileResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.Equal(user.IsEntra, body.Data.IsEntra);
        Assert.Equal(user.IsInternalUser, body.Data.IsInternalUser);
        Assert.Equal(user.FirstName, body.Data.FirstName);
        Assert.Equal(user.LastName, body.Data.LastName);
        Assert.Equal(user.JobTitle, body.Data.JobTitle);
        Assert.Equal(user.Email, body.Data.EmailAddress);
        Assert.Equal(user.Status, body.Data.Status);
    }

    [Fact]
    public async Task GetUserProfile_Returns404_WhenUserDoesNotExist()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var request = new GetUserProfileRequest {
            UserId = Guid.NewGuid()
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserProfile_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var request = new GetUserProfileRequest {
            UserId = Guid.NewGuid()
        };

        var response = await anonymousClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserProfile_Returns400_WhenUserIdIsEmpty()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var request = new GetUserProfileRequest {
            UserId = Guid.Empty
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public async Task GetUserProfile_NormalisesWhitespaceOrNullJobTitle(string? dbJobTitle)
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.JobTitle, (_, _) => dbJobTitle)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new GetUserProfileRequest {
            UserId = user.Sub
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<GetUserProfileResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.Null(body.Data.JobTitle);
    }

    [Theory]
    [InlineData(true, true, 0)]
    [InlineData(false, false, 1)]
    [InlineData(false, true, 1)]
    [InlineData(true, false, 0)]
    public async Task GetUserProfile_MapsUserFlagAndStatusPermutations(bool isEntra, bool isInternalUser, short status)
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var user = EntityFaker.User
            .RuleFor(x => x.IsEntra, (_, _) => isEntra)
            .RuleFor(x => x.IsInternalUser, (_, _) => isInternalUser)
            .RuleFor(x => x.Status, (_, _) => status)
            .Generate();

        await this.InsertEntityAsync<DbDirectoriesContext, UserEntity>(user);

        var request = new GetUserProfileRequest {
            UserId = user.Sub
        };

        var response = await authenticatedClient.PostAsJsonAsync(endpoint, request);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<InteractionResponse<GetUserProfileResponse>>();
        Assert.NotNull(body);
        Assert.NotNull(body.Data);
        Assert.Equal(isEntra, body.Data.IsEntra);
        Assert.Equal(isInternalUser, body.Data.IsInternalUser);
        Assert.Equal(status, body.Data.Status);
    }
}
