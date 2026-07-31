using System.Net;
using System.Net.Http.Json;
using Dfe.SignIn.Core.Contracts.Features.Users.GetUserProfile;
using Dfe.SignIn.Core.Entities.Directories;
using Dfe.SignIn.Gateways.EntityFramework;
using Dfe.SignIn.TestHelpers.Integration.Data;
using Dfe.SignIn.TestHelpers.Integration.Extensions;
using Assert = Xunit.Assert;

namespace Dfe.SignIn.InternalApi.IntegrationTests.Endpoints.Users;

[Trait("Category", "Integration")]
public class GetUserProfileTests : InternalApiIntegrationEndpointTestBase
{
    private const string endpoint = "interaction/{userId}/Users.GetUserProfile";

    private string GetEndpointUrl(Guid userId) => endpoint.Replace("{userId}", userId.ToString());

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

        var response = await authenticatedClient.GetAsync(this.GetEndpointUrl(user.Sub));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GetUserProfileResponse>();
        Assert.NotNull(body);
        Assert.NotNull(body);
        Assert.Equal(user.IsEntra, body.IsEntra);
        Assert.Equal(user.IsInternalUser, body.IsInternalUser);
        Assert.Equal(user.FirstName, body.FirstName);
        Assert.Equal(user.LastName, body.LastName);
        Assert.Equal(user.JobTitle, body.JobTitle);
        Assert.Equal(user.Email, body.EmailAddress);
        Assert.Equal(user.Status, body.Status);
    }

    [Fact]
    public async Task GetUserProfile_Returns404_WhenUserDoesNotExist()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var response = await authenticatedClient.GetAsync(this.GetEndpointUrl(Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task GetUserProfile_Returns401_WhenUnauthenticated()
    {
        var anonymousClient = this.CreateClient();

        var response = await anonymousClient.GetAsync(this.GetEndpointUrl(Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task GetUserProfile_Returns404_WhenUserIdIsEmpty()
    {
        var authenticatedClient = this.CreateClient().WithAuthentication();

        var response = await authenticatedClient.GetAsync(this.GetEndpointUrl(Guid.Empty));
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
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

        var response = await authenticatedClient.GetAsync(this.GetEndpointUrl(user.Sub));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GetUserProfileResponse>();
        Assert.NotNull(body);
        Assert.Null(body.JobTitle);
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

        var response = await authenticatedClient.GetAsync(this.GetEndpointUrl(user.Sub));
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var body = await response.Content.ReadFromJsonAsync<GetUserProfileResponse>();
        Assert.NotNull(body);
        Assert.Equal(isEntra, body.IsEntra);
        Assert.Equal(isInternalUser, body.IsInternalUser);
        Assert.Equal(status, body.Status);
    }
}
